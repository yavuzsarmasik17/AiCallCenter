using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.KnowledgeBases.Commands.CreateKnowledgeBase;
using AiCallCenter.Application.KnowledgeBases.Commands.DeleteKnowledgeBase;
using AiCallCenter.Application.KnowledgeBases.Commands.UpdateKnowledgeBase;
using AiCallCenter.Application.KnowledgeBases.DTOs;
using AiCallCenter.Application.KnowledgeBases.Queries.GetKnowledgeBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies/{companyId:guid}/knowledge-bases")]
public sealed class KnowledgeBasesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICompanyAccessValidator _companyAccessValidator;

    public KnowledgeBasesController(
        ISender sender,
        ICompanyAccessValidator companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator = companyAccessValidator;
    }

    [Authorize(Roles = "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeBaseDto>>> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var knowledgeBases = await _sender.Send(
            new GetKnowledgeBasesQuery(companyId),
            cancellationToken);

        return Ok(knowledgeBases);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<KnowledgeBaseDto>> Create(
        Guid companyId,
        [FromBody] CreateKnowledgeBaseRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new CreateKnowledgeBaseCommand(
            companyId,
            request.Name,
            request.Description);

        var knowledgeBase = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            knowledgeBase);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPut("{knowledgeBaseId:guid}")]
    public async Task<ActionResult<KnowledgeBaseDto>> Update(
        Guid companyId,
        Guid knowledgeBaseId,
        [FromBody] UpdateKnowledgeBaseRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new UpdateKnowledgeBaseCommand(
            companyId,
            knowledgeBaseId,
            request.Name,
            request.Description);

        var knowledgeBase = await _sender.Send(
            command,
            cancellationToken);

        return Ok(knowledgeBase);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpDelete("{knowledgeBaseId:guid}")]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new DeleteKnowledgeBaseCommand(
                companyId,
                knowledgeBaseId),
            cancellationToken);

        return NoContent();
    }
}

public sealed class CreateKnowledgeBaseRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public sealed class UpdateKnowledgeBaseRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}