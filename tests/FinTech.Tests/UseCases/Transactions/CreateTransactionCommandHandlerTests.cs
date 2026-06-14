using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Transactions.Create;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using Moq;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Tests.UseCases.Transactions;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _handler = new CreateTransactionCommandHandler(_repoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task HandleAsync_DuplicateIdempotencyKey_ReturnsExistingTransactionWithoutCreatingNew()
    {
        var existingTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = "key-001",
            Type = TransactionType.Payment,
            Amount = 500,
            Status = TransactionStatus.Completed,
            Description = "Original payment",
            CreatedAt = DateTime.UtcNow,
        };

        var command = new CreateTransactionCommand(
            IdempotencyKey: "key-001",
            Type: TransactionType.Payment,
            Amount: 999,
            LoanId: null,
            Description: "Duplicate attempt");

        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync("key-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        Result<TransactionResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existingTransaction.Id, result.Value.Id);
        Assert.Equal(500, result.Value.Amount);
        _repoMock.Verify(r => r.Insert(It.IsAny<Transaction>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NewIdempotencyKey_CreatesAndReturnsNewTransaction()
    {
        var command = new CreateTransactionCommand(
            IdempotencyKey: "key-002",
            Type: TransactionType.Payment,
            Amount: 1_500,
            LoanId: Guid.NewGuid(),
            Description: "Monthly payment");

        _repoMock.Setup(r => r.GetByIdempotencyKeyAsync("key-002", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        _repoMock.Setup(r => r.Insert(It.IsAny<Transaction>()));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        Result<TransactionResponse> result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.Amount, result.Value.Amount);
        Assert.Equal(TransactionStatus.Completed, result.Value.Status);
        _repoMock.Verify(r => r.Insert(It.IsAny<Transaction>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
