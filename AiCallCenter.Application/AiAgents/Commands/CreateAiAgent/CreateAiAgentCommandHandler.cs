using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Commands.CreateAiAgent;

public sealed class CreateAiAgentCommandHandler
    : IRequestHandler<CreateAiAgentCommand, AiAgentDto>
{
    private readonly IAiAgentRepository _aiAgentRepository;
    private readonly IAiModelCatalog _aiModelCatalog;

    public CreateAiAgentCommandHandler(
     IAiAgentRepository aiAgentRepository,
     IAiModelCatalog aiModelCatalog)
    {
        _aiAgentRepository = aiAgentRepository;
        _aiModelCatalog = aiModelCatalog;
    }

    public async Task<AiAgentDto> Handle(
        CreateAiAgentCommand request,
        CancellationToken cancellationToken)
    {
        var companyExists =
            await _aiAgentRepository.CompanyExistsAsync(
                request.CompanyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Belirtilen firma bulunamadı.");
        }

        var nameExists =
            await _aiAgentRepository.NameExistsAsync(
                request.CompanyId,
                request.Name.Trim(),
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Bu firmada aynı isimde bir yapay zekâ ajanı bulunuyor.");
        }

        _aiModelCatalog.GetRequired(
        request.Provider,
        request.ModelName);

        var aiAgent = new AiAgent(
            request.CompanyId,
            request.Name,
            request.Instructions,
            request.WelcomeMessage,
            request.Language,
            request.Provider,
            request.ModelName);

        await _aiAgentRepository.AddAsync(
            aiAgent,
            cancellationToken);

        await _aiAgentRepository.SaveChangesAsync(
            cancellationToken);

        return Map(aiAgent);
    }

    private static AiAgentDto Map(AiAgent aiAgent)
    {
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