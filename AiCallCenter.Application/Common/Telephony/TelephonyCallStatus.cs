namespace AiCallCenter.Application.Common.Telephony;

public enum TelephonyCallStatus
{
    Unknown = 0,
    Queued = 1,
    Ringing = 2,
    InProgress = 3,
    Completed = 4,
    Busy = 5,
    NoAnswer = 6,
    Cancelled = 7,
    Failed = 8
}
