export type ChatMessageRole =
    | "user"
    | "assistant"
    | "human"
    | "system";

export interface ChatAgent {
    id: string;
    companyId?: string;
    name: string;
    description?: string | null;
    isActive?: boolean;
}

export interface ChatMessage {
    id: string;
    role: ChatMessageRole;
    content: string;
    createdAt: string;
    sequenceNumber?: number;
}

export interface Conversation {
    id: string;
    companyId: string;
    agentId: string;
    agentName: string;
    customerName: string | null;
    customerPhone: string | null;
    status: number;
    title: string;
    createdAt: string;
    updatedAt: string;
    endedAt: string | null;
    messageCount: number;
    messages: ChatMessage[];
}

export interface SendChatMessageResponse {
    conversationId: string;
    aiAgentId?: string;
    answer: string;
    agentName: string;
    userMessage?: ChatMessage;
}
