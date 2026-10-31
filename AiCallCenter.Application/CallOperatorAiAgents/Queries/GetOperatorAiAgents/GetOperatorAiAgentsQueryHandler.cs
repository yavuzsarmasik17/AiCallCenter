using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Queries
    .GetOperatorAiAgents;

public sealed class GetOperatorAiAgentsQueryHandler
    : IRequestHandler<
        GetOperatorAiAgentsQuery,
        IReadOnlyList<CallOperatorAiAgentDto>>
{
    private readonly ICallOperatorAiAgentRepository _repository;

    public GetOperatorAiAgentsQueryHandler(
        ICallOperatorAiAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CallOperatorAiAgentDto>> Handle(
        GetOperatorAiAgentsQuery request,
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

        var assignments =
            await _repository.GetByOperatorAsync(
                request.CompanyId,
                request.CallOperatorId,
                cancellationToken);

        return assignments
            .Select(
                x => new CallOperatorAiAgentDto(
                    x.Id,
                    x.CompanyId,
                    x.CallOperatorId,
                    x.CallOperator.DisplayName,
                    x.AiAgentId,
                    x.AiAgent.Name,
                    x.AiAgent.Provider.ToString(),
                    x.AiAgent.ModelName,
                    x.IsPrimary,
                    x.IsActive,
                    x.AssignedAt))
            .ToList();
    }
}