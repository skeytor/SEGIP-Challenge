using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Loans.Simulate;
using FinTech.Domain.Entities;
using SharedKernel.Results;
using SharedKernel.Utils;

namespace FinTech.Tests.UseCases.Loans;

public class SimulateLoanCommandHandlerTests
{
    private readonly SimulateLoanCommandHandler _handler = new();

    [Fact]
    public async Task HandleAsync_ValidAmountAndTerm_ReturnsScheduleWithCorrectInstallmentCount()
    {
        var command = new SimulateLoanCommand(Amount: 10_000, TermMonths: 12, LoanType: LoanType.Fixed);

        Result<SimulateLoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value.Schedule.Count);
    }

    [Fact]
    public async Task HandleAsync_ValidLoan_MonthlyPaymentMatchesFrenchSystemFormula()
    {
        decimal amount = 10_000;
        int termMonths = 12;
        var command = new SimulateLoanCommand(amount, termMonths, LoanType.Fixed);

        Result<SimulateLoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        // Manually calculate expected payment: Amount * [TEM*(1+TEM)^n] / [(1+TEM)^n - 1]
        decimal tem = (decimal)(Math.Pow((double)(1 + FinancialConstants.AnnualInterestRate), 1.0 / 12) - 1);
        double factor = Math.Pow(1 + (double)tem, termMonths);
        decimal expected = Math.Round((decimal)(((double)amount * (double)tem * factor) / (factor - 1)), 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.MonthlyPayment);
    }

    [Theory]
    [InlineData(499)]
    [InlineData(0)]
    [InlineData(50_001)]
    public async Task HandleAsync_InvalidAmount_ReturnsValidationFailure(decimal invalidAmount)
    {
        var command = new SimulateLoanCommand(Amount: invalidAmount, TermMonths: 12, LoanType: LoanType.Fixed);

        Result<SimulateLoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Loan.InvalidAmount", result.Error.Code);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(61)]
    public async Task HandleAsync_InvalidTerm_ReturnsValidationFailure(int invalidTerm)
    {
        var command = new SimulateLoanCommand(Amount: 5_000, TermMonths: invalidTerm, LoanType: LoanType.Fixed);

        Result<SimulateLoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Loan.InvalidTerm", result.Error.Code);
    }
}
