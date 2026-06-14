using FinTech.Domain.Entities;
using System.Linq.Expressions;

namespace FinTech.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction, Guid>
{
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
    Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        TransactionType? type,
        TransactionStatus? status,
        Expression<Func<Transaction, TResult>> selector,
        CancellationToken ct = default);
}
