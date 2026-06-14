using FinTech.Domain.Entities;

namespace FinTech.Application.DTOs.Responses;

public sealed record LoanResponse(
    Guid Id,
    string UserId,
    decimal Amount,
    int TermMonths,
    decimal AnnualInterestRate,
    LoanType LoanType,
    LoanStatus Status,
    decimal MonthlyPayment,
    DateTime CreatedAt,
    DateTime UpdatedAt);
