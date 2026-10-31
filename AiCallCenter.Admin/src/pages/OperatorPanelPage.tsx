/* OPERATOR_PANEL_ACTIVE_CALL_V2 */
/* eslint-disable react-hooks/set-state-in-effect */
/* eslint-disable react-hooks/exhaustive-deps */

import {
    useEffect,
    useMemo,
    useState
} from "react";

import axios from "axios";

import {
    AssignmentTurnedInOutlined,
    CallEndOutlined,
    HeadsetMicOutlined,
    PersonOutlineOutlined,
    RefreshOutlined,
    SmartToyOutlined
} from "@mui/icons-material";

import {
    Alert,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField,
    Typography
} from "@mui/material";

import {
    getCallOperatorByUserId,
    getCallOperators,
    getCurrentStoredUser,
    updateCallOperatorStatus,
    USER_ROLE,
    type CallOperator
} from "../services/callOperatorService";

import {
    acceptQueuedCall,
    completeQueuedCall,
    getOperatorQueueItems,
    type CallQueueItem
} from "../services/callQueueItemService";

import {
    getCallMessages,
    sendOperatorMessage,
    MessageSpeaker,
    type CallMessageDto
} from "../services/callService";

import {
    joinCall,
    leaveCall,
    onCallMessageCreated
} from "../services/callRealtimeService";

const OPERATOR_STATUS = {
    Offline: 1,
    Available: 2,
    Busy: 3,
    OnBreak: 4,
    AfterCallWork: 5
} as const;

type OperatorStatusValue =
    (typeof OPERATOR_STATUS)[keyof typeof OPERATOR_STATUS];

interface CompleteDialogState {
    open: boolean;
    queueItemId: string;
    summary: string;
    result: string;
}

const emptyCompleteDialog: CompleteDialogState = {
    open: false,
    queueItemId: "",
    summary: "",
    result: ""
};

function getErrorMessage(
    exception: unknown,
    fallback: string
): string {
    if (!axios.isAxiosError(exception)) {
        return exception instanceof Error
            ? exception.message
            : fallback;
    }

    const data = exception.response?.data as
        | {
            detail?: string;
            title?: string;
            message?: string;
            errors?: Record<string, string[]>;
        }
        | undefined;

    return (
        (data?.errors
            ? Object.values(data.errors)
                .flat()
                .join(" ")
            : "") ||
        data?.detail ||
        data?.message ||
        data?.title ||
        fallback
    );
}

function getOperatorStatusText(
    status: number
): string {
    switch (status) {
        case OPERATOR_STATUS.Available:
            return "Müsait";

        case OPERATOR_STATUS.Busy:
            return "Meşgul";

        case OPERATOR_STATUS.OnBreak:
            return "Molada";

        case OPERATOR_STATUS.AfterCallWork:
            return "Çağrı Sonrası";

        default:
            return "Çevrimdışı";
    }
}

function getOperatorStatusColor(
    status: number
):
    | "success"
    | "error"
    | "warning"
    | "info"
    | "default" {
    switch (status) {
        case OPERATOR_STATUS.Available:
            return "success";

        case OPERATOR_STATUS.Busy:
            return "error";

        case OPERATOR_STATUS.OnBreak:
            return "warning";

        case OPERATOR_STATUS.AfterCallWork:
            return "info";

        default:
            return "default";
    }
}

function normalizeQueueStatus(
    status: CallQueueItem["status"]
):
    | "Waiting"
    | "Assigned"
    | "Accepted"
    | "Completed"
    | "Cancelled"
    | "Expired"
    | "" {
    if (typeof status === "string") {
        switch (status) {
            case "Waiting":
            case "Assigned":
            case "Accepted":
            case "Completed":
            case "Cancelled":
            case "Expired":
                return status;

            default:
                return "";
        }
    }

    switch (status) {
        case 1:
            return "Waiting";

        case 2:
            return "Assigned";

        case 3:
            return "Accepted";

        case 4:
            return "Completed";

        case 5:
            return "Cancelled";

        case 6:
            return "Expired";

        default:
            return "";
    }
}

function getQueueStatusText(
    status: CallQueueItem["status"]
): string {
    switch (normalizeQueueStatus(status)) {
        case "Waiting":
            return "Bekliyor";

        case "Assigned":
            return "Atandı";

        case "Accepted":
            return "Görüşmede";

        case "Completed":
            return "Tamamlandı";

        case "Cancelled":
            return "İptal Edildi";

        case "Expired":
            return "Süresi Doldu";

        default:
            return "Bilinmiyor";
    }
}

function isAssignedStatus(
    status: CallQueueItem["status"]
): boolean {
    return normalizeQueueStatus(status) === "Assigned";
}

function isAcceptedStatus(
    status: CallQueueItem["status"]
): boolean {
    return normalizeQueueStatus(status) === "Accepted";
}

function formatDate(
    value?: string | null
): string {
    if (!value) {
        return "-";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return new Intl.DateTimeFormat(
        "tr-TR",
        {
            dateStyle: "short",
            timeStyle: "medium"
        }
    ).format(date);
}

function formatCallDuration(
    totalSeconds: number
): string {
    const safeSeconds = Math.max(
        0,
        Math.floor(totalSeconds)
    );

    const hours = Math.floor(
        safeSeconds / 3600
    );

    const minutes = Math.floor(
        (safeSeconds % 3600) / 60
    );

    const seconds =
        safeSeconds % 60;

    return [
        hours,
        minutes,
        seconds
    ]
        .map((value) =>
            value
                .toString()
                .padStart(2, "0")
        )
        .join(":");
}

function getSpeakerText(
    speaker: number
): string {
    switch (speaker) {
        case MessageSpeaker.Customer:
            return "Müşteri";

        case MessageSpeaker.AiAgent:
            return "Yapay Zekâ";

        case MessageSpeaker.HumanAgent:
            return "Operatör";

        case MessageSpeaker.System:
            return "Sistem";

        default:
            return "Bilinmiyor";
    }
}

function getSpeakerColor(
    speaker: number
):
    | "primary"
    | "secondary"
    | "success"
    | "info"
    | "warning"
    | "default" {
    switch (speaker) {
        case MessageSpeaker.Customer:
            return "primary";

        case MessageSpeaker.AiAgent:
            return "secondary";

        case MessageSpeaker.HumanAgent:
            return "success";

        case MessageSpeaker.System:
            return "info";

        default:
            return "default";
    }
}

function OperatorPanelPage() {
    const storedUser = useMemo(
        () => getCurrentStoredUser(),
        []
    );

    const isOperatorUser =
        storedUser.role === USER_ROLE.Operator;

    const [operators, setOperators] =
        useState<CallOperator[]>([]);

    const [
        selectedOperatorId,
        setSelectedOperatorId
    ] = useState("");

    const [
        queueItems,
        setQueueItems
    ] = useState<CallQueueItem[]>([]);

    const [
        loading,
        setLoading
    ] = useState(true);

    const [
        queueLoading,
        setQueueLoading
    ] = useState(false);

    const [
        actionLoadingId,
        setActionLoadingId
    ] = useState("");

    const [
        error,
        setError
    ] = useState("");

    const [
        success,
        setSuccess
    ] = useState("");

    const [
        completeDialog,
        setCompleteDialog
    ] = useState<CompleteDialogState>(
        emptyCompleteDialog
    );

    const [
        callMessages,
        setCallMessages
    ] = useState<CallMessageDto[]>([]);

    const [
        messagesLoading,
        setMessagesLoading
    ] = useState(false);

    const [
        activeCallSeconds,
        setActiveCallSeconds
    ] = useState(0);

    const [
        operatorMessage,
        setOperatorMessage
    ] = useState("");

    const [
        operatorMessageSending,
        setOperatorMessageSending
    ] = useState(false);

    const selectedOperator =
        useMemo(
            () =>
                operators.find(
                    (item) =>
                        item.id ===
                        selectedOperatorId
                ) ?? null,
            [
                operators,
                selectedOperatorId
            ]
        );

    const activeCall =
        useMemo(
            () =>
                queueItems.find(
                    (item) =>
                        isAcceptedStatus(
                            item.status
                        )
                ) ?? null,
            [queueItems]
        );

    const assignedCalls =
        useMemo(
            () =>
                queueItems.filter(
                    (item) =>
                        isAssignedStatus(
                            item.status
                        )
                ),
            [queueItems]
        );

    async function loadOperators(): Promise<void> {
        try {
            setError("");

            if (isOperatorUser) {
                if (!storedUser.userId) {
                    throw new Error(
                        "Giriş yapan kullanıcının kimliği bulunamadı."
                    );
                }

                const ownOperator =
                    await getCallOperatorByUserId(
                        storedUser.userId
                    );

                setOperators([
                    ownOperator
                ]);

                setSelectedOperatorId(
                    ownOperator.id
                );

                return;
            }

            const data =
                await getCallOperators();

            const sorted =
                [...data].sort(
                    (a, b) =>
                        a.displayName.localeCompare(
                            b.displayName,
                            "tr"
                        )
                );

            setOperators(sorted);

            setSelectedOperatorId(
                (current) => {
                    if (current) {
                        return current;
                    }

                    return sorted.length === 1
                        ? sorted[0].id
                        : "";
                }
            );
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    isOperatorUser
                        ? "Bu kullanıcıya bağlı operatör kaydı bulunamadı."
                        : "Operatörler yüklenemedi."
                )
            );
        } finally {
            setLoading(false);
        }
    }

    async function loadQueueItems(
        operatorId: string,
        showLoader = true
    ): Promise<void> {
        if (!operatorId) {
            setQueueItems([]);
            return;
        }

        if (showLoader) {
            setQueueLoading(true);
        }

        try {
            const data =
                await getOperatorQueueItems(
                    operatorId
                );

            setQueueItems(data);
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatör çağrıları yüklenemedi."
                )
            );
        } finally {
            if (showLoader) {
                setQueueLoading(false);
            }
        }
    }

    async function refreshPanel(
        showLoader = false
    ): Promise<void> {
        if (!selectedOperatorId) {
            return;
        }

        await Promise.all([
            loadQueueItems(
                selectedOperatorId,
                showLoader
            ),
            loadOperators()
        ]);
    }

    useEffect(() => {
        void loadOperators();
    }, []);

    useEffect(() => {
        if (!selectedOperatorId) {
            setQueueItems([]);
            return;
        }

        void loadQueueItems(
            selectedOperatorId
        );

        const intervalId =
            window.setInterval(
                () => {
                    void loadQueueItems(
                        selectedOperatorId,
                        false
                    );
                },
                10000
            );

        return () => {
            window.clearInterval(
                intervalId
            );
        };
    }, [selectedOperatorId]);

    useEffect(() => {
        if (
            !activeCall?.callId ||
            !storedUser.companyId
        ) {
            setCallMessages([]);
            return;
        }

        let isActive = true;

        const loadMessages =
            async (): Promise<void> => {
                try {
                    setMessagesLoading(
                        true
                    );

                    const messages =
                        await getCallMessages(
                            storedUser.companyId!,
                            activeCall.callId
                        );

                    if (isActive) {
                        setCallMessages(
                            [...messages].sort(
                                (a, b) =>
                                    a.sequenceNumber -
                                    b.sequenceNumber
                            )
                        );
                    }
                } catch (exception) {
                    if (isActive) {
                        setError(
                            getErrorMessage(
                                exception,
                                "Görüşme geçmişi yüklenemedi."
                            )
                        );
                    }
                } finally {
                    if (isActive) {
                        setMessagesLoading(
                            false
                        );
                    }
                }
            };

        void loadMessages();

        return () => {
            isActive = false;
        };
    }, [
        activeCall?.callId,
        storedUser.companyId
    ]);

    useEffect(() => {
        if (!activeCall?.callId) {
            return;
        }

        let disposed = false;

        const unsubscribe =
            onCallMessageCreated(
                (realtimeMessage) => {
                    if (
                        realtimeMessage.callId !==
                        activeCall.callId
                    ) {
                        return;
                    }

                    setCallMessages((current) => {
                        const exists =
                            current.some(
                                (item) =>
                                    item.id ===
                                    realtimeMessage.id
                            );

                        if (exists) {
                            return current;
                        }

                        const message: CallMessageDto = {
                            id: realtimeMessage.id,
                            callId: realtimeMessage.callId,
                            speaker: realtimeMessage.speaker,
                            content: realtimeMessage.content,
                            spokenAt: realtimeMessage.spokenAt,
                            sequenceNumber:
                                realtimeMessage.sequenceNumber,
                            confidenceScore:
                                realtimeMessage.confidenceScore,
                            audioFileUrl:
                                realtimeMessage.audioFileUrl
                        };

                        return [
                            ...current,
                            message
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
                        activeCall.callId
                    );

                    if (!disposed) {
                        console.log(
                            "Operatör çağrı SignalR grubuna katıldı:",
                            activeCall.callId
                        );
                    }
                } catch (exception) {
                    console.error(
                        "Operatör SignalR çağrı bağlantısı kurulamadı:",
                        exception
                    );
                }
            };

        void connectRealtime();

        return () => {
            disposed = true;
            unsubscribe();

            void leaveCall(
                activeCall.callId
            ).catch((exception) => {
                console.error(
                    "Operatör SignalR çağrı grubundan çıkılamadı:",
                    exception
                );
            });
        };
    }, [activeCall?.callId]);

    useEffect(() => {
        if (!activeCall?.acceptedAt) {
            setActiveCallSeconds(0);
            return;
        }

        const calculate =
            (): void => {
                const acceptedAt =
                    activeCall.acceptedAt!;

                const utcAcceptedAt =
                    acceptedAt.endsWith("Z")
                        ? acceptedAt
                        : `${acceptedAt}Z`;

                const acceptedTime =
                    new Date(utcAcceptedAt).getTime();

                if (
                    Number.isNaN(
                        acceptedTime
                    )
                ) {
                    setActiveCallSeconds(
                        0
                    );
                    return;
                }

                const seconds =
                    Math.max(
                        0,
                        Math.floor(
                            (
                                Date.now() -
                                acceptedTime
                            ) /
                            1000
                        )
                    );

                setActiveCallSeconds(
                    seconds
                );
            };

        calculate();

        const intervalId =
            window.setInterval(
                calculate,
                1000
            );

        return () => {
            window.clearInterval(
                intervalId
            );
        };
    }, [activeCall?.acceptedAt]);

    async function changeStatus(
        status: OperatorStatusValue
    ): Promise<void> {
        if (!selectedOperatorId) {
            setError(
                "Önce bir operatör seçiniz."
            );
            return;
        }

        try {
            setError("");
            setSuccess("");

            setActionLoadingId(
                `status-${status}`
            );

            const updated =
                await updateCallOperatorStatus(
                    selectedOperatorId,
                    status
                );

            setOperators(
                (current) =>
                    current.map(
                        (item) =>
                            item.id ===
                                updated.id
                                ? updated
                                : item
                    )
            );

            setSuccess(
                `Operatör durumu "${getOperatorStatusText(
                    status
                )}" olarak güncellendi.`
            );
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatör durumu güncellenemedi."
                )
            );
        } finally {
            setActionLoadingId("");
        }
    }

    async function acceptCall(
        queueItemId: string
    ): Promise<void> {
        if (!selectedOperatorId) {
            setError(
                "Önce bir operatör seçiniz."
            );
            return;
        }

        try {
            setError("");
            setSuccess("");

            setActionLoadingId(
                queueItemId
            );

            await acceptQueuedCall(
                queueItemId,
                selectedOperatorId
            );

            setSuccess(
                "Çağrı başarıyla kabul edildi."
            );

            await refreshPanel(false);
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Çağrı kabul edilemedi."
                )
            );
        } finally {
            setActionLoadingId("");
        }
    }

    function openCompleteDialog(
        queueItemId: string
    ): void {
        setCompleteDialog({
            open: true,
            queueItemId,
            summary: "",
            result: ""
        });
    }

    function closeCompleteDialog(): void {
        if (actionLoadingId) {
            return;
        }

        setCompleteDialog(
            emptyCompleteDialog
        );
    }

    async function completeCall(): Promise<void> {
        if (!selectedOperatorId) {
            setError(
                "Önce bir operatör seçiniz."
            );
            return;
        }

        if (
            !completeDialog.summary.trim()
        ) {
            setError(
                "Görüşme özeti boş olamaz."
            );
            return;
        }

        try {
            setError("");
            setSuccess("");

            setActionLoadingId(
                completeDialog.queueItemId
            );

            await completeQueuedCall(
                completeDialog.queueItemId,
                {
                    callOperatorId:
                        selectedOperatorId,

                    summary:
                        completeDialog.summary.trim(),

                    result:
                        completeDialog.result.trim() ||
                        undefined
                }
            );

            setCompleteDialog(
                emptyCompleteDialog
            );

            setCallMessages([]);

            setSuccess(
                "Çağrı başarıyla tamamlandı."
            );

            await refreshPanel(false);
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Çağrı tamamlanamadı."
                )
            );
        } finally {
            setActionLoadingId("");
        }
    }

    async function handleSendOperatorMessage(): Promise<void> {
        if (!activeCall?.callId) {
            setError("Aktif görüşme bulunamadı.");
            return;
        }

        if (!storedUser.companyId) {
            setError("Şirket bilgisi bulunamadı.");
            return;
        }

        const message = operatorMessage.trim();

        if (!message) {
            setError("Gönderilecek mesaj boş olamaz.");
            return;
        }

        if (message.length > 4000) {
            setError("Mesaj en fazla 4000 karakter olabilir.");
            return;
        }

        try {
            setError("");
            setSuccess("");
            setOperatorMessageSending(true);

            const createdMessage = await sendOperatorMessage(
                storedUser.companyId,
                activeCall.callId,
                message
            );

            setCallMessages((current) => {
                if (current.some((item) => item.id === createdMessage.id)) {
                    return current;
                }

                return [...current, createdMessage].sort(
                    (a, b) => a.sequenceNumber - b.sequenceNumber
                );
            });

            setOperatorMessage("");
            setSuccess("Mesaj müşteriye gönderildi.");
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatör mesajı gönderilemedi."
                )
            );
        } finally {
            setOperatorMessageSending(false);
        }
    }

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
                        variant="h4"
                        sx={{
                            fontWeight: 800
                        }}
                    >
                        Operatör Paneli
                    </Typography>

                    <Typography
                        color="text.secondary"
                    >
                        {isOperatorUser
                            ? "Kendi durumunuzu ve çağrılarınızı yönetin."
                            : "Operatör durumunu ve atanmış çağrıları yönetin."}
                    </Typography>
                </Box>

                <Button
                    variant="outlined"
                    startIcon={
                        <RefreshOutlined />
                    }
                    disabled={
                        queueLoading ||
                        !selectedOperatorId
                    }
                    onClick={() =>
                        void refreshPanel(
                            true
                        )
                    }
                >
                    Yenile
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
                    borderRadius: 4,
                    marginBottom: 3
                }}
            >
                <CardContent>
                    <Box
                        sx={{
                            display: "grid",
                            gridTemplateColumns: {
                                xs: "1fr",
                                md: isOperatorUser
                                    ? "1fr"
                                    : "minmax(260px, 5fr) 7fr"
                            },
                            gap: 2,
                            alignItems:
                                "center"
                        }}
                    >
                        {isOperatorUser ? (
                            selectedOperator && (
                                <Typography
                                    variant="h6"
                                    sx={{
                                        fontWeight: 700
                                    }}
                                >
                                    {
                                        selectedOperator.displayName
                                    }

                                    {selectedOperator.extensionNumber
                                        ? ` (${selectedOperator.extensionNumber})`
                                        : ""}
                                </Typography>
                            )
                        ) : (
                            <FormControl
                                fullWidth
                            >
                                <InputLabel>
                                    Operatör
                                </InputLabel>

                                <Select
                                    label="Operatör"
                                    value={
                                        selectedOperatorId
                                    }
                                    onChange={(
                                        event
                                    ) =>
                                        setSelectedOperatorId(
                                            event
                                                .target
                                                .value
                                        )
                                    }
                                >
                                    {operators.map(
                                        (
                                            item
                                        ) => (
                                            <MenuItem
                                                key={
                                                    item.id
                                                }
                                                value={
                                                    item.id
                                                }
                                            >
                                                {
                                                    item.displayName
                                                }

                                                {item.extensionNumber
                                                    ? ` (${item.extensionNumber})`
                                                    : ""}
                                            </MenuItem>
                                        )
                                    )}
                                </Select>
                            </FormControl>
                        )}

                        <Box
                            sx={{
                                display: "flex",
                                flexWrap:
                                    "wrap",
                                gap: 1
                            }}
                        >
                            <Button
                                variant="contained"
                                color="success"
                                disabled={
                                    !selectedOperatorId ||
                                    !!actionLoadingId ||
                                    !!activeCall
                                }
                                onClick={() =>
                                    void changeStatus(
                                        OPERATOR_STATUS.Available
                                    )
                                }
                            >
                                Müsait
                            </Button>

                            <Button
                                variant="contained"
                                color="warning"
                                disabled={
                                    !selectedOperatorId ||
                                    !!actionLoadingId ||
                                    !!activeCall
                                }
                                onClick={() =>
                                    void changeStatus(
                                        OPERATOR_STATUS.OnBreak
                                    )
                                }
                            >
                                Molada
                            </Button>

                            <Button
                                variant="contained"
                                color="info"
                                disabled={
                                    !selectedOperatorId ||
                                    !!actionLoadingId ||
                                    !!activeCall
                                }
                                onClick={() =>
                                    void changeStatus(
                                        OPERATOR_STATUS.AfterCallWork
                                    )
                                }
                            >
                                Çağrı Sonrası
                            </Button>

                            <Button
                                variant="outlined"
                                color="inherit"
                                disabled={
                                    !selectedOperatorId ||
                                    !!actionLoadingId ||
                                    !!activeCall
                                }
                                onClick={() =>
                                    void changeStatus(
                                        OPERATOR_STATUS.Offline
                                    )
                                }
                            >
                                Çevrimdışı
                            </Button>
                        </Box>
                    </Box>

                    {selectedOperator && (
                        <Box
                            sx={{
                                display: "flex",
                                flexWrap:
                                    "wrap",
                                gap: 1,
                                marginTop: 2
                            }}
                        >
                            <Chip
                                label={getOperatorStatusText(
                                    selectedOperator.status
                                )}
                                color={getOperatorStatusColor(
                                    selectedOperator.status
                                )}
                            />

                            <Chip
                                variant="outlined"
                                label={`Aktif çağrı: ${selectedOperator.activeCallCount}/${selectedOperator.maxConcurrentCalls}`}
                            />

                            <Chip
                                variant="outlined"
                                label={
                                    selectedOperator.canReceiveCalls
                                        ? "Çağrı alabilir"
                                        : "Çağrı alamaz"
                                }
                            />
                        </Box>
                    )}
                </CardContent>
            </Card>

            {activeCall && (
                <Card
                    sx={{
                        borderRadius: 4,
                        marginBottom: 3
                    }}
                >
                    <CardContent>
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
                                gap: 2
                            }}
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems:
                                        "center",
                                    gap: 1.5
                                }}
                            >
                                <HeadsetMicOutlined
                                    color="success"
                                    fontSize="large"
                                />

                                <Box>
                                    <Typography
                                        variant="h5"
                                        sx={{
                                            fontWeight: 800
                                        }}
                                    >
                                        Aktif Görüşme
                                    </Typography>

                                    <Typography
                                        color="text.secondary"
                                    >
                                        Operatör görüşmesi devam ediyor
                                    </Typography>
                                </Box>
                            </Box>

                            <Chip
                                color="success"
                                label={`Görüşmede • ${formatCallDuration(
                                    activeCallSeconds
                                )}`}
                                sx={{
                                    fontWeight: 700,
                                    fontSize:
                                        "0.95rem"
                                }}
                            />
                        </Box>

                        <Divider
                            sx={{
                                marginY: 3
                            }}
                        />

                        <Box
                            sx={{
                                display: "grid",
                                gridTemplateColumns: {
                                    xs: "1fr",
                                    md: "repeat(2, minmax(0, 1fr))"
                                },
                                gap: 2
                            }}
                        >
                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Arayan
                                </Typography>

                                <Typography
                                    variant="h6"
                                    sx={{
                                        fontWeight: 700
                                    }}
                                >
                                    {
                                        activeCall.callerNumber
                                    }
                                </Typography>
                            </Box>

                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Aranan
                                </Typography>

                                <Typography
                                    variant="h6"
                                    sx={{
                                        fontWeight: 700
                                    }}
                                >
                                    {
                                        activeCall.calledNumber
                                    }
                                </Typography>
                            </Box>

                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Kuyruk
                                </Typography>

                                <Typography
                                    sx={{
                                        fontWeight: 600
                                    }}
                                >
                                    {
                                        activeCall.queueName
                                    }
                                </Typography>
                            </Box>

                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Aktarım Nedeni
                                </Typography>

                                <Typography
                                    sx={{
                                        fontWeight: 600
                                    }}
                                >
                                    {activeCall.transferReason ||
                                        "Belirtilmedi"}
                                </Typography>
                            </Box>

                            {activeCall.aiHandoffSummary && (
                                <Box
                                    sx={{
                                        gridColumn: {
                                            xs: "1",
                                            md: "1 / -1"
                                        }
                                    }}
                                >
                                    <Alert
                                        severity="info"
                                        icon={<SmartToyOutlined />}
                                        sx={{
                                            alignItems: "flex-start",
                                            borderRadius: 3
                                        }}
                                    >
                                        <Typography
                                            variant="subtitle1"
                                            sx={{
                                                fontWeight: 800,
                                                marginBottom: 1
                                            }}
                                        >
                                            AI Devir Özeti
                                        </Typography>

                                        <Typography
                                            variant="body2"
                                            sx={{
                                                whiteSpace: "pre-wrap",
                                                lineHeight: 1.7
                                            }}
                                        >
                                            {activeCall.aiHandoffSummary}
                                        </Typography>
                                    </Alert>
                                </Box>
                            )}

                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Atanma
                                </Typography>

                                <Typography>
                                    {formatDate(
                                        activeCall.assignedAt
                                    )}
                                </Typography>
                            </Box>

                            <Box>
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Kabul
                                </Typography>

                                <Typography>
                                    {formatDate(
                                        activeCall.acceptedAt
                                    )}
                                </Typography>
                            </Box>
                        </Box>

                        <Divider
                            sx={{
                                marginY: 3
                            }}
                        />

                        <Box
                            sx={{
                                display: "flex",
                                alignItems:
                                    "center",
                                justifyContent:
                                    "space-between",
                                gap: 2,
                                marginBottom: 2
                            }}
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems:
                                        "center",
                                    gap: 1
                                }}
                            >
                                <SmartToyOutlined
                                    color="primary"
                                />

                                <Typography
                                    variant="h6"
                                    sx={{
                                        fontWeight: 700
                                    }}
                                >
                                    Görüşme Geçmişi
                                </Typography>
                            </Box>

                            {messagesLoading && (
                                <CircularProgress
                                    size={20}
                                />
                            )}
                        </Box>

                        <Box
                            sx={{
                                border: 1,
                                borderColor:
                                    "divider",
                                borderRadius: 3,
                                padding: 2,
                                minHeight: 180,
                                maxHeight: 420,
                                overflowY:
                                    "auto",
                                backgroundColor:
                                    "background.default"
                            }}
                        >
                            {callMessages.length ===
                                0 ? (
                                <Box
                                    sx={{
                                        minHeight:
                                            140,
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
                                    <Typography
                                        color="text.secondary"
                                    >
                                        Bu çağrı için henüz konuşma kaydı bulunmuyor.
                                    </Typography>
                                </Box>
                            ) : (
                                <Box
                                    sx={{
                                        display:
                                            "grid",
                                        gap: 2
                                    }}
                                >
                                    {callMessages.map(
                                        (
                                            message
                                        ) => (
                                            <Box
                                                key={
                                                    message.id
                                                }
                                                sx={{
                                                    border: 1,
                                                    borderColor:
                                                        "divider",
                                                    borderRadius: 2,
                                                    padding: 2,
                                                    backgroundColor:
                                                        "background.paper"
                                                }}
                                            >
                                                <Box
                                                    sx={{
                                                        display:
                                                            "flex",
                                                        justifyContent:
                                                            "space-between",
                                                        alignItems:
                                                            "center",
                                                        gap: 1,
                                                        marginBottom: 1
                                                    }}
                                                >
                                                    <Box
                                                        sx={{
                                                            display:
                                                                "flex",
                                                            alignItems:
                                                                "center",
                                                            gap: 1
                                                        }}
                                                    >
                                                        {message.speaker ===
                                                            MessageSpeaker.Customer ? (
                                                            <PersonOutlineOutlined
                                                                fontSize="small"
                                                                color="primary"
                                                            />
                                                        ) : (
                                                            <SmartToyOutlined
                                                                fontSize="small"
                                                            />
                                                        )}

                                                        <Chip
                                                            size="small"
                                                            color={getSpeakerColor(
                                                                message.speaker
                                                            )}
                                                            label={getSpeakerText(
                                                                message.speaker
                                                            )}
                                                        />
                                                    </Box>

                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                    >
                                                        {formatDate(
                                                            message.spokenAt
                                                        )}
                                                    </Typography>
                                                </Box>

                                                <Typography
                                                    sx={{
                                                        whiteSpace:
                                                            "pre-wrap"
                                                    }}
                                                >
                                                    {
                                                        message.content
                                                    }
                                                </Typography>
                                            </Box>
                                        )
                                    )}
                                </Box>
                            )}
                        </Box>

                        <Box
                            sx={{
                                marginTop: 3,
                                padding: 2,
                                border: 1,
                                borderColor: "divider",
                                borderRadius: 3,
                                backgroundColor: "background.paper"
                            }}
                        >
                            <Typography
                                variant="subtitle1"
                                sx={{
                                    fontWeight: 700,
                                    marginBottom: 1
                                }}
                            >
                                Müşteriye Mesaj Gönder
                            </Typography>

                            <Typography
                                variant="body2"
                                color="text.secondary"
                                sx={{
                                    marginBottom: 2
                                }}
                            >
                                Bu mesaj görüşme geçmişine operatör mesajı olarak kaydedilecektir.
                            </Typography>

                            <TextField
                                fullWidth
                                multiline
                                minRows={3}
                                maxRows={7}
                                label="Mesaj"
                                placeholder="Müşteriye göndermek istediğiniz mesajı yazınız..."
                                value={operatorMessage}
                                disabled={operatorMessageSending}
                                slotProps={{
                                    htmlInput: {
                                        maxLength: 4000
                                    }
                                }}
                                onChange={(event) =>
                                    setOperatorMessage(
                                        event.target.value
                                    )
                                }
                                onKeyDown={(event) => {
                                    if (
                                        event.key === "Enter" &&
                                        event.ctrlKey &&
                                        !operatorMessageSending &&
                                        operatorMessage.trim()
                                    ) {
                                        event.preventDefault();
                                        void handleSendOperatorMessage();
                                    }
                                }}
                            />

                            <Box
                                sx={{
                                    display: "flex",
                                    justifyContent: "space-between",
                                    alignItems: "center",
                                    gap: 2,
                                    marginTop: 1.5
                                }}
                            >
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                >
                                    {operatorMessage.length}/4000 • Ctrl + Enter ile gönderebilirsiniz
                                </Typography>

                                <Button
                                    variant="contained"
                                    disabled={
                                        operatorMessageSending ||
                                        !operatorMessage.trim()
                                    }
                                    onClick={() =>
                                        void handleSendOperatorMessage()
                                    }
                                >
                                    {operatorMessageSending ? (
                                        <>
                                            <CircularProgress
                                                size={18}
                                                color="inherit"
                                                sx={{
                                                    marginRight: 1
                                                }}
                                            />
                                            Gönderiliyor...
                                        </>
                                    ) : (
                                        "Mesajı Gönder"
                                    )}
                                </Button>
                            </Box>
                        </Box>

                        <Box
                            sx={{
                                display: "flex",
                                justifyContent:
                                    "flex-end",
                                marginTop: 3
                            }}
                        >
                            <Button
                                variant="contained"
                                color="error"
                                size="large"
                                startIcon={
                                    <CallEndOutlined />
                                }
                                disabled={
                                    actionLoadingId ===
                                    activeCall.id
                                }
                                onClick={() =>
                                    openCompleteDialog(
                                        activeCall.id
                                    )
                                }
                            >
                                Görüşmeyi Tamamla
                            </Button>
                        </Box>
                    </CardContent>
                </Card>
            )}

            <Box
                sx={{
                    display: "flex",
                    alignItems: "center",
                    gap: 1,
                    marginBottom: 2
                }}
            >
                <AssignmentTurnedInOutlined
                    color="primary"
                />

                <Typography
                    variant="h6"
                    sx={{
                        fontWeight: 700
                    }}
                >
                    Bekleyen Atanmış Çağrılar
                </Typography>

                {assignedCalls.length >
                    0 && (
                        <Chip
                            size="small"
                            color="warning"
                            label={
                                assignedCalls.length
                            }
                        />
                    )}
            </Box>

            {queueLoading ? (
                <Box
                    sx={{
                        padding: 6,
                        textAlign: "center"
                    }}
                >
                    <CircularProgress />
                </Box>
            ) : !selectedOperatorId ? (
                <Alert severity="info">
                    Operatör kaydı seçilemedi.
                </Alert>
            ) : assignedCalls.length ===
                0 ? (
                <Alert severity="info">
                    Bu operatöre kabul bekleyen atanmış çağrı bulunmuyor.
                </Alert>
            ) : (
                <Box
                    sx={{
                        display: "grid",
                        gridTemplateColumns: {
                            xs: "1fr",
                            lg: "repeat(2, minmax(0, 1fr))"
                        },
                        gap: 2
                    }}
                >
                    {assignedCalls.map(
                        (item) => (
                            <Card
                                key={
                                    item.id
                                }
                                sx={{
                                    borderRadius: 3,
                                    height: "100%"
                                }}
                            >
                                <CardContent>
                                    <Box
                                        sx={{
                                            display:
                                                "flex",
                                            justifyContent:
                                                "space-between",
                                            alignItems:
                                                "flex-start",
                                            gap: 2
                                        }}
                                    >
                                        <Box>
                                            <Typography
                                                variant="h6"
                                                sx={{
                                                    fontWeight: 700
                                                }}
                                            >
                                                {
                                                    item.callerNumber
                                                }
                                            </Typography>

                                            <Typography
                                                color="text.secondary"
                                            >
                                                Aranan:{" "}
                                                {
                                                    item.calledNumber
                                                }
                                            </Typography>
                                        </Box>

                                        <Chip
                                            label={getQueueStatusText(
                                                item.status
                                            )}
                                            color="warning"
                                        />
                                    </Box>

                                    <Box
                                        sx={{
                                            display:
                                                "grid",
                                            gap: 0.75,
                                            marginTop: 2
                                        }}
                                    >
                                        <Typography
                                            variant="body2"
                                        >
                                            <strong>
                                                Kuyruk:
                                            </strong>{" "}
                                            {
                                                item.queueName
                                            }
                                        </Typography>

                                        <Typography
                                            variant="body2"
                                        >
                                            <strong>
                                                Öncelik:
                                            </strong>{" "}
                                            {
                                                item.priority
                                            }
                                        </Typography>

                                        <Typography
                                            variant="body2"
                                        >
                                            <strong>
                                                Aktarım nedeni:
                                            </strong>{" "}
                                            {item.transferReason ||
                                                "-"}
                                        </Typography>

                                        {item.aiHandoffSummary && (
                                            <Alert
                                                severity="info"
                                                icon={<SmartToyOutlined />}
                                                sx={{
                                                    marginTop: 1.5,
                                                    borderRadius: 2,
                                                    alignItems: "flex-start"
                                                }}
                                            >
                                                <Typography
                                                    variant="subtitle2"
                                                    sx={{
                                                        fontWeight: 800,
                                                        marginBottom: 0.75
                                                    }}
                                                >
                                                    AI Devir Özeti
                                                </Typography>

                                                <Typography
                                                    variant="body2"
                                                    sx={{
                                                        whiteSpace: "pre-wrap",
                                                        lineHeight: 1.6
                                                    }}
                                                >
                                                    {item.aiHandoffSummary}
                                                </Typography>
                                            </Alert>
                                        )}

                                        <Typography
                                            variant="body2"
                                        >
                                            <strong>
                                                Atanma:
                                            </strong>{" "}
                                            {formatDate(
                                                item.assignedAt
                                            )}
                                        </Typography>
                                    </Box>

                                    <Box
                                        sx={{
                                            marginTop: 3
                                        }}
                                    >
                                        <Button
                                            fullWidth
                                            variant="contained"
                                            color="success"
                                            startIcon={
                                                <HeadsetMicOutlined />
                                            }
                                            disabled={
                                                actionLoadingId ===
                                                item.id ||
                                                !!activeCall
                                            }
                                            onClick={() =>
                                                void acceptCall(
                                                    item.id
                                                )
                                            }
                                        >
                                            {actionLoadingId ===
                                                item.id ? (
                                                <CircularProgress
                                                    size={
                                                        20
                                                    }
                                                    color="inherit"
                                                />
                                            ) : (
                                                "Kabul Et"
                                            )}
                                        </Button>
                                    </Box>
                                </CardContent>
                            </Card>
                        )
                    )}
                </Box>
            )}

            <Dialog
                open={
                    completeDialog.open
                }
                onClose={
                    closeCompleteDialog
                }
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle>
                    Görüşmeyi Tamamla
                </DialogTitle>

                <DialogContent>
                    <Box
                        sx={{
                            display: "grid",
                            gap: 2,
                            paddingTop: 1
                        }}
                    >
                        <Alert severity="info">
                            Görüşme tamamlandığında çağrı kapatılacak ve görüşme özeti kaydedilecektir.
                        </Alert>

                        <TextField
                            fullWidth
                            required
                            multiline
                            minRows={4}
                            label="Görüşme Özeti"
                            placeholder="Müşterinin talebini ve yapılan işlemi kısaca yazınız..."
                            value={
                                completeDialog.summary
                            }
                            onChange={(
                                event
                            ) =>
                                setCompleteDialog(
                                    (
                                        current
                                    ) => ({
                                        ...current,
                                        summary:
                                            event
                                                .target
                                                .value
                                    })
                                )
                            }
                        />

                        <TextField
                            fullWidth
                            multiline
                            minRows={2}
                            label="Görüşme Sonucu"
                            placeholder="Örn: Çözüldü, bilgi verildi, geri aranacak..."
                            value={
                                completeDialog.result
                            }
                            onChange={(
                                event
                            ) =>
                                setCompleteDialog(
                                    (
                                        current
                                    ) => ({
                                        ...current,
                                        result:
                                            event
                                                .target
                                                .value
                                    })
                                )
                            }
                        />
                    </Box>
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={
                            closeCompleteDialog
                        }
                        disabled={
                            Boolean(
                                actionLoadingId
                            )
                        }
                    >
                        Vazgeç
                    </Button>

                    <Button
                        variant="contained"
                        color="error"
                        disabled={
                            Boolean(
                                actionLoadingId
                            )
                        }
                        onClick={() =>
                            void completeCall()
                        }
                    >
                        {actionLoadingId
                            ? "Tamamlanıyor..."
                            : "Görüşmeyi Tamamla"}
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
}

export default OperatorPanelPage;