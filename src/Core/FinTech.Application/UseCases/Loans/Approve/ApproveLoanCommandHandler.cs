using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;
using SharedKernel.UnitOfWork;

namespace FinTech.Application.UseCases.Loans.Approve;

public sealed record ApproveLoanCommand(Guid LoanId) : ICommand<LoanResponse>;

internal sealed class ApproveLoanCommandHandler(
    ILoanRepository loanRepo,
    ITransactionRepository transactionRepo,
    IUnitOfWork uow) : ICommandHandler<ApproveLoanCommand, LoanResponse>
{
    public async Task<Result<LoanResponse>> HandleAsync(ApproveLoanCommand command, CancellationToken cancellationToken)
    {
        Loan? loan = await loanRepo.GetByIdAsync(command.LoanId, cancellationToken);

        if (loan is null)
            return Result.Failure<LoanResponse>(Error.NotFound("Loan.NotFound", "The specified loan was not found."));

        if (loan.Status != LoanStatus.Pending)
            return Result.Failure<LoanResponse>(Error.Conflict(
                "Loan.InvalidStatus",
                $"Only pending loans can be approved. Current status: {loan.Status}."));

        loan.Status = LoanStatus.Approved;
        loan.UpdatedAt = DateTime.UtcNow;

        transactionRepo.Insert(new Transaction
        {
            IdempotencyKey = $"disbursement-{command.LoanId}",
            Type = TransactionType.Disbursement,
            Amount = loan.Amount,
            Status = TransactionStatus.Completed,
            LoanId = command.LoanId,
            Description = $"Loan disbursement for loan {command.LoanId}",
            CreatedAt = DateTime.UtcNow,
        });

        await uow.SaveChangesAsync(cancellationToken);

        return loan.ToResponse();
    }
}
