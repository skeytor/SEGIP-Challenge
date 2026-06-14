namespace FinTech.Domain.Entities;

public abstract class BaseEntity<TId> where TId : IEquatable<TId>
{
    public TId Id { get; set; } = default!;
}