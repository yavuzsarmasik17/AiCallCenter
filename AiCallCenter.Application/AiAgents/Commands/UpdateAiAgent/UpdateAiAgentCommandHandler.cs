using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Application.Common.AI;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.UpdateAiAgent;

public sealed class UpdateAiAgentCommandHandler
    : IRequestHandler<UpdateAiAgentCommand, AiAgentDto>
{
    private readonly IAiAgentRepository _aiAgentRepository;
    private readonly IAiModelCatalog _aiModelCatalog;

    public UpdateAiAgentCommandHandler(
     IAiAgentRepository aiAgentRepository,
     IAiModelCatalog aiModelCatalog)
    {
        _aiAgentRepository = aiAgentRepository;
        _aiModelCatalog = aiModelCatalog;
    }

    public async Task<AiAgentDto> Handle(
        UpdateAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        var aiAgent = await _aiAgentRepository.GetByIdAsync(
            request.CompanyId,
            request.AiAgentId,
            cancellationToken);

        if (aiAgent is null)
        {
            throw new KeyNotFoundException(
                "Yapay zekâ ajanı bulunamadı.");
        }

        var nameExists =
            await _aiAgentRepository.NameExistsForAnotherAgentAsync(
                request.CompanyId,
                request.AiAgentId,
                request.Name.Trim(),
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu firmada aynı isimde başka bir yapay zekâ ajanı bulunuyor.");
        }

        _aiModelCatalog.GetRequired(
            request.Provider,
            request.ModelName);

        aiAgent.Update(
            request.Name,
            request.Instructions,
            request.WelcomeMessage,
            request.Language);

        aiAgent.SetProvider(
            request.Provider,
            request.ModelName);

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
            IsActive = aiAgent.IsActive,
            CreatedAt = aiAgent.CreatedAt
        };
    }
}