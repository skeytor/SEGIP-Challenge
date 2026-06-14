using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;

namespace FinTech.Application.UseCases.Transactions.GetAll;

public sealed record GetTransactionsQuery(TransactionType? Type, TransactionStatus? Status)
    : IQuery<IReadOnlyCollection<TransactionResponse>>;

internal sealed class GetTransactionsQueryHandler(ITransactionRepository repo)
    : IQueryHandler<GetTransactionsQuery, IReadOnlyCollection<TransactionResponse>>
{
    public async Task<Result<IReadOnlyCollection<TransactionResponse>>> HandleAsync(
        GetTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TransactionResponse> transactions = await repo.GetAllAsync(
            query.Type,
            query.Status,
            selector: t => new TransactionResponse(
                t.Id,
                t.IdempotencyKey,
                t.Type,
                t.Amount,
                t.Status,
                t.LoanId,
                t.Description,
                t.CreatedAt),
            cancellationToken);

        return Result.Success(transactions);
    }
}
