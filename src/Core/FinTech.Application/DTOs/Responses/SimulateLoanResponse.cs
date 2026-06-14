using FinTech.Domain.Entities;
using FinTech.Domain.Utils;

namespace FinTech.Application.DTOs.Responses;

public sealed record SimulateLoanResponse(
    decimal Amount,
    int TermMonths,
    decimal AnnualInterestRate,
    decimal MonthlyPayment,
    LoanType LoanType,
    IReadOnlyList<PaymentInstallment> Schedule
);
