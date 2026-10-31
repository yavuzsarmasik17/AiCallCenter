namespace AiCallCenter.Application.Common.AI;

public interface IAiChatService
{
    Task<string> AskAsync(
        AiChatRequest request,
        CancellationToken cancellationToken = default);
}