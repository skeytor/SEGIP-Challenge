using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;

namespace FinTech.Application.UseCases.Transactions.GetById;

public sealed record GetTransactionByIdQuery(Guid TransactionId) : IQuery<TransactionResponse>;

internal sealed class GetTransactionByIdQueryHandler(ITransactionRepository repo)
    : IQueryHandler<GetTransactionByIdQuery, TransactionResponse>
{
    public async Task<Result<TransactionResponse>> HandleAsync(GetTransactionByIdQuery query, CancellationToken cancellationToken)
    {
        Transaction? transaction = await repo.GetByIdAsync(query.TransactionId, cancellationToken);

        if (transaction is null)
            return Result.Failure<TransactionResponse>(
                Error.NotFound("Transaction.NotFound", "The specified transaction was not found."));

        return transaction.ToResponse();
    }
}
