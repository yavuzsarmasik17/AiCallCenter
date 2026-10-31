namespace AiCallCenter.Application.Common.AI.Transfer;

public sealed record AiTransferDecisionRequest
{
    public string CustomerMessage { get; init; } =
        string.Empty;

    public bool TransferToHumanEnabled { get; init; }

    public IReadOnlyList<string> KnowledgeResults
    {
        get;
        init;
    } = Array.Empty<string>();
}