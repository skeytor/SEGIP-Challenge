using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Domain.Utils;
using SharedKernel.Results;

namespace FinTech.Application.UseCases;

/// <summary>
/// Defines the contract for loan-related operations.
/// </summary>
public interface ILoanService
{
    /// <summary>
    /// Simulates a loan based on the provided parameters and returns a payment schedule.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The payment schedule.</returns>
    Task<Result<SimulateLoanResponse>> SimulateAsync(SimulateLoanRequest request);

    /// <summary>
    /// Processes a loan application, performing necessary validations and returning the result of the application.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Result<LoanResponse>> ApplyForLoanAsync(ApplyForLoanRequest request);

    /// <summary>
    /// Retrieves a list of loans associated with the specified user ID. If the user ID is null, 
    /// it may return all loans or an empty collection based on implementation.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Result<IReadOnlyCollection<LoanResponse>>> GetLoansAsync(string? userId);

    Task<Result<LoanResponse>> GetLoanAsyncByIdAsync(Guid loanId);
    Task<Result<IReadOnlyCollection<PaymentScheduleResponse>>> GetScheduleAsync(Guid loanId);
    Task<Result<LoanResponse>> ApproveAsync(Guid loanId);
    Task<Result<LoanResponse>> RejectAsync(Guid loanId);
}
