using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using FinTech.Domain.Utils;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Application.UseCases;

internal sealed class LoanService(ILoanRepository repo, IUnitOfWork uow) : ILoanService
{
    private const string CurrentUserId = "user-hardcoded-001";
    private const decimal AnnualInterestRate = 0.24m; // 24% annual effective rate (TEA)
    private const int MaxActiveLoansPerUser = 3;
    private const decimal MaxIncomeRatio = 0.40m; // Maximum 40% of monthly income for loan payments
    private const int MinTermMonths = 6;
    private const int MaxTermMonths = 60;
    private const decimal MinLoanAmount = 500;
    private const decimal MaxLoanAmount = 50_000;

    public Task<Result<SimulateLoanResponse>> SimulateAsync(SimulateLoanRequest request)
    {
        Result validationResult = ValidateAmountAndTerm(request.Amount, request.TermMonths);
        if (!validationResult.IsSuccess)
        {
            return Task.FromResult(Result.Failure<SimulateLoanResponse>(validationResult.Error));
        }

        DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
        decimal monthlyRate = FinancialCalculator.GetMonthlyRate(AnnualInterestRate);
        decimal monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(request.Amount, monthlyRate, request.TermMonths);

        List<PaymentInstallment> schedule = FinancialCalculator.GenerateFixedSchedule(
            request.Amount,
            AnnualInterestRate,
            request.TermMonths,
            firstPaymentDate);

        SimulateLoanResponse response = new(
            Amount: request.Amount,
            TermMonths: request.TermMonths,
            AnnualInterestRate: AnnualInterestRate,
            MonthlyPayment: monthlyPayment,
            LoanType: request.LoanType,
            Schedule: schedule);

        return Task.FromResult(Result.Success(response));
    }

    public async Task<Result<LoanResponse>> ApplyForLoanAsync(ApplyForLoanRequest request)
    {
        Result validationResult = ValidateAmountAndTerm(request.Amount, request.TermMonths);
        if (!validationResult.IsSuccess)
        {
            return Result.Failure<LoanResponse>(validationResult.Error);
        }

        // An user must have maximum 3 active loans
        int activeLoanCount = await repo.CountActiveByUserIdAsync(CurrentUserId);

        // Check if the user has too many active loans
        if (activeLoanCount >= MaxActiveLoansPerUser)
        {
            return Result.Failure<LoanResponse>(Error.Validation(
                "Loan.TooManyActiveLoans",
                $"You cannot have more than {MaxActiveLoansPerUser} active loans."));
        }

        DateTime firstPaymentDate = DateTime.UtcNow.AddMonths(1);

        // Calculate the fixed monthly payment using the French System formula
        decimal monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(
            request.Amount,
            FinancialCalculator.GetMonthlyRate(AnnualInterestRate),
            request.TermMonths);

        List<PaymentInstallment> schedule = FinancialCalculator.GenerateFixedSchedule(
            request.Amount,
            AnnualInterestRate,
            request.TermMonths,
            firstPaymentDate
        );

        if (request.MonthlyIncome.HasValue && request.MonthlyIncome > 0)
        {
            decimal existingPayments = await repo.SumMonthlyPaymentByUserIdAsync(CurrentUserId);
            decimal totalPayments = existingPayments + monthlyPayment;
            decimal maxAllowed = request.MonthlyIncome.Value * MaxIncomeRatio;

            if (totalPayments > maxAllowed)
            {
                return Result.Failure<LoanResponse>(Error.Validation(
                    "Loan.PaymentExceedsIncomeLimit",
                    $"The total monthly payment of {totalPayments:C} exceeds the maximum allowed based on your income ({maxAllowed:C})."));
            }
        }

        Loan loan = new()
        {
            Amount = request.Amount,
            UserId = CurrentUserId,
            Term = request.TermMonths,
            InterestRate = AnnualInterestRate,
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
        if (request.Amount < 10_000 && activeLoanCount < 2)
        {
            loan.Status = LoanStatus.Approved;
            loan.UpdatedAt = DateTime.UtcNow;
        };

        await uow.SaveChangesAsync();

        return loan.ToResponse();
    }
    public async Task<Result<IReadOnlyCollection<LoanResponse>>> GetLoansAsync(string userId)
    {
        IReadOnlyCollection<LoanResponse> loans = await repo.GetAllAsync<LoanResponse>(userId, selector: loan =>
            new(loan.Id,
                loan.UserId,
                loan.Amount,
                loan.Term,
                loan.InterestRate,
                loan.LoanType,
                loan.Status,
                loan.MonthlyPayment,
                loan.CreatedAt,
                loan.UpdatedAt));

        return Result.Success(loans);
    }

    public async Task<Result<LoanResponse>> GetLoanAsyncByIdAsync(Guid loanId)
    {
        Loan? loan = await repo.GetByIdAsync(loanId);

        if (loan is null)
        {
            return Result.Failure<LoanResponse>(Error.NotFound("Loan.NotFound", "The specified loan was not found."));
        }
        return loan.ToResponse();
    }

    private static Result ValidateAmountAndTerm(decimal amount, int termMonths)
    {
        if (amount < MinLoanAmount || amount > MaxLoanAmount)
        {
            return Result.Failure(Error.Validation(
                "Loan.InvalidAmount",
                $"The loan amount must be between {MinLoanAmount:C} and {MaxLoanAmount:C}."));
        }

        if (termMonths < MinTermMonths || termMonths > MaxTermMonths)
        {
            return Result.Failure(Error.Validation(
                "Loan.InvalidTerm",
                $"The loan term must be between {MinTermMonths} and {MaxTermMonths} months."));
        }

        return Result.Success();
    }
}
