using AiCallCenter.Application.Common.Telephony;

namespace AiCallCenter.Infrastructure.Telephony.Mock;

internal sealed class MockTelephonyCallState
{
    public required Guid CompanyId { get; init; }

    public required Guid CallId { get; init; }

    public required string ProviderCallId { get; init; }

    public required string FromNumber { get; init; }

    public required string ToNumber { get; init; }

    public TelephonyCallStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; set; }
}
