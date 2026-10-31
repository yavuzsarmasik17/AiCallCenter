namespace AiCallCenter.Application.Common.AI.Handoff;

public interface IAiHandoffSummaryService
{
    Task<AiHandoffSummaryResult> CreateAsync(
        AiHandoffSummaryRequest request,
        CancellationToken cancellationToken = default);
}