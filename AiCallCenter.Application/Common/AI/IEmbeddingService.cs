namespace AiCallCenter.Application.Common.AI;

public interface IEmbeddingService
{
    string ModelName { get; }

    Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);
}