using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinTech.Persistence.Repositories;

internal class Repository<TEntity, TId>(AppDbContext context) : IRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>
    where TId : IEquatable<TId>
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    protected AppDbContext Context => context;

    public virtual Task<int> CountAsync() => _dbSet.CountAsync();

    public virtual ValueTask<TEntity?> FindAsync(TId id, CancellationToken ct = default) => _dbSet.FindAsync([id], ct);

    public virtual async Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken ct = default) => await _dbSet.Select(selector).ToListAsync(ct);

    public virtual Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default) => 
        _dbSet.FirstOrDefaultAsync(e => e.Id.Equals(id), ct);

    public virtual void Insert(TEntity entity) => _dbSet.Add(entity);
}
