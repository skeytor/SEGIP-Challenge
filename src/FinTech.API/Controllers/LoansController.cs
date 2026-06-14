using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Loans.Approve;
using FinTech.Application.UseCases.Loans.Create;
using FinTech.Application.UseCases.Loans.GetAll;
using FinTech.Application.UseCases.Loans.GetById;
using FinTech.Application.UseCases.Loans.GetSchedule;
using FinTech.Application.UseCases.Loans.Reject;
using FinTech.Application.UseCases.Loans.Simulate;
using FinTech.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;
using System.ComponentModel;

namespace FinTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoansController : ControllerBase
{
    [HttpPost("simulate")]
    [EndpointDescription("Simulate a loan to see the payment schedule and total interest.")]
    [ProducesResponseType(typeof(SimulateLoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SimulateLoanResponse>> Simulate(
        [FromBody] SimulateLoanRequest request,
        [FromServices] ICommandHandler<SimulateLoanCommand, SimulateLoanResponse> handler,
        CancellationToken ct)
    {
        var command = new SimulateLoanCommand(request.Amount, request.TermMonths, LoanType.Fixed);
        var result = await handler.HandleAsync(command, ct);
        return result.Match<ActionResult>(
            onSuccess: Ok,
            onFailure: error => BadRequest(new ProblemDetails { Title = "Simulation Failed", Detail = error.Description }));
    }

    [HttpPost]
    [EndpointDescription("Apply for a new loan.")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanResponse>> ApplyForLoan(
        [FromBody] ApplyForLoanRequest request,
        [FromServices] ICommandHandler<ApplyForLoanCommand, LoanResponse> handler,
        CancellationToken ct)
    {
        var command = new ApplyForLoanCommand(request.Amount, "user-hardcoded", request.TermMonths, LoanType.Fixed, null);
        var result = await  handler.HandleAsync(command, ct);
        return result.Match<ActionResult>(
            onSuccess: Ok,
            onFailure: error => BadRequest(new ProblemDetails { Title = "Application Failed", Detail = error.Description }));
    }

    [HttpGet]
    [EndpointDescription("Get all loan applications.")]
    [ProducesResponseType(typeof(List<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LoanResponse>>> GetLoans(
        [FromQuery][Description("The ID of the user for whom to retrieve loans.")] string? userId,
        [FromServices] IQueryHandler<GetLoansQuery, IReadOnlyCollection<LoanResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetLoansQuery(userId), ct);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [EndpointDescription("Get details of a specific loan application by ID.")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LoanResponse>> GetLoan(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetLoanByIdQuery, LoanResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetLoanByIdQuery(id), ct);
        return result.Match<ActionResult>(
            onSuccess: Ok, 
            onFailure: error => error.Type is ErrorType.NotFound 
                ? NotFound() 
                : BadRequest());
    }

    [HttpPatch("{id:guid}/approve")]
    [EndpointDescription("Approve a pending loan application.")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> Approve(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<ApproveLoanCommand, LoanResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new ApproveLoanCommand(id), ct);
        return result.Match<ActionResult>(Ok, error => error.Type == ErrorType.NotFound ? NotFound() : Conflict());
    }

    [HttpPatch("{id:guid}/reject")]
    [EndpointDescription("Reject a pending loan application.")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LoanResponse>> Reject(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RejectLoanCommand, LoanResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new RejectLoanCommand(id), ct);
        return result.Match<ActionResult>(
            onSuccess: Ok, 
            onFailure: error => error.Type == ErrorType.NotFound 
                ? NotFound() 
                : Conflict());
    }

    [HttpGet("{id:guid}/schedule")]
    [EndpointDescription("Get the payment schedule for an approved loan.")]
    [ProducesResponseType(typeof(List<PaymentScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PaymentScheduleResponse>>> GetSchedule(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetLoanScheduleQuery, IReadOnlyCollection<PaymentScheduleResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetLoanScheduleQuery(id), ct);
        return result.Match<ActionResult>(
            onSuccess: Ok, 
            onFailure: error => error.Type == ErrorType.NotFound 
                ? NotFound() 
                : BadRequest());
    }
}
