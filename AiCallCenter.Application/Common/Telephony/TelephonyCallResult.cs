namespace AiCallCenter.Application.Common.Telephony;

public sealed record TelephonyCallResult(
    bool IsSuccess,
    string Provider,
    string? ProviderCallId,
    TelephonyCallStatus Status,
    string? ErrorCode = null,
    string? ErrorMessage = null)
{
    public static TelephonyCallResult Success(
        string provider,
        string providerCallId,
        TelephonyCallStatus status)
    {
        return new TelephonyCallResult(
            true,
            provider,
            providerCallId,
            status);
    }

    public static TelephonyCallResult Failure(
        string provider,
        string errorCode,
        string errorMessage)
    {
        return new TelephonyCallResult(
            false,
            provider,
            null,
            TelephonyCallStatus.Failed,
            errorCode,
            errorMessage);
    }
}
