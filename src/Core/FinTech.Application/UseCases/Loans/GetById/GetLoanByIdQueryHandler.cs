using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.Extensions.Mappers;
using FinTech.Domain.Entities;
using FinTech.Domain.Repositories;
using SharedKernel.Results;

namespace FinTech.Application.UseCases.Loans.GetById;

public sealed record GetLoanByIdQuery(Guid LoanId) : IQuery<LoanResponse>;

internal sealed class GetLoanByIdQueryHandler(ILoanRepository repo)
    : IQueryHandler<GetLoanByIdQuery, LoanResponse>
{
    public async Task<Result<LoanResponse>> HandleAsync(GetLoanByIdQuery query, CancellationToken cancellationToken)
    {
        Loan? loan = await repo.GetByIdAsync(query.LoanId, cancellationToken);

        if (loan is null)
        {
            return Result.Failure<LoanResponse>(Error.NotFound(
                "Loan.NotFound", 
                "The specified loan was not found."));
        }

        return loan.ToResponse();
    }
}
