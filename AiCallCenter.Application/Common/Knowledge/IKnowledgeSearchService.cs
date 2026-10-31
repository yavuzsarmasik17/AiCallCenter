namespace AiCallCenter.Application.Common.Knowledge;

public interface IKnowledgeSearchService
{
    Task<IReadOnlyList<string>> SearchAsync(
        Guid companyId,
        Guid aiAgentId,
        string query,
        int maximumResultCount = 5,
        CancellationToken cancellationToken = default);
}