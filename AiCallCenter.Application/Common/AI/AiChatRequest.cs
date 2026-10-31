using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.AI;

public sealed class AiChatRequest
{
    public AiProvider Provider { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string SystemPrompt { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;
}