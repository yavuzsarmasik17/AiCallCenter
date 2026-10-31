using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.KnowledgeDocuments.Commands.CreateTextDocument;
using AiCallCenter.Application.KnowledgeDocuments.Commands.DeleteKnowledgeDocument;
using AiCallCenter.Application.KnowledgeDocuments.Commands.UploadPdfDocument;
using AiCallCenter.Application.KnowledgeDocuments.DTOs;
using AiCallCenter.Application.KnowledgeDocuments.Queries.GetKnowledgeDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route(
    "api/companies/{companyId:guid}/knowledge-bases/" +
    "{knowledgeBaseId:guid}/documents")]
public sealed class KnowledgeDocumentsController
    : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICompanyAccessValidator
        _companyAccessValidator;

    public KnowledgeDocumentsController(
        ISender sender,
        ICompanyAccessValidator
            companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator =
            companyAccessValidator;
    }

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<KnowledgeDocumentDto>>> GetAll(
        Guid companyId,
        Guid knowledgeBaseId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var documents = await _sender.Send(
            new GetKnowledgeDocumentsQuery(
                companyId,
                knowledgeBaseId),
            cancellationToken);

        return Ok(documents);
    }

    [HttpPost("text")]
    public async Task<ActionResult<
        KnowledgeDocumentDto>> CreateText(
        Guid companyId,
        Guid knowledgeBaseId,
        [FromBody] CreateTextDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var document = await _sender.Send(
            new CreateTextDocumentCommand(
                companyId,
                knowledgeBaseId,
                request.Name,
                request.Content),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            document);
    }

    [HttpPost("pdf")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<
        KnowledgeDocumentDto>> UploadPdf(
        Guid companyId,
        Guid knowledgeBaseId,
        [FromForm] UploadPdfDocumentRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        if (request.File is null ||
            request.File.Length == 0)
        {
            return BadRequest(
                new
                {
                    message =
                        "PDF dosyası seçilmelidir."
                });
        }

        if (!string.Equals(
                request.File.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                new
                {
                    message =
                        "Yalnızca PDF dosyası yüklenebilir."
                });
        }

        await using var stream =
            request.File.OpenReadStream();

        var document = await _sender.Send(
            new UploadPdfDocumentCommand(
                companyId,
                knowledgeBaseId,
                request.Name,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                stream),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            document);
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid knowledgeBaseId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new DeleteKnowledgeDocumentCommand(
                companyId,
                knowledgeBaseId,
                documentId),
            cancellationToken);

        return NoContent();
    }
}

public sealed class CreateTextDocumentRequest
{
    public string Name { get; set; } =
        string.Empty;

    public string Content { get; set; } =
        string.Empty;
}

public sealed class UploadPdfDocumentRequest
{
    public string Name { get; set; } =
        string.Empty;

    public IFormFile File { get; set; } =
        null!;
}
