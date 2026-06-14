using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinTech.Persistence.Repositories;

internal sealed class TransactionRepository(AppDbContext context) : Repository<Transaction, Guid>(context), ITransactionRepository
{
    public Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default) =>
        Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, ct);

    public async Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        TransactionType? type,
        TransactionStatus? status,
        Expression<Func<Transaction, TResult>> selector,
        CancellationToken ct = default)
    {
        IQueryable<Transaction> query = Context.Transactions.AsNoTracking();

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(selector)
            .ToListAsync(ct);
    }
}
