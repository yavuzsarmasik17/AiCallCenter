using AiCallCenter.Application.Calls.Commands.CreateCall;
using AiCallCenter.Application.Calls.Commands.CreateCallMessage;
using AiCallCenter.Application.Calls.Commands.ProcessCallCustomerMessage;
using AiCallCenter.Application.Calls.Commands.SendOperatorMessage;
using AiCallCenter.Application.Calls.Dtos;
using AiCallCenter.Application.Calls.Queries.GetCallById;
using AiCallCenter.Application.Calls.Queries.GetCallMessages;
using AiCallCenter.Application.Calls.Queries.GetCalls;
using AiCallCenter.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route("api/companies/{companyId:guid}/calls")]
public sealed class CallsController : ControllerBase
{
    private readonly ISender _sender;

    public CallsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<CallDto>>> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallsQuery(companyId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{callId:guid}")]
    public async Task<ActionResult<CallDto>> GetById(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallByIdQuery(
                companyId,
                callId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CallDto>> Create(
        Guid companyId,
        [FromBody] CreateCallRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCallCommand(
                companyId,
                request.AiAgentId,
                request.CallerNumber,
                request.CalledNumber,
                request.Direction),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                companyId,
                callId = result.Id
            },
            result);
    }

    [HttpGet("{callId:guid}/messages")]
    public async Task<ActionResult<
        IReadOnlyList<CallMessageDto>>> GetMessages(
        Guid companyId,
        Guid callId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCallMessagesQuery(
                companyId,
                callId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{callId:guid}/messages")]
    public async Task<ActionResult<CallMessageDto>>
        CreateMessage(
            Guid companyId,
            Guid callId,
            [FromBody]
            CreateCallMessageRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCallMessageCommand(
                companyId,
                callId,
                request.Speaker,
                request.Content,
                request.ConfidenceScore,
                request.AudioFileUrl),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetMessages),
            new
            {
                companyId,
                callId
            },
            result);
    }

    /*
     * Müşteri mesajını alır.
     *
     * Knowledge Base içerisinde arama yapar,
     * AI cevabını üretir ve gerekirse
     * insan temsilci aktarımını başlatır.
     */
    [HttpPost("{callId:guid}/customer-message")]
    public async Task<ActionResult<
        ProcessCallCustomerMessageResponse>>
        ProcessCustomerMessage(
            Guid companyId,
            Guid callId,
            [FromBody]
            ProcessCallCustomerMessageRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ProcessCallCustomerMessageCommand(
                companyId,
                callId,
                request.Message),
            cancellationToken);

        return Ok(result);
    }

    /*
     * İnsan operatörün aktif çağrıya
     * gönderdiği mesajı kaydeder.
     *
     * Mesaj CallMessages tablosuna
     * HumanAgent olarak eklenir.
     */
    [HttpPost("{callId:guid}/operator-message")]
    public async Task<ActionResult<CallMessageDto>>
        SendOperatorMessage(
            Guid companyId,
            Guid callId,
            [FromBody]
            SendOperatorMessageRequest request,
            CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SendOperatorMessageCommand(
                companyId,
                callId,
                request.Message),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateCallRequest(
    Guid AiAgentId,
    string CallerNumber,
    string CalledNumber,
    CallDirection Direction);

public sealed record CreateCallMessageRequest(
    MessageSpeaker Speaker,
    string Content,
    double? ConfidenceScore,
    string? AudioFileUrl);

public sealed record ProcessCallCustomerMessageRequest(
    string Message);

public sealed record SendOperatorMessageRequest(
    string Message);