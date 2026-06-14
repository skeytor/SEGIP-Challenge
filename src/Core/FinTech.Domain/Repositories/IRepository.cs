using FinTech.Domain.Entities;
using System.Linq.Expressions;

namespace FinTech.Domain.Repositories;

public interface IRepository<TEntity, in TId>
    where TEntity : BaseEntity<TId>
    where TId : IEquatable<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    ValueTask<TEntity?> FindAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector, 
        CancellationToken ct = default);
    void Insert(TEntity entity);
    Task<int> CountAsync();
}
