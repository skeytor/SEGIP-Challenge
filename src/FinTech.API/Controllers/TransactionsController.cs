using FinTech.Application.Abstractions;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases.Transactions.Create;
using FinTech.Application.UseCases.Transactions.GetAll;
using FinTech.Application.UseCases.Transactions.GetById;
using FinTech.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

namespace FinTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    [HttpPost]
    [EndpointDescription("Create a new transaction")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResponse>> Create(
        [FromBody] CreateTransactionCommand command,
        [FromServices] ICommandHandler<CreateTransactionCommand, TransactionResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(command, ct);
        return result.Match<ActionResult>(
            onSuccess: Ok, 
            onFailure: _ => BadRequest());
    }

    [HttpGet]
    [EndpointDescription("Get a transaction by ID")]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TransactionResponse>>> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status,
        [FromServices] IQueryHandler<GetTransactionsQuery, IReadOnlyCollection<TransactionResponse>> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTransactionsQuery(type, status), ct);
        return result.Match<ActionResult>(
            onSuccess: Ok,
            onFailure: _ => BadRequest());
    }

    [HttpGet("{id:guid}")]
    [EndpointDescription("Get a transaction by ID")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetTransactionByIdQuery, TransactionResponse> handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(new GetTransactionByIdQuery(id), ct);
        return result.Match<ActionResult>(
            onSuccess: Ok,
            onFailure: error => error.Type == ErrorType.NotFound 
                ? NotFound() 
                : BadRequest());
    }
}
