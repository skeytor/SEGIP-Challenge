using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;

namespace FinTech.Application.Extensions.Mappers;

internal static class TransactionMappers
{
    public static TransactionResponse ToResponse(this Transaction source) =>
        new(source.Id,
            source.IdempotencyKey,
            source.Type,
            source.Amount,
            source.Status,
            source.LoanId,
            source.Description,
            source.CreatedAt);
}
