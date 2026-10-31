import {
    useEffect,
    useMemo,
    useState
} from "react";

import axios from "axios";

import {
    Autorenew,
    CallEndOutlined,
    CheckCircleOutlined,
    HeadsetMicOutlined,
    PhoneCallbackOutlined,
    ScheduleOutlined,
    Search
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
    FormControl,
    Grid,
    InputAdornment,
    InputLabel,
    MenuItem,
    Select,
    Snackbar,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TextField,
    Tooltip,
    Typography
} from "@mui/material";

import {
    acceptQueuedCall,
    completeQueuedCall,
    getCallQueueItems,
    routeWaitingCall,
    type CallQueueItem,
    type CallQueueStatus
} from "../services/callQueueItemService";

type StatusFilter =
    | ""
    | "Waiting"
    | "Assigned"
    | "Accepted"
    | "Completed"
    | "Cancelled"
    | "Expired";

interface CompleteFormState {
    summary: string;
    result: string;
}

const initialCompleteForm: CompleteFormState = {
    summary: "",
    result: "Başarılı"
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
        ? Object.values(data.errors).flat().join(" ")
        : "";

    return (
        validationMessage ||
        data?.detail ||
        data?.message ||
        data?.title ||
        fallbackMessage
    );
}

function normalizeStatus(
    status: CallQueueStatus
): StatusFilter {
    if (typeof status === "string") {
        return status as StatusFilter;
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

function getStatusLabel(
    status: CallQueueStatus
): string {
    switch (normalizeStatus(status)) {
        case "Waiting":
            return "Bekliyor";
        case "Assigned":
            return "Atandı";
        case "Accepted":
            return "Kabul Edildi";
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

function getStatusColor(
    status: CallQueueStatus
):
    | "default"
    | "primary"
    | "secondary"
    | "error"
    | "info"
    | "success"
    | "warning" {
    switch (normalizeStatus(status)) {
        case "Waiting":
            return "warning";
        case "Assigned":
            return "info";
        case "Accepted":
            return "primary";
        case "Completed":
            return "success";
        case "Cancelled":
            return "default";
        case "Expired":
            return "error";
        default:
            return "default";
    }
}

function formatDate(
    value?: string | null
): string {
    if (!value) {
        return "—";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "—";
    }

    return new Intl.DateTimeFormat("tr-TR", {
        dateStyle: "short",
        timeStyle: "medium"
    }).format(date);
}

function formatDuration(
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

    const seconds = safeSeconds % 60;

    if (hours > 0) {
        return `${hours} sa ${minutes} dk ${seconds} sn`;
    }

    if (minutes > 0) {
        return `${minutes} dk ${seconds} sn`;
    }

    return `${seconds} sn`;
}

function LiveCallsPage() {
    const [items, setItems] =
        useState<CallQueueItem[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [refreshing, setRefreshing] =
        useState(false);

    const [processingId, setProcessingId] =
        useState("");

    const [error, setError] =
        useState("");

    const [success, setSuccess] =
        useState("");

    const [search, setSearch] =
        useState("");

    const [statusFilter, setStatusFilter] =
        useState<StatusFilter>("");

    const [completeOpen, setCompleteOpen] =
        useState(false);

    const [selectedItem, setSelectedItem] =
        useState<CallQueueItem | null>(null);

    const [completeForm, setCompleteForm] =
        useState<CompleteFormState>(
            initialCompleteForm
        );

    const fetchItems = async (
        showRefreshIndicator = false
    ): Promise<void> => {
        if (showRefreshIndicator) {
            setRefreshing(true);
        }

        try {
            setError("");

            const data =
                await getCallQueueItems(
                    statusFilter || undefined
                );

            setItems(data);
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Canlı çağrılar yüklenemedi."
                )
            );
        } finally {
            setLoading(false);
            setRefreshing(false);
        }
    };

    useEffect(() => {
        let isActive = true;

        const run = async (): Promise<void> => {
            try {
                setError("");

                const data =
                    await getCallQueueItems(
                        statusFilter || undefined
                    );

                if (isActive) {
                    setItems(data);
                }
            } catch (exception) {
                if (isActive) {
                    setError(
                        getErrorMessage(
                            exception,
                            "Canlı çağrılar yüklenemedi."
                        )
                    );
                }
            } finally {
                if (isActive) {
                    setLoading(false);
                }
            }
        };

        void run();

        const intervalId =
            window.setInterval(() => {
                void run();
            }, 10000);

        return () => {
            isActive = false;
            window.clearInterval(intervalId);
        };
    }, [statusFilter]);

    const filteredItems = useMemo(() => {
        const value = search
            .trim()
            .toLocaleLowerCase("tr-TR");

        if (!value) {
            return items;
        }

        return items.filter((item) => {
            return (
                item.queueName
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                item.callerNumber
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                item.calledNumber
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                (item.assignedOperatorName ?? "")
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                (item.transferReason ?? "")
                    .toLocaleLowerCase("tr-TR")
                    .includes(value)
            );
        });
    }, [items, search]);

    const metrics = useMemo(() => {
        return {
            waiting: items.filter(
                (item) =>
                    normalizeStatus(item.status) ===
                    "Waiting"
            ).length,
            assigned: items.filter(
                (item) =>
                    normalizeStatus(item.status) ===
                    "Assigned"
            ).length,
            accepted: items.filter(
                (item) =>
                    normalizeStatus(item.status) ===
                    "Accepted"
            ).length,
            completed: items.filter(
                (item) =>
                    normalizeStatus(item.status) ===
                    "Completed"
            ).length
        };
    }, [items]);

    const routeCall = async (
        item: CallQueueItem
    ): Promise<void> => {
        setProcessingId(item.id);
        setError("");

        try {
            await routeWaitingCall(item.id);

            setSuccess(
                "Çağrı müsait operatöre yönlendirildi."
            );

            await fetchItems();
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Çağrı yönlendirilemedi."
                )
            );
        } finally {
            setProcessingId("");
        }
    };

    const acceptCall = async (
        item: CallQueueItem
    ): Promise<void> => {
        if (!item.assignedOperatorId) {
            setError(
                "Çağrıya atanmış operatör bulunamadı."
            );
            return;
        }

        setProcessingId(item.id);
        setError("");

        try {
            await acceptQueuedCall(
                item.id,
                item.assignedOperatorId
            );

            setSuccess(
                "Çağrı operatör tarafından kabul edildi."
            );

            await fetchItems();
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Çağrı kabul edilemedi."
                )
            );
        } finally {
            setProcessingId("");
        }
    };

    const openCompleteDialog = (
        item: CallQueueItem
    ): void => {
        if (!item.assignedOperatorId) {
            setError(
                "Çağrıya atanmış operatör bulunamadı."
            );
            return;
        }

        setSelectedItem(item);
        setCompleteForm(initialCompleteForm);
        setCompleteOpen(true);
        setError("");
    };

    const closeCompleteDialog = (): void => {
        if (processingId) {
            return;
        }

        setCompleteOpen(false);
        setSelectedItem(null);
        setCompleteForm(initialCompleteForm);
    };

    const completeCall = async (): Promise<void> => {
        if (
            !selectedItem ||
            !selectedItem.assignedOperatorId
        ) {
            setError(
                "Tamamlanacak çağrı veya operatör bulunamadı."
            );
            return;
        }

        setProcessingId(selectedItem.id);
        setError("");

        try {
            await completeQueuedCall(
                selectedItem.id,
                {
                    callOperatorId:
                        selectedItem.assignedOperatorId,
                    summary:
                        completeForm.summary.trim() ||
                        undefined,
                    result:
                        completeForm.result.trim() ||
                        undefined
                }
            );

            setSuccess("Çağrı tamamlandı.");
            setCompleteOpen(false);
            setSelectedItem(null);
            setCompleteForm(initialCompleteForm);

            await fetchItems();
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Çağrı tamamlanamadı."
                )
            );
        } finally {
            setProcessingId("");
        }
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
        <Box sx={{ padding: { xs: 2, md: 3 } }}>
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
                        sx={{ fontWeight: 800 }}
                    >
                        Canlı Çağrılar
                    </Typography>

                    <Typography color="text.secondary">
                        Bekleyen, yönlendirilen ve aktif
                        çağrıları takip edin.
                    </Typography>
                </Box>

                <Button
                    variant="outlined"
                    startIcon={
                        refreshing
                            ? (
                                <CircularProgress
                                    size={18}
                                />
                            )
                            : (
                                <Autorenew />
                            )
                    }
                    disabled={refreshing}
                    onClick={() =>
                        void fetchItems(true)
                    }
                >
                    Yenile
                </Button>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    onClose={() => setError("")}
                    sx={{ marginBottom: 2 }}
                >
                    {error}
                </Alert>
            )}

            <Grid
                container
                spacing={2}
                sx={{ marginBottom: 3 }}
            >
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <Card sx={{ borderRadius: 3 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <ScheduleOutlined
                                    color="warning"
                                />
                                <Box>
                                    <Typography
                                        color="text.secondary"
                                        variant="body2"
                                    >
                                        Bekleyen
                                    </Typography>
                                    <Typography
                                        variant="h4"
                                        sx={{ fontWeight: 800 }}
                                    >
                                        {metrics.waiting}
                                    </Typography>
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <Card sx={{ borderRadius: 3 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <PhoneCallbackOutlined
                                    color="info"
                                />
                                <Box>
                                    <Typography
                                        color="text.secondary"
                                        variant="body2"
                                    >
                                        Atanmış
                                    </Typography>
                                    <Typography
                                        variant="h4"
                                        sx={{ fontWeight: 800 }}
                                    >
                                        {metrics.assigned}
                                    </Typography>
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <Card sx={{ borderRadius: 3 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <HeadsetMicOutlined
                                    color="primary"
                                />
                                <Box>
                                    <Typography
                                        color="text.secondary"
                                        variant="body2"
                                    >
                                        Görüşmede
                                    </Typography>
                                    <Typography
                                        variant="h4"
                                        sx={{ fontWeight: 800 }}
                                    >
                                        {metrics.accepted}
                                    </Typography>
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <Card sx={{ borderRadius: 3 }}>
                        <CardContent>
                            <Box
                                sx={{
                                    display: "flex",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <CheckCircleOutlined
                                    color="success"
                                />
                                <Box>
                                    <Typography
                                        color="text.secondary"
                                        variant="body2"
                                    >
                                        Tamamlanan
                                    </Typography>
                                    <Typography
                                        variant="h4"
                                        sx={{ fontWeight: 800 }}
                                    >
                                        {metrics.completed}
                                    </Typography>
                                </Box>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            <Card sx={{ borderRadius: 4 }}>
                <CardContent>
                    <Grid container spacing={2}>
                        <Grid size={{ xs: 12, md: 8 }}>
                            <TextField
                                fullWidth
                                value={search}
                                onChange={(event) =>
                                    setSearch(
                                        event.target.value
                                    )
                                }
                                placeholder="Kuyruk, numara veya operatör ara..."
                                slotProps={{
                                    input: {
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Search />
                                            </InputAdornment>
                                        )
                                    }
                                }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12, md: 4 }}>
                            <FormControl fullWidth>
                                <InputLabel>
                                    Durum
                                </InputLabel>

                                <Select
                                    label="Durum"
                                    value={statusFilter}
                                    onChange={(event) =>
                                        setStatusFilter(
                                            event.target
                                                .value as StatusFilter
                                        )
                                    }
                                >
                                    <MenuItem value="">
                                        Tümü
                                    </MenuItem>
                                    <MenuItem value="Waiting">
                                        Bekliyor
                                    </MenuItem>
                                    <MenuItem value="Assigned">
                                        Atandı
                                    </MenuItem>
                                    <MenuItem value="Accepted">
                                        Kabul Edildi
                                    </MenuItem>
                                    <MenuItem value="Completed">
                                        Tamamlandı
                                    </MenuItem>
                                    <MenuItem value="Cancelled">
                                        İptal Edildi
                                    </MenuItem>
                                    <MenuItem value="Expired">
                                        Süresi Doldu
                                    </MenuItem>
                                </Select>
                            </FormControl>
                        </Grid>
                    </Grid>
                </CardContent>

                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>Kuyruk</TableCell>
                                <TableCell>Arayan</TableCell>
                                <TableCell>Aranan</TableCell>
                                <TableCell>Operatör</TableCell>
                                <TableCell>Öncelik</TableCell>
                                <TableCell>Bekleme</TableCell>
                                <TableCell>Durum</TableCell>
                                <TableCell>Oluşturulma</TableCell>
                                <TableCell align="right">
                                    İşlem
                                </TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {filteredItems.map((item) => {
                                const status =
                                    normalizeStatus(
                                        item.status
                                    );

                                const isProcessing =
                                    processingId ===
                                    item.id;

                                return (
                                    <TableRow
                                        key={item.id}
                                        hover
                                    >
                                        <TableCell>
                                            <Typography sx={{ fontWeight: 700 }}>
                                                {item.queueName}
                                            </Typography>

                                            <Typography
                                                variant="caption"
                                                color="text.secondary"
                                            >
                                                {item.transferReason ||
                                                    "Aktarım nedeni yok"}
                                            </Typography>
                                        </TableCell>

                                        <TableCell>
                                            {item.callerNumber}
                                        </TableCell>

                                        <TableCell>
                                            {item.calledNumber}
                                        </TableCell>

                                        <TableCell>
                                            <Typography sx={{ fontWeight: 600 }}>
                                                {item.assignedOperatorName ||
                                                    "Atanmadı"}
                                            </Typography>

                                            <Typography
                                                variant="caption"
                                                color="text.secondary"
                                            >
                                                {item.assignedOperatorExtension
                                                    ? `Dahili: ${item.assignedOperatorExtension}`
                                                    : ""}
                                            </Typography>
                                        </TableCell>

                                        <TableCell>
                                            <Chip
                                                size="small"
                                                label={
                                                    item.priority
                                                }
                                                variant="outlined"
                                            />
                                        </TableCell>

                                        <TableCell>
                                            <Tooltip
                                                title={`Toplam ${item.waitingSeconds} saniye`}
                                            >
                                                <Typography>
                                                    {formatDuration(
                                                        item.waitingSeconds
                                                    )}
                                                </Typography>
                                            </Tooltip>
                                        </TableCell>

                                        <TableCell>
                                            <Chip
                                                size="small"
                                                color={getStatusColor(
                                                    item.status
                                                )}
                                                label={getStatusLabel(
                                                    item.status
                                                )}
                                            />
                                        </TableCell>

                                        <TableCell>
                                            {formatDate(
                                                item.waitingSince
                                            )}
                                        </TableCell>

                                        <TableCell align="right">
                                            {status ===
                                                "Waiting" && (
                                                <Button
                                                    size="small"
                                                    variant="contained"
                                                    startIcon={
                                                        isProcessing
                                                            ? (
                                                                <CircularProgress
                                                                    size={16}
                                                                    color="inherit"
                                                                />
                                                            )
                                                            : (
                                                                <PhoneCallbackOutlined />
                                                            )
                                                    }
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        void routeCall(
                                                            item
                                                        )
                                                    }
                                                >
                                                    Yönlendir
                                                </Button>
                                            )}

                                            {status ===
                                                "Assigned" && (
                                                <Button
                                                    size="small"
                                                    variant="contained"
                                                    color="success"
                                                    startIcon={
                                                        isProcessing
                                                            ? (
                                                                <CircularProgress
                                                                    size={16}
                                                                    color="inherit"
                                                                />
                                                            )
                                                            : (
                                                                <HeadsetMicOutlined />
                                                            )
                                                    }
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        void acceptCall(
                                                            item
                                                        )
                                                    }
                                                >
                                                    Kabul Et
                                                </Button>
                                            )}

                                            {status ===
                                                "Accepted" && (
                                                <Button
                                                    size="small"
                                                    variant="contained"
                                                    color="error"
                                                    startIcon={
                                                        <CallEndOutlined />
                                                    }
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        openCompleteDialog(
                                                            item
                                                        )
                                                    }
                                                >
                                                    Tamamla
                                                </Button>
                                            )}

                                            {status !==
                                                "Waiting" &&
                                                status !==
                                                    "Assigned" &&
                                                status !==
                                                    "Accepted" && (
                                                    <Typography
                                                        variant="body2"
                                                        color="text.secondary"
                                                    >
                                                        İşlem yok
                                                    </Typography>
                                                )}
                                        </TableCell>
                                    </TableRow>
                                );
                            })}

                            {filteredItems.length === 0 && (
                                <TableRow>
                                    <TableCell
                                        colSpan={9}
                                        align="center"
                                        sx={{
                                            paddingY: 6
                                        }}
                                    >
                                        <Typography color="text.secondary">
                                            Çağrı kaydı bulunamadı.
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>

            <Dialog
                open={completeOpen}
                onClose={closeCompleteDialog}
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle>
                    Çağrıyı Tamamla
                </DialogTitle>

                <DialogContent dividers>
                    <Typography
                        sx={{ marginBottom: 2 }}
                        color="text.secondary"
                    >
                        {selectedItem
                            ? `${selectedItem.callerNumber} numaralı çağrı tamamlanacak.`
                            : ""}
                    </Typography>

                    <TextField
                        fullWidth
                        multiline
                        minRows={4}
                        label="Görüşme Özeti"
                        value={completeForm.summary}
                        onChange={(event) =>
                            setCompleteForm(
                                (current) => ({
                                    ...current,
                                    summary:
                                        event.target.value
                                })
                            )
                        }
                        sx={{ marginBottom: 2 }}
                    />

                    <TextField
                        fullWidth
                        label="Sonuç"
                        value={completeForm.result}
                        onChange={(event) =>
                            setCompleteForm(
                                (current) => ({
                                    ...current,
                                    result:
                                        event.target.value
                                })
                            )
                        }
                    />
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={closeCompleteDialog}
                        disabled={Boolean(
                            processingId
                        )}
                    >
                        Vazgeç
                    </Button>

                    <Button
                        variant="contained"
                        color="error"
                        onClick={() =>
                            void completeCall()
                        }
                        disabled={Boolean(
                            processingId
                        )}
                    >
                        {processingId
                            ? "Tamamlanıyor..."
                            : "Çağrıyı Tamamla"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={Boolean(success)}
                autoHideDuration={3000}
                onClose={() => setSuccess("")}
                message={success}
            />
        </Box>
    );
}

export default LiveCallsPage;
