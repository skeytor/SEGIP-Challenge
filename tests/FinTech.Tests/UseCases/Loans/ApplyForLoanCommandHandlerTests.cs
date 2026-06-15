using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Loans.Create;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using Moq;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;
using SharedKernel.Utils;

namespace FinTech.Tests.UseCases.Loans;

public class ApplyForLoanCommandHandlerTests
{
    private readonly Mock<ILoanRepository> _repoMock = new();
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly ApplyForLoanCommandHandler _handler;

    public ApplyForLoanCommandHandlerTests()
    {
        _handler = new ApplyForLoanCommandHandler(_repoMock.Object, _transactionRepositoryMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidLoanWithLessThan2ActiveLoans_ReturnsApprovedLoan()
    {
        var command = new ApplyForLoanCommand(
            Amount: 5_000,
            UserId: "user-123",
            TermMonths: 12,
            LoanType: LoanType.Fixed,
            MonthlyIncome: null);

        _repoMock.Setup(r => r.CountActiveByUserIdAsync(command.UserId)).ReturnsAsync(0);
        _repoMock.Setup(r => r.Insert(It.IsAny<Loan>()));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Result<LoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoanStatus.Approved, result.Value.Status);
        Assert.Equal(command.Amount, result.Value.Amount);
    }

    [Fact]
    public async Task HandleAsync_ValidLoanWith2OrMoreActiveLoans_ReturnsLoanInPendingStatus()
    {
        var command = new ApplyForLoanCommand(
            Amount: 5_000,
            UserId: "user-123",
            TermMonths: 12,
            LoanType: LoanType.Fixed,
            MonthlyIncome: null);

        _repoMock.Setup(r => r.CountActiveByUserIdAsync(command.UserId)).ReturnsAsync(2);
        _repoMock.Setup(r => r.Insert(It.IsAny<Loan>()));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Result<LoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LoanStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_ExceedsMaxActiveLoans_ReturnsValidationFailure()
    {
        var command = new ApplyForLoanCommand(
            Amount: 5_000,
            UserId: "user-123",
            TermMonths: 12,
            LoanType: LoanType.Fixed,
            MonthlyIncome: null);

        _repoMock.Setup(r => r.CountActiveByUserIdAsync(command.UserId))
            .ReturnsAsync(FinancialConstants.MaxActiveLoansPerUser);

        Result<LoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Loan.TooManyActiveLoans", result.Error.Code);
        _repoMock.Verify(r => r.Insert(It.IsAny<Loan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_MonthlyPaymentExceeds40PercentOfIncome_ReturnsValidationFailure()
    {
        // Loan of $20,000 over 12 months will have a high monthly payment
        // Monthly income of $500 means max allowed = $200, which will be exceeded
        var command = new ApplyForLoanCommand(
            Amount: 20_000,
            UserId: "user-123",
            TermMonths: 12,
            LoanType: LoanType.Fixed,
            MonthlyIncome: 500);

        _repoMock.Setup(r => r.CountActiveByUserIdAsync(command.UserId)).ReturnsAsync(0);
        _repoMock.Setup(r => r.SumMonthlyPaymentByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        Result<LoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Loan.PaymentExceedsIncomeLimit", result.Error.Code);
        _repoMock.Verify(r => r.Insert(It.IsAny<Loan>()), Times.Never);
    }

    [Theory]
    [InlineData(499)]
    [InlineData(50_001)]
    public async Task HandleAsync_InvalidAmount_ReturnsValidationFailure(decimal invalidAmount)
    {
        var command = new ApplyForLoanCommand(invalidAmount, "user-123", 12, LoanType.Fixed, null);

        Result<LoanResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Loan.InvalidAmount", result.Error.Code);
        _repoMock.Verify(r => r.CountActiveByUserIdAsync(It.IsAny<string>()), Times.Never);
    }
}
