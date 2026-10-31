import api from "./api";

interface StoredUser {
    userId?: string;
    companyId?: string;
    fullName?: string;
    email?: string;
    role?: number;
}

function getStoredUser(): StoredUser {
    const storedUser =
        localStorage.getItem("user");

    if (!storedUser) {
        return {};
    }

    try {
        return JSON.parse(
            storedUser
        ) as StoredUser;
    } catch {
        return {};
    }
}

function getCompanyId(): string {
    const directCompanyId =
        localStorage.getItem("companyId");

    if (directCompanyId) {
        return directCompanyId;
    }

    const user = getStoredUser();

    if (user.companyId) {
        return user.companyId;
    }

    throw new Error(
        "Şirket bilgisi bulunamadı. Lütfen çıkış yapıp tekrar giriş yapınız."
    );
}

function getBaseUrl(): string {
    return `/api/companies/${getCompanyId()}/call-operators`;
}

export const USER_ROLE = {
    SuperAdmin: 1,
    CompanyAdmin: 2,
    Supervisor: 3,
    Operator: 4
} as const;

export interface CallOperator {
    id: string;
    companyId: string;
    userId: string;
    displayName: string;
    extensionNumber?: string;
    status: number;
    currentCallId?: string;
    maxConcurrentCalls: number;
    activeCallCount: number;
    canReceiveCalls: boolean;
    lastActivityAt?: string;
    lastStatusChangedAt?: string;
}

export interface CreateCallOperatorRequest {
    userId: string;
    displayName: string;
    extensionNumber?: string;
    maxConcurrentCalls: number;
    canReceiveCalls: boolean;
}

export interface UpdateCallOperatorRequest {
    displayName: string;
    extensionNumber?: string;
    maxConcurrentCalls: number;
    canReceiveCalls: boolean;
}

export interface UpdateCallOperatorStatusRequest {
    status: number;
}

export function getCurrentStoredUser(): StoredUser {
    return getStoredUser();
}

export async function getCallOperators()
    : Promise<CallOperator[]>
{
    const response =
        await api.get<CallOperator[]>(
            getBaseUrl()
        );

    return response.data;
}

export async function getCallOperator(
    operatorId: string)
    : Promise<CallOperator>
{
    const response =
        await api.get<CallOperator>(
            `${getBaseUrl()}/${operatorId}`
        );

    return response.data;
}

export async function getCallOperatorByUserId(
    userId: string)
    : Promise<CallOperator>
{
    const response =
        await api.get<CallOperator>(
            `${getBaseUrl()}/by-user/${userId}`
        );

    return response.data;
}

export async function createCallOperator(
    request: CreateCallOperatorRequest)
    : Promise<CallOperator>
{
    const response =
        await api.post<CallOperator>(
            getBaseUrl(),
            request
        );

    return response.data;
}

export async function updateCallOperator(
    operatorId: string,
    request: UpdateCallOperatorRequest)
    : Promise<CallOperator>
{
    const response =
        await api.put<CallOperator>(
            `${getBaseUrl()}/${operatorId}`,
            request
        );

    return response.data;
}

export async function updateCallOperatorStatus(
    operatorId: string,
    status: number)
    : Promise<CallOperator>
{
    const response =
        await api.patch<CallOperator>(
            `${getBaseUrl()}/${operatorId}/status`,
            {
                status
            }
        );

    return response.data;
}

export async function deleteCallOperator(
    operatorId: string)
    : Promise<void>
{
    await api.delete(
        `${getBaseUrl()}/${operatorId}`
    );
}
