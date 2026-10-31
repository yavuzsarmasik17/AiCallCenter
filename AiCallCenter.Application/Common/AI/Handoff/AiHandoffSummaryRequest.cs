namespace AiCallCenter.Application.Common.AI.Handoff;

public sealed record AiHandoffSummaryRequest
{
    public string CustomerMessage { get; init; } = string.Empty;

    public string TransferReason { get; init; } = string.Empty;

    public IReadOnlyList<string> ConversationHistory { get; init; }
        = Array.Empty<string>();

    public IReadOnlyList<string> KnowledgeResults { get; init; }
        = Array.Empty<string>();
}