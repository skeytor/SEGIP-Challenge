using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using FinTech.Domain.Utils;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;
using SharedKernel.Utils;

namespace FinTech.Application.UseCases.Loans.Create;

public sealed record ApplyForLoanCommand(
    decimal Amount,
    string UserId,
    int TermMonths,
    LoanType LoanType,
    decimal? MonthlyIncome)
    : ICommand<LoanResponse>;

internal sealed class ApplyForLoanCommandHandler(ILoanRepository repo, ITransactionRepository transactionRepo, IUnitOfWork uow)
    : ICommandHandler<ApplyForLoanCommand, LoanResponse>
{
    public async Task<Result<LoanResponse>> HandleAsync(ApplyForLoanCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount < FinancialConstants.MinLoanAmount || command.Amount > FinancialConstants.MaxLoanAmount)
        {
            return Result.Failure<LoanResponse>(Error.Validation(
                "Loan.InvalidAmount",
                $"The loan amount must be between {FinancialConstants.MinLoanAmount} and {FinancialConstants.MaxLoanAmount}."));
        }

        if (command.TermMonths < FinancialConstants.MinTermMonths || command.TermMonths > FinancialConstants.MaxTermMonths)
        {
            return Result.Failure<LoanResponse>(Error.Validation(
                "Loan.InvalidTerm",
                $"The loan term must be between {FinancialConstants.MinTermMonths} and {FinancialConstants.MaxTermMonths} months."));
        }

        // An user must have maximum 3 active loans
        int activeLoanCount = await repo.CountActiveByUserIdAsync(command.UserId);

        // Check if the user has too many active loans
        if (activeLoanCount >= FinancialConstants.MaxActiveLoansPerUser)
        {
            return Result.Failure<LoanResponse>(Error.Validation(
                "Loan.TooManyActiveLoans",
                $"You cannot have more than {FinancialConstants.MaxActiveLoansPerUser} active loans."));
        }

        DateTime firstPaymentDate = DateTime.UtcNow.AddMonths(1);

        // Calculate the fixed monthly payment using the French System formula
        decimal monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(
            command.Amount,
            FinancialCalculator.GetMonthlyRate(FinancialConstants.AnnualInterestRate),
            command.TermMonths);

        List<PaymentInstallment> schedule = FinancialCalculator.GenerateFixedSchedule(
            command.Amount,
            FinancialConstants.AnnualInterestRate,
            command.TermMonths,
            firstPaymentDate
        );

        if (command.MonthlyIncome.HasValue && command.MonthlyIncome > 0)
        {
            decimal existingPayments = await repo.SumMonthlyPaymentByUserIdAsync(command.UserId);
            decimal totalPayments = existingPayments + monthlyPayment;
            decimal maxAllowed = command.MonthlyIncome.Value * FinancialConstants.MaxIncomeRatio;

            if (totalPayments > maxAllowed)
            {
                return Result.Failure<LoanResponse>(Error.Validation(
                    "Loan.PaymentExceedsIncomeLimit",
                    $"The total monthly payment of {totalPayments:C} exceeds the maximum allowed based on your income ({maxAllowed:C})."));
            }
        }

        Loan loan = new()
        {
            Amount = command.Amount,
            UserId = command.UserId,
            Term = command.TermMonths,
            InterestRate = FinancialConstants.AnnualInterestRate,
            LoanType = LoanType.Fixed,
            Status = LoanStatus.Pending,
            MonthlyPayment = monthlyPayment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PaymentSchedules = [.. schedule.Select(s => new PaymentSchedule
            {
                PaymentNumber = s.PaymentNumber,
                DueDate = s.DueDate,
                TotalPayment = s.TotalPayment,
                Principal = s.Principal,
                Interest = s.Interest,
                RemainingBalance = s.RemainingBalance
            })]
        };

        repo.Insert(loan);

        // Automatically approve the loan: amount < 10,000 and less than 2 active loans
        if (command.Amount < 10_000 && activeLoanCount < 2)
        {
            loan.Status = LoanStatus.Approved;
            loan.UpdatedAt = DateTime.UtcNow;

            transactionRepo.Insert(new Transaction
            {
                IdempotencyKey = $"disbursement-{loan.Id}",
                Type = TransactionType.Disbursement,
                Amount = loan.Amount,
                Status = TransactionStatus.Completed,
                LoanId = loan.Id,
                Description = $"Loan disbursement for loan {loan.Id}",
                CreatedAt = DateTime.UtcNow,
            });
        }

        await uow.SaveChangesAsync();

        return loan.ToResponse();

    }
}
