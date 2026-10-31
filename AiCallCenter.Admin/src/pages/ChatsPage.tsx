import {
    useEffect,
    useMemo,
    useRef,
    useState
} from "react";
import axios from "axios";
import {
    Alert,
    Avatar,
    Box,
    Button,
    Card,
    Chip,
    CircularProgress,
    Divider,
    IconButton,
    MenuItem,
    Select,
    TextField,
    Typography
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import ChatIcon from "@mui/icons-material/Chat";
import DeleteIcon from "@mui/icons-material/Delete";
import SearchIcon from "@mui/icons-material/Search";
import SendIcon from "@mui/icons-material/Send";
import SmartToyIcon from "@mui/icons-material/SmartToy";
import {
    getChatAgents,
    sendChatMessage,
    getStoredCompanyId
} from "../services/chatService";
import type {
    ChatAgent,
    ChatMessage,
    Conversation
} from "../types/chat";

const storageKey = "ai-call-center-conversations";

function createId(): string {
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

function getApiErrorMessage(
    exception: unknown,
    fallbackMessage: string
): string {
    console.error(
        "Sohbet API hatası:",
        exception
    );

    if (!axios.isAxiosError(exception)) {
        if (exception instanceof Error) {
            return exception.message;
        }

        return fallbackMessage;
    }

    const status =
        exception.response?.status;

    const data =
        exception.response?.data as
            | {
                  detail?: string;
                  title?: string;
                  message?: string;
              }
            | undefined;

    if (status === 401) {
        return "Oturum süresi dolmuş veya token geçersiz. Yeniden giriş yapın.";
    }

    if (status === 403) {
        return "Bu firmanın AI ajanlarına erişim yetkiniz yok.";
    }

    if (status === 404) {
        return "AI ajanları API adresi bulunamadı.";
    }

    if (status === 500) {
        return "AI ajanları yüklenirken sunucu hatası oluştu.";
    }

    return (
        data?.detail ??
        data?.title ??
        data?.message ??
        exception.message ??
        fallbackMessage
    );
}

function loadStoredConversations(): Conversation[] {
    const stored =
        localStorage.getItem(storageKey);

    if (!stored) {
        return [];
    }

    try {
        return JSON.parse(stored) as Conversation[];
    } catch {
        return [];
    }
}

function formatConversationTime(
    dateValue: string
): string {
    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
        return "";
    }

    return new Intl.DateTimeFormat(
        "tr-TR",
        {
            day: "2-digit",
            month: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        }
    ).format(date);
}

function ChatsPage() {
    const companyId = getStoredCompanyId();

    const messagesEndRef =
        useRef<HTMLDivElement | null>(null);

    const [agents, setAgents] =
        useState<ChatAgent[]>([]);

    const [
        conversations,
        setConversations
    ] = useState<Conversation[]>(
        loadStoredConversations
    );

    const [
        selectedConversationId,
        setSelectedConversationId
    ] = useState<string | null>(null);

    const [
        selectedAgentId,
        setSelectedAgentId
    ] = useState("");

    const [
        messageText,
        setMessageText
    ] = useState("");

    const [
        searchText,
        setSearchText
    ] = useState("");

    const [
        loadingAgents,
        setLoadingAgents
    ] = useState(true);

    const [
        sending,
        setSending
    ] = useState(false);

    const [error, setError] =
        useState("");

    useEffect(() => {
        const loadAgents = async () => {
            if (!companyId) {
                setError(
                    "Firma kimliği bulunamadı. Yeniden giriş yapın."
                );
                setLoadingAgents(false);
                return;
            }

            try {
                const data =
                    await getChatAgents(
                        companyId
                    );

                const activeAgents =
                    data.filter(
                        (agent) =>
                            agent.isActive !== false
                    );

                setAgents(activeAgents);

                if (activeAgents.length > 0) {
                    setSelectedAgentId(
                        activeAgents[0].id
                    );
                }
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "AI ajanları yüklenemedi."
                    )
                );
            } finally {
                setLoadingAgents(false);
            }
        };

        void loadAgents();
    }, [companyId]);

    useEffect(() => {
        localStorage.setItem(
            storageKey,
            JSON.stringify(conversations)
        );
    }, [conversations]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({
            behavior: "smooth"
        });
    }, [
        selectedConversationId,
        conversations,
        sending
    ]);

    const selectedConversation =
        useMemo(
            () =>
                conversations.find(
                    (conversation) =>
                        conversation.id ===
                        selectedConversationId
                ) ?? null,
            [
                conversations,
                selectedConversationId
            ]
        );

    const filteredConversations =
        useMemo(() => {
            const query =
                searchText
                    .trim()
                    .toLocaleLowerCase("tr-TR");

            return [...conversations]
                .filter(
                    (conversation) =>
                        !query ||
                        conversation.title
                            .toLocaleLowerCase("tr-TR")
                            .includes(query) ||
                        conversation.agentName
                            .toLocaleLowerCase("tr-TR")
                            .includes(query)
                )
                .sort(
                    (left, right) =>
                        new Date(
                            right.updatedAt
                        ).getTime() -
                        new Date(
                            left.updatedAt
                        ).getTime()
                );
        }, [
            conversations,
            searchText
        ]);

    const handleNewConversation = () => {
        setSelectedConversationId(null);
        setMessageText("");
        setError("");
    };

    const handleDeleteConversation = (
        conversationId: string
    ) => {
        setConversations(
            (current) =>
                current.filter(
                    (conversation) =>
                        conversation.id !==
                        conversationId
                )
        );

        if (
            selectedConversationId ===
            conversationId
        ) {
            setSelectedConversationId(null);
        }
    };

    const handleSend = async () => {
        const trimmedMessage =
            messageText.trim();

        if (!trimmedMessage) {
            return;
        }

        if (!companyId) {
            setError(
                "Firma kimliği bulunamadı."
            );
            return;
        }

        if (!selectedAgentId) {
            setError(
                "Önce bir AI ajanı seçin."
            );
            return;
        }

        const selectedAgent =
            agents.find(
                (agent) =>
                    agent.id === selectedAgentId
            );

        if (!selectedAgent) {
            setError(
                "Seçilen AI ajanı bulunamadı."
            );
            return;
        }

        setSending(true);
        setError("");
        setMessageText("");

        const userMessage: ChatMessage = {
            id: createId(),
            role: "user",
            content: trimmedMessage,
            createdAt:
                new Date().toISOString()
        };

        let conversationId =
            selectedConversationId;

        if (!conversationId) {
            const now =
                new Date().toISOString();

            const newConversation: Conversation = {
    id: createId(),
    companyId: companyId ?? "",
    agentId: selectedAgent.id,
    agentName: selectedAgent.name,
    customerName: null,
    customerPhone: null,
    status: 1,
    title:
        trimmedMessage.length > 42
            ? `${trimmedMessage.slice(0, 42)}...`
            : trimmedMessage,
    createdAt: now,
    updatedAt: now,
    endedAt: null,
    messageCount: 1,
    messages: [userMessage]
};

            conversationId =
                newConversation.id;

            setConversations(
                (current) => [
                    newConversation,
                    ...current
                ]
            );

            setSelectedConversationId(
                conversationId
            );
        } else {
            setConversations(
                (current) =>
                    current.map(
                        (conversation) =>
                            conversation.id ===
                            conversationId
                                ? {
                                      ...conversation,
                                      updatedAt:
                                          new Date().toISOString(),
                                      messages: [
                                          ...conversation.messages,
                                          userMessage
                                      ]
                                  }
                                : conversation
                    )
            );
        }

        try {
            const response =
                await sendChatMessage(
                    companyId,
                    selectedAgent.id,
                    trimmedMessage
                );

            const assistantMessage: ChatMessage =
                {
                    id: createId(),
                    role: "assistant",
                    content: response.answer,
                    createdAt:
                        new Date().toISOString()
                };

            setConversations(
                (current) =>
                    current.map(
                        (conversation) =>
                            conversation.id ===
                            conversationId
                                ? {
                                      ...conversation,
                                      agentName:
                                          response.agentName ??
                                          conversation.agentName,
                                      updatedAt:
                                          new Date().toISOString(),
                                      messages: [
                                          ...conversation.messages,
                                          assistantMessage
                                      ]
                                  }
                                : conversation
                    )
            );
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI yanıtı alınamadı."
                )
            );
        } finally {
            setSending(false);
        }
    };

    return (
        <Box>
            <Box
                sx={{
                    display: "flex",
                    justifyContent:
                        "space-between",
                    alignItems: {
                        xs: "flex-start",
                        md: "center"
                    },
                    flexDirection: {
                        xs: "column",
                        md: "row"
                    },
                    gap: 2,
                    marginBottom: 3
                }}
            >
                <Box>
                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{ fontWeight: 800 }}
                    >
                        Sohbetler
                    </Typography>

                    <Typography
                        color="text.secondary"
                    >
                        AI ajanlarınızı gerçek
                        bilgilerle test edin.
                    </Typography>
                </Box>

                <Button
                    variant="contained"
                    startIcon={<AddIcon />}
                    onClick={
                        handleNewConversation
                    }
                    sx={{ borderRadius: 3 }}
                >
                    Yeni Sohbet
                </Button>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    sx={{ marginBottom: 2 }}
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>
            )}

            <Card
                sx={{
                    height: {
                        xs: "auto",
                        md: "calc(100vh - 180px)"
                    },
                    minHeight: 600,
                    display: "grid",
                    gridTemplateColumns: {
                        xs: "1fr",
                        md: "320px minmax(0, 1fr)"
                    },
                    borderRadius: 4,
                    overflow: "hidden",
                    border: 1,
                    borderColor: "divider"
                }}
            >
                <Box
                    sx={{
                        borderRight: {
                            xs: 0,
                            md: 1
                        },
                        borderBottom: {
                            xs: 1,
                            md: 0
                        },
                        borderColor: "divider",
                        backgroundColor:
                            "background.paper",
                        display: "flex",
                        flexDirection: "column"
                    }}
                >
                    <Box sx={{ padding: 2 }}>
                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1
                            }}
                        >
                            <SearchIcon color="action" />

                            <TextField
                                fullWidth
                                size="small"
                                placeholder="Sohbet ara..."
                                value={searchText}
                                onChange={(event) =>
                                    setSearchText(
                                        event.target.value
                                    )
                                }
                            />
                        </Box>
                    </Box>

                    <Divider />

                    <Box
                        sx={{
                            flexGrow: 1,
                            overflowY: "auto",
                            padding: 1
                        }}
                    >
                        {filteredConversations.length === 0 ? (
                            <Box
                                sx={{
                                    textAlign: "center",
                                    padding: 4
                                }}
                            >
                                <ChatIcon
                                    sx={{
                                        fontSize: 44,
                                        color:
                                            "text.disabled"
                                    }}
                                />

                                <Typography
                                    color="text.secondary"
                                    sx={{ marginTop: 1 }}
                                >
                                    Henüz sohbet yok
                                </Typography>
                            </Box>
                        ) : (
                            filteredConversations.map(
                                (conversation) => (
                                    <Box
                                        key={conversation.id}
                                        onClick={() => {
                                            setSelectedConversationId(
                                                conversation.id
                                            );
                                            setSelectedAgentId(
                                                conversation.agentId
                                            );
                                        }}
                                        sx={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: 1.5,
                                            padding: 1.5,
                                            borderRadius: 2.5,
                                            cursor: "pointer",
                                            marginBottom: 0.75,
                                            backgroundColor:
                                                selectedConversationId ===
                                                conversation.id
                                                    ? "action.selected"
                                                    : "transparent",
                                            "&:hover": {
                                                backgroundColor:
                                                    "action.hover"
                                            }
                                        }}
                                    >
                                        <Avatar
                                            sx={{
                                                width: 40,
                                                height: 40,
                                                backgroundColor:
                                                    "primary.main"
                                            }}
                                        >
                                            <SmartToyIcon />
                                        </Avatar>

                                        <Box
                                            sx={{
                                                minWidth: 0,
                                                flexGrow: 1
                                            }}
                                        >
                                            <Typography
                                                sx={{
                                                    fontWeight: 700,
                                                    overflow: "hidden",
                                                    textOverflow:
                                                        "ellipsis",
                                                    whiteSpace: "nowrap"
                                                }}
                                            >
                                                {conversation.title}
                                            </Typography>

                                            <Typography
                                                variant="caption"
                                                color="text.secondary"
                                            >
                                                {conversation.agentName}
                                                {" • "}
                                                {formatConversationTime(
                                                    conversation.updatedAt
                                                )}
                                            </Typography>
                                        </Box>

                                        <IconButton
                                            size="small"
                                            color="error"
                                            onClick={(event) => {
                                                event.stopPropagation();
                                                handleDeleteConversation(
                                                    conversation.id
                                                );
                                            }}
                                        >
                                            <DeleteIcon fontSize="small" />
                                        </IconButton>
                                    </Box>
                                )
                            )
                        )}
                    </Box>
                </Box>

                <Box
                    sx={{
                        display: "flex",
                        flexDirection: "column",
                        minWidth: 0
                    }}
                >
                    <Box
                        sx={{
                            padding: 2,
                            display: "flex",
                            alignItems: {
                                xs: "stretch",
                                sm: "center"
                            },
                            flexDirection: {
                                xs: "column",
                                sm: "row"
                            },
                            justifyContent:
                                "space-between",
                            gap: 2
                        }}
                    >
                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1.5
                            }}
                        >
                            <Avatar
                                sx={{
                                    backgroundColor:
                                        "primary.main"
                                }}
                            >
                                <SmartToyIcon />
                            </Avatar>

                            <Box>
                                <Typography
                                    sx={{ fontWeight: 800 }}
                                >
                                    {selectedConversation?.agentName ??
                                        agents.find(
                                            (agent) =>
                                                agent.id ===
                                                selectedAgentId
                                        )?.name ??
                                        "AI Ajanı"}
                                </Typography>

                                <Chip
                                    size="small"
                                    color="success"
                                    label="Çevrimiçi"
                                />
                            </Box>
                        </Box>

                        {loadingAgents ? (
                            <CircularProgress size={24} />
                        ) : (
                            <Select
                                size="small"
                                value={selectedAgentId}
                                onChange={(event) => {
                                    setSelectedAgentId(
                                        event.target.value
                                    );
                                    setSelectedConversationId(
                                        null
                                    );
                                }}
                                sx={{ minWidth: 220 }}
                            >
                                {agents.map(
                                    (agent) => (
                                        <MenuItem
                                            key={agent.id}
                                            value={agent.id}
                                        >
                                            {agent.name}
                                        </MenuItem>
                                    )
                                )}
                            </Select>
                        )}
                    </Box>

                    <Divider />

                    <Box
                        sx={{
                            flexGrow: 1,
                            overflowY: "auto",
                            padding: {
                                xs: 2,
                                md: 3
                            },
                            backgroundColor:
                                "#f8fafc"
                        }}
                    >
                        {!selectedConversation ||
                        selectedConversation.messages.length === 0 ? (
                            <Box
                                sx={{
                                    height: "100%",
                                    minHeight: 360,
                                    display: "flex",
                                    flexDirection: "column",
                                    alignItems: "center",
                                    justifyContent: "center",
                                    textAlign: "center"
                                }}
                            >
                                <Avatar
                                    sx={{
                                        width: 80,
                                        height: 80,
                                        background:
                                            "linear-gradient(135deg, #4F46E5, #7C3AED)",
                                        marginBottom: 2
                                    }}
                                >
                                    <SmartToyIcon
                                        sx={{ fontSize: 42 }}
                                    />
                                </Avatar>

                                <Typography
                                    variant="h5"
                                    sx={{ fontWeight: 800 }}
                                >
                                    Yeni bir sohbet başlatın
                                </Typography>

                                <Typography
                                    color="text.secondary"
                                    sx={{
                                        maxWidth: 480,
                                        marginTop: 1
                                    }}
                                >
                                    Bir AI ajanı seçin ve
                                    bilgi bankasındaki
                                    içeriklerle test etmeye
                                    başlayın.
                                </Typography>
                            </Box>
                        ) : (
                            selectedConversation.messages.map(
                                (message) => {
                                    const isUser =
                                        message.role === "user";

                                    return (
                                        <Box
                                            key={message.id}
                                            sx={{
                                                display: "flex",
                                                justifyContent:
                                                    isUser
                                                        ? "flex-end"
                                                        : "flex-start",
                                                marginBottom: 2
                                            }}
                                        >
                                            <Box
                                                sx={{
                                                    display: "flex",
                                                    flexDirection:
                                                        isUser
                                                            ? "row-reverse"
                                                            : "row",
                                                    alignItems: "flex-start",
                                                    gap: 1.25,
                                                    maxWidth: "82%"
                                                }}
                                            >
                                                <Avatar
                                                    sx={{
                                                        width: 36,
                                                        height: 36,
                                                        backgroundColor:
                                                            isUser
                                                                ? "secondary.main"
                                                                : "primary.main"
                                                    }}
                                                >
                                                    {isUser ? (
                                                        "S"
                                                    ) : (
                                                        <SmartToyIcon fontSize="small" />
                                                    )}
                                                </Avatar>

                                                <Box
                                                    sx={{
                                                        padding:
                                                            "12px 16px",
                                                        borderRadius:
                                                            isUser
                                                                ? "18px 4px 18px 18px"
                                                                : "4px 18px 18px 18px",
                                                        backgroundColor:
                                                            isUser
                                                                ? "primary.main"
                                                                : "background.paper",
                                                        color:
                                                            isUser
                                                                ? "primary.contrastText"
                                                                : "text.primary",
                                                        boxShadow:
                                                            "0 6px 18px rgba(15,23,42,0.08)",
                                                        whiteSpace: "pre-wrap",
                                                        wordBreak: "break-word"
                                                    }}
                                                >
                                                    <Typography>
                                                        {message.content}
                                                    </Typography>
                                                </Box>
                                            </Box>
                                        </Box>
                                    );
                                }
                            )
                        )}

                        {sending && (
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 1,
                                    color: "text.secondary"
                                }}
                            >
                                <CircularProgress size={18} />

                                <Typography variant="body2">
                                    AI düşünüyor...
                                </Typography>
                            </Box>
                        )}

                        <div ref={messagesEndRef} />
                    </Box>

                    <Divider />

                    <Box
                        sx={{
                            padding: 2,
                            backgroundColor:
                                "background.paper"
                        }}
                    >
                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "flex-end",
                                gap: 1.5
                            }}
                        >
                            <TextField
                                fullWidth
                                multiline
                                maxRows={5}
                                placeholder="Mesajınızı yazın..."
                                value={messageText}
                                disabled={
                                    sending ||
                                    loadingAgents ||
                                    agents.length === 0
                                }
                                onChange={(event) =>
                                    setMessageText(
                                        event.target.value
                                    )
                                }
                                onKeyDown={(event) => {
                                    if (
                                        event.key === "Enter" &&
                                        !event.shiftKey
                                    ) {
                                        event.preventDefault();
                                        void handleSend();
                                    }
                                }}
                            />

                            <IconButton
                                disabled={
                                    sending ||
                                    !messageText.trim() ||
                                    !selectedAgentId
                                }
                                onClick={() =>
                                    void handleSend()
                                }
                                sx={{
                                    width: 48,
                                    height: 48,
                                    backgroundColor:
                                        "primary.main",
                                    color:
                                        "primary.contrastText",
                                    "&:hover": {
                                        backgroundColor:
                                            "primary.dark"
                                    },
                                    "&.Mui-disabled": {
                                        backgroundColor:
                                            "action.disabledBackground"
                                    }
                                }}
                            >
                                <SendIcon />
                            </IconButton>
                        </Box>

                        <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{
                                display: "block",
                                marginTop: 1
                            }}
                        >
                            Enter gönderir, Shift + Enter
                            yeni satır açar.
                        </Typography>
                    </Box>
                </Box>
            </Card>
        </Box>
    );
}

export default ChatsPage;
