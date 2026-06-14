using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Repositories;
using SharedKernel.Results;

namespace FinTech.Application.UseCases.Loans.GetAll;

public sealed record GetLoansQuery(string? UserId) : IQuery<IReadOnlyCollection<LoanResponse>>;

internal sealed class GetLoansQueryHandler(ILoanRepository repo)
    : IQueryHandler<GetLoansQuery, IReadOnlyCollection<LoanResponse>>
{
    public async Task<Result<IReadOnlyCollection<LoanResponse>>> HandleAsync(GetLoansQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<LoanResponse> loans = await repo.GetAllAsync(
            query.UserId,
            selector: loan => new LoanResponse(
                loan.Id,
                loan.UserId,
                loan.Amount,
                loan.Term,
                loan.InterestRate,
                loan.LoanType,
                loan.Status,
                loan.MonthlyPayment,
                loan.CreatedAt,
                loan.UpdatedAt),
            cancellationToken);

        return Result.Success(loans);
    }
}
