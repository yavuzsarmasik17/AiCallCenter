import {
    useCallback,
    useEffect,
    useMemo,
    useState
} from "react";

import axios from "axios";

import {
    Add,
    Autorenew,
    DeleteOutlined,
    EditOutlined,
    HeadsetMicOutlined,
    MoreVert,
    PsychologyOutlined,
    StarOutlined,
    LinkOffOutlined,
    PauseCircleOutlined,
    PersonOffOutlined,
    PhoneInTalkOutlined,
    Search,
    SignalCellularAlt,
    WifiTethering
} from "@mui/icons-material";

import {
    Alert,
    Avatar,
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
    FormControlLabel,
    Grid,
    IconButton,
    InputAdornment,
    InputLabel,
    Menu,
    MenuItem,
    Select,
    Snackbar,
    Switch,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TablePagination,
    TableRow,
    TextField,
    Tooltip,
    Typography
} from "@mui/material";

import type { SelectChangeEvent } from "@mui/material";

import {
    createCallOperator,
    deleteCallOperator,
    getCallOperators,
    updateCallOperator,
    updateCallOperatorStatus,
    type CallOperator,
    type CreateCallOperatorRequest
} from "../services/callOperatorService";

import {
    assignAiAgent,
    getAvailableAiAgents,
    getOperatorAiAgents,
    removeAiAgent,
    setPrimaryAiAgent,
    type AvailableAiAgent,
    type CallOperatorAiAgent
} from "../services/callOperatorAiAgentService";

import {
    getStoredCompanyId,
    getUsers
} from "../services/userService";

import type { User } from "../types/user";

/*
 * Backend enum sırası farklıysa yalnızca bu değerleri değiştir.
 * Offline = 0, Available = 1, Busy = 2,
 * AfterCallWork = 3, OnBreak = 4
 */
const CALL_OPERATOR_STATUS = {
    Offline: 1,
    Available: 2,
    Busy: 3,
    OnBreak: 4,
    AfterCallWork: 5
} as const;

type StatusValue =
    (typeof CALL_OPERATOR_STATUS)[keyof typeof CALL_OPERATOR_STATUS];

type StatusFilter = "all" | `${StatusValue}`;

type SortOption =
    | "name-asc"
    | "name-desc"
    | "status"
    | "activity-desc";

interface OperatorFormState {
    userId: string;
    displayName: string;
    extensionNumber: string;
    maxConcurrentCalls: number;
    canReceiveCalls: boolean;
}

const initialForm: OperatorFormState = {
    userId: "",
    displayName: "",
    extensionNumber: "",
    maxConcurrentCalls: 1,
    canReceiveCalls: true
};

interface StatusMeta {
    label: string;
    color:
        | "default"
        | "success"
        | "error"
        | "warning"
        | "info";
    icon: React.ReactElement;
}

function getStatusMeta(status: number): StatusMeta {
    switch (status) {
        case CALL_OPERATOR_STATUS.Available:
            return {
                label: "Müsait",
                color: "success",
                icon: <WifiTethering fontSize="small" />
            };
        case CALL_OPERATOR_STATUS.Busy:
            return {
                label: "Meşgul",
                color: "error",
                icon: <PhoneInTalkOutlined fontSize="small" />
            };
        case CALL_OPERATOR_STATUS.AfterCallWork:
            return {
                label: "Çağrı Sonrası İşlem",
                color: "info",
                icon: <Autorenew fontSize="small" />
            };
        case CALL_OPERATOR_STATUS.OnBreak:
            return {
                label: "Molada",
                color: "warning",
                icon: <PauseCircleOutlined fontSize="small" />
            };
        case CALL_OPERATOR_STATUS.Offline:
        default:
            return {
                label: "Çevrimdışı",
                color: "default",
                icon: <PersonOffOutlined fontSize="small" />
            };
    }
}

function getApiErrorMessage(
    exception: unknown,
    fallbackMessage: string
): string {
    if (!axios.isAxiosError(exception)) {
        return fallbackMessage;
    }

    const responseData = exception.response?.data as
        | {
              detail?: string;
              title?: string;
              message?: string;
              errors?: Record<string, string[]>;
          }
        | undefined;

    const validationMessage = responseData?.errors
        ? Object.values(responseData.errors).flat().join(" ")
        : undefined;

    return (
        validationMessage ||
        responseData?.detail ||
        responseData?.message ||
        responseData?.title ||
        fallbackMessage
    );
}

function formatDate(value?: string): string {
    if (!value) {
        return "—";
    }

    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
        return "—";
    }

    return new Intl.DateTimeFormat("tr-TR", {
        dateStyle: "short",
        timeStyle: "short"
    }).format(date);
}

interface StatCardProps {
    title: string;
    value: number;
    description: string;
    icon: React.ReactNode;
    gradient: string;
}

function StatCard({
    title,
    value,
    description,
    icon,
    gradient
}: StatCardProps) {
    return (
        <Card
            sx={{
                height: "100%",
                borderRadius: 4,
                color: "common.white",
                background: gradient,
                boxShadow: "0 12px 30px rgba(15,23,42,0.12)",
                transition: "transform 0.2s ease, box-shadow 0.2s ease",
                "&:hover": {
                    transform: "translateY(-4px)",
                    boxShadow: "0 18px 42px rgba(15,23,42,0.18)"
                }
            }}
        >
            <CardContent sx={{ padding: 3 }}>
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        gap: 2
                    }}
                >
                    <Box>
                        <Typography
                            variant="body2"
                            sx={{ opacity: 0.9, fontWeight: 600 }}
                        >
                            {title}
                        </Typography>

                        <Typography
                            variant="h3"
                            sx={{
                                fontWeight: 800,
                                marginTop: 1,
                                lineHeight: 1
                            }}
                        >
                            {value}
                        </Typography>

                        <Typography
                            variant="body2"
                            sx={{ opacity: 0.85, marginTop: 1.25 }}
                        >
                            {description}
                        </Typography>
                    </Box>

                    <Avatar
                        sx={{
                            width: 50,
                            height: 50,
                            backgroundColor: "rgba(255,255,255,0.18)",
                            color: "common.white"
                        }}
                    >
                        {icon}
                    </Avatar>
                </Box>
            </CardContent>
        </Card>
    );
}

function CallOperatorsPage() {
    const companyId = getStoredCompanyId();

    const [operators, setOperators] = useState<CallOperator[]>([]);
    const [users, setUsers] = useState<User[]>([]);
    const [usersLoading, setUsersLoading] = useState(true);
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);
    const [saving, setSaving] = useState(false);
    const [deleting, setDeleting] = useState(false);
    const [statusUpdatingId, setStatusUpdatingId] =
        useState<string | null>(null);
    const [error, setError] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [searchText, setSearchText] = useState("");
    const [statusFilter, setStatusFilter] =
        useState<StatusFilter>("all");
    const [sortOption, setSortOption] =
        useState<SortOption>("name-asc");
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(10);
    const [formDialogOpen, setFormDialogOpen] = useState(false);
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [selectedOperator, setSelectedOperator] =
        useState<CallOperator | null>(null);
    const [operatorToDelete, setOperatorToDelete] =
        useState<CallOperator | null>(null);
    const [form, setForm] =
        useState<OperatorFormState>(initialForm);
    const [menuAnchor, setMenuAnchor] =
        useState<HTMLElement | null>(null);
    const [menuOperator, setMenuOperator] =
        useState<CallOperator | null>(null);

    const [aiDialogOpen, setAiDialogOpen] = useState(false);
    const [aiManagingOperator, setAiManagingOperator] =
        useState<CallOperator | null>(null);
    const [aiAssignments, setAiAssignments] =
        useState<CallOperatorAiAgent[]>([]);
    const [availableAiAgents, setAvailableAiAgents] =
        useState<AvailableAiAgent[]>([]);
    const [selectedAiAgentId, setSelectedAiAgentId] = useState("");
    const [aiLoading, setAiLoading] = useState(false);
    const [aiSaving, setAiSaving] = useState(false);

    const loadOperators = useCallback(
        async (showRefreshIndicator = false) => {
            if (showRefreshIndicator) {
                setRefreshing(true);
            }

            try {
                setError("");
                const data = await getCallOperators();
                setOperators(data);
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "Operatörler yüklenirken bir hata oluştu."
                    )
                );
            } finally {
                setLoading(false);
                setRefreshing(false);
            }
        },
        []
    );

    const loadUsers = useCallback(async () => {
        if (!companyId) {
            setUsers([]);
            setUsersLoading(false);
            setError(
                "Firma kimliği bulunamadı. Lütfen çıkış yapıp tekrar giriş yapınız."
            );
            return;
        }

        try {
            setUsersLoading(true);
            const data = await getUsers(companyId);
            setUsers(data);
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Kullanıcılar yüklenirken bir hata oluştu."
                )
            );
        } finally {
            setUsersLoading(false);
        }
    }, [companyId]);

    useEffect(() => {
        const initialLoadTimerId = window.setTimeout(() => {
            void loadOperators();
            void loadUsers();
        }, 0);

        const intervalId = window.setInterval(() => {
            void loadOperators();
        }, 10_000);

        return () => {
            window.clearTimeout(initialLoadTimerId);
            window.clearInterval(intervalId);
        };
    }, [loadOperators, loadUsers]);

    const availableUsers = useMemo(() => {
        const assignedUserIds = new Set(
            operators.map((operator) => operator.userId)
        );

        return users
            .filter(
                (user) =>
                    user.isActive &&
                    !assignedUserIds.has(user.id)
            )
            .sort((a, b) =>
                a.fullName.localeCompare(b.fullName, "tr")
            );
    }, [operators, users]);

    const summary = useMemo(() => ({
        total: operators.length,
        available: operators.filter(
            (item) => item.status === CALL_OPERATOR_STATUS.Available
        ).length,
        busy: operators.filter(
            (item) => item.status === CALL_OPERATOR_STATUS.Busy
        ).length,
        onBreak: operators.filter(
            (item) => item.status === CALL_OPERATOR_STATUS.OnBreak
        ).length,
        offline: operators.filter(
            (item) => item.status === CALL_OPERATOR_STATUS.Offline
        ).length
    }), [operators]);

    const filteredOperators = useMemo(() => {
        const normalizedSearch = searchText
            .trim()
            .toLocaleLowerCase("tr-TR");

        const filtered = operators.filter((operator) => {
            const statusMeta = getStatusMeta(operator.status);

            const matchesSearch =
                !normalizedSearch ||
                operator.displayName
                    .toLocaleLowerCase("tr-TR")
                    .includes(normalizedSearch) ||
                (operator.extensionNumber ?? "")
                    .toLocaleLowerCase("tr-TR")
                    .includes(normalizedSearch) ||
                operator.userId
                    .toLocaleLowerCase("tr-TR")
                    .includes(normalizedSearch) ||
                statusMeta.label
                    .toLocaleLowerCase("tr-TR")
                    .includes(normalizedSearch);

            const matchesStatus =
                statusFilter === "all" ||
                operator.status === Number(statusFilter);

            return matchesSearch && matchesStatus;
        });

        const sorted = [...filtered];

        sorted.sort((a, b) => {
            switch (sortOption) {
                case "name-desc":
                    return b.displayName.localeCompare(
                        a.displayName,
                        "tr"
                    );
                case "status":
                    return a.status - b.status;
                case "activity-desc": {
                    const aTime = a.lastActivityAt
                        ? new Date(a.lastActivityAt).getTime()
                        : 0;
                    const bTime = b.lastActivityAt
                        ? new Date(b.lastActivityAt).getTime()
                        : 0;
                    return bTime - aTime;
                }
                case "name-asc":
                default:
                    return a.displayName.localeCompare(
                        b.displayName,
                        "tr"
                    );
            }
        });

        return sorted;
    }, [operators, searchText, sortOption, statusFilter]);

    const pagedOperators = useMemo(() => {
        const start = page * rowsPerPage;
        return filteredOperators.slice(start, start + rowsPerPage);
    }, [filteredOperators, page, rowsPerPage]);

    const handleOpenCreate = () => {
        setSelectedOperator(null);
        setForm(initialForm);
        setError("");
        setFormDialogOpen(true);
        void loadUsers();
    };

    const handleOpenEdit = (operator: CallOperator) => {
        setSelectedOperator(operator);
        setForm({
            userId: operator.userId,
            displayName: operator.displayName,
            extensionNumber: operator.extensionNumber ?? "",
            maxConcurrentCalls: operator.maxConcurrentCalls,
            canReceiveCalls: operator.canReceiveCalls
        });
        setError("");
        setFormDialogOpen(true);
        handleCloseMenu();
    };

    const handleCloseFormDialog = () => {
        if (saving) return;
        setFormDialogOpen(false);
        setSelectedOperator(null);
        setForm(initialForm);
    };

    const validateForm = (): string | null => {
        if (!selectedOperator && !form.userId.trim()) {
            return "Lütfen bir kullanıcı seçiniz.";
        }
        if (!form.displayName.trim()) {
            return "Operatör adı zorunludur.";
        }
        if (form.maxConcurrentCalls < 1) {
            return "Maksimum çağrı sayısı en az 1 olmalıdır.";
        }
        return null;
    };

    const handleSave = async () => {
        const validationError = validateForm();
        if (validationError) {
            setError(validationError);
            return;
        }

        setSaving(true);
        setError("");

        try {
            if (selectedOperator) {
                const updated = await updateCallOperator(
                    selectedOperator.id,
                    {
                        displayName: form.displayName.trim(),
                        extensionNumber:
                            form.extensionNumber.trim() || undefined,
                        maxConcurrentCalls: form.maxConcurrentCalls,
                        canReceiveCalls: form.canReceiveCalls
                    }
                );

                setOperators((current) =>
                    current.map((item) =>
                        item.id === updated.id ? updated : item
                    )
                );
                setSuccessMessage("Operatör başarıyla güncellendi.");
            } else {
                const request: CreateCallOperatorRequest = {
                    userId: form.userId.trim(),
                    displayName: form.displayName.trim(),
                    extensionNumber:
                        form.extensionNumber.trim() || undefined,
                    maxConcurrentCalls: form.maxConcurrentCalls,
                    canReceiveCalls: form.canReceiveCalls
                };

                const created = await createCallOperator(request);
                setOperators((current) => [created, ...current]);
                setSuccessMessage("Operatör başarıyla oluşturuldu.");
            }

            setFormDialogOpen(false);
            setSelectedOperator(null);
            setForm(initialForm);
        } catch (exception) {
            setError(
                getApiErrorMessage(exception, "Operatör kaydedilemedi.")
            );
        } finally {
            setSaving(false);
        }
    };

    const handleOpenDelete = (operator: CallOperator) => {
        setOperatorToDelete(operator);
        setDeleteDialogOpen(true);
        setError("");
        handleCloseMenu();
    };

    const handleCloseDeleteDialog = () => {
        if (deleting) return;
        setDeleteDialogOpen(false);
        setOperatorToDelete(null);
    };

    const handleConfirmDelete = async () => {
        if (!operatorToDelete) return;

        setDeleting(true);
        setError("");

        try {
            await deleteCallOperator(operatorToDelete.id);
            setOperators((current) =>
                current.filter((item) => item.id !== operatorToDelete.id)
            );
            setDeleteDialogOpen(false);
            setOperatorToDelete(null);
            setSuccessMessage("Operatör başarıyla silindi.");
        } catch (exception) {
            setError(
                getApiErrorMessage(exception, "Operatör silinemedi.")
            );
        } finally {
            setDeleting(false);
        }
    };

    const handleStatusChange = async (
        operator: CallOperator,
        newStatus: number
    ) => {
        if (operator.status === newStatus) return;

        setStatusUpdatingId(operator.id);
        setError("");

        try {
            const updated = await updateCallOperatorStatus(
                operator.id,
                newStatus
            );
            setOperators((current) =>
                current.map((item) =>
                    item.id === updated.id ? updated : item
                )
            );
            setSuccessMessage("Operatör durumu güncellendi.");
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Operatör durumu güncellenemedi."
                )
            );
        } finally {
            setStatusUpdatingId(null);
        }
    };

    const handleOpenMenu = (
        event: React.MouseEvent<HTMLButtonElement>,
        operator: CallOperator
    ) => {
        setMenuAnchor(event.currentTarget);
        setMenuOperator(operator);
    };

    const handleCloseMenu = () => {
        setMenuAnchor(null);
        setMenuOperator(null);
    };

    const loadAiManagementData = useCallback(
        async (operatorId: string) => {
            setAiLoading(true);

            try {
                const [assignments, agents] = await Promise.all([
                    getOperatorAiAgents(operatorId),
                    getAvailableAiAgents(operatorId)
                ]);

                setAiAssignments(assignments);
                setAvailableAiAgents(agents);

                const firstUnassigned = agents.find(
                    (agent) => !agent.isAssigned
                );

                setSelectedAiAgentId(firstUnassigned?.id ?? "");
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "AI ajanı bilgileri yüklenemedi."
                    )
                );
            } finally {
                setAiLoading(false);
            }
        },
        []
    );

    const handleOpenAiDialog = (operator: CallOperator) => {
        setAiManagingOperator(operator);
        setAiDialogOpen(true);
        setError("");
        handleCloseMenu();
        void loadAiManagementData(operator.id);
    };

    const handleCloseAiDialog = () => {
        if (aiSaving) return;

        setAiDialogOpen(false);
        setAiManagingOperator(null);
        setAiAssignments([]);
        setAvailableAiAgents([]);
        setSelectedAiAgentId("");
    };

    const handleAssignAiAgent = async () => {
        if (!aiManagingOperator || !selectedAiAgentId) {
            setError("Lütfen atanacak AI ajanını seçiniz.");
            return;
        }

        setAiSaving(true);
        setError("");

        try {
            await assignAiAgent(
                aiManagingOperator.id,
                {
                    aiAgentId: selectedAiAgentId,
                    isPrimary: aiAssignments.length === 0
                }
            );

            await loadAiManagementData(aiManagingOperator.id);
            setSuccessMessage("AI ajanı operatöre başarıyla atandı.");
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanı atanamadı."
                )
            );
        } finally {
            setAiSaving(false);
        }
    };

    const handleSetPrimaryAiAgent = async (
        assignment: CallOperatorAiAgent
    ) => {
        if (!aiManagingOperator || assignment.isPrimary) return;

        setAiSaving(true);
        setError("");

        try {
            await setPrimaryAiAgent(
                aiManagingOperator.id,
                assignment.id
            );

            await loadAiManagementData(aiManagingOperator.id);
            setSuccessMessage("Ana AI ajanı güncellendi.");
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Ana AI ajanı güncellenemedi."
                )
            );
        } finally {
            setAiSaving(false);
        }
    };

    const handleRemoveAiAgent = async (
        assignment: CallOperatorAiAgent
    ) => {
        if (!aiManagingOperator) return;

        setAiSaving(true);
        setError("");

        try {
            await removeAiAgent(
                aiManagingOperator.id,
                assignment.id
            );

            await loadAiManagementData(aiManagingOperator.id);
            setSuccessMessage("AI ajanı bağlantısı kaldırıldı.");
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanı bağlantısı kaldırılamadı."
                )
            );
        } finally {
            setAiSaving(false);
        }
    };

    if (loading) {
        return (
            <Box
                sx={{
                    minHeight: 420,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center"
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box sx={{ minHeight: "100vh", padding: { xs: 2, md: 3 } }}>
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: { xs: "flex-start", md: "center" },
                    flexDirection: { xs: "column", md: "row" },
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
                        Operatör Yönetimi
                    </Typography>
                    <Typography
                        color="text.secondary"
                        sx={{ marginTop: 0.5 }}
                    >
                        Çağrı merkezi operatörlerini, durumlarını ve çağrı
                        kapasitelerini yönetin.
                    </Typography>
                </Box>

                <Box sx={{ display: "flex", gap: 1.5 }}>
                    <Button
                        variant="outlined"
                        startIcon={
                            refreshing
                                ? <CircularProgress size={18} />
                                : <Autorenew />
                        }
                        disabled={refreshing}
                        onClick={() => void loadOperators(true)}
                        sx={{ borderRadius: 3, minHeight: 46 }}
                    >
                        Yenile
                    </Button>
                    <Button
                        variant="contained"
                        size="large"
                        startIcon={<Add />}
                        onClick={handleOpenCreate}
                        sx={{
                            borderRadius: 3,
                            minHeight: 46,
                            paddingX: 2.5,
                            boxShadow: 3
                        }}
                    >
                        Yeni Operatör
                    </Button>
                </Box>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    onClose={() => setError("")}
                    sx={{ marginBottom: 3 }}
                >
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6, lg: 2.4 }}>
                    <StatCard
                        title="Toplam Operatör"
                        value={summary.total}
                        description="Sistemde kayıtlı"
                        icon={<HeadsetMicOutlined />}
                        gradient="linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 2.4 }}>
                    <StatCard
                        title="Müsait"
                        value={summary.available}
                        description="Çağrı almaya hazır"
                        icon={<WifiTethering />}
                        gradient="linear-gradient(135deg, #059669 0%, #10B981 100%)"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 2.4 }}>
                    <StatCard
                        title="Meşgul"
                        value={summary.busy}
                        description="Aktif görüşmede"
                        icon={<PhoneInTalkOutlined />}
                        gradient="linear-gradient(135deg, #DC2626 0%, #EF4444 100%)"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 2.4 }}>
                    <StatCard
                        title="Molada"
                        value={summary.onBreak}
                        description="Geçici olarak kapalı"
                        icon={<PauseCircleOutlined />}
                        gradient="linear-gradient(135deg, #D97706 0%, #F59E0B 100%)"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 2.4 }}>
                    <StatCard
                        title="Çevrimdışı"
                        value={summary.offline}
                        description="Sistemde aktif değil"
                        icon={<PersonOffOutlined />}
                        gradient="linear-gradient(135deg, #475569 0%, #64748B 100%)"
                    />
                </Grid>
            </Grid>

            <Card
                sx={{
                    marginTop: 3,
                    borderRadius: 4,
                    border: 1,
                    borderColor: "divider",
                    boxShadow: "0 10px 30px rgba(15,23,42,0.06)"
                }}
            >
                <CardContent sx={{ padding: 3 }}>
                    <Grid container spacing={2} sx={{ alignItems: "center" }}>
                        <Grid size={{ xs: 12, md: 6 }}>
                            <TextField
                                fullWidth
                                value={searchText}
                                onChange={(event) => {
                                    setSearchText(event.target.value);
                                    setPage(0);
                                }}
                                placeholder="Operatör, dahili numara, kullanıcı veya durum ara..."
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

                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <FormControl fullWidth>
                                <InputLabel>Durum</InputLabel>
                                <Select
                                    label="Durum"
                                    value={statusFilter}
                                    onChange={(event: SelectChangeEvent) => {
                                        setStatusFilter(
                                            event.target.value as StatusFilter
                                        );
                                        setPage(0);
                                    }}
                                >
                                    <MenuItem value="all">Tüm Durumlar</MenuItem>
                                    <MenuItem value={`${CALL_OPERATOR_STATUS.Available}`}>
                                        Müsait
                                    </MenuItem>
                                    <MenuItem value={`${CALL_OPERATOR_STATUS.Busy}`}>
                                        Meşgul
                                    </MenuItem>
                                    <MenuItem value={`${CALL_OPERATOR_STATUS.AfterCallWork}`}>
                                        Çağrı Sonrası İşlem
                                    </MenuItem>
                                    <MenuItem value={`${CALL_OPERATOR_STATUS.OnBreak}`}>
                                        Molada
                                    </MenuItem>
                                    <MenuItem value={`${CALL_OPERATOR_STATUS.Offline}`}>
                                        Çevrimdışı
                                    </MenuItem>
                                </Select>
                            </FormControl>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <FormControl fullWidth>
                                <InputLabel>Sıralama</InputLabel>
                                <Select
                                    label="Sıralama"
                                    value={sortOption}
                                    onChange={(event: SelectChangeEvent) => {
                                        setSortOption(
                                            event.target.value as SortOption
                                        );
                                        setPage(0);
                                    }}
                                >
                                    <MenuItem value="name-asc">Ad A-Z</MenuItem>
                                    <MenuItem value="name-desc">Ad Z-A</MenuItem>
                                    <MenuItem value="status">Duruma Göre</MenuItem>
                                    <MenuItem value="activity-desc">Son Aktivite</MenuItem>
                                </Select>
                            </FormControl>
                        </Grid>
                    </Grid>

                    <Box
                        sx={{
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: "center",
                            marginTop: 2,
                            gap: 1,
                            flexWrap: "wrap"
                        }}
                    >
                        <Typography variant="body2" color="text.secondary">
                            {filteredOperators.length} operatör gösteriliyor
                        </Typography>
                        <Chip
                            size="small"
                            icon={<SignalCellularAlt />}
                            label="10 saniyede otomatik yenilenir"
                            variant="outlined"
                        />
                    </Box>
                </CardContent>

                <Divider />

                {operators.length === 0 ? (
                    <CardContent sx={{ textAlign: "center", paddingY: 8 }}>
                        <Avatar
                            sx={{
                                width: 72,
                                height: 72,
                                marginX: "auto",
                                marginBottom: 2,
                                background:
                                    "linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)"
                            }}
                        >
                            <HeadsetMicOutlined sx={{ fontSize: 38 }} />
                        </Avatar>
                        <Typography variant="h6" sx={{ fontWeight: 800 }}>
                            Henüz operatör bulunmuyor
                        </Typography>
                        <Typography
                            color="text.secondary"
                            sx={{ marginTop: 1, marginBottom: 3 }}
                        >
                            İlk çağrı merkezi operatörünüzü oluşturarak başlayın.
                        </Typography>
                        <Button
                            variant="contained"
                            startIcon={<Add />}
                            onClick={handleOpenCreate}
                        >
                            İlk Operatörü Oluştur
                        </Button>
                    </CardContent>
                ) : filteredOperators.length === 0 ? (
                    <CardContent>
                        <Alert severity="info">
                            Arama ve filtrelerinize uygun operatör bulunamadı.
                        </Alert>
                    </CardContent>
                ) : (
                    <>
                        <TableContainer>
                            <Table sx={{ minWidth: 1050 }}>
                                <TableHead>
                                    <TableRow>
                                        <TableCell>Operatör</TableCell>
                                        <TableCell>Dahili</TableCell>
                                        <TableCell>Durum</TableCell>
                                        <TableCell align="center">Aktif / Maks.</TableCell>
                                        <TableCell align="center">Çağrı Alabilir</TableCell>
                                        <TableCell>Son Aktivite</TableCell>
                                        <TableCell align="right">İşlemler</TableCell>
                                    </TableRow>
                                </TableHead>

                                <TableBody>
                                    {pagedOperators.map((operator) => {
                                        const statusMeta = getStatusMeta(
                                            operator.status
                                        );
                                        const isUpdating =
                                            statusUpdatingId === operator.id;

                                        return (
                                            <TableRow hover key={operator.id}>
                                                <TableCell>
                                                    <Box
                                                        sx={{
                                                            display: "flex",
                                                            alignItems: "center",
                                                            gap: 1.5
                                                        }}
                                                    >
                                                        <Avatar
                                                            sx={{
                                                                width: 40,
                                                                height: 40,
                                                                fontWeight: 800
                                                            }}
                                                        >
                                                            {operator.displayName
                                                                .trim()
                                                                .charAt(0)
                                                                .toLocaleUpperCase("tr-TR")}
                                                        </Avatar>
                                                        <Box>
                                                            <Typography sx={{ fontWeight: 700 }}>
                                                                {operator.displayName}
                                                            </Typography>
                                                            <Typography
                                                                variant="caption"
                                                                color="text.secondary"
                                                                sx={{
                                                                    display: "block",
                                                                    maxWidth: 240,
                                                                    overflow: "hidden",
                                                                    textOverflow: "ellipsis",
                                                                    whiteSpace: "nowrap"
                                                                }}
                                                            >
                                                                {operator.userId}
                                                            </Typography>
                                                        </Box>
                                                    </Box>
                                                </TableCell>

                                                <TableCell>
                                                    <Chip
                                                        size="small"
                                                        variant="outlined"
                                                        label={
                                                            operator.extensionNumber || "—"
                                                        }
                                                    />
                                                </TableCell>

                                                <TableCell>
                                                    <FormControl
                                                        size="small"
                                                        sx={{ minWidth: 185 }}
                                                    >
                                                        <Select
                                                            value={`${operator.status}`}
                                                            disabled={isUpdating}
                                                            onChange={(
                                                                event: SelectChangeEvent
                                                            ) =>
                                                                void handleStatusChange(
                                                                    operator,
                                                                    Number(
                                                                        event.target.value
                                                                    )
                                                                )
                                                            }
                                                            renderValue={() => (
                                                                <Chip
                                                                    size="small"
                                                                    color={statusMeta.color}
                                                                    icon={statusMeta.icon}
                                                                    label={
                                                                        isUpdating
                                                                            ? "Güncelleniyor..."
                                                                            : statusMeta.label
                                                                    }
                                                                />
                                                            )}
                                                        >
                                                            <MenuItem
                                                                value={`${CALL_OPERATOR_STATUS.Available}`}
                                                            >
                                                                Müsait
                                                            </MenuItem>
                                                            <MenuItem
                                                                value={`${CALL_OPERATOR_STATUS.AfterCallWork}`}
                                                            >
                                                                Çağrı Sonrası İşlem
                                                            </MenuItem>
                                                            <MenuItem
                                                                value={`${CALL_OPERATOR_STATUS.OnBreak}`}
                                                            >
                                                                Molada
                                                            </MenuItem>
                                                            <MenuItem
                                                                value={`${CALL_OPERATOR_STATUS.Offline}`}
                                                            >
                                                                Çevrimdışı
                                                            </MenuItem>
                                                        </Select>
                                                    </FormControl>
                                                </TableCell>

                                                <TableCell align="center">
                                                    <Typography sx={{ fontWeight: 700 }}>
                                                        {operator.activeCallCount} /{" "}
                                                        {operator.maxConcurrentCalls}
                                                    </Typography>
                                                </TableCell>

                                                <TableCell align="center">
                                                    <Chip
                                                        size="small"
                                                        color={
                                                            operator.canReceiveCalls
                                                                ? "success"
                                                                : "default"
                                                        }
                                                        label={
                                                            operator.canReceiveCalls
                                                                ? "Evet"
                                                                : "Hayır"
                                                        }
                                                    />
                                                </TableCell>

                                                <TableCell>
                                                    <Typography variant="body2">
                                                        {formatDate(
                                                            operator.lastActivityAt
                                                        )}
                                                    </Typography>
                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                    >
                                                        Durum:{" "}
                                                        {formatDate(
                                                            operator.lastStatusChangedAt
                                                        )}
                                                    </Typography>
                                                </TableCell>

                                                <TableCell align="right">
                                                    <Tooltip title="Düzenle">
                                                        <IconButton
                                                            onClick={() =>
                                                                handleOpenEdit(operator)
                                                            }
                                                        >
                                                            <EditOutlined />
                                                        </IconButton>
                                                    </Tooltip>
                                                    <Tooltip title="Diğer işlemler">
                                                        <IconButton
                                                            onClick={(event) =>
                                                                handleOpenMenu(
                                                                    event,
                                                                    operator
                                                                )
                                                            }
                                                        >
                                                            <MoreVert />
                                                        </IconButton>
                                                    </Tooltip>
                                                </TableCell>
                                            </TableRow>
                                        );
                                    })}
                                </TableBody>
                            </Table>
                        </TableContainer>

                        <TablePagination
                            component="div"
                            count={filteredOperators.length}
                            page={page}
                            onPageChange={(_event, newPage) => setPage(newPage)}
                            rowsPerPage={rowsPerPage}
                            onRowsPerPageChange={(event) => {
                                setRowsPerPage(Number(event.target.value));
                                setPage(0);
                            }}
                            rowsPerPageOptions={[5, 10, 25, 50]}
                            labelRowsPerPage="Sayfa başına:"
                            labelDisplayedRows={({ from, to, count }) =>
                                `${from}-${to} / ${count}`
                            }
                        />
                    </>
                )}
            </Card>

            <Menu
                anchorEl={menuAnchor}
                open={Boolean(menuAnchor)}
                onClose={handleCloseMenu}
            >
                <MenuItem
                    onClick={() => {
                        if (menuOperator) handleOpenEdit(menuOperator);
                    }}
                >
                    <EditOutlined fontSize="small" sx={{ marginRight: 1 }} />
                    Düzenle
                </MenuItem>
                <MenuItem
                    onClick={() => {
                        if (menuOperator) handleOpenAiDialog(menuOperator);
                    }}
                >
                    <PsychologyOutlined
                        fontSize="small"
                        sx={{ marginRight: 1 }}
                    />
                    AI Ajanlarını Yönet
                </MenuItem>
                <MenuItem
                    onClick={() => {
                        if (menuOperator) handleOpenDelete(menuOperator);
                    }}
                    sx={{ color: "error.main" }}
                >
                    <DeleteOutlined fontSize="small" sx={{ marginRight: 1 }} />
                    Sil
                </MenuItem>
            </Menu>

            <Dialog
                open={formDialogOpen}
                onClose={handleCloseFormDialog}
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle sx={{ fontWeight: 800 }}>
                    {selectedOperator ? "Operatörü Düzenle" : "Yeni Operatör"}
                </DialogTitle>
                <DialogContent dividers>
                    <Grid container spacing={2.5} sx={{ paddingTop: 0.5 }}>
                        {!selectedOperator && (
                            <Grid size={{ xs: 12 }}>
                                <TextField
                                    select
                                    fullWidth
                                    required
                                    label="Kullanıcı Seç"
                                    value={form.userId}
                                    disabled={usersLoading}
                                    onChange={(event) => {
                                        const selectedUserId =
                                            event.target.value;

                                        const selectedUser =
                                            users.find(
                                                (user) =>
                                                    user.id ===
                                                    selectedUserId
                                            );

                                        setForm((current) => ({
                                            ...current,
                                            userId: selectedUserId,
                                            displayName:
                                                selectedUser?.fullName ??
                                                current.displayName
                                        }));
                                    }}
                                    helperText={
                                        usersLoading
                                            ? "Kullanıcılar yükleniyor..."
                                            : availableUsers.length === 0
                                                ? "Operatör olarak atanabilecek aktif kullanıcı bulunamadı."
                                                : "Operatör yapmak istediğiniz kullanıcıyı seçin."
                                    }
                                >
                                    {availableUsers.map((user) => (
                                        <MenuItem
                                            key={user.id}
                                            value={user.id}
                                        >
                                            <Box>
                                                <Typography
                                                    sx={{
                                                        fontWeight: 700
                                                    }}
                                                >
                                                    {user.fullName}
                                                </Typography>
                                                <Typography
                                                    variant="caption"
                                                    color="text.secondary"
                                                >
                                                    {user.email}
                                                </Typography>
                                            </Box>
                                        </MenuItem>
                                    ))}
                                </TextField>
                            </Grid>
                        )}

                        <Grid size={{ xs: 12 }}>
                            <TextField
                                fullWidth
                                required
                                label="Görünen Ad"
                                value={form.displayName}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        displayName: event.target.value
                                    }))
                                }
                                autoFocus
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                fullWidth
                                label="Dahili Numara"
                                value={form.extensionNumber}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        extensionNumber: event.target.value
                                    }))
                                }
                                placeholder="101"
                            />
                        </Grid>

                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                fullWidth
                                required
                                type="number"
                                label="Maksimum Eş Zamanlı Çağrı"
                                value={form.maxConcurrentCalls}
                                onChange={(event) =>
                                    setForm((current) => ({
                                        ...current,
                                        maxConcurrentCalls: Math.max(
                                            1,
                                            Number(event.target.value) || 1
                                        )
                                    }))
                                }
                                slotProps={{
                                    htmlInput: { min: 1, max: 20 }
                                }}
                            />
                        </Grid>

                        <Grid size={{ xs: 12 }}>
                            <FormControlLabel
                                control={
                                    <Switch
                                        checked={form.canReceiveCalls}
                                        onChange={(event) =>
                                            setForm((current) => ({
                                                ...current,
                                                canReceiveCalls:
                                                    event.target.checked
                                            }))
                                        }
                                    />
                                }
                                label="Bu operatör çağrı alabilir"
                            />
                        </Grid>
                    </Grid>
                </DialogContent>

                <DialogActions sx={{ padding: 2.5 }}>
                    <Button
                        onClick={handleCloseFormDialog}
                        disabled={saving}
                    >
                        Vazgeç
                    </Button>
                    <Button
                        variant="contained"
                        onClick={() => void handleSave()}
                        disabled={
                            saving ||
                            (!selectedOperator &&
                                (usersLoading ||
                                    availableUsers.length === 0))
                        }
                        startIcon={
                            saving ? <CircularProgress size={18} /> : undefined
                        }
                    >
                        {saving ? "Kaydediliyor..." : "Kaydet"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={aiDialogOpen}
                onClose={handleCloseAiDialog}
                fullWidth
                maxWidth="md"
            >
                <DialogTitle sx={{ fontWeight: 800 }}>
                    AI Ajanı Yönetimi
                </DialogTitle>

                <DialogContent dividers>
                    <Typography
                        color="text.secondary"
                        sx={{ marginBottom: 2 }}
                    >
                        <strong>{aiManagingOperator?.displayName}</strong>{" "}
                        operatörünün kullanacağı AI ajanlarını yönetin. Ana ajan,
                        çağrı sırasında varsayılan olarak kullanılır.
                    </Typography>

                    {aiLoading ? (
                        <Box
                            sx={{
                                minHeight: 180,
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center"
                            }}
                        >
                            <CircularProgress />
                        </Box>
                    ) : (
                        <>
                            <Grid
                                container
                                spacing={2}
                                sx={{ alignItems: "center", marginBottom: 3 }}
                            >
                                <Grid size={{ xs: 12, md: 8 }}>
                                    <FormControl fullWidth>
                                        <InputLabel>AI Ajanı Seç</InputLabel>
                                        <Select
                                            label="AI Ajanı Seç"
                                            value={selectedAiAgentId}
                                            onChange={(event: SelectChangeEvent) =>
                                                setSelectedAiAgentId(
                                                    event.target.value
                                                )
                                            }
                                        >
                                            {availableAiAgents
                                                .filter(
                                                    (agent) => !agent.isAssigned
                                                )
                                                .map((agent) => (
                                                    <MenuItem
                                                        key={agent.id}
                                                        value={agent.id}
                                                    >
                                                        <Box>
                                                            <Typography
                                                                sx={{ fontWeight: 700 }}
                                                            >
                                                                {agent.name}
                                                            </Typography>
                                                            <Typography
                                                                variant="caption"
                                                                color="text.secondary"
                                                            >
                                                                {agent.provider} ·{" "}
                                                                {agent.modelName}
                                                            </Typography>
                                                        </Box>
                                                    </MenuItem>
                                                ))}
                                        </Select>
                                    </FormControl>
                                </Grid>

                                <Grid size={{ xs: 12, md: 4 }}>
                                    <Button
                                        fullWidth
                                        variant="contained"
                                        startIcon={
                                            aiSaving
                                                ? <CircularProgress size={18} />
                                                : <Add />
                                        }
                                        disabled={
                                            aiSaving ||
                                            !selectedAiAgentId ||
                                            availableAiAgents.every(
                                                (agent) => agent.isAssigned
                                            )
                                        }
                                        onClick={() =>
                                            void handleAssignAiAgent()
                                        }
                                        sx={{ minHeight: 56 }}
                                    >
                                        Ajan Ata
                                    </Button>
                                </Grid>
                            </Grid>

                            {availableAiAgents.length > 0 &&
                                availableAiAgents.every(
                                    (agent) => agent.isAssigned
                                ) && (
                                    <Alert
                                        severity="info"
                                        sx={{ marginBottom: 2 }}
                                    >
                                        Kullanılabilir tüm AI ajanları bu
                                        operatöre atanmış durumda.
                                    </Alert>
                                )}

                            {aiAssignments.length === 0 ? (
                                <Alert severity="warning">
                                    Bu operatöre henüz AI ajanı atanmamış. İlk
                                    atanan ajan otomatik olarak ana ajan olur.
                                </Alert>
                            ) : (
                                <TableContainer
                                    sx={{
                                        border: 1,
                                        borderColor: "divider",
                                        borderRadius: 2
                                    }}
                                >
                                    <Table>
                                        <TableHead>
                                            <TableRow>
                                                <TableCell>AI Ajanı</TableCell>
                                                <TableCell>Sağlayıcı / Model</TableCell>
                                                <TableCell>Durum</TableCell>
                                                <TableCell align="right">
                                                    İşlemler
                                                </TableCell>
                                            </TableRow>
                                        </TableHead>
                                        <TableBody>
                                            {aiAssignments.map((assignment) => (
                                                <TableRow key={assignment.id}>
                                                    <TableCell>
                                                        <Box
                                                            sx={{
                                                                display: "flex",
                                                                alignItems: "center",
                                                                gap: 1
                                                            }}
                                                        >
                                                            <PsychologyOutlined />
                                                            <Box>
                                                                <Typography
                                                                    sx={{ fontWeight: 700 }}
                                                                >
                                                                    {assignment.aiAgentName}
                                                                </Typography>
                                                                <Typography
                                                                    variant="caption"
                                                                    color="text.secondary"
                                                                >
                                                                    Atanma:{" "}
                                                                    {formatDate(
                                                                        assignment.assignedAt
                                                                    )}
                                                                </Typography>
                                                            </Box>
                                                        </Box>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Typography variant="body2">
                                                            {assignment.provider}
                                                        </Typography>
                                                        <Typography
                                                            variant="caption"
                                                            color="text.secondary"
                                                        >
                                                            {assignment.modelName}
                                                        </Typography>
                                                    </TableCell>
                                                    <TableCell>
                                                        <Chip
                                                            size="small"
                                                            color={
                                                                assignment.isPrimary
                                                                    ? "success"
                                                                    : "default"
                                                            }
                                                            icon={
                                                                assignment.isPrimary
                                                                    ? <StarOutlined />
                                                                    : undefined
                                                            }
                                                            label={
                                                                assignment.isPrimary
                                                                    ? "Ana Ajan"
                                                                    : "Yardımcı Ajan"
                                                            }
                                                        />
                                                    </TableCell>
                                                    <TableCell align="right">
                                                        {!assignment.isPrimary && (
                                                            <Tooltip title="Ana ajan yap">
                                                                <span>
                                                                    <IconButton
                                                                        disabled={aiSaving}
                                                                        onClick={() =>
                                                                            void handleSetPrimaryAiAgent(
                                                                                assignment
                                                                            )
                                                                        }
                                                                    >
                                                                        <StarOutlined />
                                                                    </IconButton>
                                                                </span>
                                                            </Tooltip>
                                                        )}
                                                        <Tooltip title="Bağlantıyı kaldır">
                                                            <span>
                                                                <IconButton
                                                                    color="error"
                                                                    disabled={aiSaving}
                                                                    onClick={() =>
                                                                        void handleRemoveAiAgent(
                                                                            assignment
                                                                        )
                                                                    }
                                                                >
                                                                    <LinkOffOutlined />
                                                                </IconButton>
                                                            </span>
                                                        </Tooltip>
                                                    </TableCell>
                                                </TableRow>
                                            ))}
                                        </TableBody>
                                    </Table>
                                </TableContainer>
                            )}
                        </>
                    )}
                </DialogContent>

                <DialogActions sx={{ padding: 2.5 }}>
                    <Button
                        onClick={handleCloseAiDialog}
                        disabled={aiSaving}
                    >
                        Kapat
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={deleteDialogOpen}
                onClose={handleCloseDeleteDialog}
                maxWidth="xs"
                fullWidth
            >
                <DialogTitle sx={{ fontWeight: 800 }}>
                    Operatörü Sil
                </DialogTitle>
                <DialogContent dividers>
                    <Alert severity="warning" sx={{ marginBottom: 2 }}>
                        Bu işlem geri alınamaz.
                    </Alert>
                    <Typography>
                        <strong>{operatorToDelete?.displayName}</strong> isimli
                        operatörü silmek istediğinize emin misiniz?
                    </Typography>
                </DialogContent>
                <DialogActions sx={{ padding: 2.5 }}>
                    <Button
                        onClick={handleCloseDeleteDialog}
                        disabled={deleting}
                    >
                        Vazgeç
                    </Button>
                    <Button
                        color="error"
                        variant="contained"
                        onClick={() => void handleConfirmDelete()}
                        disabled={deleting}
                        startIcon={
                            deleting
                                ? <CircularProgress size={18} />
                                : <DeleteOutlined />
                        }
                    >
                        {deleting ? "Siliniyor..." : "Sil"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={Boolean(successMessage)}
                autoHideDuration={3000}
                onClose={() => setSuccessMessage("")}
                message={successMessage}
            />
        </Box>
    );
}

export default CallOperatorsPage;