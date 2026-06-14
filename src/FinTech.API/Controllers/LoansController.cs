using FinTech.Application.DTOs.Requests;
using FinTech.Application.DTOs.Responses;
using FinTech.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost()]
    [ProducesResponseType(typeof(SimulateLoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SimulateLoanResponse>> ApplyForLoan([FromBody] ApplyForLoanRequest request)
    {
        var result = await service.ApplyForLoanAsync(request);
        return result.Match<ActionResult>(
            success => Ok(success),
            error => BadRequest()
        );
    }

}
