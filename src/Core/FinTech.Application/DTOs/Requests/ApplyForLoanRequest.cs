using System.ComponentModel.DataAnnotations;

namespace FinTech.Application.DTOs.Requests;

public sealed record ApplyForLoanRequest(
    [Range(500, 50_000, ErrorMessage = "The loan amount must be between 500 and 50,000.")]
    decimal Amount,

    [Range(6, 60, ErrorMessage = "The loan term must be between 6 and 60 months.")]
    int TermMonths,

    [Range(0.01, double.MaxValue, ErrorMessage = "The monthly income must be greater than 0.")]
    decimal? MonthlyIncome
);
