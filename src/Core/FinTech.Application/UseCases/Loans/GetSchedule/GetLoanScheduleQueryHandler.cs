using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;

namespace FinTech.Application.UseCases.Loans.GetSchedule;

public sealed record GetLoanScheduleQuery(Guid LoanId) : IQuery<IReadOnlyCollection<PaymentScheduleResponse>>;

internal sealed class GetLoanScheduleQueryHandler(ILoanRepository repo)
    : IQueryHandler<GetLoanScheduleQuery, IReadOnlyCollection<PaymentScheduleResponse>>
{
    public async Task<Result<IReadOnlyCollection<PaymentScheduleResponse>>> HandleAsync(
        GetLoanScheduleQuery query,
        CancellationToken cancellationToken)
    {
        bool loanExists = await repo.GetByIdAsync(query.LoanId, cancellationToken) is not null;
        if (!loanExists)
        {
            return Result.Failure<IReadOnlyCollection<PaymentScheduleResponse>>(
                Error.NotFound("Loan.NotFound", "The specified loan was not found."));
        }

        IReadOnlyCollection<PaymentScheduleResponse> schedule = await repo.GetScheduleByLoanIdAsync(
            query.LoanId,
            selector: s => new PaymentScheduleResponse(
                s.PaymentNumber,
                s.DueDate,
                s.TotalPayment,
                s.Principal,
                s.Interest,
                s.RemainingBalance,
                s.Status),
            cancellationToken);

        return Result.Success(schedule);
    }
}
