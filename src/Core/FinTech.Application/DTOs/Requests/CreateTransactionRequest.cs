using FinTech.Domain.Entities;

namespace FinTech.Application.DTOs.Requests;

public sealed record CreateTransactionRequest(
    string IdempotencyKey,
    TransactionType Type,
    decimal Amount,
    Guid? LoanId,
    string Description);
