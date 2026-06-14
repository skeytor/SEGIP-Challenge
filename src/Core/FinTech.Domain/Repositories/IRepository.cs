using FinTech.Domain.Entities;

namespace FinTech.Domain.Repositories;

public interface IRepository<TEntity, in TId>
    where TEntity : BaseEntity<TId>
    where TId : IEquatable<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    ValueTask<TEntity?> FindAsync(TId id, CancellationToken ct = default);
    Task InsertAsync(TEntity entity, CancellationToken ct = default);
    Task<int> CountAsync();
}
