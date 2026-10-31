using AiCallCenter.Application.AiAgents.DTOs;
using AiCallCenter.Application.AiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.AiAgents.Queries.GetAiAgents;

public sealed class GetAiAgentsQueryHandler
    : IRequestHandler<
        GetAiAgentsQuery,
        IReadOnlyList<AiAgentDto>>
{
    private readonly IAiAgentRepository _aiAgentRepository;

    public GetAiAgentsQueryHandler(
        IAiAgentRepository aiAgentRepository)
    {
        _aiAgentRepository = aiAgentRepository;
    }

    public async Task<IReadOnlyList<AiAgentDto>> Handle(
        GetAiAgentsQuery request,
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

        var agents =
            await _aiAgentRepository.GetByCompanyIdAsync(
                request.CompanyId,
                cancellationToken);

        return agents
         .Select(agent => new AiAgentDto
     {
         Id = agent.Id,
         CompanyId = agent.CompanyId,
         Name = agent.Name,
         Instructions = agent.Instructions,
         WelcomeMessage = agent.WelcomeMessage,
         Language = agent.Language,
         Provider = agent.Provider,
         ModelName = agent.ModelName,
         VoiceName = agent.VoiceName,
         MaxCallDurationMinutes = agent.MaxCallDurationMinutes,
         TransferToHumanEnabled = agent.TransferToHumanEnabled,
         IsActive = agent.IsActive,
         CreatedAt = agent.CreatedAt
     })
     .ToList();
    }
}