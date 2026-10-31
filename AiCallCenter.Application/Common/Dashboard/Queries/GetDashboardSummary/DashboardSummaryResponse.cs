namespace AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;

public sealed class DashboardSummaryResponse
{
    public DashboardAgentSummary Agents { get; set; } = new();

    public DashboardConversationSummary Conversations { get; set; } = new();

    public DashboardKnowledgeSummary Knowledge { get; set; } = new();

    public IReadOnlyList<DashboardDailyConversationItem>
        LastSevenDaysConversations
    { get; set; }
        = Array.Empty<DashboardDailyConversationItem>();

    public IReadOnlyList<DashboardTopAgentItem>
        TopAgents
    { get; set; }
        = Array.Empty<DashboardTopAgentItem>();

    public IReadOnlyList<DashboardRecentDocumentItem>
        RecentDocuments
    { get; set; }
        = Array.Empty<DashboardRecentDocumentItem>();
}

public sealed class DashboardAgentSummary
{
    public int TotalCount { get; set; }

    public int ActiveCount { get; set; }

    public int PassiveCount { get; set; }
}

public sealed class DashboardConversationSummary
{
    public int TotalCount { get; set; }

    public int TodayCount { get; set; }

    public int ActiveCount { get; set; }

    public int CompletedCount { get; set; }

    public int CancelledCount { get; set; }
}

public sealed class DashboardKnowledgeSummary
{
    public int KnowledgeBaseCount { get; set; }

    public int ActiveKnowledgeBaseCount { get; set; }

    public int DocumentCount { get; set; }

    public int ReadyDocumentCount { get; set; }

    public int ProcessingDocumentCount { get; set; }

    public int FailedDocumentCount { get; set; }

    public int ChunkCount { get; set; }

    public int EmbeddedChunkCount { get; set; }
}

public sealed class DashboardDailyConversationItem
{
    public DateTime Date { get; set; }

    public string DayName { get; set; } = string.Empty;

    public int Count { get; set; }
}

public sealed class DashboardTopAgentItem
{
    public Guid AiAgentId { get; set; }

    public string AiAgentName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public int ConversationCount { get; set; }
}

public sealed class DashboardRecentDocumentItem
{
    public Guid DocumentId { get; set; }

    public Guid KnowledgeBaseId { get; set; }

    public string KnowledgeBaseName { get; set; } = string.Empty;

    public string DocumentName { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int ChunkCount { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}