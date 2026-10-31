namespace AiCallCenter.Application.Common.Telephony;

public sealed record TransferTelephonyCallRequest(
    Guid CompanyId,
    Guid CallId,
    string ProviderCallId,
    string Target);
