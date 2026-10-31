import api from "./api";

export interface AiAgent {
    id: string;
    companyId: string;
    name: string;
    instructions: string;
    welcomeMessage: string;
    language: number;
    provider: number;
    modelName: string;
    voiceName: string;
    maxCallDurationMinutes: number;
    transferToHumanEnabled: boolean;
    transferCallQueueId?: string | null;
    isActive: boolean;
    createdAt: string;
}

export interface AiAgentKnowledgeBase {
    id: string;
    knowledgeBaseId: string;
    knowledgeBaseName: string;
    knowledgeBaseDescription?: string | null;
    isActive: boolean;
}

export interface CreateAiAgentRequest {
    name: string;
    instructions: string;
    welcomeMessage: string;
    language: number;
    provider: number;
    modelName: string;
}

export interface UpdateAiAgentRequest {
    name: string;
    instructions: string;
    welcomeMessage: string;
    language: number;
    provider: number;
    modelName: string;
}

export interface UpdateAiAgentSettingsRequest {
    voiceName: string;
    maxCallDurationMinutes: number;
    transferToHumanEnabled: boolean;
    transferCallQueueId?: string | null;
    isActive: boolean;
}

export interface ChatWithAiAgentRequest {
    conversationId?: string | null;
    message: string;
}

export interface ChatWithAiAgentResponse {
    conversationId: string;
    aiAgentId: string;
    aiAgentName: string;
    userMessage: string;
    answer: string;
}

interface StoredUser {
    companyId?: string;
}

function getCompanyId(): string {
    const storedUser = localStorage.getItem("user");

    if (!storedUser) {
        throw new Error(
            "Kullanıcı bilgisi bulunamadı."
        );
    }

    const user = JSON.parse(
        storedUser
    ) as StoredUser;

    if (!user.companyId) {
        throw new Error(
            "Firma bilgisi bulunamadı."
        );
    }

    return user.companyId;
}

export async function getAiAgents():
    Promise<AiAgent[]> {
    const companyId = getCompanyId();

    const response = await api.get<AiAgent[]>(
        `/api/companies/${companyId}/ai-agents`
    );

    return response.data;
}

export async function createAiAgent(
    request: CreateAiAgentRequest
): Promise<AiAgent> {
    const companyId = getCompanyId();

    const response = await api.post<AiAgent>(
        `/api/companies/${companyId}/ai-agents`,
        request
    );

    return response.data;
}

export async function updateAiAgent(
    aiAgentId: string,
    request: UpdateAiAgentRequest
): Promise<AiAgent> {
    const companyId = getCompanyId();

    const response = await api.put<AiAgent>(
        `/api/companies/${companyId}/ai-agents/${aiAgentId}`,
        request
    );

    return response.data;
}

export async function updateAiAgentSettings(
    aiAgentId: string,
    request: UpdateAiAgentSettingsRequest
): Promise<AiAgent> {
    const companyId = getCompanyId();

    const response = await api.patch<AiAgent>(
        `/api/companies/${companyId}/ai-agents/${aiAgentId}/settings`,
        request
    );

    return response.data;
}

export async function deleteAiAgent(
    aiAgentId: string
): Promise<void> {
    const companyId = getCompanyId();

    await api.delete(
        `/api/companies/${companyId}/ai-agents/${aiAgentId}`
    );
}

export async function getAttachedKnowledgeBases(
    aiAgentId: string
): Promise<AiAgentKnowledgeBase[]> {
    const companyId = getCompanyId();

    const response =
        await api.get<AiAgentKnowledgeBase[]>(
            `/api/companies/${companyId}/ai-agents/${aiAgentId}/knowledge-bases`
        );

    return response.data;
}

export async function attachKnowledgeBase(
    aiAgentId: string,
    knowledgeBaseId: string
): Promise<void> {
    const companyId = getCompanyId();

    await api.post(
        `/api/companies/${companyId}/ai-agents/${aiAgentId}/knowledge-bases/${knowledgeBaseId}`
    );
}

export async function detachKnowledgeBase(
    aiAgentId: string,
    knowledgeBaseId: string
): Promise<void> {
    const companyId = getCompanyId();

    await api.delete(
        `/api/companies/${companyId}/ai-agents/${aiAgentId}/knowledge-bases/${knowledgeBaseId}`
    );
}

export async function chatWithAiAgent(
    aiAgentId: string,
    request: ChatWithAiAgentRequest
): Promise<ChatWithAiAgentResponse> {
    const companyId = getCompanyId();

    const response =
        await api.post<ChatWithAiAgentResponse>(
            `/api/companies/${companyId}/ai-agents/${aiAgentId}/chat`,
            request
        );

    return response.data;
}