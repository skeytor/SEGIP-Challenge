using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinTech.Persistence.Repositories;

internal sealed class LoanRepository(AppDbContext context) : Repository<Loan, Guid>(context), ILoanRepository
{
    public Task<int> CountActiveByUserIdAsync(string userId) => 
        Context.Loans.AsNoTracking().CountAsync(l => l.UserId == userId && l.Status == LoanStatus.Active);

    public async Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(
        string userId,
        Expression<Func<Loan, TResult>> selector,
        CancellationToken ct = default)
    {
        IQueryable<Loan> root = Context.Loans.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(userId))
        {
            root = root.Where(l => l.UserId == userId);
        }
        return await root.Select(selector).ToListAsync(ct);
    }

    public Task<TResult?> GetWithScheduleAsync<TResult>(
        Guid id,
        Expression<Func<Loan, TResult>> selector,
        CancellationToken ct = default) => 
            Context.Loans.AsNoTracking()
                .Where(l => l.Id == id)
                .Include(l => l.PaymentSchedules)
                .Select(selector)
                .FirstOrDefaultAsync(ct);

    public Task<decimal> SumMonthlyPaymentByUserIdAsync(string userId, CancellationToken ct = default) => 
        Context.Loans.AsNoTracking()
            .Where(l => l.UserId == userId && (l.Status == LoanStatus.Active))
            .SumAsync(l => l.MonthlyPayment, ct);
}
