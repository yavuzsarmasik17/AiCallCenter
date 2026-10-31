using AiCallCenter.Application.Common.Dashboard;
using AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;
using AiCallCenter.Domain.Enums;
using AiCallCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiCallCenter.Infrastructure.Dashboard;

public sealed class DashboardReadService
    : IDashboardReadService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardReadService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        var companyExists = await _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(
                company => company.Id == companyId,
                cancellationToken);

        if (!companyExists)
        {
            throw new KeyNotFoundException(
                "Firma bulunamadı.");
        }

        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);
        var sevenDaysAgoUtc = todayUtc.AddDays(-6);

        var agentSummary = await GetAgentSummaryAsync(
            companyId,
            cancellationToken);

        var conversationSummary =
            await GetConversationSummaryAsync(
                companyId,
                todayUtc,
                tomorrowUtc,
                cancellationToken);

        var knowledgeSummary =
            await GetKnowledgeSummaryAsync(
                companyId,
                cancellationToken);

        var lastSevenDaysConversations =
            await GetLastSevenDaysConversationsAsync(
                companyId,
                sevenDaysAgoUtc,
                todayUtc,
                cancellationToken);

        var topAgents = await GetTopAgentsAsync(
            companyId,
            cancellationToken);

        var recentDocuments =
            await GetRecentDocumentsAsync(
                companyId,
                cancellationToken);

        return new DashboardSummaryResponse
        {
            Agents = agentSummary,
            Conversations = conversationSummary,
            Knowledge = knowledgeSummary,
            LastSevenDaysConversations =
                lastSevenDaysConversations,
            TopAgents = topAgents,
            RecentDocuments = recentDocuments
        };
    }

    private async Task<DashboardAgentSummary>
        GetAgentSummaryAsync(
            Guid companyId,
            CancellationToken cancellationToken)
    {
        var totalCount = await _dbContext.AiAgents
            .AsNoTracking()
            .CountAsync(
                agent => agent.CompanyId == companyId,
                cancellationToken);

        var activeCount = await _dbContext.AiAgents
            .AsNoTracking()
            .CountAsync(
                agent =>
                    agent.CompanyId == companyId &&
                    agent.IsActive,
                cancellationToken);

        return new DashboardAgentSummary
        {
            TotalCount = totalCount,
            ActiveCount = activeCount,
            PassiveCount = totalCount - activeCount
        };
    }

    private async Task<DashboardConversationSummary>
        GetConversationSummaryAsync(
            Guid companyId,
            DateTime todayUtc,
            DateTime tomorrowUtc,
            CancellationToken cancellationToken)
    {
        var totalCount =
            await _dbContext.AiConversations
                .AsNoTracking()
                .CountAsync(
                    conversation =>
                        conversation.CompanyId == companyId,
                    cancellationToken);

        var todayCount =
            await _dbContext.AiConversations
                .AsNoTracking()
                .CountAsync(
                    conversation =>
                        conversation.CompanyId == companyId &&
                        conversation.StartedAt >= todayUtc &&
                        conversation.StartedAt < tomorrowUtc,
                    cancellationToken);

        var activeCount =
            await _dbContext.AiConversations
                .AsNoTracking()
                .CountAsync(
                    conversation =>
                        conversation.CompanyId == companyId &&
                        conversation.Status ==
                        ConversationStatus.Active,
                    cancellationToken);

        var completedCount =
            await _dbContext.AiConversations
                .AsNoTracking()
                .CountAsync(
                    conversation =>
                        conversation.CompanyId == companyId &&
                        conversation.Status ==
                        ConversationStatus.Completed,
                    cancellationToken);

        var cancelledCount =
            await _dbContext.AiConversations
                .AsNoTracking()
                .CountAsync(
                    conversation =>
                        conversation.CompanyId == companyId &&
                        conversation.Status ==
                        ConversationStatus.Cancelled,
                    cancellationToken);

        return new DashboardConversationSummary
        {
            TotalCount = totalCount,
            TodayCount = todayCount,
            ActiveCount = activeCount,
            CompletedCount = completedCount,
            CancelledCount = cancelledCount
        };
    }

    private async Task<DashboardKnowledgeSummary>
        GetKnowledgeSummaryAsync(
            Guid companyId,
            CancellationToken cancellationToken)
    {
        var knowledgeBaseCount =
            await _dbContext.KnowledgeBases
                .AsNoTracking()
                .CountAsync(
                    knowledgeBase =>
                        knowledgeBase.CompanyId == companyId,
                    cancellationToken);

        var activeKnowledgeBaseCount =
            await _dbContext.KnowledgeBases
                .AsNoTracking()
                .CountAsync(
                    knowledgeBase =>
                        knowledgeBase.CompanyId == companyId &&
                        knowledgeBase.IsActive,
                    cancellationToken);

        var documentQuery =
            _dbContext.KnowledgeDocuments
                .AsNoTracking()
                .Where(document =>
                    document.KnowledgeBase.CompanyId ==
                    companyId);

        var documentCount =
            await documentQuery.CountAsync(
                cancellationToken);

        var readyDocumentCount =
            await documentQuery.CountAsync(
                document =>
                    document.Status ==
                    DocumentStatus.Ready,
                cancellationToken);

        var processingDocumentCount =
            await documentQuery.CountAsync(
                document =>
                    document.Status ==
                    DocumentStatus.Processing,
                cancellationToken);

        var failedDocumentCount =
            await documentQuery.CountAsync(
                document =>
                    document.Status ==
                    DocumentStatus.Failed,
                cancellationToken);

        var chunkQuery =
            _dbContext.KnowledgeChunks
                .AsNoTracking()
                .Where(chunk =>
                    chunk.KnowledgeDocument
                        .KnowledgeBase.CompanyId ==
                    companyId);

        var chunkCount = await chunkQuery.CountAsync(
            cancellationToken);

        var embeddedChunkCount =
            await chunkQuery.CountAsync(
                chunk =>
                    chunk.EmbeddingJson != null &&
                    chunk.EmbeddingJson != string.Empty,
                cancellationToken);

        return new DashboardKnowledgeSummary
        {
            KnowledgeBaseCount = knowledgeBaseCount,
            ActiveKnowledgeBaseCount =
                activeKnowledgeBaseCount,
            DocumentCount = documentCount,
            ReadyDocumentCount = readyDocumentCount,
            ProcessingDocumentCount =
                processingDocumentCount,
            FailedDocumentCount = failedDocumentCount,
            ChunkCount = chunkCount,
            EmbeddedChunkCount = embeddedChunkCount
        };
    }

    private async Task<IReadOnlyList<
        DashboardDailyConversationItem>>
        GetLastSevenDaysConversationsAsync(
            Guid companyId,
            DateTime sevenDaysAgoUtc,
            DateTime todayUtc,
            CancellationToken cancellationToken)
    {
        var tomorrowUtc = todayUtc.AddDays(1);

        var databaseItems =
            await _dbContext.AiConversations
                .AsNoTracking()
                .Where(conversation =>
                    conversation.CompanyId == companyId &&
                    conversation.StartedAt >=
                    sevenDaysAgoUtc &&
                    conversation.StartedAt <
                    tomorrowUtc)
                .GroupBy(conversation =>
                    conversation.StartedAt.Date)
                .Select(group =>
                    new
                    {
                        Date = group.Key,
                        Count = group.Count()
                    })
                .ToListAsync(cancellationToken);

        var countByDate = databaseItems.ToDictionary(
            item => item.Date,
            item => item.Count);

        var result =
            new List<DashboardDailyConversationItem>();

        for (var dayIndex = 0;
             dayIndex < 7;
             dayIndex++)
        {
            var date = sevenDaysAgoUtc.AddDays(dayIndex);

            countByDate.TryGetValue(
                date,
                out var count);

            result.Add(
                new DashboardDailyConversationItem
                {
                    Date = date,
                    DayName = GetTurkishDayName(
                        date.DayOfWeek),
                    Count = count
                });
        }

        return result;
    }

    private async Task<IReadOnlyList<
        DashboardTopAgentItem>>
        GetTopAgentsAsync(
            Guid companyId,
            CancellationToken cancellationToken)
    {
        return await _dbContext.AiAgents
            .AsNoTracking()
            .Where(agent =>
                agent.CompanyId == companyId)
            .Select(agent =>
                new DashboardTopAgentItem
                {
                    AiAgentId = agent.Id,
                    AiAgentName = agent.Name,
                    IsActive = agent.IsActive,
                    Provider =
                        agent.Provider.ToString(),
                    ModelName = agent.ModelName,
                    ConversationCount =
                        agent.Conversations.Count
                })
            .OrderByDescending(item =>
                item.ConversationCount)
            .ThenBy(item => item.AiAgentName)
            .Take(5)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<
        DashboardRecentDocumentItem>>
        GetRecentDocumentsAsync(
            Guid companyId,
            CancellationToken cancellationToken)
    {
        return await _dbContext.KnowledgeDocuments
            .AsNoTracking()
            .Where(document =>
                document.KnowledgeBase.CompanyId ==
                companyId)
            .OrderByDescending(document =>
                document.UploadedAt)
            .Take(5)
            .Select(document =>
                new DashboardRecentDocumentItem
                {
                    DocumentId = document.Id,
                    KnowledgeBaseId =
                        document.KnowledgeBaseId,
                    KnowledgeBaseName =
                        document.KnowledgeBase.Name,
                    DocumentName = document.Name,
                    DocumentType =
                        document.DocumentType.ToString(),
                    Status =
                        document.Status.ToString(),
                    FileSizeBytes =
                        document.FileSizeBytes,
                    ChunkCount =
                        document.Chunks.Count,
                    UploadedAt =
                        document.UploadedAt,
                    ProcessedAt =
                        document.ProcessedAt
                })
            .ToListAsync(cancellationToken);
    }

    private static string GetTurkishDayName(
        DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Pzt",
            DayOfWeek.Tuesday => "Sal",
            DayOfWeek.Wednesday => "Çar",
            DayOfWeek.Thursday => "Per",
            DayOfWeek.Friday => "Cum",
            DayOfWeek.Saturday => "Cmt",
            DayOfWeek.Sunday => "Paz",
            _ => string.Empty
        };
    }
}