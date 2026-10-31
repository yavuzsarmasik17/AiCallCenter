namespace AiCallCenter.Application.Common.Telephony;

public sealed record TelephonyWebhookEvent(
    string Provider,
    string ProviderCallId,
    TelephonyCallStatus Status,
    string? FromNumber,
    string? ToNumber,
    DateTime OccurredAtUtc,
    string? RawPayload = null);
