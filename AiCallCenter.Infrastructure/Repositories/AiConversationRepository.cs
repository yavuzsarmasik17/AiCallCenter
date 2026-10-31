using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Domain.Entities;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Repositories;

public sealed class AiConversationRepository
    : IAiConversationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AiConversationRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AiConversation?> GetActiveByIdAsync(
        Guid companyId,
        Guid aiAgentId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.AiConversations
            .FirstOrDefaultAsync(
                conversation =>
                    conversation.Id == conversationId &&
                    conversation.CompanyId == companyId &&
                    conversation.AiAgentId == aiAgentId &&
                    conversation.Status == ConversationStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AiConversation>>
        GetByCompanyIdAsync(
            Guid companyId,
            CancellationToken cancellationToken = default)
    {
        return await _dbContext.AiConversations
            .AsNoTracking()
            .Include(x => x.AiAgent)
            .Include(x => x.Messages)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<AiConversation?> GetByIdWithMessagesAsync(
    Guid companyId,
    Guid conversationId,
    CancellationToken cancellationToken = default)
    {
        return _dbContext.AiConversations
            .Include(x => x.AiAgent)
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == conversationId &&
                    x.CompanyId == companyId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AiConversationMessage>>
        GetMessagesAsync(
            Guid conversationId,
            int maximumMessageCount = 20,
            CancellationToken cancellationToken = default)
    {
        if (maximumMessageCount <= 0)
        {
            return Array.Empty<AiConversationMessage>();
        }

        var messages = await _dbContext.AiConversationMessages
            .AsNoTracking()
            .Where(message =>
                message.ConversationId == conversationId)
            .OrderByDescending(message =>
                message.SequenceNumber)
            .Take(maximumMessageCount)
            .ToListAsync(cancellationToken);

        return messages
            .OrderBy(message => message.SequenceNumber)
            .ToList();
    }

    public async Task<int> GetNextSequenceNumberAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var lastSequenceNumber =
            await _dbContext.AiConversationMessages
                .Where(message =>
                    message.ConversationId == conversationId)
                .MaxAsync(
                    message => (int?)message.SequenceNumber,
                    cancellationToken);

        return (lastSequenceNumber ?? 0) + 1;
    }

    public async Task AddConversationAsync(
        AiConversation conversation,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AiConversations.AddAsync(
            conversation,
            cancellationToken);
    }

    public async Task AddMessageAsync(
        AiConversationMessage message,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AiConversationMessages.AddAsync(
            message,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}