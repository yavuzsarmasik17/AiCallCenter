using AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies/{companyId:guid}/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(DashboardSummaryResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<
        DashboardSummaryResponse>> GetSummary(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetDashboardSummaryQuery(companyId),
            cancellationToken);

        return Ok(response);
    }
}