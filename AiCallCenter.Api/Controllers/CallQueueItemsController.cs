using AiCallCenter.Application.CallQueueItems.Commands.AcceptQueuedCall;
using AiCallCenter.Application.CallQueueItems.Commands.CompleteQueuedCall;
using AiCallCenter.Application.CallQueueItems.Commands.EnqueueCall;
using AiCallCenter.Application.CallQueueItems.Dtos;
using AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItemById;
using AiCallCenter.Application.CallQueueItems.Queries.GetCallQueueItems;
using AiCallCenter.Application.CallQueueItems.Queries.GetOperatorCallQueueItems;
using AiCallCenter.Application.CallQueueItems.Queries.GetWaitingCallQueueItems;
using AiCallCenter.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/call-queue-items")]
public sealed class CallQueueItemsController : ControllerBase
{
    private readonly ISender _sender;

    public CallQueueItemsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CallQueueItemListDto>>> GetAll(
        Guid companyId,
        [FromQuery] CallQueueStatus? status,
        [FromQuery] Guid? callQueueId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallQueueItemsQuery(companyId, status, callQueueId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("waiting")]
    public async Task<ActionResult<IReadOnlyList<CallQueueItemListDto>>> GetWaiting(
        Guid companyId,
        [FromQuery] Guid? callQueueId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetWaitingCallQueueItemsQuery(companyId, callQueueId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("operator/{operatorId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CallQueueItemListDto>>> GetByOperator(
        Guid companyId,
        Guid operatorId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetOperatorCallQueueItemsQuery(companyId, operatorId, activeOnly),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{queueItemId:guid}")]
    public async Task<ActionResult<CallQueueItemListDto>> GetById(
        Guid companyId,
        Guid queueItemId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallQueueItemByIdQuery(companyId, queueItemId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EnqueueCallResultDto>> Enqueue(
        Guid companyId,
        [FromBody] EnqueueCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new EnqueueCallCommand(
                companyId,
                request.CallQueueId,
                request.CallId,
                request.Priority,
                request.TransferReason),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { companyId, queueItemId = result.QueueItemId },
            result);
    }

    [HttpPost("{queueItemId:guid}/accept")]
    public async Task<ActionResult<QueuedCallActionResultDto>> Accept(
        Guid companyId,
        Guid queueItemId,
        [FromBody] OperatorActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AcceptQueuedCallCommand(
                companyId,
                queueItemId,
                request.CallOperatorId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{queueItemId:guid}/complete")]
    public async Task<ActionResult<QueuedCallActionResultDto>> Complete(
        Guid companyId,
        Guid queueItemId,
        [FromBody] CompleteQueuedCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CompleteQueuedCallCommand(
                companyId,
                queueItemId,
                request.CallOperatorId,
                request.Summary,
                request.Result),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record EnqueueCallRequest(
    Guid CallQueueId,
    Guid CallId,
    int? Priority,
    string? TransferReason);

public sealed record OperatorActionRequest(Guid CallOperatorId);

public sealed record CompleteQueuedCallRequest(
    Guid CallOperatorId,
    string? Summary,
    string? Result);
