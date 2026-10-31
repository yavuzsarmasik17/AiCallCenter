using AiCallCenter.Application.CallOperatorAiAgents.Commands
    .AssignAiAgent;
using AiCallCenter.Application.CallOperatorAiAgents.Commands
    .RemoveAiAgent;
using AiCallCenter.Application.CallOperatorAiAgents.Commands
    .SetPrimaryAiAgent;
using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetAvailableAiAgents;
using AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetOperatorAiAgents;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route(
    "api/companies/{companyId:guid}/call-operators/{operatorId:guid}/ai-agents")]
public sealed class CallOperatorAiAgentsController
    : ControllerBase
{
    private readonly ISender _sender;

    public CallOperatorAiAgentsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CallOperatorAiAgentDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<CallOperatorAiAgentDto>>> GetAll(
        Guid companyId,
        Guid operatorId,
        CancellationToken cancellationToken)
    {
        var query = new GetOperatorAiAgentsQuery(
            companyId,
            operatorId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("available")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AvailableAiAgentDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<AvailableAiAgentDto>>> GetAvailable(
        Guid companyId,
        Guid operatorId,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableAiAgentsQuery(
            companyId,
            operatorId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CallOperatorAiAgentDto),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<
        CallOperatorAiAgentDto>> Assign(
        Guid companyId,
        Guid operatorId,
        [FromBody] AssignAiAgentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignAiAgentCommand(
            companyId,
            operatorId,
            request.AiAgentId,
            request.IsPrimary);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPut("{assignmentId:guid}/primary")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPrimary(
        Guid companyId,
        Guid operatorId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var command = new SetPrimaryAiAgentCommand(
            companyId,
            operatorId,
            assignmentId);

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{assignmentId:guid}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        Guid companyId,
        Guid operatorId,
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveAiAgentCommand(
            companyId,
            operatorId,
            assignmentId);

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }
}

public sealed record AssignAiAgentRequest(
    Guid AiAgentId,
    bool IsPrimary);