using AiCallCenter.Application.AiAgents.DTOs;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgentSettings;

public sealed record UpdateAiAgentSettingsCommand(
    Guid CompanyId,
    Guid AiAgentId,
    string VoiceName,
    int MaxCallDurationMinutes,
    bool TransferToHumanEnabled,
    Guid? TransferCallQueueId,
    bool IsActive)
    : IRequest<AiAgentDto>;