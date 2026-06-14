using FinTech.Domain.Entities;

namespace FinTech.Domain.Repositories;

public interface ILoanRepository : IRepository<Loan, Guid>
{
    Task<IReadOnlyCollection<TResult>> GetAllByUserId<TResult>(string userId);
    Task<IReadOnlyCollection<TResult>> GetWithScheduleAsync<TResult>(
        Guid loanId, 
        CancellationToken ct = default);
    Task<int> CountActiveByUserIdAsync(string userId);

    Task<decimal> SumMonthlyPaymentByUserIdAsync(string userId, CancellationToken ct = default);
}
