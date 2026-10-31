namespace AiCallCenter.Application.Common.AI.Transfer;

public interface IAiTransferDecisionService
{
    Task<AiTransferDecision> DecideAsync(
        AiTransferDecisionRequest request,
        CancellationToken cancellationToken = default);
}