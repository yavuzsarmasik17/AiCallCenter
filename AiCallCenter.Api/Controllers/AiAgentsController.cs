using AiCallCenter.Application.AiAgents.Commands.DeleteAiAgent;
using AiCallCenter.Application.AiAgents.Commands.AttachKnowledgeBase;
using AiCallCenter.Application.AiAgents.Commands.ChatWithAiAgent;
using AiCallCenter.Application.AiAgents.Commands.CreateAiAgent;
using AiCallCenter.Application.AiAgents.Commands.DetachKnowledgeBase;
using AiCallCenter.Application.AiAgents.Commands.UpdateAiAgent;
using AiCallCenter.Application.AiAgents.Commands.UpdateAiAgentSettings;
using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Queries.GetAiAgents;
using AiCallCenter.Application.AiAgents.Queries.GetAttachedKnowledgeBases;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies/{companyId:guid}/ai-agents")]
public sealed class AiAgentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICompanyAccessValidator _companyAccessValidator;

    public AiAgentsController(
        ISender sender,
        ICompanyAccessValidator companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator = companyAccessValidator;
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiAgentDto>>> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var agents = await _sender.Send(
            new GetAiAgentsQuery(companyId),
            cancellationToken);

        return Ok(agents);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<AiAgentDto>> Create(
        Guid companyId,
        [FromBody] CreateAiAgentRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new CreateAiAgentCommand(
            companyId,
            request.Name,
            request.Instructions,
            request.WelcomeMessage,
            request.Language,
            request.Provider,
            request.ModelName);

        var agent = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            agent);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPut("{aiAgentId:guid}")]
    public async Task<ActionResult<AiAgentDto>> Update(
        Guid companyId,
        Guid aiAgentId,
        [FromBody] UpdateAiAgentRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new UpdateAiAgentCommand(
            companyId,
            aiAgentId,
            request.Name,
            request.Instructions,
            request.WelcomeMessage,
            request.Language,
            request.Provider,
            request.ModelName);

        var agent = await _sender.Send(
            command,
            cancellationToken);

        return Ok(agent);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpDelete("{aiAgentId:guid}")]
    public async Task<IActionResult> Delete(
    Guid companyId,
    Guid aiAgentId,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new DeleteAiAgentCommand(
                companyId,
                aiAgentId),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPatch("{aiAgentId:guid}/settings")]
    public async Task<ActionResult<AiAgentDto>> UpdateSettings(
    Guid companyId,
    Guid aiAgentId,
    [FromBody] UpdateAiAgentSettingsRequest request,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new UpdateAiAgentSettingsCommand(
            companyId,
            aiAgentId,
            request.VoiceName,
            request.MaxCallDurationMinutes,
            request.TransferToHumanEnabled,
            request.TransferCallQueueId,
            request.IsActive);

        var agent = await _sender.Send(
            command,
            cancellationToken);

        return Ok(agent);
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpPost("{aiAgentId:guid}/chat")]
    public async Task<ActionResult<ChatWithAiAgentResponse>> Chat(
        Guid companyId,
        Guid aiAgentId,
        [FromBody] ChatWithAiAgentRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new ChatWithAiAgentCommand(
            companyId,
            aiAgentId,
            request.ConversationId,
            request.Message);

        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPost("{aiAgentId:guid}/knowledge-bases/{knowledgeBaseId:guid}")]
    public async Task<IActionResult> AttachKnowledgeBase(
        Guid companyId,
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new AttachKnowledgeBaseCommand(
                companyId,
                aiAgentId,
                knowledgeBaseId),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpDelete("{aiAgentId:guid}/knowledge-bases/{knowledgeBaseId:guid}")]
    public async Task<IActionResult> DetachKnowledgeBase(
        Guid companyId,
        Guid aiAgentId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new DetachKnowledgeBaseCommand(
                companyId,
                aiAgentId,
                knowledgeBaseId),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet("{aiAgentId:guid}/knowledge-bases")]
    public async Task<
        ActionResult<IReadOnlyList<AiAgentKnowledgeBaseDto>>>
        GetAttachedKnowledgeBases(
            Guid companyId,
            Guid aiAgentId,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var knowledgeBases = await _sender.Send(
            new GetAttachedKnowledgeBasesQuery(
                companyId,
                aiAgentId),
            cancellationToken);

        return Ok(knowledgeBases);
    }
}

public sealed class CreateAiAgentRequest
{
    public string Name { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string WelcomeMessage { get; set; } = string.Empty;

    public AgentLanguage Language { get; set; }
        = AgentLanguage.Turkish;

    public AiProvider Provider { get; set; }
        = AiProvider.OpenAI;

    public string ModelName { get; set; }
        = "gpt-5-mini";
}

public sealed class UpdateAiAgentRequest
{
    public string Name { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string WelcomeMessage { get; set; } = string.Empty;

    public AgentLanguage Language { get; set; }
        = AgentLanguage.Turkish;

    public AiProvider Provider { get; set; }
        = AiProvider.OpenAI;

    public string ModelName { get; set; }
        = "gpt-5-mini";
}

public sealed class ChatWithAiAgentRequest
{
    public Guid? ConversationId { get; set; }

    public string Message { get; set; } = string.Empty;
}

public sealed record UpdateAiAgentSettingsRequest(
    string VoiceName,
    int MaxCallDurationMinutes,
    bool TransferToHumanEnabled,
    Guid? TransferCallQueueId,
    bool IsActive);

