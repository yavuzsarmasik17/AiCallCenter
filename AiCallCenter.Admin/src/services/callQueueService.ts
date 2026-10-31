import api from "./api";

interface StoredUser {
    companyId?: string;
}

function getCompanyId(): string {
    const direct = localStorage.getItem("companyId");
    if (direct) return direct;

    const storedUser = localStorage.getItem("user");
    if (storedUser) {
        try {
            const user = JSON.parse(storedUser) as StoredUser;
            if (user.companyId) return user.companyId;
        } catch {
            // Geçersiz kullanıcı verisi aşağıdaki hata ile karşılanır.
        }
    }

    throw new Error(
        "Şirket bilgisi bulunamadı. Lütfen çıkış yapıp tekrar giriş yapınız."
    );
}

function getBaseUrl(): string {
    return `/api/companies/${getCompanyId()}/call-queues`;
}

export interface CallQueue {
    id: string;
    companyId: string;
    aiAgentId?: string | null;
    aiAgentName?: string | null;
    name: string;
    description?: string | null;
    priority: number;
    maxWaitingSeconds: number;
    maxQueueSize: number;
    isActive: boolean;
    operatorCount: number;
    waitingCallCount: number;
}

export interface CallQueueOperator {
    assignmentId: string;
    callOperatorId: string;
    displayName: string;
    extensionNumber?: string | null;
    operatorStatus: number;
    priority: number;
    isActive: boolean;
    assignedAt: string;
}

export interface QueueAiAgent {
    id: string;
    name: string;
    isActive: boolean;
    provider?: number | string;
    modelName?: string;
}

export interface CreateCallQueueRequest {
    name: string;
    description?: string;
    priority: number;
    maxWaitingSeconds: number;
    maxQueueSize: number;
    aiAgentId?: string | null;
}

export interface UpdateCallQueueRequest
    extends CreateCallQueueRequest {
    isActive: boolean;
}

export async function getCallQueues(): Promise<CallQueue[]> {
    const response =
        await api.get<CallQueue[]>(getBaseUrl());

    return response.data;
}

export async function createCallQueue(
    request: CreateCallQueueRequest
): Promise<CallQueue> {
    const response =
        await api.post<CallQueue>(
            getBaseUrl(),
            request
        );

    return response.data;
}

export async function updateCallQueue(
    queueId: string,
    request: UpdateCallQueueRequest
): Promise<CallQueue> {
    const response =
        await api.put<CallQueue>(
            `${getBaseUrl()}/${queueId}`,
            request
        );

    return response.data;
}

export async function deleteCallQueue(
    queueId: string
): Promise<void> {
    await api.delete(
        `${getBaseUrl()}/${queueId}`
    );
}

export async function getQueueOperators(
    queueId: string
): Promise<CallQueueOperator[]> {
    const response =
        await api.get<CallQueueOperator[]>(
            `${getBaseUrl()}/${queueId}/operators`
        );

    return response.data;
}

export async function assignOperatorToQueue(
    queueId: string,
    callOperatorId: string,
    priority: number
): Promise<CallQueueOperator> {
    const response =
        await api.post<CallQueueOperator>(
            `${getBaseUrl()}/${queueId}/operators`,
            {
                callOperatorId,
                priority
            }
        );

    return response.data;
}

export async function removeOperatorFromQueue(
    queueId: string,
    assignmentId: string
): Promise<void> {
    await api.delete(
        `${getBaseUrl()}/${queueId}/operators/${assignmentId}`
    );
}

export async function getQueueAiAgents(): Promise<QueueAiAgent[]> {
    const response =
        await api.get<QueueAiAgent[]>(
            `/api/companies/${getCompanyId()}/ai-agents`
        );

    return response.data;
}