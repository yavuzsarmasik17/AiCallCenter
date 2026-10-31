import api from "./api";

export const MessageSpeaker = {
    Customer: 1,
    AiAgent: 2,
    HumanAgent: 3,
    System: 4
} as const;

export type MessageSpeaker =
    (typeof MessageSpeaker)[keyof typeof MessageSpeaker];

export const CallDirection = {
    Inbound: 1,
    Outbound: 2
} as const;

export type CallDirection =
    (typeof CallDirection)[keyof typeof CallDirection];

export interface CallDto {
    id: string;
    companyId: string;
    aiAgentId: string;
    aiAgentName: string | null;
    providerCallId: string | null;
    callerNumber: string;
    calledNumber: string;
    direction: CallDirection;
    status: number;
    startedAt: string;
    answeredAt: string | null;
    endedAt: string | null;
    durationSeconds: number;
    summary: string | null;
    result: string | null;
    transferredToHuman: boolean;
    transferNumber: string | null;
}

export interface CreateCallRequest {
    aiAgentId: string;
    callerNumber: string;
    calledNumber: string;
    direction: CallDirection;
}

export interface CallMessageDto {
    id: string;
    callId: string;
    speaker: MessageSpeaker;
    content: string;
    spokenAt: string;
    sequenceNumber: number;
    confidenceScore: number | null;
    audioFileUrl: string | null;
}

export interface CreateCallMessageRequest {
    speaker: MessageSpeaker;
    content: string;
    confidenceScore: number | null;
    audioFileUrl: string | null;
}

/*
 * Müşterinin mesajını çağrı sistemine göndermek
 * için kullanılan request.
 */
export interface ProcessCallCustomerMessageRequest {
    message: string;
}

/*
 * Backend'deki ProcessCallCustomerMessageResponse
 * modelinin frontend karşılığı.
 *
 * Çağrı AI tarafından yönetiliyorsa:
 * - aiMessage dolu
 * - answer dolu
 * - handledByHuman = false
 *
 * Çağrı insan operatöre devredilmişse:
 * - aiMessage = null
 * - answer = null
 * - handledByHuman = true
 */
export interface ProcessCallCustomerMessageResponse {
    callId: string;

    customerMessage: CallMessageDto;

    aiMessage: CallMessageDto | null;

    answer: string | null;

    handledByHuman: boolean;
}

/*
 * İnsan operatörün aktif görüşmeye
 * göndereceği mesaj.
 */
export interface SendOperatorMessageRequest {
    message: string;
}

export async function createCall(
    companyId: string,
    request: CreateCallRequest
): Promise<CallDto> {
    const response = await api.post<CallDto>(
        `/api/companies/${companyId}/calls`,
        request
    );

    return response.data;
}

export async function getCalls(
    companyId: string
): Promise<CallDto[]> {
    const response = await api.get<CallDto[]>(
        `/api/companies/${companyId}/calls`
    );

    return response.data;
}

export async function getCallById(
    companyId: string,
    callId: string
): Promise<CallDto> {
    const response = await api.get<CallDto>(
        `/api/companies/${companyId}/calls/${callId}`
    );

    return response.data;
}

export async function getCallMessages(
    companyId: string,
    callId: string
): Promise<CallMessageDto[]> {
    const response = await api.get<CallMessageDto[]>(
        `/api/companies/${companyId}/calls/${callId}/messages`
    );

    return response.data;
}

export async function createCallMessage(
    companyId: string,
    callId: string,
    request: CreateCallMessageRequest
): Promise<CallMessageDto> {
    const response = await api.post<CallMessageDto>(
        `/api/companies/${companyId}/calls/${callId}/messages`,
        request
    );

    return response.data;
}

/*
 * Müşteri mesajını gönderir.
 *
 * Çağrı AI tarafından yönetiliyorsa:
 * - Customer mesajı kaydedilir.
 * - Knowledge Base aranır.
 * - Transfer kararı değerlendirilir.
 * - Gerekirse AI cevabı oluşturulur.
 *
 * Çağrı daha önce insan operatöre devredilmişse:
 * - Customer mesajı yine kaydedilir.
 * - Realtime olarak operatöre iletilir.
 * - Knowledge Base çalışmaz.
 * - AI çalışmaz.
 * - Yeni AI mesajı oluşturulmaz.
 */
export async function processCallCustomerMessage(
    companyId: string,
    callId: string,
    message: string
): Promise<ProcessCallCustomerMessageResponse> {
    const response =
        await api.post<ProcessCallCustomerMessageResponse>(
            `/api/companies/${companyId}/calls/${callId}/customer-message`,
            {
                message
            }
        );

    return response.data;
}

/*
 * İnsan operatörün aktif çağrıya
 * gönderdiği mesajı kaydeder.
 *
 * Backend mesajı:
 * Speaker = HumanAgent
 * olarak CallMessages tablosuna ekler.
 */
export async function sendOperatorMessage(
    companyId: string,
    callId: string,
    message: string
): Promise<CallMessageDto> {
    const response =
        await api.post<CallMessageDto>(
            `/api/companies/${companyId}/calls/${callId}/operator-message`,
            {
                message
            }
        );

    return response.data;
}