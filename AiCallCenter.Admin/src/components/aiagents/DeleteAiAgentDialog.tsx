import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle
} from "@mui/material";

interface DeleteAiAgentDialogProps {
    open: boolean;
    agentName: string;
    deleting: boolean;
    onClose: () => void;
    onConfirm: () => void;
}

function DeleteAiAgentDialog({
    open,
    agentName,
    deleting,
    onClose,
    onConfirm
}: DeleteAiAgentDialogProps) {
    return (
        <Dialog
            open={open}
            onClose={deleting ? undefined : onClose}
            fullWidth
            maxWidth="xs"
        >
            <DialogTitle>
                AI Ajanını Sil
            </DialogTitle>

            <DialogContent>
                <DialogContentText>
                    <strong>{agentName}</strong> adlı AI ajanını
                    silmek istediğinize emin misiniz?
                </DialogContentText>
            </DialogContent>

            <DialogActions>
                <Button
                    onClick={onClose}
                    disabled={deleting}
                >
                    Vazgeç
                </Button>

                <Button
                    color="error"
                    variant="contained"
                    onClick={onConfirm}
                    disabled={deleting}
                >
                    {deleting ? "Siliniyor..." : "Sil"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default DeleteAiAgentDialog;