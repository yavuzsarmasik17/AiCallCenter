import { useEffect, useMemo, useState } from "react";
import axios from "axios";
import {
    Alert,
    Box,
    Button,
    Checkbox,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControlLabel,
    List,
    ListItem,
    ListItemText,
    Typography
} from "@mui/material";
import {
    attachKnowledgeBase,
    detachKnowledgeBase,
    getAttachedKnowledgeBases,
    type AiAgent,
    type AiAgentKnowledgeBase
} from "../../services/aiAgentService";
import {
    getKnowledgeBases,
    type KnowledgeBase
} from "../../services/knowledgeBaseService";

interface AiAgentKnowledgeBasesDialogProps {
    open: boolean;
    agent: AiAgent | null;
    onClose: () => void;
}

function getApiErrorMessage(
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
          }
        | undefined;

    return (
        data?.detail ??
        data?.title ??
        fallbackMessage
    );
}

function AiAgentKnowledgeBasesDialog({
    open,
    agent,
    onClose
}: AiAgentKnowledgeBasesDialogProps) {
    const [knowledgeBases, setKnowledgeBases] =
        useState<KnowledgeBase[]>([]);

    const [
        attachedKnowledgeBases,
        setAttachedKnowledgeBases
    ] = useState<AiAgentKnowledgeBase[]>([]);

    const [loading, setLoading] =
        useState(false);

    const [changingId, setChangingId] =
        useState<string | null>(null);

    const [error, setError] =
        useState("");

    useEffect(() => {
        if (!open || !agent) {
            return;
        }

        const loadData = async () => {
            setLoading(true);
            setError("");

            try {
                const [
                    allKnowledgeBases,
                    attachedItems
                ] = await Promise.all([
                    getKnowledgeBases(),
                    getAttachedKnowledgeBases(agent.id)
                ]);

                setKnowledgeBases(
                    allKnowledgeBases
                );

                setAttachedKnowledgeBases(
                    attachedItems
                );
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "Bilgi bankaları yüklenemedi."
                    )
                );
            } finally {
                setLoading(false);
            }
        };

        void loadData();
    }, [open, agent]);

    const attachedIds = useMemo(
        () =>
            new Set(
                attachedKnowledgeBases.map(
                    (item) =>
                        item.knowledgeBaseId
                )
            ),
        [attachedKnowledgeBases]
    );

    const handleToggle = async (
        knowledgeBase: KnowledgeBase
    ) => {
        if (!agent) {
            return;
        }

        const isAttached =
            attachedIds.has(
                knowledgeBase.id
            );

        setChangingId(
            knowledgeBase.id
        );

        setError("");

        try {
            if (isAttached) {
                await detachKnowledgeBase(
                    agent.id,
                    knowledgeBase.id
                );

                setAttachedKnowledgeBases(
                    (current) =>
                        current.filter(
                            (item) =>
                                item.knowledgeBaseId !==
                                knowledgeBase.id
                        )
                );
            } else {
                await attachKnowledgeBase(
                    agent.id,
                    knowledgeBase.id
                );

                setAttachedKnowledgeBases(
                    (current) => [
                        ...current,
                        {
                            id: knowledgeBase.id,
                            knowledgeBaseId:
                                knowledgeBase.id,
                            knowledgeBaseName:
                                knowledgeBase.name,
                            knowledgeBaseDescription:
                                knowledgeBase.description,
                            isActive:
                                knowledgeBase.isActive
                        }
                    ]
                );
            }
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    isAttached
                        ? "Bilgi bankası bağlantısı kaldırılamadı."
                        : "Bilgi bankası bağlanamadı."
                )
            );
        } finally {
            setChangingId(null);
        }
    };

    return (
        <Dialog
            open={open}
            onClose={
                changingId
                    ? undefined
                    : onClose
            }
            fullWidth
            maxWidth="sm"
        >
            <DialogTitle>
                Bilgi Bankalarını Yönet
            </DialogTitle>

            <DialogContent>
                <Typography
                    component="p"
                    color="text.secondary"
                    sx={{ marginBottom: 2 }}
                >
                    {agent?.name ?? "AI ajanı"} için
                    kullanılacak bilgi bankalarını seçin.
                </Typography>

                {error && (
                    <Alert
                        severity="error"
                        sx={{ marginBottom: 2 }}
                    >
                        {error}
                    </Alert>
                )}

                {loading ? (
                    <Box
                        sx={{
                            display: "flex",
                            justifyContent: "center",
                            paddingY: 5
                        }}
                    >
                        <CircularProgress />
                    </Box>
                ) : knowledgeBases.length === 0 ? (
                    <Typography
                        component="p"
                        color="text.secondary"
                        sx={{
                            textAlign: "center",
                            paddingY: 4
                        }}
                    >
                        Henüz bilgi bankası bulunmuyor.
                    </Typography>
                ) : (
                    <List disablePadding>
                        {knowledgeBases.map(
                            (knowledgeBase) => {
                                const checked =
                                    attachedIds.has(
                                        knowledgeBase.id
                                    );

                                const changing =
                                    changingId ===
                                    knowledgeBase.id;

                                return (
                                    <ListItem
                                        key={
                                            knowledgeBase.id
                                        }
                                        divider
                                        disableGutters
                                    >
                                        <FormControlLabel
                                            sx={{
                                                width: "100%",
                                                margin: 0
                                            }}
                                            control={
                                                <Checkbox
                                                    checked={
                                                        checked
                                                    }
                                                    disabled={
                                                        changing
                                                    }
                                                    onChange={() =>
                                                        void handleToggle(
                                                            knowledgeBase
                                                        )
                                                    }
                                                />
                                            }
                                            label={
                                                <ListItemText
                                                    primary={
                                                        knowledgeBase.name
                                                    }
                                                    secondary={
                                                        knowledgeBase
                                                            .description ??
                                                        "Açıklama bulunmuyor."
                                                    }
                                                />
                                            }
                                        />
                                    </ListItem>
                                );
                            }
                        )}
                    </List>
                )}
            </DialogContent>

            <DialogActions>
                <Button
                    onClick={onClose}
                    disabled={Boolean(changingId)}
                >
                    Kapat
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default AiAgentKnowledgeBasesDialog;