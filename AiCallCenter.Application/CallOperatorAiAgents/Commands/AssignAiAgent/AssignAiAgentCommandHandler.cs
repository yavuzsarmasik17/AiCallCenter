using AiCallCenter.Application.CallOperatorAiAgents.Dtos;
using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.CallOperatorAiAgents.Commands
    .AssignAiAgent;

public sealed class AssignAiAgentCommandHandler
    : IRequestHandler<
        AssignAiAgentCommand,
        CallOperatorAiAgentDto>
{
    private readonly ICallOperatorAiAgentRepository _repository;

    public AssignAiAgentCommandHandler(
        ICallOperatorAiAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<CallOperatorAiAgentDto> Handle(
        AssignAiAgentCommand request,
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

        if (request.AiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "AI ajanı kimliği geçersiz.");
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

        var aiAgentExists =
            await _repository.AiAgentExistsAsync(
                request.CompanyId,
                request.AiAgentId,
                cancellationToken);

        if (!aiAgentExists)
        {
            throw new KeyNotFoundException(
                "Aktif AI ajanı bulunamadı.");
        }

        var assignmentExists =
            await _repository.AssignmentExistsAsync(
                request.CompanyId,
                request.CallOperatorId,
                request.AiAgentId,
                cancellationToken);

        if (assignmentExists)
        {
            throw new InvalidOperationException(
                "Bu AI ajanı operatöre zaten atanmış.");
        }

        if (request.IsPrimary)
        {
            var currentPrimary =
                await _repository.GetPrimaryAsync(
                    request.CompanyId,
                    request.CallOperatorId,
                    cancellationToken);

            currentPrimary?.RemovePrimary();
        }

        var assignment = new CallOperatorAiAgent(
            request.CompanyId,
            request.CallOperatorId,
            request.AiAgentId,
            request.IsPrimary);

        await _repository.AddAsync(
            assignment,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        var savedAssignment =
            await _repository.GetByIdAsync(
                request.CompanyId,
                request.CallOperatorId,
                assignment.Id,
                cancellationToken);

        if (savedAssignment is null)
        {
            throw new InvalidOperationException(
                "AI ajanı ataması kaydedildi ancak okunamadı.");
        }

        return new CallOperatorAiAgentDto(
            savedAssignment.Id,
            savedAssignment.CompanyId,
            savedAssignment.CallOperatorId,
            savedAssignment.CallOperator.DisplayName,
            savedAssignment.AiAgentId,
            savedAssignment.AiAgent.Name,
            savedAssignment.AiAgent.Provider.ToString(),
            savedAssignment.AiAgent.ModelName,
            savedAssignment.IsPrimary,
            savedAssignment.IsActive,
            savedAssignment.AssignedAt);
    }
}