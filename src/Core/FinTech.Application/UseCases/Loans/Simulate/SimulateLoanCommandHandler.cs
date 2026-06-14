using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;
using FinTech.Domain.Utils;
using SharedKernel.Results;
using SharedKernel.Utils;

namespace FinTech.Application.UseCases.Loans.Simulate;

public sealed record SimulateLoanCommand(decimal Amount, int TermMonths, LoanType LoanType) 
    : ICommand<SimulateLoanResponse>;

internal sealed class SimulateLoanCommandHandler : ICommandHandler<SimulateLoanCommand, SimulateLoanResponse>
{
    public Task<Result<SimulateLoanResponse>> HandleAsync(SimulateLoanCommand command, CancellationToken cancellationToken)
    {
        if (command.Amount < 500 || command.Amount > 50_000)
        {
            return Task.FromResult(Result.Failure<SimulateLoanResponse>(Error.Validation(
                "Loan.InvalidAmount",
                $"The loan amount must be between 500 and 50,000.")));
        }

        if (command.TermMonths < 6 || command.TermMonths > 60)
        {
            return Task.FromResult(Result.Failure<SimulateLoanResponse>(Error.Validation(
                "Loan.InvalidTerm",
                $"The loan term must be between 6 and 60 months.")));
        }

        DateTime firstPaymentDate = DateTime.Today.AddMonths(1);
        decimal monthlyRate = FinancialCalculator.GetMonthlyRate(FinancialConstants.AnnualInterestRate);
        decimal monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(command.Amount, monthlyRate, command.TermMonths);

        List<PaymentInstallment> schedule = FinancialCalculator.GenerateFixedSchedule(
            command.Amount,
            FinancialConstants.AnnualInterestRate,
            command.TermMonths,
            firstPaymentDate);

        SimulateLoanResponse response = new(
            Amount: command.Amount,
            TermMonths: command.TermMonths,
            AnnualInterestRate: FinancialConstants.AnnualInterestRate,
            MonthlyPayment: monthlyPayment,
            LoanType: command.LoanType,
            Schedule: schedule);

        return Task.FromResult(Result.Success(response));
    }
}
