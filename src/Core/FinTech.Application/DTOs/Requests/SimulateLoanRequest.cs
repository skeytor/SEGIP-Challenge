using FinTech.Domain.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Application.DTOs.Requests;

public sealed record SimulateLoanRequest(
    [property: Description("The amount of the loan.")]
    [property : Range(500, 50_000, ErrorMessage = "The loan amount must be between 500 and 50,000.")] 
    decimal Amount,

    [property: Description("The term of the loan in months.")]
    [property: Range(6, 60, ErrorMessage = "The loan term must be between 6 and 60 months.")]
    int TermMonths,

    [property: Description("The type of the loan.")]
    [Required]
    LoanType LoanType
);