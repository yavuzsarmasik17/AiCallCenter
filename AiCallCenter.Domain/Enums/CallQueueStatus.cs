namespace AiCallCenter.Domain.Enums;

public enum CallQueueStatus
{
    Waiting = 1,
    Assigned = 2,
    Accepted = 3,
    Completed = 4,
    Cancelled = 5,
    Expired = 6
}