using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Application.UseCases;

internal sealed class TransactionService(ITransactionRepository repo, IUnitOfWork uow) : ITransactionService
{
    public async Task<Result<TransactionResponse>> CreateAsync(CreateTransactionRequest request)
    {
        // Idempotency check: return existing transaction if same key was already processed
        Transaction? existing = await repo.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing is not null)
        {
            return existing.ToResponse();
        }

        Transaction transaction = new()
        {
            IdempotencyKey = request.IdempotencyKey,
            Type = request.Type,
            Amount = request.Amount,
            Status = TransactionStatus.Completed,
            LoanId = request.LoanId,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
        };

        repo.Insert(transaction);
        await uow.SaveChangesAsync();

        return transaction.ToResponse();
    }

    public async Task<Result<IReadOnlyCollection<TransactionResponse>>> GetAllAsync(TransactionType? type, TransactionStatus? status)
    {
        IReadOnlyCollection<TransactionResponse> transactions = await repo.GetAllAsync(
            type,
            status,
            selector: t => new TransactionResponse(
                t.Id,
                t.IdempotencyKey,
                t.Type,
                t.Amount,
                t.Status,
                t.LoanId,
                t.Description,
                t.CreatedAt));

        return Result.Success(transactions);
    }

    public async Task<Result<TransactionResponse>> GetByIdAsync(Guid id)
    {
        Transaction? transaction = await repo.GetByIdAsync(id);

        if (transaction is null)
            return Result.Failure<TransactionResponse>(
                Error.NotFound("Transaction.NotFound", "The specified transaction was not found."));

        return transaction.ToResponse();
    }
}
