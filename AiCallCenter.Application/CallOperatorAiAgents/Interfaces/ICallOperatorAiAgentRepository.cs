using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.CallOperatorAiAgents.Interfaces;

public interface ICallOperatorAiAgentRepository
{
    Task<bool> CallOperatorExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default);

    Task<bool> AiAgentExistsAsync(
        Guid companyId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task<bool> AssignmentExistsAsync(
        Guid companyId,
        Guid callOperatorId,
        Guid aiAgentId,
        CancellationToken cancellationToken = default);

    Task<CallOperatorAiAgent?> GetByIdAsync(
        Guid companyId,
        Guid callOperatorId,
        Guid assignmentId,
        CancellationToken cancellationToken = default);

    Task<CallOperatorAiAgent?> GetPrimaryAsync(
        Guid companyId,
        Guid callOperatorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CallOperatorAiAgent>>
        GetByOperatorAsync(
            Guid companyId,
            Guid callOperatorId,
            CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiAgent>>
        GetAvailableAiAgentsAsync(
            Guid companyId,
            CancellationToken cancellationToken = default);

    Task AddAsync(
        CallOperatorAiAgent assignment,
        CancellationToken cancellationToken = default);

    void Remove(CallOperatorAiAgent assignment);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}