using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetAvailableAiAgents;

public sealed class GetAvailableAiAgentsQueryHandler
    : IRequestHandler<
        GetAvailableAiAgentsQuery,
        IReadOnlyList<AvailableAiAgentDto>>
{
    private readonly ICallOperatorAiAgentRepository _repository;

    public GetAvailableAiAgentsQueryHandler(
        ICallOperatorAiAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AvailableAiAgentDto>> Handle(
        GetAvailableAiAgentsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.");
        }

        var operatorExists =
            await _repository.CallOperatorExistsAsync(
                request.CompanyId,
                request.CallOperatorId,
                cancellationToken);

        if (!operatorExists)
        {
            throw new KeyNotFoundException(
                "Çağrı operatörü bulunamadı.");
        }

        var aiAgents =
            await _repository.GetAvailableAiAgentsAsync(
                request.CompanyId,
                cancellationToken);

        var assignments =
            await _repository.GetByOperatorAsync(
                request.CompanyId,
                request.CallOperatorId,
                cancellationToken);

        return aiAgents
            .Select(
                aiAgent =>
                {
                    var assignment = assignments
                        .FirstOrDefault(
                            x => x.AiAgentId == aiAgent.Id);

                    return new AvailableAiAgentDto(
                        aiAgent.Id,
                        aiAgent.Name,
                        aiAgent.Provider.ToString(),
                        aiAgent.ModelName,
                        aiAgent.IsActive,
                        assignment is not null,
                        assignment?.IsPrimary ?? false);
                })
            .ToList();
    }
}