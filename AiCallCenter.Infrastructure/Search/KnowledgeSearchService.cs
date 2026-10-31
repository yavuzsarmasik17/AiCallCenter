using System.Text.Json;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.Common.Knowledge;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Search;

public sealed class KnowledgeSearchService
    : IKnowledgeSearchService
{
    private const double MinimumSimilarityScore = 0.25;
    private const int MaximumCandidateCount = 1000;

    private readonly ApplicationDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeSearchService(
        ApplicationDbContext dbContext,
        IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<string>> SearchAsync(
        Guid companyId,
        Guid aiAgentId,
        string query,
        int maximumResultCount = 5,
        CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            return Array.Empty<string>();
        }

        if (aiAgentId == Guid.Empty)
        {
            return Array.Empty<string>();
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<string>();
        }

        if (maximumResultCount <= 0)
        {
            return Array.Empty<string>();
        }

        var normalizedQuery = query.Trim();

        var candidates = await _dbContext.KnowledgeChunks
            .AsNoTracking()
            .Where(chunk =>
                chunk.KnowledgeDocument.Status ==
                    DocumentStatus.Ready &&

                chunk.KnowledgeDocument
                    .KnowledgeBase.IsActive &&

                chunk.KnowledgeDocument
                    .KnowledgeBase.CompanyId ==
                    companyId &&

                chunk.KnowledgeDocument
                    .KnowledgeBase.AiAgents.Any(connection =>
                        connection.AiAgentId == aiAgentId))
            .Select(chunk => new KnowledgeCandidate
            {
                Content = chunk.Content,
                EmbeddingJson = chunk.EmbeddingJson
            })
            .Take(MaximumCandidateCount)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return Array.Empty<string>();
        }

        float[]? queryEmbedding;

        try
        {
            queryEmbedding =
                await _embeddingService.CreateEmbeddingAsync(
                    normalizedQuery,
                    cancellationToken);
        }
        catch
        {
            return SearchByKeywords(
                candidates,
                normalizedQuery,
                maximumResultCount);
        }

        if (queryEmbedding is null ||
            queryEmbedding.Length == 0)
        {
            return SearchByKeywords(
                candidates,
                normalizedQuery,
                maximumResultCount);
        }

        var vectorResults = candidates
            .Where(candidate =>
                !string.IsNullOrWhiteSpace(
                    candidate.Content))
            .Where(candidate =>
                !string.IsNullOrWhiteSpace(
                    candidate.EmbeddingJson))
            .Select(candidate =>
            {
                var embedding =
                    TryDeserializeEmbedding(
                        candidate.EmbeddingJson);

                var score = embedding is null
                    ? 0
                    : CalculateCosineSimilarity(
                        queryEmbedding,
                        embedding);

                return new SearchResult
                {
                    Content = candidate.Content.Trim(),
                    Score = score
                };
            })
            .Where(result =>
                result.Score >= MinimumSimilarityScore)
            .GroupBy(
                result => result.Content,
                StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                group.OrderByDescending(result =>
                        result.Score)
                    .First())
            .OrderByDescending(result =>
                result.Score)
            .Take(maximumResultCount)
            .ToList();

        if (vectorResults.Count > 0)
        {
            return vectorResults
                .Select(result => result.Content)
                .ToList();
        }

        return SearchByKeywords(
            candidates,
            normalizedQuery,
            maximumResultCount);
    }

    private static float[]? TryDeserializeEmbedding(
        string? embeddingJson)
    {
        if (string.IsNullOrWhiteSpace(embeddingJson))
        {
            return null;
        }

        try
        {
            var embedding =
                JsonSerializer.Deserialize<float[]>(
                    embeddingJson);

            if (embedding is null ||
                embedding.Length == 0)
            {
                return null;
            }

            return embedding;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static double CalculateCosineSimilarity(
        IReadOnlyList<float> firstVector,
        IReadOnlyList<float> secondVector)
    {
        if (firstVector.Count == 0)
        {
            return 0;
        }

        if (secondVector.Count == 0)
        {
            return 0;
        }

        if (firstVector.Count != secondVector.Count)
        {
            return 0;
        }

        double dotProduct = 0;
        double firstMagnitude = 0;
        double secondMagnitude = 0;

        for (var index = 0;
             index < firstVector.Count;
             index++)
        {
            var firstValue =
                firstVector[index];

            var secondValue =
                secondVector[index];

            dotProduct +=
                firstValue * secondValue;

            firstMagnitude +=
                firstValue * firstValue;

            secondMagnitude +=
                secondValue * secondValue;
        }

        if (firstMagnitude <= 0)
        {
            return 0;
        }

        if (secondMagnitude <= 0)
        {
            return 0;
        }

        return dotProduct /
               (Math.Sqrt(firstMagnitude) *
                Math.Sqrt(secondMagnitude));
    }

    private static IReadOnlyList<string> SearchByKeywords(
        IReadOnlyList<KnowledgeCandidate> candidates,
        string query,
        int maximumResultCount)
    {
        var words = query
            .ToLowerInvariant()
            .Split(
                new[]
                {
                    ' ',
                    ',',
                    '.',
                    '?',
                    '!',
                    ';',
                    ':',
                    '\r',
                    '\n',
                    '\t',
                    '-',
                    '_',
                    '/',
                    '\\',
                    '(',
                    ')'
                },
                StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length >= 3)
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();

        if (words.Count == 0)
        {
            return Array.Empty<string>();
        }

        return candidates
            .Where(candidate =>
                !string.IsNullOrWhiteSpace(
                    candidate.Content))
            .Select(candidate => new SearchResult
            {
                Content = candidate.Content.Trim(),

                Score = words.Count(word =>
                    candidate.Content.Contains(
                        word,
                        StringComparison.OrdinalIgnoreCase))
            })
            .Where(result =>
                result.Score > 0)
            .GroupBy(
                result => result.Content,
                StringComparer.OrdinalIgnoreCase)
            .Select(group =>
                group.OrderByDescending(result =>
                        result.Score)
                    .First())
            .OrderByDescending(result =>
                result.Score)
            .Take(maximumResultCount)
            .Select(result => result.Content)
            .ToList();
    }

    private sealed class KnowledgeCandidate
    {
        public string Content { get; set; } =
            string.Empty;

        public string? EmbeddingJson { get; set; }
    }

    private sealed class SearchResult
    {
        public string Content { get; set; } =
            string.Empty;

        public double Score { get; set; }
    }
}