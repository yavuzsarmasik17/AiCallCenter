import {
    useEffect,
    useState
} from "react";

import axios from "axios";

import {
    AddIcCallOutlined,
    ArrowBack,
    PsychologyOutlined,
    QueueOutlined,
    SendOutlined
} from "@mui/icons-material";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Divider,
    Grid,
    MenuItem,
    Paper,
    TextField,
    Typography
} from "@mui/material";

import {
    useNavigate
} from "react-router-dom";

import {
    getAiAgents,
    type AiAgent
} from "../services/aiAgentService";

import {
    getCallQueues,
    type CallQueue
} from "../services/callQueueService";

import {
    createCall,
    getCallMessages,
    processCallCustomerMessage,
    MessageSpeaker,
    type CallDirection,
    type CallDto,
    type CallMessageDto
} from "../services/callService";

import {
    joinCall,
    leaveCall,
    onCallMessageCreated
} from "../services/callRealtimeService";

import {
    enqueueCall
} from "../services/callQueueItemService";

interface FormState {
    aiAgentId: string;
    callQueueId: string;
    callerNumber: string;
    calledNumber: string;
    direction: CallDirection;
    priority: number;
    transferReason: string;
}

const initialForm: FormState = {
    aiAgentId: "",
    callQueueId: "",
    callerNumber: "",
    calledNumber: "",
    direction: 1,
    priority: 10,
    transferReason: ""
};

function getErrorMessage(
    exception: unknown,
    fallbackMessage: string
): string {
    if (!axios.isAxiosError(exception)) {
        return exception instanceof Error
            ? exception.message
            : fallbackMessage;
    }

    const data = exception.response?.data as
        | {
            detail?: string;
            title?: string;
            message?: string;
            errors?: Record<string, string[]>;
        }
        | undefined;

    const validationMessage = data?.errors
        ? Object.values(data.errors)
            .flat()
            .join(" ")
        : "";

    return (
        validationMessage ||
        data?.detail ||
        data?.message ||
        data?.title ||
        fallbackMessage
    );
}

function getSpeakerName(
    speaker: number
): string {
    switch (speaker) {
        case MessageSpeaker.Customer:
            return "Müşteri";

        case MessageSpeaker.AiAgent:
            return "AI";

        case MessageSpeaker.HumanAgent:
            return "Temsilci";

        case MessageSpeaker.System:
            return "Sistem";

        default:
            return "Bilinmiyor";
    }
}

function CreateCallPage() {
    const navigate = useNavigate();

    const storedUser =
        localStorage.getItem("user");

    const companyId = storedUser
        ? (
            JSON.parse(storedUser) as {
                companyId?: string;
            }
        ).companyId ?? ""
        : "";

    const [agents, setAgents] =
        useState<AiAgent[]>([]);

    const [queues, setQueues] =
        useState<CallQueue[]>([]);

    const [form, setForm] =
        useState<FormState>(initialForm);

    const [loading, setLoading] =
        useState(true);

    const [saving, setSaving] =
        useState(false);

    const [
        startingAiTest,
        setStartingAiTest
    ] = useState(false);

    const [
        sendingMessage,
        setSendingMessage
    ] = useState(false);

    const [error, setError] =
        useState("");

    const [success, setSuccess] =
        useState("");

    const [testCall, setTestCall] =
        useState<CallDto | null>(null);

    const [messages, setMessages] =
        useState<CallMessageDto[]>([]);

    const [
        customerMessage,
        setCustomerMessage
    ] = useState("");

    const [handledByHuman, setHandledByHuman] = useState(false);

    useEffect(() => {
        const loadData =
            async (): Promise<void> => {
                try {
                    setError("");

                    const [
                        agentData,
                        queueData
                    ] = await Promise.all([
                        getAiAgents(),
                        getCallQueues()
                    ]);

                    const activeAgents =
                        agentData
                            .filter(
                                (item) =>
                                    item.isActive
                            )
                            .sort((a, b) =>
                                a.name.localeCompare(
                                    b.name,
                                    "tr"
                                )
                            );

                    const allQueues =
                        [...queueData].sort(
                            (a, b) =>
                                a.name.localeCompare(
                                    b.name,
                                    "tr"
                                )
                        );

                    setAgents(activeAgents);
                    setQueues(allQueues);

                    setForm((current) => ({
                        ...current,

                        aiAgentId:
                            activeAgents.length === 1
                                ? activeAgents[0].id
                                : current.aiAgentId,

                        callQueueId:
                            allQueues.length === 1
                                ? allQueues[0].id
                                : current.callQueueId
                    }));

                    if (
                        allQueues.length === 0
                    ) {
                        setError(
                            "Çağrı kuyruğu bulunamadı. Normal çağrıyı kuyruğa eklemek için önce Çağrı Kuyrukları ekranından bir kuyruk oluşturunuz."
                        );
                    }
                } catch (exception) {
                    setError(
                        getErrorMessage(
                            exception,
                            "Form verileri yüklenemedi."
                        )
                    );
                } finally {
                    setLoading(false);
                }
            };

        void loadData();
    }, []);

    useEffect(() => {
        if (!testCall) {
            return;
        }

        let disposed = false;

        const unsubscribe =
            onCallMessageCreated(
                (notification) => {
                    if (
                        notification.callId !==
                        testCall.id
                    ) {
                        return;
                    }

                    const realtimeMessage =
                        notification;

                    setMessages((current) => {
                        const exists =
                            current.some(
                                (item) =>
                                    item.id ===
                                    realtimeMessage.id
                            );

                        if (exists) {
                            return current;
                        }

                        return [
                            ...current,
                            realtimeMessage
                        ].sort(
                            (a, b) =>
                                a.sequenceNumber -
                                b.sequenceNumber
                        );
                    });
                }
            );

        const connectRealtime =
            async (): Promise<void> => {
                try {
                    await joinCall(
                        testCall.id
                    );

                    if (!disposed) {
                        console.log(
                            "Çağrı SignalR grubuna katıldı:",
                            testCall.id
                        );
                    }
                } catch (exception) {
                    console.error(
                        "SignalR çağrı bağlantısı kurulamadı:",
                        exception
                    );
                }
            };

        void connectRealtime();

        return () => {
            disposed = true;
            unsubscribe();

            void leaveCall(
                testCall.id
            ).catch((exception) => {
                console.error(
                    "SignalR çağrı grubundan çıkılamadı:",
                    exception
                );
            });
        };
    }, [testCall]);

    const validateCallFields =
        (): string | null => {
            if (!form.aiAgentId) {
                return "Lütfen AI ajanı seçiniz.";
            }

            if (!form.callerNumber.trim()) {
                return "Arayan numara zorunludur.";
            }

            if (!form.calledNumber.trim()) {
                return "Aranan numara zorunludur.";
            }

            return null;
        };

    const validateNormalCall =
        (): string | null => {
            const callValidation =
                validateCallFields();

            if (callValidation) {
                return callValidation;
            }

            if (!form.callQueueId) {
                return "Lütfen çağrı kuyruğu seçiniz.";
            }

            const selectedQueue =
                queues.find(
                    (item) =>
                        item.id ===
                        form.callQueueId
                );

            if (
                selectedQueue &&
                !selectedQueue.isActive
            ) {
                return "Seçilen çağrı kuyruğu pasif. Önce kuyruğu aktif hale getiriniz.";
            }

            if (
                form.priority < 1 ||
                form.priority > 100
            ) {
                return "Öncelik 1 ile 100 arasında olmalıdır.";
            }

            return null;
        };

    const submit =
        async (): Promise<void> => {
            const validationError =
                validateNormalCall();

            if (validationError) {
                setError(validationError);
                return;
            }

            setSaving(true);
            setError("");
            setSuccess("");

            try {
                if (!companyId) {
                    setError(
                        "Firma bilgisi bulunamadı. Lütfen yeniden giriş yapınız."
                    );

                    return;
                }

                const createdCall =
                    await createCall(
                        companyId,
                        {
                            aiAgentId:
                                form.aiAgentId,

                            callerNumber:
                                form.callerNumber.trim(),

                            calledNumber:
                                form.calledNumber.trim(),

                            direction:
                                form.direction
                        }
                    );

                await enqueueCall({
                    callQueueId:
                        form.callQueueId,

                    callId:
                        createdCall.id,

                    priority:
                        form.priority,

                    transferReason:
                        form.transferReason.trim() ||
                        null
                });

                setSuccess(
                    "Çağrı oluşturuldu ve kuyruğa başarıyla eklendi."
                );

                setForm((current) => ({
                    ...initialForm,

                    aiAgentId:
                        current.aiAgentId,

                    callQueueId:
                        current.callQueueId
                }));
            } catch (exception) {
                setError(
                    getErrorMessage(
                        exception,
                        "Çağrı oluşturulamadı."
                    )
                );
            } finally {
                setSaving(false);
            }
        };

    const startAiTest =
        async (): Promise<void> => {
            const validationError =
                validateCallFields();

            if (validationError) {
                setError(validationError);
                return;
            }

            if (!companyId) {
                setError(
                    "Firma bilgisi bulunamadı. Lütfen yeniden giriş yapınız."
                );

                return;
            }

            setStartingAiTest(true);
            setError("");
            setSuccess("");

            try {
                const createdCall =
                    await createCall(
                        companyId,
                        {
                            aiAgentId:
                                form.aiAgentId,

                            callerNumber:
                                form.callerNumber.trim(),

                            calledNumber:
                                form.calledNumber.trim(),

                            direction:
                                form.direction
                        }
                    );

                setTestCall(createdCall);
                setMessages([]);
                setCustomerMessage("");
                setHandledByHuman(false);

                setSuccess(
                    "AI test çağrısı oluşturuldu. Şimdi aşağıdaki alandan müşteri mesajı gönderebilirsiniz."
                );
            } catch (exception) {
                setError(
                    getErrorMessage(
                        exception,
                        "AI test çağrısı başlatılamadı."
                    )
                );
            } finally {
                setStartingAiTest(false);
            }
        };

    const refreshMessages =
        async (
            callId: string
        ): Promise<void> => {
            if (!companyId) {
                return;
            }

            const callMessages =
                await getCallMessages(
                    companyId,
                    callId
                );

            setMessages(
                [...callMessages].sort(
                    (a, b) =>
                        a.sequenceNumber -
                        b.sequenceNumber
                )
            );
        };

    const sendCustomerMessage =
        async (): Promise<void> => {
            if (!testCall) {
                setError("Önce AI test çağrısını başlatınız.");
                return;
            }

            const message = customerMessage.trim();
            if (!message) {
                setError("Müşteri mesajı boş olamaz.");
                return;
            }

            if (!companyId) {
                setError("Firma bilgisi bulunamadı. Lütfen yeniden giriş yapınız.");
                return;
            }

            setSendingMessage(true);
            setError("");

            try {
                const response = await processCallCustomerMessage(
                    companyId,
                    testCall.id,
                    message
                );

                setCustomerMessage("");
                setHandledByHuman(response.handledByHuman);

                setMessages((current) => {
                    const received: CallMessageDto[] = [response.customerMessage];
                    if (response.aiMessage) {
                        received.push(response.aiMessage);
                    }
                    const receivedIds = new Set(received.map((item) => item.id));
                    return [
                        ...current.filter((item) => !receivedIds.has(item.id)),
                        ...received
                    ].sort((a, b) => a.sequenceNumber - b.sequenceNumber);
                });

                await refreshMessages(testCall.id);
            } catch (exception) {
                setError(getErrorMessage(
                    exception,
                    handledByHuman
                        ? "Müşteri mesajı operatöre iletilemedi."
                        : "Müşteri mesajı işlenemedi."
                ));
            } finally {
                setSendingMessage(false);
            }
        };

    const closeAiTest = (): void => {
        setTestCall(null);
        setMessages([]);
        setCustomerMessage("");
        setHandledByHuman(false);
        setSuccess("");
        setError("");
    };

    if (loading) {
        return (
            <Box
                sx={{
                    padding: 8,
                    textAlign: "center"
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box
            sx={{
                padding: {
                    xs: 2,
                    md: 3
                }
            }}
        >
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
                        variant="h4"
                        sx={{
                            fontWeight: 800
                        }}
                    >
                        Yeni Çağrı
                    </Typography>

                    <Typography
                        color="text.secondary"
                    >
                        Normal çağrı oluşturabilir
                        veya Mock AI görüşmesini
                        test edebilirsiniz.
                    </Typography>
                </Box>

                <Button
                    variant="outlined"
                    startIcon={
                        <ArrowBack />
                    }
                    onClick={() =>
                        navigate(
                            "/live-calls"
                        )
                    }
                >
                    Canlı Çağrılar
                </Button>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    onClose={() =>
                        setError("")
                    }
                    sx={{
                        marginBottom: 2
                    }}
                >
                    {error}
                </Alert>
            )}

            {success && (
                <Alert
                    severity="success"
                    onClose={() =>
                        setSuccess("")
                    }
                    sx={{
                        marginBottom: 2
                    }}
                >
                    {success}
                </Alert>
            )}

            <Card
                sx={{
                    borderRadius: 4
                }}
            >
                <CardContent
                    sx={{
                        padding: 3
                    }}
                >
                    <Box
                        sx={{
                            display: "flex",
                            alignItems: "center",
                            gap: 1,
                            marginBottom: 3
                        }}
                    >
                        <AddIcCallOutlined
                            color="primary"
                        />

                        <Typography
                            variant="h6"
                            sx={{
                                fontWeight: 700
                            }}
                        >
                            Çağrı Bilgileri
                        </Typography>
                    </Box>

                    <Grid
                        container
                        spacing={2}
                    >
                        <Grid
                            size={{
                                xs: 12,
                                md: 6
                            }}
                        >
                            <TextField
                                select
                                fullWidth
                                required
                                label="AI Ajanı"
                                value={
                                    form.aiAgentId
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            aiAgentId:
                                                event
                                                    .target
                                                    .value
                                        })
                                    )
                                }
                            >
                                {agents.length ===
                                    0 ? (
                                    <MenuItem
                                        value=""
                                        disabled
                                    >
                                        Aktif AI ajanı
                                        bulunamadı
                                    </MenuItem>
                                ) : (
                                    agents.map(
                                        (
                                            agent
                                        ) => (
                                            <MenuItem
                                                key={
                                                    agent.id
                                                }
                                                value={
                                                    agent.id
                                                }
                                            >
                                                {
                                                    agent.name
                                                }
                                            </MenuItem>
                                        )
                                    )
                                )}
                            </TextField>
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 6
                            }}
                        >
                            <TextField
                                select
                                fullWidth
                                label="Çağrı Kuyruğu"
                                value={
                                    form.callQueueId
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            callQueueId:
                                                event
                                                    .target
                                                    .value
                                        })
                                    )
                                }
                                helperText="Normal çağrı için kullanılır. AI test çağrısı başlangıçta kuyruğa eklenmez."
                            >
                                {queues.length ===
                                    0 ? (
                                    <MenuItem
                                        value=""
                                        disabled
                                    >
                                        Çağrı kuyruğu
                                        bulunamadı
                                    </MenuItem>
                                ) : (
                                    queues.map(
                                        (
                                            queue
                                        ) => (
                                            <MenuItem
                                                key={
                                                    queue.id
                                                }
                                                value={
                                                    queue.id
                                                }
                                            >
                                                {
                                                    queue.name
                                                }

                                                {!queue.isActive
                                                    ? " (Pasif)"
                                                    : ""}
                                            </MenuItem>
                                        )
                                    )
                                )}
                            </TextField>
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 6
                            }}
                        >
                            <TextField
                                fullWidth
                                required
                                label="Arayan Numara"
                                placeholder="05xx xxx xx xx"
                                value={
                                    form.callerNumber
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            callerNumber:
                                                event
                                                    .target
                                                    .value
                                        })
                                    )
                                }
                            />
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 6
                            }}
                        >
                            <TextField
                                fullWidth
                                required
                                label="Aranan Numara"
                                placeholder="0850 xxx xx xx"
                                value={
                                    form.calledNumber
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            calledNumber:
                                                event
                                                    .target
                                                    .value
                                        })
                                    )
                                }
                            />
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 4
                            }}
                        >
                            <TextField
                                select
                                fullWidth
                                label="Çağrı Yönü"
                                value={
                                    form.direction
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            direction:
                                                Number(
                                                    event
                                                        .target
                                                        .value
                                                ) as CallDirection
                                        })
                                    )
                                }
                            >
                                <MenuItem
                                    value={1}
                                >
                                    Gelen çağrı
                                </MenuItem>

                                <MenuItem
                                    value={2}
                                >
                                    Giden çağrı
                                </MenuItem>
                            </TextField>
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 4
                            }}
                        >
                            <TextField
                                fullWidth
                                type="number"
                                label="Öncelik"
                                value={
                                    form.priority
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            priority:
                                                Number(
                                                    event
                                                        .target
                                                        .value
                                                )
                                        })
                                    )
                                }
                                slotProps={{
                                    htmlInput: {
                                        min: 1,
                                        max: 100
                                    }
                                }}
                            />
                        </Grid>

                        <Grid
                            size={{
                                xs: 12,
                                md: 4
                            }}
                        >
                            <TextField
                                fullWidth
                                label="Aktarım Nedeni"
                                value={
                                    form.transferReason
                                }
                                disabled={
                                    testCall !== null
                                }
                                onChange={(
                                    event
                                ) =>
                                    setForm(
                                        (
                                            current
                                        ) => ({
                                            ...current,

                                            transferReason:
                                                event
                                                    .target
                                                    .value
                                        })
                                    )
                                }
                            />
                        </Grid>

                        {!testCall && (
                            <>
                                <Grid
                                    size={{
                                        xs: 12,
                                        md: 6
                                    }}
                                >
                                    <Button
                                        fullWidth
                                        size="large"
                                        variant="contained"
                                        startIcon={
                                            saving ? (
                                                <CircularProgress
                                                    size={
                                                        20
                                                    }
                                                    color="inherit"
                                                />
                                            ) : (
                                                <QueueOutlined />
                                            )
                                        }
                                        disabled={
                                            saving ||
                                            startingAiTest ||
                                            agents.length ===
                                            0 ||
                                            queues.length ===
                                            0
                                        }
                                        onClick={() =>
                                            void submit()
                                        }
                                        sx={{
                                            minHeight: 52
                                        }}
                                    >
                                        {saving
                                            ? "Oluşturuluyor..."
                                            : "Çağrıyı Oluştur ve Kuyruğa Ekle"}
                                    </Button>
                                </Grid>

                                <Grid
                                    size={{
                                        xs: 12,
                                        md: 6
                                    }}
                                >
                                    <Button
                                        fullWidth
                                        size="large"
                                        variant="outlined"
                                        startIcon={
                                            startingAiTest ? (
                                                <CircularProgress
                                                    size={
                                                        20
                                                    }
                                                    color="inherit"
                                                />
                                            ) : (
                                                <PsychologyOutlined />
                                            )
                                        }
                                        disabled={
                                            startingAiTest ||
                                            saving ||
                                            agents.length ===
                                            0
                                        }
                                        onClick={() =>
                                            void startAiTest()
                                        }
                                        sx={{
                                            minHeight: 52
                                        }}
                                    >
                                        {startingAiTest
                                            ? "AI Test Çağrısı Başlatılıyor..."
                                            : "AI Test Çağrısı Başlat"}
                                    </Button>
                                </Grid>
                            </>
                        )}
                    </Grid>
                </CardContent>
            </Card>

            {testCall && (
                <Card
                    sx={{
                        borderRadius: 4,
                        marginTop: 3
                    }}
                >
                    <CardContent
                        sx={{
                            padding: 3
                        }}
                    >
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
                                marginBottom: 2
                            }}
                        >
                            <Box>
                                <Box
                                    sx={{
                                        display:
                                            "flex",

                                        alignItems:
                                            "center",

                                        gap: 1
                                    }}
                                >
                                    <PsychologyOutlined
                                        color="primary"
                                    />

                                    <Typography
                                        variant="h6"
                                        sx={{
                                            fontWeight:
                                                700
                                        }}
                                    >
                                        Mock AI
                                        Görüşmesi
                                    </Typography>

                                    <Chip
                                        label="TEST"
                                        size="small"
                                        color="primary"
                                        variant="outlined"
                                    />
                                </Box>

                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                    sx={{
                                        marginTop: 1
                                    }}
                                >
                                    Arayan:{" "}
                                    {
                                        testCall.callerNumber
                                    }{" "}
                                    • Aranan:{" "}
                                    {
                                        testCall.calledNumber
                                    }
                                </Typography>

                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                >
                                    Call ID:{" "}
                                    {testCall.id}
                                </Typography>
                            </Box>

                            <Button
                                variant="outlined"
                                color="inherit"
                                onClick={
                                    closeAiTest
                                }
                                disabled={
                                    sendingMessage
                                }
                            >
                                Testi Kapat
                            </Button>
                        </Box>

                        <Divider
                            sx={{
                                marginBottom: 2
                            }}
                        />

                        {handledByHuman && (
                            <Alert severity="info" sx={{ marginBottom: 2, borderRadius: 3 }}>
                                <Typography variant="subtitle2" sx={{ fontWeight: 800 }}>
                                    Çağrı insan operatöre devredildi
                                </Typography>
                                <Typography variant="body2">
                                    AI artık bu görüşmeye cevap vermiyor.
                                    Yeni müşteri mesajları operatöre iletilecektir.
                                </Typography>
                            </Alert>
                        )}

                        <Paper
                            variant="outlined"
                            sx={{
                                minHeight: 280,
                                maxHeight: 450,
                                overflowY: "auto",
                                padding: 2,
                                marginBottom: 2,
                                borderRadius: 3
                            }}
                        >
                            {messages.length ===
                                0 ? (
                                <Box
                                    sx={{
                                        minHeight: 240,

                                        display:
                                            "flex",

                                        alignItems:
                                            "center",

                                        justifyContent:
                                            "center",

                                        textAlign:
                                            "center"
                                    }}
                                >
                                    <Box>
                                        <PsychologyOutlined
                                            sx={{
                                                fontSize:
                                                    48,

                                                marginBottom:
                                                    1
                                            }}
                                            color="disabled"
                                        />

                                        <Typography
                                            color="text.secondary"
                                        >
                                            Henüz konuşma
                                            yok.
                                        </Typography>

                                        <Typography
                                            variant="body2"
                                            color="text.secondary"
                                        >
                                            Aşağıdan müşteri
                                            mesajını
                                            gönderin.
                                        </Typography>
                                    </Box>
                                </Box>
                            ) : (
                                <Box
                                    sx={{
                                        display:
                                            "flex",

                                        flexDirection:
                                            "column",

                                        gap: 2
                                    }}
                                >
                                    {messages.map(
                                        (
                                            message
                                        ) => {
                                            const isCustomer =
                                                message.speaker ===
                                                MessageSpeaker.Customer;

                                            const isAi =
                                                message.speaker ===
                                                MessageSpeaker.AiAgent;

                                            return (
                                                <Box
                                                    key={
                                                        message.id
                                                    }
                                                    sx={{
                                                        display:
                                                            "flex",

                                                        justifyContent:
                                                            isCustomer
                                                                ? "flex-end"
                                                                : "flex-start"
                                                    }}
                                                >
                                                    <Paper
                                                        elevation={
                                                            0
                                                        }
                                                        sx={{
                                                            padding:
                                                                1.5,

                                                            maxWidth:
                                                            {
                                                                xs: "90%",
                                                                md: "70%"
                                                            },

                                                            borderRadius:
                                                                3,

                                                            bgcolor:
                                                                isCustomer
                                                                    ? "primary.main"
                                                                    : isAi
                                                                        ? "action.hover"
                                                                        : "background.default",

                                                            color:
                                                                isCustomer
                                                                    ? "primary.contrastText"
                                                                    : "text.primary",

                                                            border:
                                                                isCustomer
                                                                    ? "none"
                                                                    : "1px solid",

                                                            borderColor:
                                                                "divider"
                                                        }}
                                                    >
                                                        <Typography
                                                            variant="caption"
                                                            sx={{
                                                                display:
                                                                    "block",

                                                                fontWeight:
                                                                    700,

                                                                marginBottom:
                                                                    0.5,

                                                                opacity:
                                                                    0.8
                                                            }}
                                                        >
                                                            {getSpeakerName(
                                                                message.speaker
                                                            )}
                                                        </Typography>

                                                        <Typography
                                                            variant="body1"
                                                            sx={{
                                                                whiteSpace:
                                                                    "pre-wrap"
                                                            }}
                                                        >
                                                            {
                                                                message.content
                                                            }
                                                        </Typography>
                                                    </Paper>
                                                </Box>
                                            );
                                        }
                                    )}
                                </Box>
                            )}
                        </Paper>

                        <Box
                            sx={{
                                display: "flex",
                                gap: 1,
                                alignItems: "stretch"
                            }}
                        >
                            <TextField
                                fullWidth
                                multiline
                                maxRows={4}
                                label={handledByHuman ? "Operatöre müşteri mesajı" : "Müşteri mesajı"}
                                placeholder={handledByHuman
                                    ? "Müşterinin operatöre göndereceği mesaj..."
                                    : "Örn: Çalışma saatleriniz nedir?"}
                                value={
                                    customerMessage
                                }
                                disabled={
                                    sendingMessage
                                }
                                onChange={(
                                    event
                                ) =>
                                    setCustomerMessage(
                                        event.target
                                            .value
                                    )
                                }
                                onKeyDown={(
                                    event
                                ) => {
                                    if (
                                        event.key ===
                                        "Enter" &&
                                        !event.shiftKey
                                    ) {
                                        event.preventDefault();

                                        if (
                                            !sendingMessage &&
                                            customerMessage.trim()
                                        ) {
                                            void sendCustomerMessage();
                                        }
                                    }
                                }}
                            />

                            <Button
                                variant="contained"
                                startIcon={
                                    sendingMessage ? (
                                        <CircularProgress
                                            size={20}
                                            color="inherit"
                                        />
                                    ) : (
                                        <SendOutlined />
                                    )
                                }
                                disabled={
                                    sendingMessage ||
                                    !customerMessage.trim()
                                }
                                onClick={() =>
                                    void sendCustomerMessage()
                                }
                                sx={{
                                    minWidth: {
                                        xs: 110,
                                        md: 140
                                    }
                                }}
                            >
                                {sendingMessage
                                    ? "Bekle..."
                                    : handledByHuman
                                        ? "Operatöre Gönder"
                                        : "Gönder"}
                            </Button>
                        </Box>

                        <Typography
                            variant="caption"
                            color="text.secondary"
                            sx={{
                                display: "block",
                                marginTop: 1
                            }}
                        >
                            Enter: gönder • Shift +
                            Enter: yeni satır
                        </Typography>
                    </CardContent>
                </Card>
            )}
        </Box>
    );
}

export default CreateCallPage;