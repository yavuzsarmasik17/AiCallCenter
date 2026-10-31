namespace AiCallCenter.Application.Common.Telephony;

public interface ITelephonyProvider
{
    string Name { get; }

    Task<TelephonyCallResult> StartOutboundCallAsync(
        StartOutboundCallRequest request,
        CancellationToken cancellationToken = default);

    Task<TelephonyCallResult> TransferCallAsync(
        TransferTelephonyCallRequest request,
        CancellationToken cancellationToken = default);

    Task<TelephonyCallResult> HangupCallAsync(
        Guid companyId,
        Guid callId,
        string providerCallId,
        CancellationToken cancellationToken = default);
}
