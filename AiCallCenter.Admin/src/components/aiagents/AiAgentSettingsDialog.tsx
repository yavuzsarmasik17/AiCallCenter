import { useEffect, useMemo, useState } from "react";
import axios from "axios";

import {
    Alert,
    Button,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    FormControlLabel,
    InputLabel,
    MenuItem,
    Select,
    Switch,
    TextField,
    Typography
} from "@mui/material";

import {
    updateAiAgentSettings,
    type AiAgent,
    type UpdateAiAgentSettingsRequest
} from "../../services/aiAgentService";

import {
    getCallQueues,
    type CallQueue
} from "../../services/callQueueService";

interface AiAgentSettingsDialogProps {
    open: boolean;
    agent: AiAgent | null;
    onClose: () => void;
    onUpdated: (agent: AiAgent) => void;
}

interface SettingsFormProps {
    agent: AiAgent;
    onClose: () => void;
    onUpdated: (agent: AiAgent) => void;
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
        data?.title ||
        data?.message ||
        fallbackMessage
    );
}

function SettingsForm({
    agent,
    onClose,
    onUpdated
}: SettingsFormProps) {
    const [form, setForm] =
        useState<UpdateAiAgentSettingsRequest>(() => ({
            voiceName:
                agent.voiceName || "alloy",

            maxCallDurationMinutes:
                agent.maxCallDurationMinutes || 15,

            transferToHumanEnabled:
                agent.transferToHumanEnabled,

            transferCallQueueId:
                agent.transferCallQueueId ?? null,

            isActive:
                agent.isActive
        }));

    const [queues, setQueues] =
        useState<CallQueue[]>([]);

    const [loadingQueues, setLoadingQueues] =
        useState(true);

    const [saving, setSaving] =
        useState(false);

    const [error, setError] =
        useState("");

    useEffect(() => {
        let mounted = true;

        const loadQueues = async () => {
            setLoadingQueues(true);

            try {
                const data =
                    await getCallQueues();

                if (!mounted) {
                    return;
                }

                setQueues(data);
            } catch (exception) {
                if (!mounted) {
                    return;
                }

                setError(
                    getApiErrorMessage(
                        exception,
                        "Çağrı kuyrukları yüklenemedi."
                    )
                );
            } finally {
                if (mounted) {
                    setLoadingQueues(false);
                }
            }
        };

        void loadQueues();

        return () => {
            mounted = false;
        };
    }, []);

    const availableQueues = useMemo(() => {
        return queues
            .filter((queue) => {
                /*
                 * Aktif kuyrukları gösteriyoruz.
                 *
                 * Eğer mevcut ajan daha önce bir kuyruğa
                 * bağlanmışsa ve o kuyruk sonradan pasif
                 * yapılmışsa mevcut seçimi de listede
                 * göstermeye devam ediyoruz.
                 */
                return (
                    queue.isActive ||
                    queue.id ===
                    form.transferCallQueueId
                );
            })
            .sort((a, b) =>
                a.name.localeCompare(
                    b.name,
                    "tr"
                )
            );
    }, [
        queues,
        form.transferCallQueueId
    ]);

    const selectedQueue =
        queues.find(
            (queue) =>
                queue.id ===
                form.transferCallQueueId
        ) ?? null;

    const handleTransferEnabledChange = (
        enabled: boolean
    ) => {
        setForm((current) => ({
            ...current,

            transferToHumanEnabled:
                enabled,

            transferCallQueueId:
                enabled
                    ? current.transferCallQueueId
                    : null
        }));

        setError("");
    };

    const handleSave = async () => {
        setError("");

        if (
            form.maxCallDurationMinutes < 1 ||
            form.maxCallDurationMinutes > 120
        ) {
            setError(
                "Maksimum çağrı süresi 1 ile 120 dakika arasında olmalıdır."
            );

            return;
        }

        if (
            form.transferToHumanEnabled &&
            !form.transferCallQueueId
        ) {
            setError(
                "İnsan temsilciye aktarım aktifken bir aktarım kuyruğu seçmelisiniz."
            );

            return;
        }

        if (
            form.transferToHumanEnabled &&
            selectedQueue &&
            !selectedQueue.isActive
        ) {
            setError(
                "Seçili aktarım kuyruğu pasif. Lütfen aktif bir kuyruk seçiniz."
            );

            return;
        }

        setSaving(true);

        try {
            const updatedAgent =
                await updateAiAgentSettings(
                    agent.id,
                    {
                        voiceName:
                            form.voiceName,

                        maxCallDurationMinutes:
                            form.maxCallDurationMinutes,

                        transferToHumanEnabled:
                            form.transferToHumanEnabled,

                        transferCallQueueId:
                            form.transferToHumanEnabled
                                ? form.transferCallQueueId
                                : null,

                        isActive:
                            form.isActive
                    }
                );

            onUpdated(updatedAgent);
            onClose();
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "AI ajan ayarları güncellenemedi."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    return (
        <>
            <DialogTitle>
                AI Ajan Ayarları
            </DialogTitle>

            <DialogContent>
                {error && (
                    <Alert
                        severity="error"
                        sx={{
                            marginTop: 1,
                            marginBottom: 2
                        }}
                    >
                        {error}
                    </Alert>
                )}

                <FormControl
                    fullWidth
                    sx={{
                        marginTop: 1,
                        marginBottom: 2
                    }}
                >
                    <InputLabel>
                        Ses
                    </InputLabel>

                    <Select
                        label="Ses"
                        value={form.voiceName}
                        onChange={(event) =>
                            setForm(
                                (current) => ({
                                    ...current,

                                    voiceName:
                                        event.target.value
                                })
                            )
                        }
                    >
                        <MenuItem value="alloy">
                            Alloy
                        </MenuItem>

                        <MenuItem value="echo">
                            Echo
                        </MenuItem>

                        <MenuItem value="fable">
                            Fable
                        </MenuItem>

                        <MenuItem value="onyx">
                            Onyx
                        </MenuItem>

                        <MenuItem value="nova">
                            Nova
                        </MenuItem>

                        <MenuItem value="shimmer">
                            Shimmer
                        </MenuItem>
                    </Select>
                </FormControl>

                <TextField
                    fullWidth
                    type="number"
                    label="Maksimum Çağrı Süresi (Dakika)"
                    value={
                        form.maxCallDurationMinutes
                    }
                    slotProps={{
                        htmlInput: {
                            min: 1,
                            max: 120
                        }
                    }}
                    onChange={(event) =>
                        setForm(
                            (current) => ({
                                ...current,

                                maxCallDurationMinutes:
                                    Number(
                                        event.target.value
                                    )
                            })
                        )
                    }
                    sx={{
                        marginBottom: 2
                    }}
                />

                <FormControlLabel
                    control={
                        <Switch
                            checked={
                                form.transferToHumanEnabled
                            }
                            onChange={(event) =>
                                handleTransferEnabledChange(
                                    event.target.checked
                                )
                            }
                        />
                    }
                    label="İnsan temsilciye aktarma aktif"
                    sx={{
                        marginBottom: 1
                    }}
                />

                {form.transferToHumanEnabled && (
                    <>
                        <FormControl
                            fullWidth
                            disabled={
                                loadingQueues ||
                                saving
                            }
                            sx={{
                                marginBottom: 1
                            }}
                        >
                            <InputLabel>
                                Aktarım Kuyruğu
                            </InputLabel>

                            <Select
                                label="Aktarım Kuyruğu"
                                value={
                                    form.transferCallQueueId ??
                                    ""
                                }
                                onChange={(event) =>
                                    setForm(
                                        (current) => ({
                                            ...current,

                                            transferCallQueueId:
                                                event.target
                                                    .value ||
                                                null
                                        })
                                    )
                                }
                            >
                                <MenuItem value="">
                                    <em>
                                        Kuyruk seçiniz
                                    </em>
                                </MenuItem>

                                {availableQueues.map(
                                    (queue) => (
                                        <MenuItem
                                            key={
                                                queue.id
                                            }
                                            value={
                                                queue.id
                                            }
                                            disabled={
                                                !queue.isActive
                                            }
                                        >
                                            {queue.name}

                                            {!queue.isActive
                                                ? " (Pasif)"
                                                : ""}
                                        </MenuItem>
                                    )
                                )}
                            </Select>
                        </FormControl>

                        {loadingQueues && (
                            <Typography
                                variant="caption"
                                color="text.secondary"
                                sx={{
                                    display: "flex",
                                    alignItems:
                                        "center",
                                    gap: 1,
                                    marginBottom: 2
                                }}
                            >
                                <CircularProgress
                                    size={14}
                                />

                                Kuyruklar yükleniyor...
                            </Typography>
                        )}

                        {!loadingQueues &&
                            availableQueues.length ===
                            0 && (
                                <Alert
                                    severity="warning"
                                    sx={{
                                        marginBottom: 2
                                    }}
                                >
                                    Aktif çağrı kuyruğu
                                    bulunamadı. Önce Çağrı
                                    Kuyrukları ekranından
                                    aktif bir kuyruk
                                    oluşturmalısınız.
                                </Alert>
                            )}

                        {!loadingQueues &&
                            selectedQueue &&
                            selectedQueue.isActive && (
                                <Typography
                                    variant="caption"
                                    color="text.secondary"
                                    sx={{
                                        display:
                                            "block",
                                        marginBottom: 2
                                    }}
                                >
                                    İnsan aktarımı
                                    gerektiğinde çağrı
                                    &quot;
                                    {selectedQueue.name}
                                    &quot; kuyruğuna
                                    gönderilecek.
                                </Typography>
                            )}
                    </>
                )}

                <FormControlLabel
                    control={
                        <Switch
                            checked={
                                form.isActive
                            }
                            onChange={(event) =>
                                setForm(
                                    (current) => ({
                                        ...current,

                                        isActive:
                                            event.target.checked
                                    })
                                )
                            }
                        />
                    }
                    label="AI ajanı aktif"
                />
            </DialogContent>

            <DialogActions>
                <Button
                    onClick={onClose}
                    disabled={saving}
                >
                    İptal
                </Button>

                <Button
                    variant="contained"
                    onClick={() =>
                        void handleSave()
                    }
                    disabled={
                        saving ||
                        loadingQueues
                    }
                >
                    {saving
                        ? "Kaydediliyor..."
                        : "Kaydet"}
                </Button>
            </DialogActions>
        </>
    );
}

function AiAgentSettingsDialog({
    open,
    agent,
    onClose,
    onUpdated
}: AiAgentSettingsDialogProps) {
    return (
        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth="sm"
        >
            {open && agent && (
                <SettingsForm
                    key={agent.id}
                    agent={agent}
                    onClose={onClose}
                    onUpdated={onUpdated}
                />
            )}
        </Dialog>
    );
}

export default AiAgentSettingsDialog;