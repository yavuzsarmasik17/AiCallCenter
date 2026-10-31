import axios from "axios";

import type {
    ChatAgent,
    ChatMessage,
    Conversation,
    SendChatMessageResponse
} from "../types/chat";

const apiBaseUrl =
    import.meta.env.VITE_API_BASE_URL ?? "";

interface ApiConversation {
    id?: string;
    conversationId?: string;
    companyId?: string;
    aiAgentId?: string;
    agentId?: string;
    aiAgentName?: string;
    agentName?: string;
    customerName?: string | null;
    customerPhone?: string | null;
    status?: number;
    startedAt?: string;
    createdAt?: string;
    endedAt?: string | null;
    updatedAt?: string;
    messageCount?: number;
    messages?: ApiConversationMessage[];
}

interface ApiConversationMessage {
    id?: string;
    speaker?: number | string;
    role?: string;
    content?: string;
    message?: string;
    createdAt?: string;
    sentAt?: string;
    sequenceNumber?: number;
}

function getHeaders() {
    const token =
        localStorage.getItem("accessToken");

    return token
        ? {
              Authorization: `Bearer ${token}`
          }
        : {};
}

function unwrapArray<T>(data: unknown): T[] {
    if (Array.isArray(data)) {
        return data as T[];
    }

    if (data && typeof data === "object") {
        const value = data as {
            items?: unknown;
            data?: unknown;
        };

        if (Array.isArray(value.items)) {
            return value.items as T[];
        }

        if (Array.isArray(value.data)) {
            return value.data as T[];
        }
    }

    return [];
}

function createFallbackId(): string {
    if (
        typeof crypto !== "undefined" &&
        "randomUUID" in crypto
    ) {
        return crypto.randomUUID();
    }

    return `${Date.now()}-${Math.random()
        .toString(16)
        .slice(2)}`;
}

function mapSpeakerToRole(
    speaker: number | string | undefined
): ChatMessage["role"] {
    if (
        speaker === 1 ||
        speaker === "1" ||
        speaker === "Customer" ||
        speaker === "customer"
    ) {
        return "user";
    }

    if (
        speaker === 2 ||
        speaker === "2" ||
        speaker === "AiAgent" ||
        speaker === "aiAgent" ||
        speaker === "assistant"
    ) {
        return "assistant";
    }

    if (
        speaker === 3 ||
        speaker === "3" ||
        speaker === "HumanAgent" ||
        speaker === "humanAgent" ||
        speaker === "human"
    ) {
        return "human";
    }

    return "system";
}

function mapMessage(
    message: ApiConversationMessage
): ChatMessage {
    const role =
        message.role === "user" ||
        message.role === "assistant" ||
        message.role === "human" ||
        message.role === "system"
            ? message.role
            : mapSpeakerToRole(message.speaker);

    return {
        id: message.id ?? createFallbackId(),
        role,
        content:
            message.content ??
            message.message ??
            "",
        createdAt:
            message.createdAt ??
            message.sentAt ??
            new Date().toISOString(),
        sequenceNumber:
            message.sequenceNumber
    };
}

function mapConversation(
    value: ApiConversation
): Conversation {
    const messages = (value.messages ?? [])
        .map(mapMessage)
        .sort(
            (a, b) =>
                (a.sequenceNumber ?? 0) -
                (b.sequenceNumber ?? 0)
        );

    const startedAt =
        value.startedAt ??
        value.createdAt ??
        new Date().toISOString();

    const lastMessage =
        messages.length > 0
            ? messages[messages.length - 1]
            : undefined;

    const updatedAt =
        value.updatedAt ??
        value.endedAt ??
        lastMessage?.createdAt ??
        startedAt;

    const firstUserMessage = messages
        .find(
            (message) =>
                message.role === "user"
        )
        ?.content.trim();

    const title = firstUserMessage
        ? firstUserMessage.length > 42
            ? `${firstUserMessage.slice(0, 42)}...`
            : firstUserMessage
        : value.customerName ??
          "Yeni sohbet";

    return {
        id:
            value.id ??
            value.conversationId ??
            createFallbackId(),
        companyId: value.companyId ?? "",
        agentId:
            value.aiAgentId ??
            value.agentId ??
            "",
        agentName:
            value.aiAgentName ??
            value.agentName ??
            "AI AjanÄ±",
        customerName:
            value.customerName ?? null,
        customerPhone:
            value.customerPhone ?? null,
        status: value.status ?? 1,
        title,
        createdAt: startedAt,
        updatedAt,
        endedAt: value.endedAt ?? null,
        messageCount:
            value.messageCount ??
            messages.length,
        messages
    };
}

export function getStoredCompanyId():
    | string
    | null {
    const companyId =
        localStorage.getItem("companyId");

    if (companyId) {
        return companyId;
    }

    const storedUser =
        localStorage.getItem("user");

    if (!storedUser) {
        return null;
    }

    try {
        const user = JSON.parse(
            storedUser
        ) as {
            companyId?: string;
            CompanyId?: string;
        };

        return (
            user.companyId ??
            user.CompanyId ??
            null
        );
    } catch {
        return null;
    }
}

export async function getChatAgents(
    companyId: string
): Promise<ChatAgent[]> {
    const response = await axios.get(
        `${apiBaseUrl}/api/companies/${companyId}/ai-agents`,
        {
            headers: getHeaders()
        }
    );

    return unwrapArray<ChatAgent>(
        response.data
    );
}

export async function getConversations(
    companyId: string
): Promise<Conversation[]> {
    const response = await axios.get(
        `${apiBaseUrl}/api/companies/${companyId}/conversations`,
        {
            headers: getHeaders()
        }
    );

    return unwrapArray<ApiConversation>(
        response.data
    ).map(mapConversation);
}

export async function getConversationDetail(
    companyId: string,
    conversationId: string
): Promise<Conversation> {
    const response = await axios.get(
        `${apiBaseUrl}/api/companies/${companyId}/conversations/${conversationId}`,
        {
            headers: getHeaders()
        }
    );

    const data =
        response.data?.data ??
        response.data;

    return mapConversation(
        data as ApiConversation
    );
}

export async function sendChatMessage(
    companyId: string,
    agentId: string,
    message: string,
    conversationId: string | null = null
): Promise<SendChatMessageResponse> {
    const response = await axios.post(
        `${apiBaseUrl}/api/companies/${companyId}/ai-agents/${agentId}/chat`,
        {
            conversationId,
            message
        },
        {
            headers: getHeaders()
        }
    );

    return {
        conversationId:
            response.data.conversationId,
        aiAgentId:
            response.data.aiAgentId,
        answer:
            response.data.answer ??
            response.data.response ??
            response.data.message ??
            "AI yanÄ±tÄ± alÄ±namadÄ±.",
        agentName:
            response.data.aiAgentName ??
            response.data.agentName ??
            "AI Asistan",
        userMessage:
            response.data.userMessage
    };
}

export async function cancelConversation(
    companyId: string,
    conversationId: string
): Promise<void> {
    await axios.post(
        `${apiBaseUrl}/api/companies/${companyId}/conversations/${conversationId}/cancel`,
        {},
        {
            headers: getHeaders()
        }
    );
}
