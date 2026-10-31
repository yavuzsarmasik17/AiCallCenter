using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgentSettings;

public sealed class UpdateAiAgentSettingsCommandHandler
    : IRequestHandler<
        UpdateAiAgentSettingsCommand,
        AiAgentDto>
{
    private readonly IAiAgentRepository _aiAgentRepository;

    public UpdateAiAgentSettingsCommandHandler(
        IAiAgentRepository aiAgentRepository)
    {
        _aiAgentRepository = aiAgentRepository;
    }

    public async Task<AiAgentDto> Handle(
        UpdateAiAgentSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var aiAgent =
            await _aiAgentRepository.GetByIdAsync(
                request.CompanyId,
                request.AiAgentId,
                cancellationToken);

        if (aiAgent is null)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        aiAgent.ChangeVoice(
            request.VoiceName);

        aiAgent.ChangeMaxCallDuration(
            request.MaxCallDurationMinutes);

        if (request.TransferToHumanEnabled)
        {
            aiAgent.EnableHumanTransfer();

            aiAgent.SetTransferCallQueue(
                request.TransferCallQueueId);
        }
        else
        {
            aiAgent.DisableHumanTransfer();

            aiAgent.ClearTransferCallQueue();
        }

        if (request.IsActive)
        {
            aiAgent.Activate();
        }
        else
        {
            aiAgent.Deactivate();
        }

        await _aiAgentRepository.SaveChangesAsync(
            cancellationToken);

        return new AiAgentDto
        {
            Id = aiAgent.Id,
            CompanyId = aiAgent.CompanyId,
            Name = aiAgent.Name,
            Instructions = aiAgent.Instructions,
            WelcomeMessage = aiAgent.WelcomeMessage,
            Language = aiAgent.Language,

            Provider = aiAgent.Provider,
            ModelName = aiAgent.ModelName,

            VoiceName = aiAgent.VoiceName,

            MaxCallDurationMinutes =
                aiAgent.MaxCallDurationMinutes,

            TransferToHumanEnabled =
                aiAgent.TransferToHumanEnabled,

            TransferCallQueueId =
                aiAgent.TransferCallQueueId,

            IsActive = aiAgent.IsActive,
            CreatedAt = aiAgent.CreatedAt
        };
    }
}