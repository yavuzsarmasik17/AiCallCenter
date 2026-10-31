import {
    useCallback,
    useEffect,
    useMemo,
    useState
} from "react";

import type {
    ReactNode
} from "react";

import {
    Alert,
    Avatar,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Divider,
    Grid,
    Typography,
    useTheme
} from "@mui/material";

import {
    AutoAwesome,
    ChatBubbleOutlined,
    DescriptionOutlined,
    ErrorOutlined,
    RefreshOutlined,
    StorageOutlined,
    TrendingUpOutlined
} from "@mui/icons-material";

import {
    Bar,
    BarChart,
    CartesianGrid,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis
} from "recharts";

import {
    getDashboardSummary
} from "../services/dashboardService";

import type {
    DashboardSummaryResponse
} from "../services/dashboardService";

interface StoredUser {
    fullName?: string;
    email?: string;
    role?: number;
    companyId?: string;
}

interface SummaryCardProps {
    title: string;
    value: number;
    description: string;
    icon: ReactNode;
}

interface DetailCardProps {
    title: string;
    value: number;
    description: string;
    icon?: ReactNode;
}

function readStoredUser(): StoredUser {
    const storedUser = localStorage.getItem("user");

    if (!storedUser) {
        return {};
    }

    try {
        return JSON.parse(storedUser) as StoredUser;
    } catch {
        return {};
    }
}

function getDocumentStatusColor(
    status: string
): "success" | "warning" | "error" | "default" {
    const normalizedStatus = status.toLowerCase();

    if (
        normalizedStatus === "ready" ||
        normalizedStatus === "hazır"
    ) {
        return "success";
    }

    if (
        normalizedStatus === "processing" ||
        normalizedStatus === "işleniyor"
    ) {
        return "warning";
    }

    if (
        normalizedStatus === "failed" ||
        normalizedStatus === "hatalı"
    ) {
        return "error";
    }

    return "default";
}

function SummaryCard({
    title,
    value,
    description,
    icon
}: SummaryCardProps) {
    return (
        <Card
            sx={{
                height: "100%",
                borderRadius: 4,
                boxShadow: 3,
                border: 0,
                background:
                    "linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)",
                color: "common.white",
                transition: "transform 0.2s, box-shadow 0.2s",
                "&:hover": {
                    transform: "translateY(-4px)",
                    boxShadow: 8
                }
            }}
        >
            <CardContent
                sx={{
                    height: "100%",
                    padding: 3
                }}
            >
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "flex-start",
                        gap: 2
                    }}
                >
                    <Box>
                        <Typography
                            variant="body2"
                            sx={{
                                color: "rgba(255,255,255,0.88)",
                                fontWeight: 600
                            }}
                        >
                            {title}
                        </Typography>

                        <Typography
                            variant="h4"
                            sx={{
                                marginTop: 1,
                                fontWeight: 700
                            }}
                        >
                            {value}
                        </Typography>

                        <Typography
                            variant="body2"
                            sx={{
                                marginTop: 1,
                                color: "rgba(255,255,255,0.82)"
                            }}
                        >
                            {description}
                        </Typography>
                    </Box>

                    <Box
                        sx={{
                            width: 52,
                            height: 52,
                            borderRadius: 2.5,
                            backgroundColor: "rgba(255,255,255,0.18)",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            flexShrink: 0
                        }}
                    >
                        {icon}
                    </Box>
                </Box>
            </CardContent>
        </Card>
    );
}

function DetailCard({
    title,
    value,
    description,
    icon
}: DetailCardProps) {
    const appearance =
        title === "Aktif Sohbet"
            ? {
                  accent: "#2563EB",
                  background:
                      "linear-gradient(135deg, rgba(37,99,235,0.11) 0%, rgba(255,255,255,0.98) 72%)"
              }
            : title === "Bilgi Bankası"
              ? {
                    accent: "#7C3AED",
                    background:
                        "linear-gradient(135deg, rgba(124,58,237,0.11) 0%, rgba(255,255,255,0.98) 72%)"
                }
              : {
                    accent: "#DC2626",
                    background:
                        "linear-gradient(135deg, rgba(220,38,38,0.11) 0%, rgba(255,255,255,0.98) 72%)"
                };

    return (
        <Card
            sx={{
                height: "100%",
                borderRadius: 4,
                border: 1,
                borderColor: "divider",
                borderTop: `4px solid ${appearance.accent}`,
                background: appearance.background,
                boxShadow: "0 8px 24px rgba(15,23,42,0.06)",
                transition: "transform 0.2s ease, box-shadow 0.2s ease",
                "&:hover": {
                    transform: "translateY(-3px)",
                    boxShadow: "0 14px 30px rgba(15,23,42,0.10)"
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
                        alignItems: "center",
                        gap: 2
                    }}
                >
                    <Box>
                        <Typography
                            variant="body2"
                            sx={{
                                color: "text.secondary",
                                fontWeight: 700,
                                letterSpacing: 0.2
                            }}
                        >
                            {title}
                        </Typography>

                        <Typography
                            variant="h4"
                            sx={{
                                marginTop: 0.75,
                                fontWeight: 800,
                                color: appearance.accent
                            }}
                        >
                            {value}
                        </Typography>

                        <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{
                                marginTop: 0.75
                            }}
                        >
                            {description}
                        </Typography>
                    </Box>

                    {icon && (
                        <Avatar
                            sx={{
                                width: 50,
                                height: 50,
                                backgroundColor: appearance.accent,
                                color: "common.white",
                                boxShadow: `0 8px 20px ${appearance.accent}40`
                            }}
                        >
                            {icon}
                        </Avatar>
                    )}
                </Box>
            </CardContent>
        </Card>
    );
}

function DashboardPage() {
    const theme = useTheme();
    const [dashboard, setDashboard] =
        useState<DashboardSummaryResponse | null>(null);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const user = useMemo(() => readStoredUser(), []);

    const companyId =
        user.companyId ??
        localStorage.getItem("companyId") ??
        "";

    const loadDashboard = useCallback(async () => {
        if (!companyId) {
            setDashboard(null);
            setLoading(false);
            setError("Firma kimliği bulunamadı.");
            return;
        }

        try {
            setLoading(true);
            setError("");

            const response = await getDashboardSummary(companyId);

            setDashboard(response);
        } catch (requestError) {
            console.error(requestError);
            setDashboard(null);
            setError(
                "Dashboard verileri alınamadı. API bağlantısını ve firma kimliğini kontrol edin."
            );
        } finally {
            setLoading(false);
        }
    }, [companyId]);

    useEffect(() => {
        const timerId = window.setTimeout(() => {
            void loadDashboard();
        }, 0);

        return () => {
            window.clearTimeout(timerId);
        };
    }, [loadDashboard]);

    const handleLogout = () => {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("user");
        localStorage.removeItem("companyId");

        window.location.replace("/login");
    };

    if (loading) {
        return (
            <Box
                sx={{
                    minHeight: 500,
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center"
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    if (!dashboard) {
        return (
            <Alert severity="error">
                {error || "Dashboard verisi bulunamadı."}
            </Alert>
        );
    }

    return (
        <Box
            sx={{
                minHeight: "100vh",
                backgroundColor: "background.default",
                padding: {
                    xs: 2,
                    md: 3
                }
            }}
        >
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
                    gap: 2,
                    marginBottom: 4
                }}
            >
                <Box>
                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{
                            fontWeight: 700
                        }}
                    >
                        Kontrol Paneli
                    </Typography>

                    <Typography color="text.secondary">
                        Hoş geldiniz, {user.fullName ?? "Kullanıcı"}
                    </Typography>
                </Box>

                <Box
                    sx={{
                        display: "flex",
                        gap: 2,
                        flexWrap: "wrap"
                    }}
                >
                    <Button
                        variant="contained"
                        startIcon={<RefreshOutlined />}
                        onClick={() => {
                            void loadDashboard();
                        }}
                    >
                        Yenile
                    </Button>

                    <Button
                        variant="outlined"
                        onClick={handleLogout}
                    >
                        Çıkış Yap
                    </Button>
                </Box>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    sx={{
                        marginBottom: 3
                    }}
                >
                    {error}
                </Alert>
            )}

            <Grid container spacing={3}>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <SummaryCard
                        title="AI Ajanları"
                        value={dashboard.agents.activeCount}
                        description={`${dashboard.agents.totalCount} toplam ajan`}
                        icon={<AutoAwesome />}
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <SummaryCard
                        title="Sohbetler"
                        value={dashboard.conversations.totalCount}
                        description={`${dashboard.conversations.todayCount} bugün`}
                        icon={<ChatBubbleOutlined />}
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <SummaryCard
                        title="Belgeler"
                        value={dashboard.knowledge.documentCount}
                        description={`${dashboard.knowledge.readyDocumentCount} hazır`}
                        icon={<DescriptionOutlined />}
                    />
                </Grid>

                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <SummaryCard
                        title="Chunk"
                        value={dashboard.knowledge.chunkCount}
                        description={`${dashboard.knowledge.embeddedChunkCount} embedding`}
                        icon={<StorageOutlined />}
                    />
                </Grid>
            </Grid>

            <Grid
                container
                spacing={3}
                sx={{
                    marginTop: 3
                }}
            >
                <Grid size={{ xs: 12, md: 4 }}>
                    <DetailCard
                        title="Aktif Sohbet"
                        value={dashboard.conversations.activeCount}
                        description="Devam eden konuşmalar"
                        icon={<ChatBubbleOutlined />}
                    />
                </Grid>

                <Grid size={{ xs: 12, md: 4 }}>
                    <DetailCard
                        title="Bilgi Bankası"
                        value={dashboard.knowledge.knowledgeBaseCount}
                        description="Toplam bilgi bankası"
                        icon={<StorageOutlined />}
                    />
                </Grid>

                <Grid size={{ xs: 12, md: 4 }}>
                    <DetailCard
                        title="Hatalı Belgeler"
                        value={dashboard.knowledge.failedDocumentCount}
                        description="İşlenemeyen belgeler"
                        icon={<ErrorOutlined color="error" />}
                    />
                </Grid>
            </Grid>

            <Card
                sx={{
                    marginTop: 3,
                    borderRadius: 4,
                    border: 1,
                    borderColor: "divider",
                    boxShadow: "0 12px 32px rgba(15,23,42,0.07)",
                    overflow: "hidden"
                }}
            >
                <Box
                    sx={{
                        padding: {
                            xs: 2.5,
                            md: 3
                        },
                        background:
                            "linear-gradient(135deg, rgba(37,99,235,0.10) 0%, rgba(124,58,237,0.06) 55%, rgba(255,255,255,0) 100%)",
                        borderBottom: 1,
                        borderColor: "divider"
                    }}
                >
                    <Box
                        sx={{
                            display: "flex",
                            justifyContent: "space-between",
                            alignItems: {
                                xs: "flex-start",
                                sm: "center"
                            },
                            flexDirection: {
                                xs: "column",
                                sm: "row"
                            },
                            gap: 2
                        }}
                    >
                        <Box>
                            <Typography
                                variant="h5"
                                sx={{
                                    fontWeight: 800
                                }}
                            >
                                Sohbet Analizi
                            </Typography>

                            <Typography
                                variant="body2"
                                color="text.secondary"
                                sx={{
                                    marginTop: 0.5
                                }}
                            >
                                Son 7 günlük görüşme performansı
                            </Typography>
                        </Box>

                        <Chip
                            icon={<TrendingUpOutlined />}
                            label={`${dashboard.conversations.todayCount} bugün`}
                            color="primary"
                            sx={{
                                fontWeight: 700
                            }}
                        />
                    </Box>

                    <Grid
                        container
                        spacing={2}
                        sx={{
                            marginTop: 1
                        }}
                    >
                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Box
                                sx={{
                                    padding: 2,
                                    borderRadius: 3,
                                    backgroundColor:
                                        "rgba(255,255,255,0.78)",
                                    border: 1,
                                    borderColor: "divider"
                                }}
                            >
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                >
                                    Toplam sohbet
                                </Typography>

                                <Typography
                                    variant="h5"
                                    sx={{
                                        fontWeight: 800,
                                        marginTop: 0.5
                                    }}
                                >
                                    {dashboard.conversations.totalCount}
                                </Typography>
                            </Box>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Box
                                sx={{
                                    padding: 2,
                                    borderRadius: 3,
                                    backgroundColor:
                                        "rgba(255,255,255,0.78)",
                                    border: 1,
                                    borderColor: "divider"
                                }}
                            >
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                >
                                    Aktif görüşme
                                </Typography>

                                <Typography
                                    variant="h5"
                                    sx={{
                                        fontWeight: 800,
                                        marginTop: 0.5
                                    }}
                                >
                                    {dashboard.conversations.activeCount}
                                </Typography>
                            </Box>
                        </Grid>

                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Box
                                sx={{
                                    padding: 2,
                                    borderRadius: 3,
                                    backgroundColor:
                                        "rgba(255,255,255,0.78)",
                                    border: 1,
                                    borderColor: "divider"
                                }}
                            >
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                >
                                    Tamamlanan
                                </Typography>

                                <Typography
                                    variant="h5"
                                    sx={{
                                        fontWeight: 800,
                                        marginTop: 0.5
                                    }}
                                >
                                    {
                                        dashboard.conversations
                                            .completedCount
                                    }
                                </Typography>
                            </Box>
                        </Grid>
                    </Grid>
                </Box>

                <CardContent
                    sx={{
                        padding: {
                            xs: 2,
                            md: 3
                        }
                    }}
                >
                    <Box
                        sx={{
                            width: "100%",
                            height: 340
                        }}
                    >
                        <ResponsiveContainer
                            width="100%"
                            height="100%"
                        >
                            <BarChart
                                data={
                                    dashboard.lastSevenDaysConversations
                                }
                                margin={{
                                    top: 10,
                                    right: 20,
                                    left: 0,
                                    bottom: 0
                                }}
                            >
                                <CartesianGrid
                                    strokeDasharray="3 3"
                                    vertical={false}
                                    stroke={theme.palette.divider}
                                />

                                <XAxis
                                    dataKey="dayName"
                                    tickLine={false}
                                    axisLine={false}
                                />

                                <YAxis
                                    allowDecimals={false}
                                    tickLine={false}
                                    axisLine={false}
                                />

                                <Tooltip
                                    cursor={{
                                        fill:
                                            theme.palette.action.hover
                                    }}
                                    contentStyle={{
                                        borderRadius: 12,
                                        border:
                                            `1px solid ${theme.palette.divider}`,
                                        boxShadow:
                                            "0 10px 30px rgba(15,23,42,0.12)"
                                    }}
                                />

                                <Bar
                                    dataKey="count"
                                    radius={[10, 10, 0, 0]}
                                    fill={theme.palette.primary.main}
                                    maxBarSize={84}
                                />
                            </BarChart>
                        </ResponsiveContainer>
                    </Box>
                </CardContent>
            </Card>

            <Grid
                container
                spacing={3}
                sx={{
                    marginTop: 3
                }}
            >
                <Grid size={{ xs: 12, lg: 7 }}>
                    <Card
                        sx={{
                            height: "100%",
                            borderRadius: 4,
                            border: 1,
                            borderColor: "divider",
                            boxShadow:
                                "0 12px 32px rgba(15,23,42,0.07)",
                            overflow: "hidden"
                        }}
                    >
                        <Box
                            sx={{
                                padding: 3,
                                background:
                                    "linear-gradient(135deg, rgba(5,150,105,0.11) 0%, rgba(255,255,255,0) 100%)",
                                borderBottom: 1,
                                borderColor: "divider"
                            }}
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    justifyContent:
                                        "space-between",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <Box>
                                    <Typography
                                        variant="h6"
                                        sx={{
                                            fontWeight: 800
                                        }}
                                    >
                                        Son Belgeler
                                    </Typography>

                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        Bilgi bankasına son eklenen içerikler
                                    </Typography>
                                </Box>

                                <Avatar
                                    sx={{
                                        backgroundColor:
                                            "success.main",
                                        color: "common.white"
                                    }}
                                >
                                    <DescriptionOutlined />
                                </Avatar>
                            </Box>
                        </Box>

                        <CardContent
                            sx={{
                                padding: 3
                            }}
                        >
                            {dashboard.recentDocuments.length ===
                            0 ? (
                                <Typography color="text.secondary">
                                    Belge bulunamadı.
                                </Typography>
                            ) : (
                                dashboard.recentDocuments.map(
                                    (document, index) => (
                                        <Box
                                            key={
                                                document.documentId
                                            }
                                        >
                                            <Box
                                                sx={{
                                                    display: "flex",
                                                    alignItems: {
                                                        xs: "flex-start",
                                                        sm: "center"
                                                    },
                                                    flexDirection: {
                                                        xs: "column",
                                                        sm: "row"
                                                    },
                                                    gap: 2,
                                                    paddingY: 2
                                                }}
                                            >
                                                <Avatar
                                                    variant="rounded"
                                                    sx={{
                                                        width: 48,
                                                        height: 48,
                                                        backgroundColor:
                                                            "success.light",
                                                        color:
                                                            "success.dark",
                                                        flexShrink: 0
                                                    }}
                                                >
                                                    <DescriptionOutlined />
                                                </Avatar>

                                                <Box
                                                    sx={{
                                                        flexGrow: 1,
                                                        minWidth: 0
                                                    }}
                                                >
                                                    <Typography
                                                        sx={{
                                                            fontWeight: 800
                                                        }}
                                                    >
                                                        {
                                                            document.documentName
                                                        }
                                                    </Typography>

                                                    <Typography
                                                        variant="body2"
                                                        color="text.secondary"
                                                        sx={{
                                                            marginTop: 0.4
                                                        }}
                                                    >
                                                        {
                                                            document.knowledgeBaseName
                                                        }
                                                    </Typography>

                                                    <Box
                                                        sx={{
                                                            display: "flex",
                                                            alignItems:
                                                                "center",
                                                            gap: 1,
                                                            flexWrap:
                                                                "wrap",
                                                            marginTop: 0.8
                                                        }}
                                                    >
                                                        <Chip
                                                            size="small"
                                                            variant="outlined"
                                                            label={
                                                                document.documentType
                                                            }
                                                        />

                                                        <Chip
                                                            size="small"
                                                            variant="outlined"
                                                            label={`${document.chunkCount} chunk`}
                                                        />
                                                    </Box>
                                                </Box>

                                                <Chip
                                                    size="small"
                                                    color={getDocumentStatusColor(
                                                        document.status
                                                    )}
                                                    label={
                                                        document.status
                                                    }
                                                    sx={{
                                                        fontWeight: 700
                                                    }}
                                                />
                                            </Box>

                                            {index <
                                                dashboard
                                                    .recentDocuments
                                                    .length -
                                                    1 && <Divider />}
                                        </Box>
                                    )
                                )
                            )}
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, lg: 5 }}>
                    <Card
                        sx={{
                            height: "100%",
                            borderRadius: 4,
                            border: 1,
                            borderColor: "divider",
                            boxShadow:
                                "0 12px 32px rgba(15,23,42,0.07)",
                            overflow: "hidden"
                        }}
                    >
                        <Box
                            sx={{
                                padding: 3,
                                background:
                                    "linear-gradient(135deg, rgba(124,58,237,0.12) 0%, rgba(255,255,255,0) 100%)",
                                borderBottom: 1,
                                borderColor: "divider"
                            }}
                        >
                            <Box
                                sx={{
                                    display: "flex",
                                    justifyContent:
                                        "space-between",
                                    alignItems: "center",
                                    gap: 2
                                }}
                            >
                                <Box>
                                    <Typography
                                        variant="h6"
                                        sx={{
                                            fontWeight: 800
                                        }}
                                    >
                                        En Aktif AI Ajanları
                                    </Typography>

                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                    >
                                        En çok sohbet gerçekleştiren ajanlar
                                    </Typography>
                                </Box>

                                <Avatar
                                    sx={{
                                        backgroundColor:
                                            "secondary.main",
                                        color: "common.white"
                                    }}
                                >
                                    <AutoAwesome />
                                </Avatar>
                            </Box>
                        </Box>

                        <CardContent
                            sx={{
                                padding: 3
                            }}
                        >
                            {dashboard.topAgents.length === 0 ? (
                                <Typography color="text.secondary">
                                    Veri bulunamadı.
                                </Typography>
                            ) : (
                                dashboard.topAgents.map(
                                    (agent, index) => (
                                        <Box
                                            key={agent.aiAgentId}
                                        >
                                            <Box
                                                sx={{
                                                    display: "flex",
                                                    alignItems: "center",
                                                    gap: 2,
                                                    paddingY: 2
                                                }}
                                            >
                                                <Avatar
                                                    sx={{
                                                        width: 52,
                                                        height: 52,
                                                        background:
                                                            "linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)",
                                                        fontWeight: 800
                                                    }}
                                                >
                                                    {agent.aiAgentName
                                                        .charAt(0)
                                                        .toUpperCase()}
                                                </Avatar>

                                                <Box
                                                    sx={{
                                                        flexGrow: 1,
                                                        minWidth: 0
                                                    }}
                                                >
                                                    <Box
                                                        sx={{
                                                            display: "flex",
                                                            alignItems:
                                                                "center",
                                                            gap: 1,
                                                            flexWrap:
                                                                "wrap"
                                                        }}
                                                    >
                                                        <Typography
                                                            sx={{
                                                                fontWeight: 800
                                                            }}
                                                        >
                                                            {
                                                                agent.aiAgentName
                                                            }
                                                        </Typography>

                                                        <Chip
                                                            size="small"
                                                            color={
                                                                agent.isActive
                                                                    ? "success"
                                                                    : "default"
                                                            }
                                                            label={
                                                                agent.isActive
                                                                    ? "Aktif"
                                                                    : "Pasif"
                                                            }
                                                        />
                                                    </Box>

                                                    <Typography
                                                        variant="body2"
                                                        color="text.secondary"
                                                        sx={{
                                                            marginTop: 0.5
                                                        }}
                                                    >
                                                        {
                                                            agent.provider
                                                        }
                                                        {" • "}
                                                        {
                                                            agent.modelName
                                                        }
                                                    </Typography>
                                                </Box>

                                                <Box
                                                    sx={{
                                                        minWidth: 78,
                                                        textAlign:
                                                            "right",
                                                        padding: 1.25,
                                                        borderRadius: 2.5,
                                                        backgroundColor:
                                                            "action.hover"
                                                    }}
                                                >
                                                    <Typography
                                                        variant="h6"
                                                        sx={{
                                                            fontWeight: 800,
                                                            lineHeight: 1
                                                        }}
                                                    >
                                                        {
                                                            agent.conversationCount
                                                        }
                                                    </Typography>

                                                    <Typography
                                                        variant="caption"
                                                        color="text.secondary"
                                                    >
                                                        sohbet
                                                    </Typography>
                                                </Box>
                                            </Box>

                                            {index <
                                                dashboard.topAgents
                                                    .length -
                                                    1 && <Divider />}
                                        </Box>
                                    )
                                )
                            )}
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>
        </Box>
    );
}

export default DashboardPage;