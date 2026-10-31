using AiCallCenter.Application.CallRouting.Commands
    .RouteWaitingCall;
using AiCallCenter.Application.CallRouting.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/call-routing")]
public sealed class CallRoutingController
    : ControllerBase
{
    private readonly ISender _sender;

    public CallRoutingController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("route/{queueItemId:guid}")]
    [ProducesResponseType(
        typeof(CallRoutingResultDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<
        CallRoutingResultDto>> Route(
        Guid companyId,
        Guid queueItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RouteWaitingCallCommand(
                companyId,
                queueItemId),
            cancellationToken);

        return Ok(result);
    }
}
