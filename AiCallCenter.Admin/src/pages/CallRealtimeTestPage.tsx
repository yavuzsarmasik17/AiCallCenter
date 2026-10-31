import {
    Alert,
    Box,
    Button,
    Chip,
    CircularProgress,
    Container,
    Divider,
    Paper,
    Stack,
    TextField,
    Typography
} from "@mui/material";

import {
    useCallback,
    useEffect,
    useMemo,
    useState
} from "react";

import type {
    FormEvent
} from "react";

import {
    useCallRealtime
} from "../hooks/useCallRealtime";

import {
    createCallMessage,
    getCallMessages,
    MessageSpeaker
} from "../services/callService";

import type {
    CallMessageDto
} from "../services/callService";

import type {
    RealtimeCallMessage
} from "../services/callHubService";

interface CallRealtimeTestPageProps {
    companyId: string;
    callId: string;
}

function getSpeakerName(
    speaker: CallMessageDto["speaker"]
): string {
    switch (speaker) {
        case MessageSpeaker.Customer:
            return "Müşteri";

        case MessageSpeaker.AiAgent:
            return "Yapay Zekâ";

        case MessageSpeaker.HumanAgent:
            return "İnsan Temsilci";

        case MessageSpeaker.System:
            return "Sistem";

        default:
            return "Bilinmeyen";
    }
}

function sortMessages(
    messages: CallMessageDto[]
): CallMessageDto[] {
    return [...messages].sort(
        (first, second) =>
            first.sequenceNumber -
            second.sequenceNumber
    );
}

function convertRealtimeMessage(
    message: RealtimeCallMessage
): CallMessageDto {
    return {
        id: message.id,
        callId: message.callId,
        speaker:
            message.speaker as
                CallMessageDto["speaker"],
        content: message.content,
        spokenAt: message.spokenAt,
        sequenceNumber:
            message.sequenceNumber,
        confidenceScore:
            message.confidenceScore,
        audioFileUrl:
            message.audioFileUrl
    };
}

export default function CallRealtimeTestPage({
    companyId,
    callId
}: CallRealtimeTestPageProps) {
    const [
        messages,
        setMessages
    ] = useState<CallMessageDto[]>([]);

    const [
        messageText,
        setMessageText
    ] = useState("");

    const [
        loading,
        setLoading
    ] = useState(true);

    const [
        sending,
        setSending
    ] = useState(false);

    const [
        pageErrorMessage,
        setPageErrorMessage
    ] = useState<string | null>(null);

    const handleRealtimeMessage =
        useCallback(
            (
                realtimeMessage:
                    RealtimeCallMessage
            ): void => {
                const incomingMessage =
                    convertRealtimeMessage(
                        realtimeMessage
                    );

                setMessages(
                    (previousMessages) => {
                        const messageExists =
                            previousMessages.some(
                                (message) =>
                                    message.id ===
                                    incomingMessage.id
                            );

                        if (messageExists) {
                            return previousMessages;
                        }

                        return sortMessages([
                            ...previousMessages,
                            incomingMessage
                        ]);
                    }
                );
            },
            []
        );

    const {
        isConnected,
        errorMessage: realtimeError
    } = useCallRealtime({
        companyId,
        callId,
        onMessageCreated:
            handleRealtimeMessage
    });

    useEffect(() => {
        let cancelled = false;

        const loadMessages =
            async (): Promise<void> => {
                try {
                    const result =
                        await getCallMessages(
                            companyId,
                            callId
                        );

                    if (!cancelled) {
                        setMessages(
                            sortMessages(result)
                        );

                        setPageErrorMessage(
                            null
                        );
                    }
                } catch (error) {
                    console.error(error);

                    if (!cancelled) {
                        setPageErrorMessage(
                            error instanceof Error
                                ? error.message
                                : "Mesajlar getirilemedi."
                        );
                    }
                } finally {
                    if (!cancelled) {
                        setLoading(false);
                    }
                }
            };

        void loadMessages();

        return () => {
            cancelled = true;
        };
    }, [companyId, callId]);

    const sortedMessages =
        useMemo(
            () => sortMessages(messages),
            [messages]
        );

    const handleSubmit = async (
        event: FormEvent<HTMLFormElement>
    ): Promise<void> => {
        event.preventDefault();

        const content =
            messageText.trim();

        if (!content) {
            return;
        }

        try {
            setSending(true);
            setPageErrorMessage(null);

            await createCallMessage(
                companyId,
                callId,
                {
                    speaker:
                        MessageSpeaker.Customer,
                    content,
                    confidenceScore: 1,
                    audioFileUrl: null
                }
            );

            setMessageText("");
        } catch (error) {
            console.error(error);

            setPageErrorMessage(
                error instanceof Error
                    ? error.message
                    : "Mesaj gönderilemedi."
            );
        } finally {
            setSending(false);
        }
    };

    return (
        <Container
            maxWidth="md"
            sx={{ py: 4 }}
        >
            <Stack spacing={2}>
                <Box>
                    <Typography
                        variant="h4"
                        sx={{
                            fontWeight: 700
                        }}
                    >
                        Canlı Çağrı Testi
                    </Typography>

                    <Typography
                        variant="body2"
                        color="text.secondary"
                    >
                        Çağrı ID: {callId}
                    </Typography>
                </Box>

                <Stack
                    direction="row"
                    spacing={1}
                    sx={{
                        alignItems: "center"
                    }}
                >
                    <Chip
                        label={
                            isConnected
                                ? "SignalR bağlı"
                                : "SignalR bağlı değil"
                        }
                        color={
                            isConnected
                                ? "success"
                                : "error"
                        }
                        size="small"
                    />
                </Stack>

                {realtimeError && (
                    <Alert severity="error">
                        {realtimeError}
                    </Alert>
                )}

                {pageErrorMessage && (
                    <Alert severity="error">
                        {pageErrorMessage}
                    </Alert>
                )}

                <Paper
                    variant="outlined"
                    sx={{
                        minHeight: 420,
                        maxHeight: 550,
                        overflowY: "auto",
                        p: 2
                    }}
                >
                    {loading ? (
                        <Box
                            sx={{
                                minHeight: 350,
                                display: "flex",
                                justifyContent:
                                    "center",
                                alignItems:
                                    "center"
                            }}
                        >
                            <CircularProgress />
                        </Box>
                    ) : sortedMessages.length ===
                      0 ? (
                        <Box
                            sx={{
                                minHeight: 350,
                                display: "flex",
                                justifyContent:
                                    "center",
                                alignItems:
                                    "center"
                            }}
                        >
                            <Typography
                                color="text.secondary"
                            >
                                Bu çağrıya ait mesaj
                                bulunamadı.
                            </Typography>
                        </Box>
                    ) : (
                        <Stack spacing={1.5}>
                            {sortedMessages.map(
                                (message) => (
                                    <Paper
                                        key={message.id}
                                        variant="outlined"
                                        sx={{
                                            p: 1.5
                                        }}
                                    >
                                        <Stack
                                            direction="row"
                                            spacing={1}
                                            sx={{
                                                justifyContent:
                                                    "space-between",
                                                alignItems:
                                                    "center"
                                            }}
                                        >
                                            <Typography
                                                variant="subtitle2"
                                                sx={{
                                                    fontWeight:
                                                        700
                                                }}
                                            >
                                                {getSpeakerName(
                                                    message.speaker
                                                )}
                                            </Typography>

                                            <Typography
                                                variant="caption"
                                                color="text.secondary"
                                            >
                                                {new Date(
                                                    message.spokenAt
                                                ).toLocaleString(
                                                    "tr-TR"
                                                )}
                                            </Typography>
                                        </Stack>

                                        <Divider
                                            sx={{
                                                my: 1
                                            }}
                                        />

                                        <Typography>
                                            {
                                                message.content
                                            }
                                        </Typography>
                                    </Paper>
                                )
                            )}
                        </Stack>
                    )}
                </Paper>

                <Paper
                    component="form"
                    onSubmit={handleSubmit}
                    variant="outlined"
                    sx={{ p: 2 }}
                >
                    <Stack spacing={2}>
                        <TextField
                            label="Test müşteri mesajı"
                            value={messageText}
                            onChange={(event) =>
                                setMessageText(
                                    event.target
                                        .value
                                )
                            }
                            multiline
                            minRows={2}
                            fullWidth
                            disabled={sending}
                        />

                        <Button
                            type="submit"
                            variant="contained"
                            disabled={
                                sending ||
                                !messageText.trim()
                            }
                        >
                            {sending
                                ? "Gönderiliyor..."
                                : "Mesaj Gönder"}
                        </Button>
                    </Stack>
                </Paper>
            </Stack>
        </Container>
    );
}
