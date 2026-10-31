import api from "./api";

interface StoredUser {
    companyId?: string;
}

function getCompanyId(): string {
    const directCompanyId =
        localStorage.getItem("companyId");

    if (directCompanyId) {
        return directCompanyId;
    }

    const storedUser =
        localStorage.getItem("user");

    if (storedUser) {
        try {
            const user =
                JSON.parse(storedUser) as StoredUser;

            if (user.companyId) {
                return user.companyId;
            }
        } catch {
            // Geçersiz kullanıcı verisi varsa aşağıdaki hata fırlatılır.
        }
    }

    throw new Error(
        "Şirket bilgisi bulunamadı. Lütfen çıkış yapıp tekrar giriş yapınız."
    );
}

function getBaseUrl(operatorId: string): string {
    const companyId = getCompanyId();

    return `/api/companies/${companyId}/call-operators/${operatorId}/ai-agents`;
}

export interface CallOperatorAiAgent {
    id: string;
    companyId: string;
    callOperatorId: string;
    callOperatorName: string;
    aiAgentId: string;
    aiAgentName: string;
    provider: string;
    modelName: string;
    isPrimary: boolean;
    isActive: boolean;
    assignedAt: string;
}

export interface AvailableAiAgent {
    id: string;
    name: string;
    provider: string;
    modelName: string;
    isActive: boolean;
    isAssigned: boolean;
    isPrimary: boolean;
}

export interface AssignAiAgentRequest {
    aiAgentId: string;
    isPrimary: boolean;
}

export async function getOperatorAiAgents(
    operatorId: string
): Promise<CallOperatorAiAgent[]> {
    const response =
        await api.get<CallOperatorAiAgent[]>(
            getBaseUrl(operatorId)
        );

    return response.data;
}

export async function getAvailableAiAgents(
    operatorId: string
): Promise<AvailableAiAgent[]> {
    const response =
        await api.get<AvailableAiAgent[]>(
            `${getBaseUrl(operatorId)}/available`
        );

    return response.data;
}

export async function assignAiAgent(
    operatorId: string,
    request: AssignAiAgentRequest
): Promise<CallOperatorAiAgent> {
    const response =
        await api.post<CallOperatorAiAgent>(
            getBaseUrl(operatorId),
            request
        );

    return response.data;
}

export async function setPrimaryAiAgent(
    operatorId: string,
    assignmentId: string
): Promise<void> {
    await api.put(
        `${getBaseUrl(operatorId)}/${assignmentId}/primary`
    );
}

export async function removeAiAgent(
    operatorId: string,
    assignmentId: string
): Promise<void> {
    await api.delete(
        `${getBaseUrl(operatorId)}/${assignmentId}`
    );
}