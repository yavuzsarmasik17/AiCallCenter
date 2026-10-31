namespace AiCallCenter.Application.AiAgents.Commands.ChatWithAiAgent;

public sealed class ChatWithAiAgentResponse
{
    public Guid ConversationId { get; set; }

    public Guid AiAgentId { get; set; }

    public string AiAgentName { get; set; } = string.Empty;

    public string UserMessage { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;
}