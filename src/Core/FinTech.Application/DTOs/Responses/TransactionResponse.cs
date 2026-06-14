using FinTech.Domain.Entities;

namespace FinTech.Application.DTOs.Responses;

public sealed record TransactionResponse(
    Guid Id,
    string IdempotencyKey,
    TransactionType Type,
    decimal Amount,
    TransactionStatus Status,
    Guid? LoanId,
    string Description,
    DateTime CreatedAt);
