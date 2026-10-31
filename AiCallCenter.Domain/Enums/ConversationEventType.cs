namespace AiCallCenter.Domain.Enums;

public enum ConversationEventType
{
    Created = 1,
    IncomingCallReceived = 2,
    CustomerMessageReceived = 3,
    AiStarted = 4,
    AiResponded = 5,
    AddedToQueue = 6,
    AssignedToOperator = 7,
    OperatorJoined = 8,
    OperatorLeft = 9,
    TransferredToOperator = 10,
    TransferredToAi = 11,
    CallAnswered = 12,
    CallEnded = 13,
    RecordingStarted = 14,
    RecordingCompleted = 15,
    NoteAdded = 16,
    Completed = 17,
    Cancelled = 18,
    Failed = 19
}