using FinTech.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Application.DTOs.Requests;

public sealed record SimulateLoanRequest(
    [Range(500, 50_000, ErrorMessage = "The loan amount must be between 500 and 50,000.")] 
    decimal Amount,

    [Range(6, 60, ErrorMessage = "The loan term must be between 6 and 60 months.")]
    int TermMonths,

    [Required]
    LoanType LoanType
);