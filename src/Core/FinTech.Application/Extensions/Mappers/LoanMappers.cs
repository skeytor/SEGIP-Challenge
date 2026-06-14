using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;

namespace FinTech.Application.Extensions.Mappers;

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

    public static PaymentScheduleResponse ToResponse(this PaymentSchedule source) =>
        new(source.PaymentNumber,
            source.DueDate,
            source.TotalPayment,
            source.Principal,
            source.Interest,
            source.RemainingBalance,
            source.Status);
}
