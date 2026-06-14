using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases;
using FinTech.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Results;

namespace FinTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TransactionsController(ITransactionService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResponse>> Create([FromBody] CreateTransactionRequest request)
    {
        var result = await service.CreateAsync(request);
        return result.Match<ActionResult>(
            Ok,
            _ => BadRequest()
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TransactionResponse>>> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status)
    {
        var result = await service.GetAllAsync(type, status);
        return result.Match<ActionResult>(
            Ok,
            _ => BadRequest()
        );
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById([FromRoute] Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result.Match<ActionResult>(
            Ok,
            error => error.Type == ErrorType.NotFound ? NotFound() : BadRequest()
        );
    }
}
