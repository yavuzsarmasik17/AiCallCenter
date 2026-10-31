namespace AiCallCenter.Application.Common.AI;

public interface IOpenAiService
{
    Task<string> AskAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken cancellationToken = default);
}