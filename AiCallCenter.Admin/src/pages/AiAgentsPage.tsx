import {
    useEffect,
    useMemo,
    useState
} from "react";

import axios from "axios";

import {
    Alert,
    Avatar,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Grid,
    InputAdornment,
    MenuItem,
    Select,
    Snackbar,
    TextField,
    Typography
} from "@mui/material";

import {
    Add,
    CheckCircleOutlined,
    FilterAltOutlined,
    Search,
    SmartToyOutlined,
    ToggleOffOutlined
} from "@mui/icons-material";

import AiAgentCard from
    "../components/aiagents/AiAgentCard";

import AiAgentChatDialog from
    "../components/aiagents/AiAgentChatDialog";

import AiAgentDialog from
    "../components/aiagents/AiAgentDialog";

import AiAgentKnowledgeBasesDialog from
    "../components/aiagents/AiAgentKnowledgeBasesDialog";

import AiAgentSettingsDialog from
    "../components/aiagents/AiAgentSettingsDialog";

import DeleteAiAgentDialog from
    "../components/aiagents/DeleteAiAgentDialog";

import {
    createAiAgent,
    deleteAiAgent,
    getAiAgents,
    updateAiAgent,
    type AiAgent,
    type CreateAiAgentRequest
} from "../services/aiAgentService";

type StatusFilter =
    | "all"
    | "active"
    | "passive";

type ProviderFilter =
    | "all"
    | "1"
    | "2"
    | "3"
    | "4";

type SortOption =
    | "name-asc"
    | "name-desc"
    | "active-first";

const initialForm: CreateAiAgentRequest = {
    name: "",
    instructions:
        "Müşterilere kısa, doğru ve nazik cevaplar ver.",
    welcomeMessage:
        "Merhaba, size nasıl yardımcı olabilirim?",
    language: 1,
    provider: 1,
    modelName: "gpt-5-mini"
};

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
              }
            | undefined;

    return (
        responseData?.detail ??
        responseData?.title ??
        fallbackMessage
    );
}

function getProviderName(
    provider: number
): string {
    switch (provider) {
        case 1:
            return "OpenAI";
        case 2:
            return "Gemini";
        case 3:
            return "Grok";
        case 4:
            return "DeepSeek";
        default:
            return "Bilinmiyor";
    }
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
                boxShadow:
                    "0 12px 30px rgba(15,23,42,0.12)",
                transition:
                    "transform 0.2s ease, box-shadow 0.2s ease",
                "&:hover": {
                    transform: "translateY(-4px)",
                    boxShadow:
                        "0 18px 42px rgba(15,23,42,0.18)"
                }
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
                        justifyContent: "space-between",
                        gap: 2
                    }}
                >
                    <Box>
                        <Typography
                            variant="body2"
                            sx={{
                                opacity: 0.9,
                                fontWeight: 600
                            }}
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
                            sx={{
                                opacity: 0.85,
                                marginTop: 1.25
                            }}
                        >
                            {description}
                        </Typography>
                    </Box>

                    <Avatar
                        sx={{
                            width: 50,
                            height: 50,
                            backgroundColor:
                                "rgba(255,255,255,0.18)",
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

function AiAgentsPage() {
    const [agents, setAgents] =
        useState<AiAgent[]>([]);

    const [loading, setLoading] =
        useState(true);

    const [saving, setSaving] =
        useState(false);

    const [deleting, setDeleting] =
        useState(false);

    const [error, setError] =
        useState("");

    const [
        successMessage,
        setSuccessMessage
    ] = useState("");

    const [
        searchText,
        setSearchText
    ] = useState("");

    const [
        statusFilter,
        setStatusFilter
    ] = useState<StatusFilter>("all");

    const [
        providerFilter,
        setProviderFilter
    ] = useState<ProviderFilter>("all");

    const [
        sortOption,
        setSortOption
    ] = useState<SortOption>("active-first");

    const [
        agentDialogOpen,
        setAgentDialogOpen
    ] = useState(false);

    const [
        deleteDialogOpen,
        setDeleteDialogOpen
    ] = useState(false);

    const [
        knowledgeBaseDialogOpen,
        setKnowledgeBaseDialogOpen
    ] = useState(false);

    const [
        chatDialogOpen,
        setChatDialogOpen
    ] = useState(false);

    const [
        settingsDialogOpen,
        setSettingsDialogOpen
    ] = useState(false);

    const [
        selectedAgent,
        setSelectedAgent
    ] = useState<AiAgent | null>(null);

    const [
        agentToDelete,
        setAgentToDelete
    ] = useState<AiAgent | null>(null);

    const [
        knowledgeBaseAgent,
        setKnowledgeBaseAgent
    ] = useState<AiAgent | null>(null);

    const [
        chatAgent,
        setChatAgent
    ] = useState<AiAgent | null>(null);

    const [
        settingsAgent,
        setSettingsAgent
    ] = useState<AiAgent | null>(null);

    const [form, setForm] =
        useState<CreateAiAgentRequest>(
            initialForm
        );

    const loadAgents = async () => {
        try {
            setError("");

            const data =
                await getAiAgents();

            setAgents(data);
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanları yüklenirken bir hata oluştu."
                )
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const timerId = window.setTimeout(() => {
            void loadAgents();
        }, 0);

        return () => {
            window.clearTimeout(timerId);
        };
    }, []);

    const summary = useMemo(() => {
        const activeCount =
            agents.filter(
                (agent) => agent.isActive
            ).length;

        const passiveCount =
            agents.length - activeCount;

        const providerCounts =
            agents.reduce<Record<number, number>>(
                (counts, agent) => {
                    counts[agent.provider] =
                        (counts[agent.provider] ?? 0) + 1;

                    return counts;
                },
                {}
            );

        return {
            totalCount: agents.length,
            activeCount,
            passiveCount,
            providerCounts
        };
    }, [agents]);

    const filteredAgents = useMemo(() => {
        const normalizedSearch =
            searchText.trim().toLocaleLowerCase("tr-TR");

        const filtered =
            agents.filter((agent) => {
                const matchesSearch =
                    !normalizedSearch ||
                    agent.name
                        .toLocaleLowerCase("tr-TR")
                        .includes(normalizedSearch) ||
                    agent.modelName
                        .toLocaleLowerCase("tr-TR")
                        .includes(normalizedSearch) ||
                    getProviderName(agent.provider)
                        .toLocaleLowerCase("tr-TR")
                        .includes(normalizedSearch);

                const matchesStatus =
                    statusFilter === "all" ||
                    (statusFilter === "active" &&
                        agent.isActive) ||
                    (statusFilter === "passive" &&
                        !agent.isActive);

                const matchesProvider =
                    providerFilter === "all" ||
                    agent.provider ===
                        Number(providerFilter);

                return (
                    matchesSearch &&
                    matchesStatus &&
                    matchesProvider
                );
            });

        return [...filtered].sort((a, b) => {
            if (sortOption === "name-asc") {
                return a.name.localeCompare(
                    b.name,
                    "tr"
                );
            }

            if (sortOption === "name-desc") {
                return b.name.localeCompare(
                    a.name,
                    "tr"
                );
            }

            return Number(b.isActive) -
                Number(a.isActive);
        });
    }, [
        agents,
        providerFilter,
        searchText,
        sortOption,
        statusFilter
    ]);

    const handleOpenCreate = () => {
        setSelectedAgent(null);
        setForm(initialForm);
        setError("");
        setAgentDialogOpen(true);
    };

    const handleOpenEdit = (
        agent: AiAgent
    ) => {
        setSelectedAgent(agent);

        setForm({
            name: agent.name,
            instructions:
                agent.instructions,
            welcomeMessage:
                agent.welcomeMessage,
            language:
                agent.language,
            provider:
                agent.provider,
            modelName:
                agent.modelName
        });

        setError("");
        setAgentDialogOpen(true);
    };

    const handleOpenKnowledgeBases = (
        agent: AiAgent
    ) => {
        setKnowledgeBaseAgent(agent);
        setError("");
        setKnowledgeBaseDialogOpen(true);
    };

    const handleCloseKnowledgeBases = () => {
        setKnowledgeBaseDialogOpen(false);
        setKnowledgeBaseAgent(null);
    };

    const handleOpenChat = (
        agent: AiAgent
    ) => {
        setChatAgent(agent);
        setError("");
        setChatDialogOpen(true);
    };

    const handleCloseChat = () => {
        setChatDialogOpen(false);
        setChatAgent(null);
    };

    const handleOpenSettings = (
        agent: AiAgent
    ) => {
        setSettingsAgent(agent);
        setError("");
        setSettingsDialogOpen(true);
    };

    const handleCloseSettings = () => {
        setSettingsDialogOpen(false);
        setSettingsAgent(null);
    };

    const handleSettingsUpdated = (
        updatedAgent: AiAgent
    ) => {
        setAgents((current) =>
            current.map((agent) =>
                agent.id === updatedAgent.id
                    ? {
                          ...agent,
                          ...updatedAgent
                      }
                    : agent
            )
        );

        setSuccessMessage(
            "AI ajan ayarları başarıyla güncellendi."
        );
    };

    const handleCloseAgentDialog = () => {
        if (saving) {
            return;
        }

        setAgentDialogOpen(false);
        setSelectedAgent(null);
        setForm(initialForm);
    };

    const handleSave = async () => {
        if (!form.name.trim()) {
            setError(
                "Ajan adı zorunludur."
            );
            return;
        }

        if (!form.instructions.trim()) {
            setError(
                "Ajan talimatları zorunludur."
            );
            return;
        }

        if (!form.welcomeMessage.trim()) {
            setError(
                "Karşılama mesajı zorunludur."
            );
            return;
        }

        setSaving(true);
        setError("");

        try {
            if (selectedAgent) {
                const updatedAgent =
                    await updateAiAgent(
                        selectedAgent.id,
                        form
                    );

                setAgents((current) =>
                    current.map((agent) =>
                        agent.id ===
                        updatedAgent.id
                            ? updatedAgent
                            : agent
                    )
                );

                setSuccessMessage(
                    "AI ajanı başarıyla güncellendi."
                );
            } else {
                const createdAgent =
                    await createAiAgent(
                        form
                    );

                setAgents((current) => [
                    createdAgent,
                    ...current
                ]);

                setSuccessMessage(
                    "AI ajanı başarıyla oluşturuldu."
                );
            }

            setAgentDialogOpen(false);
            setSelectedAgent(null);
            setForm(initialForm);
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanı kaydedilemedi."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    const handleOpenDelete = (
        agent: AiAgent
    ) => {
        setAgentToDelete(agent);
        setError("");
        setDeleteDialogOpen(true);
    };

    const handleCloseDeleteDialog = () => {
        if (deleting) {
            return;
        }

        setDeleteDialogOpen(false);
        setAgentToDelete(null);
    };

    const handleConfirmDelete = async () => {
        if (!agentToDelete) {
            return;
        }

        setDeleting(true);
        setError("");

        try {
            await deleteAiAgent(
                agentToDelete.id
            );

            setAgents((current) =>
                current.filter(
                    (agent) =>
                        agent.id !==
                        agentToDelete.id
                )
            );

            setDeleteDialogOpen(false);
            setAgentToDelete(null);

            setSuccessMessage(
                "AI ajanı başarıyla silindi."
            );
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajanı silinemedi."
                )
            );
        } finally {
            setDeleting(false);
        }
    };

    if (loading) {
        return (
            <Box
                sx={{
                    minHeight: 400,
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center"
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box
            sx={{
                minHeight: "100vh",
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
                    marginBottom: 3,
                    gap: 2
                }}
            >
                <Box>
                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{
                            fontWeight: 800
                        }}
                    >
                        AI Ajanları
                    </Typography>

                    <Typography
                        color="text.secondary"
                        sx={{
                            marginTop: 0.5
                        }}
                    >
                        Yapay zekâ ajanlarınızı yönetin,
                        test edin ve geliştirin.
                    </Typography>
                </Box>

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
                    Yeni Ajan
                </Button>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    sx={{
                        marginBottom: 3
                    }}
                    onClose={() =>
                        setError("")
                    }
                >
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <StatCard
                        title="Toplam Ajan"
                        value={summary.totalCount}
                        description="Sistemde kayıtlı ajanlar"
                        icon={<SmartToyOutlined />}
                        gradient="linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)"
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <StatCard
                        title="Aktif Ajan"
                        value={summary.activeCount}
                        description="Kullanıma hazır ajanlar"
                        icon={<CheckCircleOutlined />}
                        gradient="linear-gradient(135deg, #059669 0%, #10B981 100%)"
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <StatCard
                        title="Pasif Ajan"
                        value={summary.passiveCount}
                        description="Devre dışı ajanlar"
                        icon={<ToggleOffOutlined />}
                        gradient="linear-gradient(135deg, #64748B 0%, #94A3B8 100%)"
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <StatCard
                        title="Provider Sayısı"
                        value={
                            Object.values(
                                summary.providerCounts
                            ).filter(
                                (count) => count > 0
                            ).length
                        }
                        description="Kullanılan AI sağlayıcıları"
                        icon={<FilterAltOutlined />}
                        gradient="linear-gradient(135deg, #EA580C 0%, #F59E0B 100%)"
                    />
                </Grid>
            </Grid>

            <Card
                sx={{
                    marginTop: 3,
                    marginBottom: 3,
                    borderRadius: 4,
                    border: 1,
                    borderColor: "divider",
                    boxShadow:
                        "0 10px 30px rgba(15,23,42,0.06)"
                }}
            >
                <CardContent
                    sx={{
                        padding: 3
                    }}
                >
                    <Grid
                        container
                        spacing={2}
                        sx={{
                            alignItems: "center"
                        }}
                    >
                        <Grid size={{ xs: 12, md: 5 }}>
                            <TextField
                                fullWidth
                                value={searchText}
                                onChange={(event) =>
                                    setSearchText(
                                        event.target.value
                                    )
                                }
                                placeholder="Ajan, model veya provider ara..."
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

                        <Grid size={{ xs: 12, sm: 4, md: 2.5 }}>
                            <Select
                                fullWidth
                                value={statusFilter}
                                onChange={(event) =>
                                    setStatusFilter(
                                        event.target
                                            .value as StatusFilter
                                    )
                                }
                            >
                                <MenuItem value="all">
                                    Tüm Durumlar
                                </MenuItem>
                                <MenuItem value="active">
                                    Aktif
                                </MenuItem>
                                <MenuItem value="passive">
                                    Pasif
                                </MenuItem>
                            </Select>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 4, md: 2.5 }}>
                            <Select
                                fullWidth
                                value={providerFilter}
                                onChange={(event) =>
                                    setProviderFilter(
                                        event.target
                                            .value as ProviderFilter
                                    )
                                }
                            >
                                <MenuItem value="all">
                                    Tüm Providerlar
                                </MenuItem>
                                <MenuItem value="1">
                                    OpenAI
                                </MenuItem>
                                <MenuItem value="2">
                                    Gemini
                                </MenuItem>
                                <MenuItem value="3">
                                    Grok
                                </MenuItem>
                                <MenuItem value="4">
                                    DeepSeek
                                </MenuItem>
                            </Select>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 4, md: 2 }}>
                            <Select
                                fullWidth
                                value={sortOption}
                                onChange={(event) =>
                                    setSortOption(
                                        event.target
                                            .value as SortOption
                                    )
                                }
                            >
                                <MenuItem value="active-first">
                                    Aktif Önce
                                </MenuItem>
                                <MenuItem value="name-asc">
                                    A-Z
                                </MenuItem>
                                <MenuItem value="name-desc">
                                    Z-A
                                </MenuItem>
                            </Select>
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
                        <Typography
                            variant="body2"
                            color="text.secondary"
                        >
                            {filteredAgents.length} ajan gösteriliyor
                        </Typography>

                        <Box
                            sx={{
                                display: "flex",
                                gap: 1,
                                flexWrap: "wrap"
                            }}
                        >
                            {Object.entries(
                                summary.providerCounts
                            ).map(([provider, count]) => (
                                <Chip
                                    key={provider}
                                    size="small"
                                    label={`${getProviderName(
                                        Number(provider)
                                    )}: ${count}`}
                                    variant="outlined"
                                />
                            ))}
                        </Box>
                    </Box>
                </CardContent>
            </Card>

            {agents.length === 0 ? (
                <Card
                    sx={{
                        borderRadius: 4,
                        border: 1,
                        borderColor: "divider"
                    }}
                >
                    <CardContent
                        sx={{
                            textAlign: "center",
                            padding: 7
                        }}
                    >
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
                            <SmartToyOutlined
                                sx={{
                                    fontSize: 38
                                }}
                            />
                        </Avatar>

                        <Typography
                            variant="h6"
                            sx={{
                                fontWeight: 800
                            }}
                        >
                            Henüz AI ajanı bulunmuyor
                        </Typography>

                        <Typography
                            color="text.secondary"
                            sx={{
                                marginTop: 1,
                                marginBottom: 3
                            }}
                        >
                            İlk ajanınızı oluşturarak başlayın.
                        </Typography>

                        <Button
                            variant="contained"
                            startIcon={<Add />}
                            onClick={handleOpenCreate}
                        >
                            İlk Ajanı Oluştur
                        </Button>
                    </CardContent>
                </Card>
            ) : filteredAgents.length === 0 ? (
                <Alert severity="info">
                    Seçtiğiniz filtrelere uygun ajan bulunamadı.
                </Alert>
            ) : (
                <Grid
                    container
                    spacing={3}
                >
                    {filteredAgents.map(
                        (agent) => (
                            <Grid
                                key={agent.id}
                                size={{
                                    xs: 12,
                                    md: 6,
                                    xl: 4
                                }}
                            >
                                <AiAgentCard
                                    agent={agent}
                                    onEdit={
                                        handleOpenEdit
                                    }
                                    onDelete={
                                        handleOpenDelete
                                    }
                                    onManageKnowledgeBases={
                                        handleOpenKnowledgeBases
                                    }
                                    onTest={
                                        handleOpenChat
                                    }
                                    onSettings={
                                        handleOpenSettings
                                    }
                                />
                            </Grid>
                        )
                    )}
                </Grid>
            )}

            <AiAgentDialog
                open={agentDialogOpen}
                title={
                    selectedAgent
                        ? "AI Ajanını Düzenle"
                        : "Yeni AI Ajanı"
                }
                saving={saving}
                form={form}
                onClose={
                    handleCloseAgentDialog
                }
                onSave={handleSave}
                onChange={setForm}
            />

            <DeleteAiAgentDialog
                open={deleteDialogOpen}
                agentName={
                    agentToDelete?.name ??
                    ""
                }
                deleting={deleting}
                onClose={
                    handleCloseDeleteDialog
                }
                onConfirm={
                    handleConfirmDelete
                }
            />

            <AiAgentKnowledgeBasesDialog
                open={
                    knowledgeBaseDialogOpen
                }
                agent={
                    knowledgeBaseAgent
                }
                onClose={
                    handleCloseKnowledgeBases
                }
            />

            <AiAgentChatDialog
                open={chatDialogOpen}
                agent={chatAgent}
                onClose={handleCloseChat}
            />

            <AiAgentSettingsDialog
                open={settingsDialogOpen}
                agent={settingsAgent}
                onClose={handleCloseSettings}
                onUpdated={
                    handleSettingsUpdated
                }
            />

            <Snackbar
                open={Boolean(
                    successMessage
                )}
                autoHideDuration={3000}
                onClose={() =>
                    setSuccessMessage("")
                }
                message={
                    successMessage
                }
            />
        </Box>
    );
}

export default AiAgentsPage;