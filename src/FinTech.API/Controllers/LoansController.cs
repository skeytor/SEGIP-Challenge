using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

namespace FinTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoansController(ILoanService service) : ControllerBase
{
    /// <summary>
    /// Simulates a loan based on the provided parameters.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(SimulateLoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SimulateLoanResponse>> Simulate([FromBody] SimulateLoanRequest request)
    {
        var result = await service.SimulateAsync(request);
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> ApplyForLoan([FromBody] ApplyForLoanRequest request)
    {
        var result = await service.ApplyForLoanAsync(request);
        return result.Match<ActionResult>(
            Ok,
            error => BadRequest()
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans([FromQuery] string? userId)
    {
        var result = await service.GetLoansAsync(userId);
        return result.Match<ActionResult>(
            Ok,
            error => BadRequest()
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> GetLoan([FromRoute] Guid id)
    {
        var result = await service.GetLoanAsyncByIdAsync(id);
        return result.Match<ActionResult>(
            Ok,
            error => BadRequest()
        );
    }

    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> Approve([FromRoute] Guid id)
    {
        var result = await service.ApproveAsync(id);
        return result.Match<ActionResult>(
            Ok,
            error => error.Type == ErrorType.NotFound ? NotFound() : Conflict()
        );
    }

    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> Reject([FromRoute] Guid id)
    {
        var result = await service.RejectAsync(id);
        return result.Match<ActionResult>(
            Ok,
            error => error.Type == ErrorType.NotFound ? NotFound() : Conflict()
        );
    }

    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(List<PaymentScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PaymentScheduleResponse>>> GetSchedule([FromRoute] Guid id)
    {
        var result = await service.GetScheduleAsync(id);
        return result.Match<ActionResult>(
            Ok,
            error => NotFound()
        );
    }
}
