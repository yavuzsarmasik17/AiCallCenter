import { useEffect, useState } from "react";
import axios from "axios";
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
    DialogContentText,
    DialogTitle,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Snackbar,
    TextField,
    Typography
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import DeleteIcon from "@mui/icons-material/Delete";
import DescriptionIcon from "@mui/icons-material/Description";
import PictureAsPdfIcon from "@mui/icons-material/PictureAsPdf";
import UploadFileIcon from "@mui/icons-material/UploadFile";
import { useNavigate, useParams } from "react-router-dom";
import {
    createTextDocument,
    deleteKnowledgeDocument,
    getKnowledgeDocuments,
    uploadPdfDocument,
    type KnowledgeDocument
} from "../services/knowledgeDocumentService";

function getStatusLabel(status: number): string {
    switch (status) {
        case 1:
            return "Bekliyor";
        case 2:
            return "İşleniyor";
        case 3:
            return "Hazır";
        case 4:
            return "Hatalı";
        default:
            return "Bilinmiyor";
    }
}

function getStatusColor(
    status: number
): "default" | "warning" | "info" | "success" | "error" {
    switch (status) {
        case 1:
            return "warning";
        case 2:
            return "info";
        case 3:
            return "success";
        case 4:
            return "error";
        default:
            return "default";
    }
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

    return data?.detail ?? data?.title ?? fallbackMessage;
}

function KnowledgeBaseDocumentsPage() {
    const navigate = useNavigate();
    const { knowledgeBaseId } = useParams();

    const [documents, setDocuments] =
        useState<KnowledgeDocument[]>([]);

    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [deleting, setDeleting] = useState(false);

    const [textDialogOpen, setTextDialogOpen] =
        useState(false);

    const [pdfDialogOpen, setPdfDialogOpen] =
        useState(false);

    const [deleteDialogOpen, setDeleteDialogOpen] =
        useState(false);

    const [selectedDocument, setSelectedDocument] =
        useState<KnowledgeDocument | null>(null);

    const [name, setName] = useState("");
    const [content, setContent] = useState("");

    const [pdfName, setPdfName] = useState("");
    const [pdfFile, setPdfFile] =
        useState<File | null>(null);

    const [error, setError] = useState("");

    const [successMessage, setSuccessMessage] =
        useState("");

    useEffect(() => {
        const loadDocuments = async () => {
            if (!knowledgeBaseId) {
                setError(
                    "Bilgi bankası kimliği bulunamadı."
                );

                setLoading(false);
                return;
            }

            try {
                const data =
                    await getKnowledgeDocuments(
                        knowledgeBaseId
                    );

                setDocuments(data);
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "Belgeler yüklenirken bir hata oluştu."
                    )
                );
            } finally {
                setLoading(false);
            }
        };

        void loadDocuments();
    }, [knowledgeBaseId]);

    const handleCreateText = async () => {
        if (!knowledgeBaseId) {
            setError(
                "Bilgi bankası kimliği bulunamadı."
            );
            return;
        }

        if (!name.trim()) {
            setError("Belge adı zorunludur.");
            return;
        }

        if (!content.trim()) {
            setError("Belge içeriği zorunludur.");
            return;
        }

        setSaving(true);
        setError("");

        try {
            const createdDocument =
                await createTextDocument(
                    knowledgeBaseId,
                    {
                        name: name.trim(),
                        content: content.trim()
                    }
                );

            setDocuments((current) => [
                createdDocument,
                ...current
            ]);

            setName("");
            setContent("");
            setTextDialogOpen(false);

            setSuccessMessage(
                "Metin belgesi başarıyla oluşturuldu."
            );
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Metin belgesi oluşturulamadı."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    const handleUploadPdf = async () => {
        if (!knowledgeBaseId) {
            setError(
                "Bilgi bankası kimliği bulunamadı."
            );
            return;
        }

        if (!pdfName.trim()) {
            setError("Belge adı zorunludur.");
            return;
        }

        if (!pdfFile) {
            setError("PDF dosyası seçmelisiniz.");
            return;
        }

        if (pdfFile.type !== "application/pdf") {
            setError(
                "Yalnızca PDF dosyası yükleyebilirsiniz."
            );
            return;
        }

        setSaving(true);
        setError("");

        try {
            const uploadedDocument =
                await uploadPdfDocument(
                    knowledgeBaseId,
                    pdfName.trim(),
                    pdfFile
                );

            setDocuments((current) => [
                uploadedDocument,
                ...current
            ]);

            setPdfName("");
            setPdfFile(null);
            setPdfDialogOpen(false);

            setSuccessMessage(
                "PDF belgesi başarıyla yüklendi."
            );
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "PDF belgesi yüklenemedi."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    const handleOpenDelete = (
        document: KnowledgeDocument
    ) => {
        setSelectedDocument(document);
        setError("");
        setDeleteDialogOpen(true);
    };

    const handleCloseDelete = () => {
        if (deleting) {
            return;
        }

        setDeleteDialogOpen(false);
        setSelectedDocument(null);
    };

    const handleDelete = async () => {
        if (!knowledgeBaseId || !selectedDocument) {
            return;
        }

        setDeleting(true);
        setError("");

        try {
            await deleteKnowledgeDocument(
                knowledgeBaseId,
                selectedDocument.id
            );

            setDocuments((current) =>
                current.filter(
                    (document) =>
                        document.id !==
                        selectedDocument.id
                )
            );

            setDeleteDialogOpen(false);
            setSelectedDocument(null);

            setSuccessMessage(
                "Belge başarıyla silindi."
            );
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Belge silinemedi."
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
                    display: "flex",
                    justifyContent: "center",
                    paddingTop: 8
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box>
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: 4,
                    gap: 2,
                    flexWrap: "wrap"
                }}
            >
                <Box>
                    <Button
                        startIcon={<ArrowBackIcon />}
                        onClick={() =>
                            navigate("/knowledge-bases")
                        }
                        sx={{ marginBottom: 1 }}
                    >
                        Bilgi Bankalarına Dön
                    </Button>

                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{ fontWeight: 700 }}
                    >
                        Bilgi Bankası Belgeleri
                    </Typography>

                    <Typography
                        component="p"
                        color="text.secondary"
                    >
                        AI tarafından kullanılacak
                        belgeleri yönetin.
                    </Typography>
                </Box>

                <Box
                    sx={{
                        display: "flex",
                        gap: 1.5,
                        flexWrap: "wrap"
                    }}
                >
                    <Button
                        variant="outlined"
                        startIcon={<UploadFileIcon />}
                        onClick={() => {
                            setError("");
                            setPdfDialogOpen(true);
                        }}
                    >
                        PDF Yükle
                    </Button>

                    <Button
                        variant="contained"
                        startIcon={<AddIcon />}
                        onClick={() => {
                            setError("");
                            setTextDialogOpen(true);
                        }}
                    >
                        Metin Ekle
                    </Button>
                </Box>
            </Box>

            {error && (
                <Alert
                    severity="error"
                    sx={{ marginBottom: 3 }}
                    onClose={() => setError("")}
                >
                    {error}
                </Alert>
            )}

            {documents.length === 0 ? (
                <Card>
                    <CardContent
                        sx={{
                            textAlign: "center",
                            padding: 6
                        }}
                    >
                        <DescriptionIcon
                            sx={{
                                fontSize: 58,
                                color: "text.secondary",
                                marginBottom: 2
                            }}
                        />

                        <Typography
                            component="h2"
                            variant="h6"
                            sx={{ fontWeight: 700 }}
                        >
                            Henüz belge bulunmuyor
                        </Typography>

                        <Typography
                            component="p"
                            color="text.secondary"
                            sx={{ marginTop: 1 }}
                        >
                            Metin ekleyebilir veya PDF
                            dosyası yükleyebilirsiniz.
                        </Typography>
                    </CardContent>
                </Card>
            ) : (
                <Card>
                    <List disablePadding>
                        {documents.map((document) => (
                            <ListItem
                                key={document.id}
                                divider
                                secondaryAction={
                                    <Box
                                        sx={{
                                            display: "flex",
                                            alignItems: "center",
                                            gap: 1
                                        }}
                                    >
                                        <Chip
                                            label={getStatusLabel(
                                                document.status
                                            )}
                                            color={getStatusColor(
                                                document.status
                                            )}
                                            size="small"
                                        />

                                        <IconButton
                                            color="error"
                                            onClick={() =>
                                                handleOpenDelete(
                                                    document
                                                )
                                            }
                                            aria-label="Belgeyi sil"
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                    </Box>
                                }
                            >
                                {document.documentType === 2 ? (
                                    <PictureAsPdfIcon
                                        color="error"
                                        sx={{ marginRight: 2 }}
                                    />
                                ) : (
                                    <DescriptionIcon
                                        color="primary"
                                        sx={{ marginRight: 2 }}
                                    />
                                )}

                                <ListItemText
                                    primary={document.name}
                                    secondary={
                                        document.originalFileName ??
                                        new Date(
                                            document.uploadedAt
                                        ).toLocaleString(
                                            "tr-TR"
                                        )
                                    }
                                />
                            </ListItem>
                        ))}
                    </List>
                </Card>
            )}

            <Dialog
                open={textDialogOpen}
                onClose={
                    saving
                        ? undefined
                        : () => setTextDialogOpen(false)
                }
                fullWidth
                maxWidth="md"
            >
                <DialogTitle>
                    Yeni Metin Belgesi
                </DialogTitle>

                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        required
                        label="Belge Adı"
                        value={name}
                        onChange={(event) =>
                            setName(event.target.value)
                        }
                        sx={{
                            marginTop: 1,
                            marginBottom: 2
                        }}
                    />

                    <TextField
                        fullWidth
                        required
                        multiline
                        minRows={10}
                        label="Belge İçeriği"
                        value={content}
                        onChange={(event) =>
                            setContent(event.target.value)
                        }
                    />
                </DialogContent>

                <DialogActions>
                    <Button
                        disabled={saving}
                        onClick={() =>
                            setTextDialogOpen(false)
                        }
                    >
                        İptal
                    </Button>

                    <Button
                        variant="contained"
                        disabled={saving}
                        onClick={handleCreateText}
                    >
                        {saving
                            ? "Kaydediliyor..."
                            : "Kaydet"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={pdfDialogOpen}
                onClose={
                    saving
                        ? undefined
                        : () => setPdfDialogOpen(false)
                }
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle>
                    PDF Belgesi Yükle
                </DialogTitle>

                <DialogContent>
                    <TextField
                        autoFocus
                        fullWidth
                        required
                        label="Belge Adı"
                        value={pdfName}
                        onChange={(event) =>
                            setPdfName(event.target.value)
                        }
                        sx={{
                            marginTop: 1,
                            marginBottom: 3
                        }}
                    />

                    <Button
                        component="label"
                        variant="outlined"
                        fullWidth
                        startIcon={<UploadFileIcon />}
                        sx={{
                            minHeight: 54
                        }}
                    >
                        {pdfFile
                            ? pdfFile.name
                            : "PDF Dosyası Seç"}

                        <input
                            hidden
                            type="file"
                            accept="application/pdf,.pdf"
                            onChange={(event) => {
                                const selectedFile =
                                    event.target.files?.[0] ??
                                    null;

                                setPdfFile(selectedFile);
                            }}
                        />
                    </Button>

                    {pdfFile && (
                        <Typography
                            component="p"
                            color="text.secondary"
                            sx={{
                                marginTop: 2,
                                fontSize: 14
                            }}
                        >
                            Dosya boyutu:{" "}
                            {(
                                pdfFile.size /
                                1024 /
                                1024
                            ).toFixed(2)}{" "}
                            MB
                        </Typography>
                    )}
                </DialogContent>

                <DialogActions>
                    <Button
                        disabled={saving}
                        onClick={() =>
                            setPdfDialogOpen(false)
                        }
                    >
                        İptal
                    </Button>

                    <Button
                        variant="contained"
                        disabled={saving}
                        onClick={handleUploadPdf}
                    >
                        {saving
                            ? "Yükleniyor..."
                            : "PDF Yükle"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Dialog
                open={deleteDialogOpen}
                onClose={
                    deleting
                        ? undefined
                        : handleCloseDelete
                }
                fullWidth
                maxWidth="xs"
            >
                <DialogTitle>
                    Belgeyi Sil
                </DialogTitle>

                <DialogContent>
                    <DialogContentText>
                        <strong>
                            {selectedDocument?.name ?? ""}
                        </strong>{" "}
                        adlı belgeyi silmek istediğinize
                        emin misiniz?
                    </DialogContentText>
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={handleCloseDelete}
                        disabled={deleting}
                    >
                        Vazgeç
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={handleDelete}
                        disabled={deleting}
                    >
                        {deleting
                            ? "Siliniyor..."
                            : "Sil"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={Boolean(successMessage)}
                autoHideDuration={3000}
                message={successMessage}
                onClose={() =>
                    setSuccessMessage("")
                }
            />
        </Box>
    );
}

export default KnowledgeBaseDocumentsPage;