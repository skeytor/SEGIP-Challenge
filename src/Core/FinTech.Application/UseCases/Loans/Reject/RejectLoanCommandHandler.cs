using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Application.UseCases.Loans.Reject;

public sealed record RejectLoanCommand(Guid LoanId) : ICommand<LoanResponse>;

internal sealed class RejectLoanCommandHandler(ILoanRepository repo, IUnitOfWork uow)
    : ICommandHandler<RejectLoanCommand, LoanResponse>
{
    public async Task<Result<LoanResponse>> HandleAsync(RejectLoanCommand command, CancellationToken cancellationToken)
    {
        Loan? loan = await repo.GetByIdAsync(command.LoanId, cancellationToken);

        if (loan is null)
        {
            return Result.Failure<LoanResponse>(Error.NotFound("Loan.NotFound", "The specified loan was not found."));
        }

        if (loan.Status is not LoanStatus.Pending)
        {
            return Result.Failure<LoanResponse>(Error.Conflict(
                "Loan.InvalidStatus",
                $"Only pending loans can be rejected. Current status: {loan.Status}."));
        }

        loan.Status = LoanStatus.Rejected;
        loan.UpdatedAt = DateTime.UtcNow;

        await uow.SaveChangesAsync(cancellationToken);

        return loan.ToResponse();
    }
}
