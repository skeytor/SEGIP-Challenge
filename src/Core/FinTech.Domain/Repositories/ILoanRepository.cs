using FinTech.Domain.Entities;
using System.Linq.Expressions;

namespace FinTech.Domain.Repositories;

public interface ILoanRepository : IRepository<Loan, Guid>
{
    Task<IReadOnlyCollection<TResult>> GetScheduleByLoanIdAsync<TResult>(
        Guid loanId,
        Expression<Func<PaymentSchedule, TResult>> selector,
        CancellationToken ct = default);
    Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        string? userId,
        Expression<Func<Loan, TResult>> selector,
        CancellationToken ct = default);
    Task<int> CountActiveByUserIdAsync(string userId);
    Task<decimal> SumMonthlyPaymentByUserIdAsync(string userId, CancellationToken ct = default);
}
