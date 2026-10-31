using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.AI;

public interface IAiProviderClient
{
    AiProvider Provider { get; }

    Task<string> AskAsync(
        string modelName,
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default);
}