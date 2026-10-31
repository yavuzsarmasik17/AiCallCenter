import {
    useEffect,
    useRef,
    useState,
    type FormEvent
} from "react";
import axios from "axios";
import {
    Alert,
    Avatar,
    Box,
    Button,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    IconButton,
    Paper,
    TextField,
    Typography
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import PersonIcon from "@mui/icons-material/Person";
import RefreshIcon from "@mui/icons-material/Refresh";
import SendIcon from "@mui/icons-material/Send";
import SmartToyIcon from "@mui/icons-material/SmartToy";
import {
    chatWithAiAgent,
    type AiAgent
} from "../../services/aiAgentService";

interface AiAgentChatDialogProps {
    open: boolean;
    agent: AiAgent | null;
    onClose: () => void;
}

interface ChatSessionProps {
    agent: AiAgent;
    onClose: () => void;
}

interface ChatMessage {
    id: string;
    sender: "user" | "agent";
    content: string;
}

function getApiErrorMessage(
    exception: unknown,
    fallbackMessage: string
): string {
    if (!axios.isAxiosError(exception)) {
        return fallbackMessage;
    }

    const responseData =
        exception.response?.data as
            | {
                  detail?: string;
                  title?: string;
                  message?: string;
              }
            | undefined;

    return (
        responseData?.detail ??
        responseData?.title ??
        responseData?.message ??
        fallbackMessage
    );
}

function createMessageId(): string {
    return `${Date.now()}-${Math.random()}`;
}

function createWelcomeMessage(
    agent: AiAgent
): ChatMessage {
    return {
        id: createMessageId(),
        sender: "agent",
        content:
            agent.welcomeMessage ||
            "Merhaba, size nasıl yardımcı olabilirim?"
    };
}

function ChatSession({
    agent,
    onClose
}: ChatSessionProps) {
    const [conversationId, setConversationId] =
        useState<string | null>(null);

    const [messages, setMessages] =
        useState<ChatMessage[]>(() => [
            createWelcomeMessage(agent)
        ]);

    const [message, setMessage] = useState("");
    const [sending, setSending] = useState(false);
    const [error, setError] = useState("");

    const messagesEndRef =
        useRef<HTMLDivElement | null>(null);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({
            behavior: "smooth"
        });
    }, [messages, sending]);

    const handleResetConversation = () => {
        if (sending) {
            return;
        }

        setConversationId(null);
        setMessage("");
        setError("");
        setMessages([
            createWelcomeMessage(agent)
        ]);
    };

    const handleSend = async (
        event: FormEvent<HTMLFormElement>
    ) => {
        event.preventDefault();

        const trimmedMessage = message.trim();

        if (!trimmedMessage || sending) {
            return;
        }

        const userMessage: ChatMessage = {
            id: createMessageId(),
            sender: "user",
            content: trimmedMessage
        };

        setMessages((current) => [
            ...current,
            userMessage
        ]);

        setMessage("");
        setSending(true);
        setError("");

        try {
            const response =
                await chatWithAiAgent(
                    agent.id,
                    {
                        conversationId,
                        message: trimmedMessage
                    }
                );

            setConversationId(
                response.conversationId
            );

            const agentMessage: ChatMessage = {
                id: createMessageId(),
                sender: "agent",
                content: response.answer
            };

            setMessages((current) => [
                ...current,
                agentMessage
            ]);
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanından cevap alınamadı."
                )
            );
        } finally {
            setSending(false);
        }
    };

    const handleClose = () => {
        if (!sending) {
            onClose();
        }
    };

    return (
        <>
            <DialogTitle
                sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 1.5,
                    paddingRight: 7
                }}
            >
                <Avatar
                    sx={{
                        width: 38,
                        height: 38
                    }}
                >
                    <SmartToyIcon />
                </Avatar>

                <Box sx={{ flexGrow: 1 }}>
                    <Typography
                        component="h2"
                        variant="h6"
                        sx={{ fontWeight: 700 }}
                    >
                        {agent.name}
                    </Typography>

                    <Typography
                        component="p"
                        color="text.secondary"
                        sx={{ fontSize: 13 }}
                    >
                        Test sohbeti
                    </Typography>
                </Box>

                <IconButton
                    onClick={
                        handleResetConversation
                    }
                    disabled={sending}
                    aria-label="Sohbeti sıfırla"
                    title="Yeni sohbet başlat"
                >
                    <RefreshIcon />
                </IconButton>

                <IconButton
                    onClick={handleClose}
                    disabled={sending}
                    aria-label="Pencereyi kapat"
                    sx={{
                        position: "absolute",
                        right: 10,
                        top: 10
                    }}
                >
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent
                dividers
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    padding: 0,
                    backgroundColor: "#f5f7fb"
                }}
            >
                {error && (
                    <Alert
                        severity="error"
                        onClose={() =>
                            setError("")
                        }
                        sx={{
                            margin: 2,
                            marginBottom: 0
                        }}
                    >
                        {error}
                    </Alert>
                )}

                <Box
                    sx={{
                        flexGrow: 1,
                        overflowY: "auto",
                        padding: {
                            xs: 2,
                            md: 3
                        }
                    }}
                >
                    {messages.map(
                        (chatMessage) => {
                            const isUser =
                                chatMessage.sender ===
                                "user";

                            return (
                                <Box
                                    key={
                                        chatMessage.id
                                    }
                                    sx={{
                                        display: "flex",
                                        justifyContent:
                                            isUser
                                                ? "flex-end"
                                                : "flex-start",
                                        alignItems:
                                            "flex-end",
                                        gap: 1,
                                        marginBottom: 2
                                    }}
                                >
                                    {!isUser && (
                                        <Avatar
                                            sx={{
                                                width: 32,
                                                height: 32
                                            }}
                                        >
                                            <SmartToyIcon
                                                sx={{
                                                    fontSize: 19
                                                }}
                                            />
                                        </Avatar>
                                    )}

                                    <Paper
                                        elevation={0}
                                        sx={{
                                            maxWidth: {
                                                xs: "82%",
                                                md: "70%"
                                            },
                                            paddingX: 2,
                                            paddingY: 1.5,
                                            borderRadius: 3,
                                            whiteSpace:
                                                "pre-wrap",
                                            wordBreak:
                                                "break-word",
                                            backgroundColor:
                                                isUser
                                                    ? "primary.main"
                                                    : "background.paper",
                                            color: isUser
                                                ? "primary.contrastText"
                                                : "text.primary",
                                            border: isUser
                                                ? "none"
                                                : "1px solid",
                                            borderColor:
                                                "divider"
                                        }}
                                    >
                                        <Typography
                                            component="p"
                                            sx={{
                                                fontSize: 15,
                                                lineHeight: 1.55
                                            }}
                                        >
                                            {
                                                chatMessage.content
                                            }
                                        </Typography>
                                    </Paper>

                                    {isUser && (
                                        <Avatar
                                            sx={{
                                                width: 32,
                                                height: 32
                                            }}
                                        >
                                            <PersonIcon
                                                sx={{
                                                    fontSize: 19
                                                }}
                                            />
                                        </Avatar>
                                    )}
                                </Box>
                            );
                        }
                    )}

                    {sending && (
                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1,
                                marginBottom: 2
                            }}
                        >
                            <Avatar
                                sx={{
                                    width: 32,
                                    height: 32
                                }}
                            >
                                <SmartToyIcon
                                    sx={{
                                        fontSize: 19
                                    }}
                                />
                            </Avatar>

                            <Paper
                                elevation={0}
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 1.5,
                                    paddingX: 2,
                                    paddingY: 1.5,
                                    borderRadius: 3,
                                    border: "1px solid",
                                    borderColor:
                                        "divider"
                                }}
                            >
                                <CircularProgress
                                    size={18}
                                />

                                <Typography
                                    component="p"
                                    color="text.secondary"
                                    sx={{ fontSize: 14 }}
                                >
                                    AI cevap
                                    hazırlıyor...
                                </Typography>
                            </Paper>
                        </Box>
                    )}

                    <div ref={messagesEndRef} />
                </Box>
            </DialogContent>

            <DialogActions
                sx={{
                    padding: 2,
                    display: "block"
                }}
            >
                <Box
                    component="form"
                    onSubmit={handleSend}
                    sx={{
                        display: "flex",
                        alignItems: "flex-end",
                        gap: 1.5
                    }}
                >
                    <TextField
                        fullWidth
                        multiline
                        maxRows={4}
                        placeholder="Mesajınızı yazın..."
                        value={message}
                        disabled={sending}
                        onChange={(event) =>
                            setMessage(
                                event.target.value
                            )
                        }
                        onKeyDown={(event) => {
                            if (
                                event.key ===
                                    "Enter" &&
                                !event.shiftKey
                            ) {
                                event.preventDefault();

                                event.currentTarget
                                    .closest("form")
                                    ?.requestSubmit();
                            }
                        }}
                    />

                    <Button
                        type="submit"
                        variant="contained"
                        disabled={
                            sending ||
                            !message.trim()
                        }
                        endIcon={
                            sending
                                ? undefined
                                : <SendIcon />
                        }
                        sx={{
                            minHeight: 56,
                            minWidth: 115
                        }}
                    >
                        {sending ? (
                            <CircularProgress
                                size={22}
                                color="inherit"
                            />
                        ) : (
                            "Gönder"
                        )}
                    </Button>
                </Box>

                <Typography
                    component="p"
                    color="text.secondary"
                    sx={{
                        marginTop: 1,
                        fontSize: 12
                    }}
                >
                    Göndermek için Enter, alt satır
                    için Shift + Enter kullanın.
                </Typography>
            </DialogActions>
        </>
    );
}

function AiAgentChatDialog({
    open,
    agent,
    onClose
}: AiAgentChatDialogProps) {
    return (
        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth="md"
slotProps={{
    paper: {
        sx: {
            height: {
                xs: "92vh",
                md: "82vh"
            },
            maxHeight: 760
        }
    }
}}
        >
            {open && agent && (
                <ChatSession
                    key={agent.id}
                    agent={agent}
                    onClose={onClose}
                />
            )}
        </Dialog>
    );
}

export default AiAgentChatDialog;