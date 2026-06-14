using FinTech.Domain.Entities;
using System.Linq.Expressions;

namespace FinTech.Domain.Repositories;

public interface ILoanRepository : IRepository<Loan, Guid>
{
    Task<IReadOnlyCollection<TResult>> GetWithScheduleAsync<TResult>(
        Guid id,
        Expression<Func<Loan, TResult>> selector,
        CancellationToken ct = default);
    Task<int> CountActiveByUserIdAsync(string userId);

    Task<decimal> SumMonthlyPaymentByUserIdAsync(string userId, CancellationToken ct = default);
}
