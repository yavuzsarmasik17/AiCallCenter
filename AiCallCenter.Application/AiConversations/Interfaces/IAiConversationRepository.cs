using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.AiConversations.Interfaces;

public interface IAiConversationRepository
{
    Task<AiConversation?> GetActiveByIdAsync(
        Guid companyId,
        Guid aiAgentId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<AiConversation?> GetByIdWithMessagesAsync(
        Guid companyId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiConversation>> GetByCompanyIdAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiConversationMessage>> GetMessagesAsync(
        Guid conversationId,
        int maximumMessageCount = 20,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSequenceNumberAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task AddConversationAsync(
        AiConversation conversation,
        CancellationToken cancellationToken = default);

    Task AddMessageAsync(
        AiConversationMessage message,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}