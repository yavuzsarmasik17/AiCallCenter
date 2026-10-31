import api from "./api";

interface StoredUser {
    companyId?: string;
}

function getCompanyId(): string {
    const directCompanyId = localStorage.getItem("companyId");

    if (directCompanyId) {
        return directCompanyId;
    }

    const storedUser = localStorage.getItem("user");

    if (storedUser) {
        try {
            const user = JSON.parse(storedUser) as StoredUser;

            if (user.companyId) {
                return user.companyId;
            }
        } catch {
            // Geçersiz kullanıcı verisi aşağıdaki hata ile karşılanır.
        }
    }

    throw new Error(
        "Şirket bilgisi bulunamadı. Lütfen çıkış yapıp tekrar giriş yapınız."
    );
}

function getQueueItemsBaseUrl(): string {
    return `/api/companies/${getCompanyId()}/call-queue-items`;
}

function getRoutingBaseUrl(): string {
    return `/api/companies/${getCompanyId()}/call-routing`;
}

export type CallQueueStatus =
    | 1
    | 2
    | 3
    | 4
    | 5
    | 6
    | "Waiting"
    | "Assigned"
    | "Accepted"
    | "Completed"
    | "Cancelled"
    | "Expired";

export interface CallQueueItem {
    id: string;
    companyId: string;
    callQueueId: string;
    queueName: string;
    callId: string;
    callerNumber: string;
    calledNumber: string;
    assignedOperatorId?: string | null;
    assignedOperatorName?: string | null;
    assignedOperatorExtension?: string | null;
    status: CallQueueStatus;
    priority: number;
    transferReason?: string | null;
    aiHandoffSummary?: string | null;
    waitingSince: string;
    assignedAt?: string | null;
    acceptedAt?: string | null;
    completedAt?: string | null;
    cancelledAt?: string | null;
    expiresAt?: string | null;
    isExpired: boolean;
    waitingSeconds: number;
}

export interface CallRoutingResult {
    queueItemId: string;
    callId: string;
    operatorId?: string | null;
    operatorName?: string | null;
    extensionNumber?: string | null;
    routed: boolean;
    message?: string | null;
}

export interface QueuedCallActionResult {
    queueItemId: string;
    callId: string;
    assignedOperatorId?: string | null;
    status: CallQueueStatus;
    acceptedAt?: string | null;
    completedAt?: string | null;
}

export interface EnqueueCallRequest {
    callQueueId: string;
    callId: string;
    priority?: number | null;
    transferReason?: string | null;
}

export interface EnqueueCallResult {
    queueItemId: string;
    callId: string;
    callQueueId?: string;
    status?: CallQueueStatus;
    priority?: number;
    waitingSince?: string;
    expiresAt?: string | null;
}

export interface CompleteQueuedCallRequest {
    callOperatorId: string;
    summary?: string;
    result?: string;
}

export async function getCallQueueItems(
    status?: string,
    callQueueId?: string
): Promise<CallQueueItem[]> {
    const response = await api.get<CallQueueItem[]>(
        getQueueItemsBaseUrl(),
        {
            params: {
                status: status || undefined,
                callQueueId: callQueueId || undefined
            }
        }
    );

    return response.data;
}

export async function getWaitingCallQueueItems(
    callQueueId?: string
): Promise<CallQueueItem[]> {
    const response = await api.get<CallQueueItem[]>(
        `${getQueueItemsBaseUrl()}/waiting`,
        {
            params: {
                callQueueId: callQueueId || undefined
            }
        }
    );

    return response.data;
}

export async function getCallQueueItemById(
    queueItemId: string
): Promise<CallQueueItem> {
    const response = await api.get<CallQueueItem>(
        `${getQueueItemsBaseUrl()}/${queueItemId}`
    );

    return response.data;
}

export async function enqueueCall(
    request: EnqueueCallRequest
): Promise<EnqueueCallResult> {
    const response = await api.post<EnqueueCallResult>(
        getQueueItemsBaseUrl(),
        {
            callQueueId: request.callQueueId,
            callId: request.callId,
            priority: request.priority ?? null,
            transferReason: request.transferReason?.trim() || null
        }
    );

    return response.data;
}

export async function getOperatorQueueItems(
    operatorId: string,
    activeOnly = true
): Promise<CallQueueItem[]> {
    const response = await api.get<CallQueueItem[]>(
        `${getQueueItemsBaseUrl()}/operator/${operatorId}`,
        {
            params: {
                activeOnly
            }
        }
    );

    return response.data;
}

export async function routeWaitingCall(
    queueItemId: string
): Promise<CallRoutingResult> {
    const response = await api.post<CallRoutingResult>(
        `${getRoutingBaseUrl()}/route/${queueItemId}`
    );

    return response.data;
}

export async function acceptQueuedCall(
    queueItemId: string,
    callOperatorId: string
): Promise<QueuedCallActionResult> {
    const response = await api.post<QueuedCallActionResult>(
        `${getQueueItemsBaseUrl()}/${queueItemId}/accept`,
        {
            callOperatorId
        }
    );

    return response.data;
}

export async function completeQueuedCall(
    queueItemId: string,
    request: CompleteQueuedCallRequest
): Promise<QueuedCallActionResult> {
    const response = await api.post<QueuedCallActionResult>(
        `${getQueueItemsBaseUrl()}/${queueItemId}/complete`,
        request
    );

    return response.data;
}
