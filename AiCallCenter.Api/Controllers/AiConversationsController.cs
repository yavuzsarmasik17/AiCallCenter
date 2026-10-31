using AiCallCenter.Application.AiConversations.Commands.CreateConversation;
using AiCallCenter.Api.Models;
using AiCallCenter.Application.AiConversations.Commands.CancelConversation;
using AiCallCenter.Application.AiConversations.Commands.CompleteConversation;
using AiCallCenter.Application.AiConversations.DTOs;
using AiCallCenter.Application.AiConversations.Queries.GetConversationDetail;
using AiCallCenter.Application.AiConversations.Queries.GetConversations;
using AiCallCenter.Application.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies/{companyId:guid}/conversations")]
public sealed class AiConversationsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICompanyAccessValidator _companyAccessValidator;

    public AiConversationsController(
        ISender sender,
        ICompanyAccessValidator companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator = companyAccessValidator;
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AiConversationDto>>> GetAll(
    Guid companyId,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var conversations = await _sender.Send(
            new GetConversationsQuery(companyId),
            cancellationToken);

        return Ok(conversations);
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<AiConversationDetailDto>> GetDetail(
    Guid companyId,
    Guid conversationId,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var conversation = await _sender.Send(
            new GetConversationDetailQuery(
                companyId,
                conversationId),
            cancellationToken);

        return Ok(conversation);
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
    Guid companyId,
    [FromBody] CreateAiConversationRequest request,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var conversationId = await _sender.Send(
            new CreateConversationCommand(
                companyId,
                request.AiAgentId,
                request.CustomerName,
                request.CustomerPhone),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetDetail),
            new
            {
                companyId,
                conversationId
            },
            conversationId);
    }


    [Authorize(Roles = "CompanyAdmin,Supervisor,SuperAdmin")]
    [HttpPost("{conversationId:guid}/complete")]
    public async Task<IActionResult> Complete(
    Guid companyId,
    Guid conversationId,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new CompleteConversationCommand(
                companyId,
                conversationId),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,SuperAdmin")]
    [HttpPost("{conversationId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
    Guid companyId,
    Guid conversationId,
    CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new CancelConversationCommand(
                companyId,
                conversationId),
            cancellationToken);

        return NoContent();
    }
}