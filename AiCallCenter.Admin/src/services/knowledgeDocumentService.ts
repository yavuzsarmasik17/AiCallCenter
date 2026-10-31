import axios from "axios";

const apiBaseUrl =
    import.meta.env.VITE_API_BASE_URL ?? "";

export interface KnowledgeDocument {
    id: string;
    knowledgeBaseId?: string;
    name: string;
    originalFileName?: string | null;
    filePath?: string | null;
    mimeType?: string | null;
    fileSizeBytes?: number;
    documentType: number;
    status: number;
    errorMessage?: string | null;
    uploadedAt: string;
    processedAt?: string | null;
}

export interface CreateTextDocumentRequest {
    name: string;
    content: string;
}

interface StoredUser {
    companyId?: string;
    CompanyId?: string;
}

function getAccessToken(): string | null {
    return localStorage.getItem("accessToken");
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

            const companyId =
                user.companyId ??
                user.CompanyId;

            if (companyId) {
                return companyId;
            }
        } catch {
            // Geçersiz localStorage verisi aşağıdaki hataya düşer.
        }
    }

    throw new Error(
        "Firma kimliği bulunamadı. Yeniden giriş yapın."
    );
}

function getAuthorizationHeaders():
    Record<string, string> {
    const token = getAccessToken();

    if (!token) {
        return {};
    }

    return {
        Authorization: `Bearer ${token}`
    };
}

function getDocumentsBaseUrl(
    knowledgeBaseId: string
): string {
    const companyId = getCompanyId();

    return (
        `${apiBaseUrl}/api/companies/${companyId}` +
        `/knowledge-bases/${knowledgeBaseId}/documents`
    );
}

export async function getKnowledgeDocuments(
    knowledgeBaseId: string
): Promise<KnowledgeDocument[]> {
    const response =
        await axios.get<KnowledgeDocument[]>(
            getDocumentsBaseUrl(
                knowledgeBaseId
            ),
            {
                headers:
                    getAuthorizationHeaders()
            }
        );

    return response.data;
}

export async function createTextDocument(
    knowledgeBaseId: string,
    request: CreateTextDocumentRequest
): Promise<KnowledgeDocument> {
    const response =
        await axios.post<KnowledgeDocument>(
            `${getDocumentsBaseUrl(
                knowledgeBaseId
            )}/text`,
            request,
            {
                headers: {
                    ...getAuthorizationHeaders(),
                    "Content-Type":
                        "application/json"
                }
            }
        );

    return response.data;
}

export async function uploadPdfDocument(
    knowledgeBaseId: string,
    name: string,
    file: File
): Promise<KnowledgeDocument> {
    const formData = new FormData();

    // Backend model alan adlarıyla birebir aynı olmalıdır.
    formData.append("Name", name);
    formData.append("File", file, file.name);

    const response =
        await axios.post<KnowledgeDocument>(
            `${getDocumentsBaseUrl(
                knowledgeBaseId
            )}/pdf`,
            formData,
            {
                headers:
                    getAuthorizationHeaders()
            }
        );

    /*
     * Content-Type değerini elle yazmıyoruz.
     * Tarayıcı/Axios multipart boundary bilgisini
     * otomatik olarak ekler.
     */
    return response.data;
}

export async function deleteKnowledgeDocument(
    knowledgeBaseId: string,
    documentId: string
): Promise<void> {
    await axios.delete(
        `${getDocumentsBaseUrl(
            knowledgeBaseId
        )}/${documentId}`,
        {
            headers:
                getAuthorizationHeaders()
        }
    );
}
