namespace AiCallCenter.Application.Common.Telephony;

public sealed record StartOutboundCallRequest(
    Guid CompanyId,
    Guid CallId,
    string ToNumber,
    string? FromNumber = null,
    Uri? AnswerUrl = null,
    Uri? StatusCallbackUrl = null);
