using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.AiAgents.DTOs;

public sealed class AiAgentDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public string WelcomeMessage { get; set; } = string.Empty;

    public AgentLanguage Language { get; set; }

    public string VoiceName { get; set; } = string.Empty;

    public int MaxCallDurationMinutes { get; set; }

    public bool TransferToHumanEnabled { get; set; }

    public Guid? TransferCallQueueId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public AiProvider Provider { get; set; }

    public string ModelName { get; set; } = string.Empty;
}