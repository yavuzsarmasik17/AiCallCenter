import api from "./api";

export interface KnowledgeBase {
    id: string;
    companyId: string;
    name: string;
    description?: string | null;
    isActive: boolean;
    createdAt: string;
}

export interface CreateKnowledgeBaseRequest {
    name: string;
    description?: string;
}

export interface UpdateKnowledgeBaseRequest {
    name: string;
    description?: string;
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

export async function getKnowledgeBases():
    Promise<KnowledgeBase[]> {
    const companyId = getCompanyId();

    const response = await api.get<KnowledgeBase[]>(
        `/api/companies/${companyId}/knowledge-bases`
    );

    return response.data;
}

export async function createKnowledgeBase(
    request: CreateKnowledgeBaseRequest
): Promise<KnowledgeBase> {
    const companyId = getCompanyId();

    const response = await api.post<KnowledgeBase>(
        `/api/companies/${companyId}/knowledge-bases`,
        request
    );

    return response.data;
}

export async function updateKnowledgeBase(
    knowledgeBaseId: string,
    request: UpdateKnowledgeBaseRequest
): Promise<KnowledgeBase> {
    const companyId = getCompanyId();

    const response = await api.put<KnowledgeBase>(
        `/api/companies/${companyId}/knowledge-bases/${knowledgeBaseId}`,
        request
    );

    return response.data;
}

export async function deleteKnowledgeBase(
    knowledgeBaseId: string
): Promise<void> {
    const companyId = getCompanyId();

    await api.delete(
        `/api/companies/${companyId}/knowledge-bases/${knowledgeBaseId}`
    );
}