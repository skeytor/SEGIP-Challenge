using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;

namespace FinTech.Application.Mappers;

internal static class LoanMappers
{
    public static LoanResponse ToResponse(this Loan source) => 
        new(source.Id,
            source.UserId,
            source.Amount,
            source.Term,
            source.InterestRate,
            source.LoanType,
            source.Status,
            source.MonthlyPayment,
            source.CreatedAt,
            source.UpdatedAt);
}
