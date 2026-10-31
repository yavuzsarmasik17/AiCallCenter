import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    TextField
} from "@mui/material";

import type { CreateAiAgentRequest } from "../../services/aiAgentService";

interface Props {
    open: boolean;
    title: string;
    saving: boolean;
    form: CreateAiAgentRequest;
    onClose: () => void;
    onSave: () => void;
    onChange: (form: CreateAiAgentRequest) => void;
}

const providerModels: Record<number, string[]> = {
    1: ["gpt-5-mini", "gpt-5"],
    2: ["gemini-2.5-flash", "gemini-2.5-pro"],
    3: ["grok-3-mini", "grok-3"],
    4: ["deepseek-chat", "deepseek-reasoner"]
};

function AiAgentDialog({
    open,
    title,
    saving,
    form,
    onClose,
    onSave,
    onChange
}: Props) {

    const update = (
    key: keyof CreateAiAgentRequest,
    value: string | number
) => {
        onChange({
            ...form,
            [key]: value
        });
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            fullWidth
            maxWidth="sm"
        >
            <DialogTitle>
                {title}
            </DialogTitle>

            <DialogContent>

                <TextField
                    label="Ajan Adı"
                    fullWidth
                    margin="normal"
                    value={form.name}
                    onChange={(e)=>
                        update("name",e.target.value)}
                />

                <TextField
                    label="Talimatlar"
                    fullWidth
                    margin="normal"
                    multiline
                    minRows={4}
                    value={form.instructions}
                    onChange={(e)=>
                        update("instructions",e.target.value)}
                />

                <TextField
                    label="Karşılama Mesajı"
                    fullWidth
                    margin="normal"
                    multiline
                    minRows={2}
                    value={form.welcomeMessage}
                    onChange={(e)=>
                        update("welcomeMessage",e.target.value)}
                />

                <FormControl
                    fullWidth
                    margin="normal"
                >
                    <InputLabel>
                        Sağlayıcı
                    </InputLabel>

                    <Select
                        value={form.provider}
                        label="Sağlayıcı"
                        onChange={(e)=>{

                            const provider=
                                Number(e.target.value);

                            onChange({
                                ...form,
                                provider,
                                modelName:
                                    providerModels[provider][0]
                            });

                        }}
                    >

                        <MenuItem value={1}>
                            OpenAI
                        </MenuItem>

                        <MenuItem value={2}>
                            Gemini
                        </MenuItem>

                        <MenuItem value={3}>
                            Grok
                        </MenuItem>

                        <MenuItem value={4}>
                            DeepSeek
                        </MenuItem>

                    </Select>

                </FormControl>

                <FormControl
                    fullWidth
                    margin="normal"
                >

                    <InputLabel>
                        Model
                    </InputLabel>

                    <Select
                        value={form.modelName}
                        label="Model"
                        onChange={(e)=>
                            update(
                                "modelName",
                                e.target.value
                            )
                        }
                    >

                        {providerModels[
                            form.provider
                        ].map(model=>(

                            <MenuItem
                                key={model}
                                value={model}
                            >
                                {model}
                            </MenuItem>

                        ))}

                    </Select>

                </FormControl>

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
                    onClick={onSave}
                    disabled={saving}
                >
                    {saving
                        ? "Kaydediliyor..."
                        : "Kaydet"}
                </Button>

            </DialogActions>

        </Dialog>
    );
}

export default AiAgentDialog;