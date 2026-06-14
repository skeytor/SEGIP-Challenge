using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Entities;
using SharedKernel.Results;

namespace FinTech.Application.UseCases;

public interface ITransactionService
{
    Task<Result<TransactionResponse>> CreateAsync(CreateTransactionRequest request);
    Task<Result<IReadOnlyCollection<TransactionResponse>>> GetAllAsync(TransactionType? type, TransactionStatus? status);
    Task<Result<TransactionResponse>> GetByIdAsync(Guid id);
}
