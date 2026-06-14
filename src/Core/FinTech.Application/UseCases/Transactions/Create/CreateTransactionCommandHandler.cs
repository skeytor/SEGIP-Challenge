using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Application.UseCases.Transactions.Create;

public sealed record CreateTransactionCommand(
    string IdempotencyKey,
    TransactionType Type,
    decimal Amount,
    Guid? LoanId,
    string Description) : ICommand<TransactionResponse>;

internal sealed class CreateTransactionCommandHandler(ITransactionRepository repo, IUnitOfWork uow)
    : ICommandHandler<CreateTransactionCommand, TransactionResponse>
{
    public async Task<Result<TransactionResponse>> HandleAsync(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        // Idempotency check: return existing transaction if same key was already processed
        Transaction? existing = await repo.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing.ToResponse();
        }

        Transaction transaction = new()
        {
            IdempotencyKey = command.IdempotencyKey,
            Type = command.Type,
            Amount = command.Amount,
            Status = TransactionStatus.Completed,
            LoanId = command.LoanId,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow,
        };

        repo.Insert(transaction);
        await uow.SaveChangesAsync(cancellationToken);

        return transaction.ToResponse();
    }
}
