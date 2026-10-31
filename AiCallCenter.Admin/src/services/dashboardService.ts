import api from "./api";

export interface DashboardAgentSummary {
    totalCount: number;
    activeCount: number;
    passiveCount: number;
}

export interface DashboardConversationSummary {
    totalCount: number;
    todayCount: number;
    activeCount: number;
    completedCount: number;
    cancelledCount: number;
}

export interface DashboardKnowledgeSummary {
    knowledgeBaseCount: number;
    activeKnowledgeBaseCount: number;
    documentCount: number;
    readyDocumentCount: number;
    processingDocumentCount: number;
    failedDocumentCount: number;
    chunkCount: number;
    embeddedChunkCount: number;
}

export interface DashboardDailyConversationItem {
    date: string;
    dayName: string;
    count: number;
}

export interface DashboardTopAgentItem {
    aiAgentId: string;
    aiAgentName: string;
    isActive: boolean;
    provider: string;
    modelName: string;
    conversationCount: number;
}

export interface DashboardRecentDocumentItem {
    documentId: string;
    knowledgeBaseId: string;
    knowledgeBaseName: string;
    documentName: string;
    documentType: string;
    status: string;
    fileSizeBytes: number;
    chunkCount: number;
    uploadedAt: string;
    processedAt?: string | null;
}

export interface DashboardSummaryResponse {
    agents: DashboardAgentSummary;
    conversations: DashboardConversationSummary;
    knowledge: DashboardKnowledgeSummary;
    lastSevenDaysConversations: DashboardDailyConversationItem[];
    topAgents: DashboardTopAgentItem[];
    recentDocuments: DashboardRecentDocumentItem[];
}

export async function getDashboardSummary(
    companyId: string
): Promise<DashboardSummaryResponse> {
    const response = await api.get<DashboardSummaryResponse>(
        `/api/companies/${companyId}/dashboard`
    );

    return response.data;
}