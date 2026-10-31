using AiCallCenter.Application.CallQueues.Commands
    .AssignOperatorToQueue;
using AiCallCenter.Application.CallQueues.Commands
    .CreateCallQueue;
using AiCallCenter.Application.CallQueues.Commands
    .DeleteCallQueue;
using AiCallCenter.Application.CallQueues.Commands
    .RemoveOperatorFromQueue;
using AiCallCenter.Application.CallQueues.Commands
    .UpdateCallQueue;
using AiCallCenter.Application.CallQueues.Dtos;
using AiCallCenter.Application.CallQueues.Queries
    .GetCallQueueById;
using AiCallCenter.Application.CallQueues.Queries
    .GetCallQueues;
using AiCallCenter.Application.CallQueues.Queries
    .GetQueueOperators;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/call-queues")]
public sealed class CallQueuesController : ControllerBase
{
    private readonly ISender _sender;

    public CallQueuesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CallQueueDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<CallQueueDto>>> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallQueuesQuery(companyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{queueId:guid}")]
    [ProducesResponseType(
        typeof(CallQueueDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CallQueueDto>> GetById(
        Guid companyId,
        Guid queueId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallQueueByIdQuery(
                companyId,
                queueId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CallQueueDto),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<CallQueueDto>> Create(
        Guid companyId,
        [FromBody] CreateCallQueueRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCallQueueCommand(
            companyId,
            request.Name,
            request.Description,
            request.Priority,
            request.MaxWaitingSeconds,
            request.MaxQueueSize,
            request.AiAgentId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPut("{queueId:guid}")]
    [ProducesResponseType(
        typeof(CallQueueDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<CallQueueDto>> Update(
        Guid companyId,
        Guid queueId,
        [FromBody] UpdateCallQueueRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCallQueueCommand(
            companyId,
            queueId,
            request.Name,
            request.Description,
            request.Priority,
            request.MaxWaitingSeconds,
            request.MaxQueueSize,
            request.AiAgentId,
            request.IsActive);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{queueId:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid queueId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteCallQueueCommand(
                companyId,
                queueId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{queueId:guid}/operators")]
    [ProducesResponseType(
        typeof(IReadOnlyList<CallQueueOperatorDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<CallQueueOperatorDto>>> GetOperators(
        Guid companyId,
        Guid queueId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetQueueOperatorsQuery(
                companyId,
                queueId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{queueId:guid}/operators")]
    [ProducesResponseType(
        typeof(CallQueueOperatorDto),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<
        CallQueueOperatorDto>> AssignOperator(
        Guid companyId,
        Guid queueId,
        [FromBody] AssignQueueOperatorRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new AssignOperatorToQueueCommand(
                companyId,
                queueId,
                request.CallOperatorId,
                request.Priority);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpDelete(
        "{queueId:guid}/operators/{assignmentId:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveOperator(
        Guid companyId,
        Guid queueId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RemoveOperatorFromQueueCommand(
                companyId,
                queueId,
                assignmentId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record CreateCallQueueRequest(
    string Name,
    string? Description,
    int Priority,
    int MaxWaitingSeconds,
    int MaxQueueSize,
    Guid? AiAgentId);

public sealed record UpdateCallQueueRequest(
    string Name,
    string? Description,
    int Priority,
    int MaxWaitingSeconds,
    int MaxQueueSize,
    Guid? AiAgentId,
    bool IsActive);

public sealed record AssignQueueOperatorRequest(
    Guid CallOperatorId,
    int Priority);