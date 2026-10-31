using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Common.Telephony;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies/{companyId:guid}/telephony")]
public sealed class TelephonyController
    : ControllerBase
{
    private readonly ITelephonyProviderResolver
        _providerResolver;

    private readonly ICompanyAccessValidator
        _companyAccessValidator;

    public TelephonyController(
        ITelephonyProviderResolver providerResolver,
        ICompanyAccessValidator companyAccessValidator)
    {
        _providerResolver = providerResolver;
        _companyAccessValidator =
            companyAccessValidator;
    }

    [Authorize(
        Roles =
            "SuperAdmin,CompanyAdmin,Supervisor,Operator")]
    [HttpPost("outbound-calls")]
    public async Task<ActionResult<TelephonyCallResult>>
        StartOutboundCall(
            Guid companyId,
            [FromBody]
            StartOutboundTelephonyCallApiRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var provider = _providerResolver.Resolve();

        var result =
            await provider.StartOutboundCallAsync(
                new StartOutboundCallRequest(
                    companyId,
                    request.CallId,
                    request.ToNumber,
                    request.FromNumber,
                    CreateOptionalUri(request.AnswerUrl),
                    CreateOptionalUri(
                        request.StatusCallbackUrl)),
                cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    [Authorize(
        Roles =
            "SuperAdmin,CompanyAdmin,Supervisor,Operator")]
    [HttpPost("calls/{callId:guid}/transfer")]
    public async Task<ActionResult<TelephonyCallResult>>
        TransferCall(
            Guid companyId,
            Guid callId,
            [FromBody]
            TransferTelephonyCallApiRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var provider = _providerResolver.Resolve();

        var result =
            await provider.TransferCallAsync(
                new TransferTelephonyCallRequest(
                    companyId,
                    callId,
                    request.ProviderCallId,
                    request.Target),
                cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    [Authorize(
        Roles =
            "SuperAdmin,CompanyAdmin,Supervisor,Operator")]
    [HttpPost("calls/{callId:guid}/hangup")]
    public async Task<ActionResult<TelephonyCallResult>>
        HangupCall(
            Guid companyId,
            Guid callId,
            [FromBody]
            HangupTelephonyCallApiRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var provider = _providerResolver.Resolve();

        var result =
            await provider.HangupCallAsync(
                companyId,
                callId,
                request.ProviderCallId,
                cancellationToken);

        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    private static Uri? CreateOptionalUri(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Uri.TryCreate(
            value.Trim(),
            UriKind.Absolute,
            out var uri)
                ? uri
                : throw new ArgumentException(
                    $"Geçersiz URL: {value}");
    }
}

public sealed class StartOutboundTelephonyCallApiRequest
{
    public Guid CallId { get; set; }

    public string ToNumber { get; set; }
        = string.Empty;

    public string? FromNumber { get; set; }

    public string? AnswerUrl { get; set; }

    public string? StatusCallbackUrl { get; set; }
}

public sealed class TransferTelephonyCallApiRequest
{
    public string ProviderCallId { get; set; }
        = string.Empty;

    public string Target { get; set; }
        = string.Empty;
}

public sealed class HangupTelephonyCallApiRequest
{
    public string ProviderCallId { get; set; }
        = string.Empty;
}
