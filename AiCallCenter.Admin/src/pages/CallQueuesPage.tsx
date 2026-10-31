import { useCallback, useEffect, useMemo, useState } from "react";
import axios from "axios";

import {
    Add,
    Autorenew,
    DeleteOutlined,
    EditOutlined,
    GroupsOutlined,
    MoreVert,
    Search,
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
    FormControlLabel,
    Grid,
    IconButton,
    InputAdornment,
    Menu,
    MenuItem,
    Snackbar,
    Switch,
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
    assignOperatorToQueue,
    createCallQueue,
    deleteCallQueue,
    getCallQueues,
    getQueueAiAgents,
    getQueueOperators,
    removeOperatorFromQueue,
    updateCallQueue,
    type CallQueue,
    type CallQueueOperator,
    type QueueAiAgent
} from "../services/callQueueService";

import {
    getCallOperators,
    type CallOperator
} from "../services/callOperatorService";

interface QueueFormState {
    name: string;
    description: string;
    priority: number;
    maxWaitingSeconds: number;
    maxQueueSize: number;
    aiAgentId: string;
    isActive: boolean;
}

const initialForm: QueueFormState = {
    name: "",
    description: "",
    priority: 10,
    maxWaitingSeconds: 300,
    maxQueueSize: 100,
    aiAgentId: "",
    isActive: true
};

function getErrorMessage(
    exception: unknown,
    fallbackMessage: string
): string {
    if (!axios.isAxiosError(exception)) {
        return fallbackMessage;
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

function CallQueuesPage() {
    const [queues, setQueues] = useState<CallQueue[]>([]);
    const [agents, setAgents] = useState<QueueAiAgent[]>([]);
    const [operators, setOperators] = useState<CallOperator[]>([]);

    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [saving, setSaving] = useState(false);
    const [operatorSaving, setOperatorSaving] = useState(false);

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [search, setSearch] = useState("");

    const [formOpen, setFormOpen] = useState(false);
    const [operatorOpen, setOperatorOpen] = useState(false);

    const [selectedQueue, setSelectedQueue] =
        useState<CallQueue | null>(null);

    const [form, setForm] =
        useState<QueueFormState>(initialForm);

    const [queueOperators, setQueueOperators] =
        useState<CallQueueOperator[]>([]);

    const [selectedOperatorId, setSelectedOperatorId] =
        useState("");

    const [operatorPriority, setOperatorPriority] =
        useState(10);

    const [menuAnchor, setMenuAnchor] =
        useState<HTMLElement | null>(null);

    const [menuQueue, setMenuQueue] =
        useState<CallQueue | null>(null);

    const loadData = useCallback(
        async (showRefreshIndicator = false): Promise<void> => {
            if (showRefreshIndicator) {
                setRefreshing(true);
            }

            try {
                setError("");

                const [queueData, agentData, operatorData] =
                    await Promise.all([
                        getCallQueues(),
                        getQueueAiAgents(),
                        getCallOperators()
                    ]);

                setQueues(queueData);

                setAgents(
                    agentData
                        .filter((item) => item.isActive)
                        .sort((a, b) =>
                            a.name.localeCompare(b.name, "tr")
                        )
                );

                setOperators(
                    [...operatorData].sort((a, b) =>
                        a.displayName.localeCompare(
                            b.displayName,
                            "tr"
                        )
                    )
                );
            } catch (exception) {
                setError(
                    getErrorMessage(
                        exception,
                        "Veriler yüklenemedi."
                    )
                );
            } finally {
                setLoading(false);
                setRefreshing(false);
            }
        },
        []
    );

    useEffect(() => {
        const runInitialLoad = async (): Promise<void> => {
            await loadData();
        };

        void runInitialLoad();
    }, [loadData]);

    const filteredQueues = useMemo(() => {
        const value = search
            .trim()
            .toLocaleLowerCase("tr-TR");

        return queues.filter((queue) => {
            if (!value) {
                return true;
            }

            return (
                queue.name
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                (queue.description ?? "")
                    .toLocaleLowerCase("tr-TR")
                    .includes(value) ||
                (queue.aiAgentName ?? "")
                    .toLocaleLowerCase("tr-TR")
                    .includes(value)
            );
        });
    }, [queues, search]);

    const availableOperators = useMemo(() => {
        const assignedOperatorIds = new Set(
            queueOperators.map(
                (item) => item.callOperatorId
            )
        );

        return operators.filter(
            (operator) =>
                !assignedOperatorIds.has(operator.id)
        );
    }, [operators, queueOperators]);

    const closeMenu = (): void => {
        setMenuAnchor(null);
        setMenuQueue(null);
    };

    const openCreate = (): void => {
        setSelectedQueue(null);
        setForm(initialForm);
        setError("");
        setFormOpen(true);
    };

    const openEdit = (queue: CallQueue): void => {
        setSelectedQueue(queue);

        setForm({
            name: queue.name,
            description: queue.description ?? "",
            priority: queue.priority,
            maxWaitingSeconds: queue.maxWaitingSeconds,
            maxQueueSize: queue.maxQueueSize,
            aiAgentId: queue.aiAgentId ?? "",
            isActive: queue.isActive
        });

        setError("");
        setFormOpen(true);
        closeMenu();
    };

    const closeForm = (): void => {
        if (saving) {
            return;
        }

        setFormOpen(false);
        setSelectedQueue(null);
        setForm(initialForm);
    };

    const validateForm = (): string | null => {
        if (!form.name.trim()) {
            return "Kuyruk adı zorunludur.";
        }

        if (form.priority < 1 || form.priority > 100) {
            return "Öncelik 1 ile 100 arasında olmalıdır.";
        }

        if (
            form.maxWaitingSeconds < 10 ||
            form.maxWaitingSeconds > 86400
        ) {
            return "Bekleme süresi 10 ile 86400 saniye arasında olmalıdır.";
        }

        if (
            form.maxQueueSize < 1 ||
            form.maxQueueSize > 10000
        ) {
            return "Kapasite 1 ile 10000 arasında olmalıdır.";
        }

        return null;
    };

    const saveQueue = async (): Promise<void> => {
        const validationError = validateForm();

        if (validationError) {
            setError(validationError);
            return;
        }

        setSaving(true);
        setError("");

        try {
            if (selectedQueue) {
                const updated = await updateCallQueue(
                    selectedQueue.id,
                    {
                        name: form.name.trim(),
                        description:
                            form.description.trim() ||
                            undefined,
                        priority: form.priority,
                        maxWaitingSeconds:
                            form.maxWaitingSeconds,
                        maxQueueSize: form.maxQueueSize,
                        aiAgentId:
                            form.aiAgentId || null,
                        isActive: form.isActive
                    }
                );

                setQueues((current) =>
                    current.map((item) =>
                        item.id === updated.id
                            ? updated
                            : item
                    )
                );

                setSuccess("Kuyruk güncellendi.");
            } else {
                const created = await createCallQueue({
                    name: form.name.trim(),
                    description:
                        form.description.trim() || undefined,
                    priority: form.priority,
                    maxWaitingSeconds:
                        form.maxWaitingSeconds,
                    maxQueueSize: form.maxQueueSize,
                    aiAgentId: form.aiAgentId || null
                });

                setQueues((current) => [
                    created,
                    ...current
                ]);

                setSuccess("Kuyruk oluşturuldu.");
            }

            closeForm();
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Kuyruk kaydedilemedi."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    const removeQueue = async (
        queue: CallQueue
    ): Promise<void> => {
        const confirmed = window.confirm(
            `"${queue.name}" kuyruğu silinsin mi?`
        );

        if (!confirmed) {
            closeMenu();
            return;
        }

        try {
            setError("");

            await deleteCallQueue(queue.id);

            setQueues((current) =>
                current.filter(
                    (item) => item.id !== queue.id
                )
            );

            setSuccess("Kuyruk silindi.");
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Kuyruk silinemedi."
                )
            );
        } finally {
            closeMenu();
        }
    };

    const openOperators = async (
        queue: CallQueue
    ): Promise<void> => {
        setSelectedQueue(queue);
        setOperatorOpen(true);
        setSelectedOperatorId("");
        setOperatorPriority(10);
        setQueueOperators([]);
        setError("");
        closeMenu();

        try {
            const data = await getQueueOperators(queue.id);
            setQueueOperators(data);
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatörler yüklenemedi."
                )
            );
        }
    };

    const closeOperators = (): void => {
        if (operatorSaving) {
            return;
        }

        setOperatorOpen(false);
        setSelectedQueue(null);
        setQueueOperators([]);
        setSelectedOperatorId("");
        setOperatorPriority(10);
    };

    const addOperator = async (): Promise<void> => {
        if (!selectedQueue) {
            setError("Kuyruk seçimi bulunamadı.");
            return;
        }

        if (!selectedOperatorId) {
            setError("Lütfen operatör seçiniz.");
            return;
        }

        if (
            operatorPriority < 1 ||
            operatorPriority > 100
        ) {
            setError(
                "Operatör önceliği 1 ile 100 arasında olmalıdır."
            );
            return;
        }

        setOperatorSaving(true);
        setError("");

        try {
            const created =
                await assignOperatorToQueue(
                    selectedQueue.id,
                    selectedOperatorId,
                    operatorPriority
                );

            setQueueOperators((current) => [
                ...current,
                created
            ]);

            setQueues((current) =>
                current.map((queue) =>
                    queue.id === selectedQueue.id
                        ? {
                              ...queue,
                              operatorCount:
                                  queue.operatorCount + 1
                          }
                        : queue
                )
            );

            setSelectedOperatorId("");
            setOperatorPriority(10);
            setSuccess("Operatör kuyruğa bağlandı.");
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatör bağlanamadı."
                )
            );
        } finally {
            setOperatorSaving(false);
        }
    };

    const removeOperator = async (
        assignment: CallQueueOperator
    ): Promise<void> => {
        if (!selectedQueue) {
            return;
        }

        setOperatorSaving(true);
        setError("");

        try {
            await removeOperatorFromQueue(
                selectedQueue.id,
                assignment.assignmentId
            );

            setQueueOperators((current) =>
                current.filter(
                    (item) =>
                        item.assignmentId !==
                        assignment.assignmentId
                )
            );

            setQueues((current) =>
                current.map((queue) =>
                    queue.id === selectedQueue.id
                        ? {
                              ...queue,
                              operatorCount: Math.max(
                                  0,
                                  queue.operatorCount - 1
                              )
                          }
                        : queue
                )
            );

            setSuccess(
                "Operatör kuyruktan kaldırıldı."
            );
        } catch (exception) {
            setError(
                getErrorMessage(
                    exception,
                    "Operatör kaldırılamadı."
                )
            );
        } finally {
            setOperatorSaving(false);
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
                    justifyContent: "space-between",
                    alignItems: {
                        xs: "flex-start",
                        md: "center"
                    },
                    flexDirection: {
                        xs: "column",
                        md: "row"
                    },
                    marginBottom: 3,
                    gap: 2
                }}
            >
                <Box>
                    <Typography
                        variant="h4"
                        sx={{ fontWeight: 800 }}
                    >
                        Çağrı Kuyrukları
                    </Typography>

                    <Typography color="text.secondary">
                        Kuyruk, AI ajanı ve operatör
                        bağlantılarını yönetin.
                    </Typography>
                </Box>

                <Box sx={{ display: "flex", gap: 1 }}>
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
                            void loadData(true)
                        }
                    >
                        Yenile
                    </Button>

                    <Button
                        variant="contained"
                        startIcon={<Add />}
                        onClick={openCreate}
                    >
                        Yeni Kuyruk
                    </Button>
                </Box>
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

            <Card sx={{ borderRadius: 4 }}>
                <CardContent>
                    <TextField
                        fullWidth
                        value={search}
                        onChange={(event) =>
                            setSearch(event.target.value)
                        }
                        placeholder="Kuyruk veya AI ajanı ara..."
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
                </CardContent>

                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell>Kuyruk</TableCell>
                                <TableCell>AI Ajanı</TableCell>
                                <TableCell>Öncelik</TableCell>
                                <TableCell>Bekleme</TableCell>
                                <TableCell>Operatör</TableCell>
                                <TableCell>Bekleyen</TableCell>
                                <TableCell>Durum</TableCell>
                                <TableCell align="right">
                                    İşlem
                                </TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {filteredQueues.map((queue) => (
                                <TableRow
                                    key={queue.id}
                                    hover
                                >
                                    <TableCell>
                                        <Typography sx={{ fontWeight: 700 }}>
                                            {queue.name}
                                        </Typography>

                                        <Typography
                                            variant="caption"
                                            color="text.secondary"
                                        >
                                            {queue.description ||
                                                "—"}
                                        </Typography>
                                    </TableCell>

                                    <TableCell>
                                        <Chip
                                            size="small"
                                            icon={
                                                <SmartToyOutlined />
                                            }
                                            label={
                                                queue.aiAgentName ||
                                                "Bağlı değil"
                                            }
                                            variant="outlined"
                                        />
                                    </TableCell>

                                    <TableCell>
                                        {queue.priority}
                                    </TableCell>

                                    <TableCell>
                                        {Math.ceil(
                                            queue.maxWaitingSeconds /
                                                60
                                        )}{" "}
                                        dk
                                    </TableCell>

                                    <TableCell>
                                        <Button
                                            size="small"
                                            startIcon={
                                                <GroupsOutlined />
                                            }
                                            onClick={() =>
                                                void openOperators(
                                                    queue
                                                )
                                            }
                                        >
                                            {queue.operatorCount}
                                        </Button>
                                    </TableCell>

                                    <TableCell>
                                        {queue.waitingCallCount}
                                    </TableCell>

                                    <TableCell>
                                        <Chip
                                            size="small"
                                            color={
                                                queue.isActive
                                                    ? "success"
                                                    : "default"
                                            }
                                            label={
                                                queue.isActive
                                                    ? "Aktif"
                                                    : "Pasif"
                                            }
                                        />
                                    </TableCell>

                                    <TableCell align="right">
                                        <Tooltip title="Düzenle">
                                            <IconButton
                                                onClick={() =>
                                                    openEdit(
                                                        queue
                                                    )
                                                }
                                            >
                                                <EditOutlined />
                                            </IconButton>
                                        </Tooltip>

                                        <IconButton
                                            onClick={(event) => {
                                                setMenuAnchor(
                                                    event.currentTarget
                                                );
                                                setMenuQueue(
                                                    queue
                                                );
                                            }}
                                        >
                                            <MoreVert />
                                        </IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}

                            {filteredQueues.length === 0 && (
                                <TableRow>
                                    <TableCell
                                        colSpan={8}
                                        align="center"
                                        sx={{ paddingY: 5 }}
                                    >
                                        <Typography color="text.secondary">
                                            Kayıt bulunamadı.
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>

            <Menu
                anchorEl={menuAnchor}
                open={Boolean(menuAnchor)}
                onClose={closeMenu}
            >
                <MenuItem
                    onClick={() => {
                        if (menuQueue) {
                            void openOperators(menuQueue);
                        }
                    }}
                >
                    <GroupsOutlined
                        sx={{ marginRight: 1 }}
                    />
                    Operatörleri Yönet
                </MenuItem>

                <MenuItem
                    onClick={() => {
                        if (menuQueue) {
                            void removeQueue(menuQueue);
                        }
                    }}
                    sx={{ color: "error.main" }}
                >
                    <DeleteOutlined
                        sx={{ marginRight: 1 }}
                    />
                    Sil
                </MenuItem>
            </Menu>

            <Dialog
                open={formOpen}
                onClose={closeForm}
                fullWidth
                maxWidth="md"
            >
                <DialogTitle>
                    {selectedQueue
                        ? "Kuyruğu Düzenle"
                        : "Yeni Kuyruk"}
                </DialogTitle>

                <DialogContent dividers>
                    <Grid container spacing={2}>
                        <Grid size={{ xs: 12, md: 6 }}>
                            <TextField
                                fullWidth
                                required
                                label="Kuyruk Adı"
                                value={form.name}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        name: event.target.value
                                    }))
                                }
                            />
                        </Grid>

                        <Grid size={{ xs: 12, md: 6 }}>
                            <TextField
                                select
                                fullWidth
                                label="AI Ajanı"
                                value={form.aiAgentId}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        aiAgentId:
                                            event.target.value
                                    }))
                                }
                            >
                                <MenuItem value="">
                                    Bağlama
                                </MenuItem>

                                {agents.map((agent) => (
                                    <MenuItem
                                        key={agent.id}
                                        value={agent.id}
                                    >
                                        {agent.name}
                                    </MenuItem>
                                ))}
                            </TextField>
                        </Grid>

                        <Grid size={{ xs: 12 }}>
                            <TextField
                                fullWidth
                                multiline
                                minRows={3}
                                label="Açıklama"
                                value={form.description}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        description:
                                            event.target.value
                                    }))
                                }
                            />
                        </Grid>

                        <Grid size={{ xs: 12, md: 4 }}>
                            <TextField
                                fullWidth
                                type="number"
                                label="Öncelik"
                                value={form.priority}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        priority: Number(
                                            event.target.value
                                        )
                                    }))
                                }
                                slotProps={{
                                    htmlInput: {
                                        min: 1,
                                        max: 100
                                    }
                                }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12, md: 4 }}>
                            <TextField
                                fullWidth
                                type="number"
                                label="Bekleme (sn)"
                                value={
                                    form.maxWaitingSeconds
                                }
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        maxWaitingSeconds:
                                            Number(
                                                event.target
                                                    .value
                                            )
                                    }))
                                }
                                slotProps={{
                                    htmlInput: {
                                        min: 10,
                                        max: 86400
                                    }
                                }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12, md: 4 }}>
                            <TextField
                                fullWidth
                                type="number"
                                label="Kapasite"
                                value={form.maxQueueSize}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        maxQueueSize: Number(
                                            event.target.value
                                        )
                                    }))
                                }
                                slotProps={{
                                    htmlInput: {
                                        min: 1,
                                        max: 10000
                                    }
                                }}
                            />
                        </Grid>

                        {selectedQueue && (
                            <Grid size={{ xs: 12 }}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={
                                                form.isActive
                                            }
                                            onChange={(
                                                event
                                            ) =>
                                                setForm(
                                                    (
                                                        current
                                                    ) => ({
                                                        ...current,
                                                        isActive:
                                                            event
                                                                .target
                                                                .checked
                                                    })
                                                )
                                            }
                                        />
                                    }
                                    label="Kuyruk aktif"
                                />
                            </Grid>
                        )}
                    </Grid>
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={closeForm}
                        disabled={saving}
                    >
                        Vazgeç
                    </Button>

                    <Button
                        variant="contained"
                        onClick={() =>
                            void saveQueue()
                        }
                        disabled={saving}
                    >
                        {saving
                            ? "Kaydediliyor..."
                            : "Kaydet"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={operatorOpen}
                onClose={closeOperators}
                fullWidth
                maxWidth="md"
            >
                <DialogTitle>
                    {selectedQueue?.name} — Operatörler
                </DialogTitle>

                <DialogContent dividers>
                    <Grid
                        container
                        spacing={2}
                        sx={{ marginBottom: 3 }}
                    >
                        <Grid size={{ xs: 12, md: 7 }}>
                            <TextField
                                select
                                fullWidth
                                label="Operatör"
                                value={selectedOperatorId}
                                onChange={(event) =>
                                    setSelectedOperatorId(
                                        event.target.value
                                    )
                                }
                            >
                                {availableOperators.length ===
                                0 ? (
                                    <MenuItem
                                        value=""
                                        disabled
                                    >
                                        Eklenebilecek operatör yok
                                    </MenuItem>
                                ) : (
                                    availableOperators.map(
                                        (operator) => (
                                            <MenuItem
                                                key={operator.id}
                                                value={
                                                    operator.id
                                                }
                                            >
                                                {
                                                    operator.displayName
                                                }
                                            </MenuItem>
                                        )
                                    )
                                )}
                            </TextField>
                        </Grid>

                        <Grid size={{ xs: 12, md: 3 }}>
                            <TextField
                                fullWidth
                                type="number"
                                label="Öncelik"
                                value={operatorPriority}
                                onChange={(event) =>
                                    setOperatorPriority(
                                        Number(
                                            event.target.value
                                        )
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

                        <Grid size={{ xs: 12, md: 2 }}>
                            <Button
                                fullWidth
                                variant="contained"
                                startIcon={<Add />}
                                onClick={() =>
                                    void addOperator()
                                }
                                disabled={
                                    operatorSaving ||
                                    !selectedOperatorId
                                }
                                sx={{ height: 56 }}
                            >
                                Ekle
                            </Button>
                        </Grid>
                    </Grid>

                    {queueOperators.length === 0 ? (
                        <Alert severity="info">
                            Bu kuyruğa operatör
                            bağlanmamış.
                        </Alert>
                    ) : (
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>
                                        Operatör
                                    </TableCell>
                                    <TableCell>
                                        Dahili
                                    </TableCell>
                                    <TableCell>
                                        Öncelik
                                    </TableCell>
                                    <TableCell align="right">
                                        İşlem
                                    </TableCell>
                                </TableRow>
                            </TableHead>

                            <TableBody>
                                {queueOperators.map(
                                    (item) => (
                                        <TableRow
                                            key={
                                                item.assignmentId
                                            }
                                        >
                                            <TableCell>
                                                {
                                                    item.displayName
                                                }
                                            </TableCell>

                                            <TableCell>
                                                {item.extensionNumber ||
                                                    "—"}
                                            </TableCell>

                                            <TableCell>
                                                {item.priority}
                                            </TableCell>

                                            <TableCell align="right">
                                                <Button
                                                    color="error"
                                                    startIcon={
                                                        <DeleteOutlined />
                                                    }
                                                    disabled={
                                                        operatorSaving
                                                    }
                                                    onClick={() =>
                                                        void removeOperator(
                                                            item
                                                        )
                                                    }
                                                >
                                                    Kaldır
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    )
                                )}
                            </TableBody>
                        </Table>
                    )}
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={closeOperators}
                        disabled={operatorSaving}
                    >
                        Kapat
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

export default CallQueuesPage;