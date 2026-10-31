namespace AiCallCenter.Domain.Enums;

public enum CallEventType
{
    Created = 1,
    Ringing = 2,
    Answered = 3,
    AiStarted = 4,
    AiResponded = 5,
    AddedToQueue = 6,
    AssignedToOperator = 7,
    OperatorAccepted = 8,
    TransferredToHuman = 9,
    OperatorJoined = 10,
    OperatorLeft = 11,
    RecordingStarted = 12,
    RecordingCompleted = 13,
    Completed = 14,
    Cancelled = 15,
    Failed = 16,
    NoteAdded = 17
}