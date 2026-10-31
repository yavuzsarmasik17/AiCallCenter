namespace AiCallCenter.Application.Common.AI.Transfer;

public sealed record AiTransferDecision(
    bool ShouldTransfer,
    string? Reason,
    double Confidence)
{
    public static AiTransferDecision Continue(
        double confidence = 1.0)
    {
        return new AiTransferDecision(
            false,
            null,
            confidence);
    }

    public static AiTransferDecision Transfer(
        string reason,
        double confidence = 1.0)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Aktarım nedeni boş olamaz.",
                nameof(reason));
        }

        return new AiTransferDecision(
            true,
            reason.Trim(),
            confidence);
    }
}