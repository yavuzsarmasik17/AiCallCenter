import {
    Avatar,
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    Divider,
    Typography
} from "@mui/material";

import {
    ChatOutlined,
    DeleteOutlined,
    EditOutlined,
    MenuBookOutlined,
    SettingsOutlined,
    SmartToyOutlined
} from "@mui/icons-material";

import type {
    AiAgent
} from "../../services/aiAgentService";

interface AiAgentCardProps {
    agent: AiAgent;
    onEdit: (agent: AiAgent) => void;
    onDelete: (agent: AiAgent) => void;
    onManageKnowledgeBases: (agent: AiAgent) => void;
    onTest: (agent: AiAgent) => void;
    onSettings: (agent: AiAgent) => void;
}

interface ProviderAppearance {
    name: string;
    gradient: string;
    softBackground: string;
    textColor: string;
}

function getProviderAppearance(
    provider: number
): ProviderAppearance {
    switch (provider) {
        case 1:
            return {
                name: "OpenAI",
                gradient:
                    "linear-gradient(135deg, #0F766E 0%, #14B8A6 100%)",
                softBackground:
                    "rgba(20,184,166,0.12)",
                textColor: "#0F766E"
            };

        case 2:
            return {
                name: "Gemini",
                gradient:
                    "linear-gradient(135deg, #2563EB 0%, #7C3AED 100%)",
                softBackground:
                    "rgba(99,102,241,0.12)",
                textColor: "#4F46E5"
            };

        case 3:
            return {
                name: "Grok",
                gradient:
                    "linear-gradient(135deg, #111827 0%, #374151 100%)",
                softBackground:
                    "rgba(55,65,81,0.12)",
                textColor: "#1F2937"
            };

        case 4:
            return {
                name: "DeepSeek",
                gradient:
                    "linear-gradient(135deg, #0284C7 0%, #2563EB 100%)",
                softBackground:
                    "rgba(37,99,235,0.12)",
                textColor: "#1D4ED8"
            };

        default:
            return {
                name: "Bilinmiyor",
                gradient:
                    "linear-gradient(135deg, #64748B 0%, #94A3B8 100%)",
                softBackground:
                    "rgba(100,116,139,0.12)",
                textColor: "#475569"
            };
    }
}

function AiAgentCard({
    agent,
    onEdit,
    onDelete,
    onManageKnowledgeBases,
    onTest,
    onSettings
}: AiAgentCardProps) {
    const provider =
        getProviderAppearance(agent.provider);

    const initial =
        agent.name.trim().charAt(0).toUpperCase() ||
        "A";

    return (
        <Card
            sx={{
                height: "100%",
                display: "flex",
                flexDirection: "column",
                borderRadius: 4,
                overflow: "hidden",
                border: 1,
                borderColor: "divider",
                boxShadow:
                    "0 10px 30px rgba(15,23,42,0.07)",
                transition:
                    "transform 0.2s ease, box-shadow 0.2s ease",
                "&:hover": {
                    transform: "translateY(-5px)",
                    boxShadow:
                        "0 18px 40px rgba(15,23,42,0.12)"
                }
            }}
        >
            <Box
                sx={{
                    height: 8,
                    background: provider.gradient
                }}
            />

            <CardContent
                sx={{
                    flexGrow: 1,
                    padding: 3
                }}
            >
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "flex-start",
                        gap: 2,
                        marginBottom: 2.5
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
                                width: 54,
                                height: 54,
                                background: provider.gradient,
                                color: "common.white",
                                fontWeight: 800,
                                fontSize: 22,
                                boxShadow:
                                    "0 10px 24px rgba(15,23,42,0.16)"
                            }}
                        >
                            {initial}
                        </Avatar>

                        <Box>
                            <Typography
                                component="h2"
                                variant="h6"
                                sx={{
                                    fontWeight: 800,
                                    lineHeight: 1.2
                                }}
                            >
                                {agent.name}
                            </Typography>

                            <Typography
                                variant="body2"
                                color="text.secondary"
                                sx={{
                                    marginTop: 0.5
                                }}
                            >
                                Yapay zekâ asistanı
                            </Typography>
                        </Box>
                    </Box>

                    <Chip
                        label={
                            agent.isActive
                                ? "Aktif"
                                : "Pasif"
                        }
                        color={
                            agent.isActive
                                ? "success"
                                : "default"
                        }
                        size="small"
                        sx={{
                            fontWeight: 700
                        }}
                    />
                </Box>

                <Box
                    sx={{
                        padding: 2,
                        borderRadius: 3,
                        backgroundColor:
                            provider.softBackground,
                        border: 1,
                        borderColor: "divider",
                        marginBottom: 2.5
                    }}
                >
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{
                            lineHeight: 1.65,
                            minHeight: 46
                        }}
                    >
                        {agent.welcomeMessage}
                    </Typography>
                </Box>

                <Box
                    sx={{
                        display: "flex",
                        gap: 1,
                        flexWrap: "wrap",
                        marginBottom: 2.5
                    }}
                >
                    <Chip
                        icon={<SmartToyOutlined />}
                        label={provider.name}
                        size="small"
                        sx={{
                            backgroundColor:
                                provider.softBackground,
                            color: provider.textColor,
                            fontWeight: 700
                        }}
                    />

                    <Chip
                        label={agent.modelName}
                        size="small"
                        variant="outlined"
                        sx={{
                            fontWeight: 600
                        }}
                    />
                </Box>

                <Divider sx={{ marginBottom: 2 }} />

                <Box
                    sx={{
                        display: "grid",
                        gridTemplateColumns:
                            "repeat(2, minmax(0, 1fr))",
                        gap: 1.25
                    }}
                >
                    <Button
                        variant="contained"
                        size="small"
                        startIcon={<ChatOutlined />}
                        onClick={() => onTest(agent)}
                        sx={{
                            justifyContent: "flex-start",
                            minHeight: 42,
                            borderRadius: 2.5,
                            background: provider.gradient,
                            boxShadow: "none",
                            "&:hover": {
                                boxShadow:
                                    "0 8px 18px rgba(15,23,42,0.15)"
                            }
                        }}
                    >
                        Test Et
                    </Button>

                    <Button
                        variant="outlined"
                        size="small"
                        startIcon={
                            <MenuBookOutlined />
                        }
                        onClick={() =>
                            onManageKnowledgeBases(agent)
                        }
                        sx={{
                            justifyContent: "flex-start",
                            minHeight: 42,
                            borderRadius: 2.5
                        }}
                    >
                        Bilgi Bankaları
                    </Button>

                    <Button
                        variant="outlined"
                        size="small"
                        startIcon={
                            <SettingsOutlined />
                        }
                        onClick={() =>
                            onSettings(agent)
                        }
                        sx={{
                            justifyContent: "flex-start",
                            minHeight: 42,
                            borderRadius: 2.5
                        }}
                    >
                        Ayarlar
                    </Button>

                    <Button
                        variant="outlined"
                        size="small"
                        startIcon={<EditOutlined />}
                        onClick={() => onEdit(agent)}
                        sx={{
                            justifyContent: "flex-start",
                            minHeight: 42,
                            borderRadius: 2.5
                        }}
                    >
                        Düzenle
                    </Button>

                    <Button
                        variant="outlined"
                        color="error"
                        size="small"
                        startIcon={<DeleteOutlined />}
                        onClick={() => onDelete(agent)}
                        sx={{
                            justifyContent: "flex-start",
                            minHeight: 42,
                            borderRadius: 2.5,
                            gridColumn: "1 / -1"
                        }}
                    >
                        Sil
                    </Button>
                </Box>
            </CardContent>
        </Card>
    );
}

export default AiAgentCard;