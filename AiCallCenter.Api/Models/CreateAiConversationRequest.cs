namespace AiCallCenter.Api.Models;

public sealed record CreateAiConversationRequest(
    Guid AiAgentId,
    string? CustomerName,
    string? CustomerPhone);