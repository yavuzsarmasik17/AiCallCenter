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
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    Divider,
    MenuItem,
    Select,
    Snackbar,
    TextField,
    Typography
} from "@mui/material";

import AddIcon from "@mui/icons-material/Add";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import DeleteIcon from "@mui/icons-material/Delete";
import DescriptionIcon from "@mui/icons-material/Description";
import EditIcon from "@mui/icons-material/Edit";
import FilterListIcon from "@mui/icons-material/FilterList";
import FolderOpenIcon from "@mui/icons-material/FolderOpen";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import SearchIcon from "@mui/icons-material/Search";
import SortByAlphaIcon from "@mui/icons-material/SortByAlpha";
import VisibilityIcon from "@mui/icons-material/Visibility";

import {
    useNavigate
} from "react-router-dom";

import {
    createKnowledgeBase,
    deleteKnowledgeBase,
    getKnowledgeBases,
    updateKnowledgeBase,
    type CreateKnowledgeBaseRequest,
    type KnowledgeBase
} from "../services/knowledgeBaseService";

type StatusFilter =
    | "all"
    | "active"
    | "passive";

type SortOption =
    | "name-asc"
    | "name-desc";

const initialForm: CreateKnowledgeBaseRequest = {
    name: "",
    description: ""
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
                  message?: string;
              }
            | undefined;

    return (
        responseData?.detail ??
        responseData?.title ??
        responseData?.message ??
        fallbackMessage
    );
}

function KnowledgeBasesPage() {
    const navigate = useNavigate();

    const [
        knowledgeBases,
        setKnowledgeBases
    ] = useState<KnowledgeBase[]>([]);

    const [
        loading,
        setLoading
    ] = useState(true);

    const [
        saving,
        setSaving
    ] = useState(false);

    const [
        deleting,
        setDeleting
    ] = useState(false);

    const [
        formDialogOpen,
        setFormDialogOpen
    ] = useState(false);

    const [
        deleteDialogOpen,
        setDeleteDialogOpen
    ] = useState(false);

    const [
        selectedKnowledgeBase,
        setSelectedKnowledgeBase
    ] = useState<KnowledgeBase | null>(null);

    const [
        knowledgeBaseToDelete,
        setKnowledgeBaseToDelete
    ] = useState<KnowledgeBase | null>(null);

    const [
        error,
        setError
    ] = useState("");

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
        sortOption,
        setSortOption
    ] = useState<SortOption>("name-asc");

    const [
        form,
        setForm
    ] = useState<CreateKnowledgeBaseRequest>(
        initialForm
    );

    useEffect(() => {
        const loadKnowledgeBases =
            async () => {
                try {
                    setError("");

                    const data =
                        await getKnowledgeBases();

                    setKnowledgeBases(data);
                } catch (exception) {
                    setError(
                        getApiErrorMessage(
                            exception,
                            "Bilgi bankaları yüklenirken bir hata oluştu."
                        )
                    );
                } finally {
                    setLoading(false);
                }
            };

        void loadKnowledgeBases();
    }, []);

    const summary = useMemo(() => {
        const activeCount =
            knowledgeBases.filter(
                (knowledgeBase) =>
                    knowledgeBase.isActive
            ).length;

        return {
            totalCount:
                knowledgeBases.length,
            activeCount,
            passiveCount:
                knowledgeBases.length -
                activeCount
        };
    }, [knowledgeBases]);

    const filteredKnowledgeBases =
        useMemo(() => {
            const normalizedSearch =
                searchText
                    .trim()
                    .toLocaleLowerCase(
                        "tr-TR"
                    );

            const filtered =
                knowledgeBases.filter(
                    (knowledgeBase) => {
                        const matchesSearch =
                            !normalizedSearch ||
                            knowledgeBase.name
                                .toLocaleLowerCase(
                                    "tr-TR"
                                )
                                .includes(
                                    normalizedSearch
                                ) ||
                            (
                                knowledgeBase.description ??
                                ""
                            )
                                .toLocaleLowerCase(
                                    "tr-TR"
                                )
                                .includes(
                                    normalizedSearch
                                );

                        const matchesStatus =
                            statusFilter ===
                                "all" ||
                            (
                                statusFilter ===
                                    "active" &&
                                knowledgeBase.isActive
                            ) ||
                            (
                                statusFilter ===
                                    "passive" &&
                                !knowledgeBase.isActive
                            );

                        return (
                            matchesSearch &&
                            matchesStatus
                        );
                    }
                );

            return [...filtered].sort(
                (left, right) => {
                    const comparison =
                        left.name.localeCompare(
                            right.name,
                            "tr-TR"
                        );

                    return sortOption ===
                        "name-asc"
                        ? comparison
                        : -comparison;
                }
            );
        }, [
            knowledgeBases,
            searchText,
            sortOption,
            statusFilter
        ]);

    const handleOpenCreate = () => {
        setSelectedKnowledgeBase(null);
        setForm(initialForm);
        setError("");
        setFormDialogOpen(true);
    };

    const handleOpenEdit = (
        knowledgeBase: KnowledgeBase
    ) => {
        setSelectedKnowledgeBase(
            knowledgeBase
        );

        setForm({
            name: knowledgeBase.name,
            description:
                knowledgeBase.description ??
                ""
        });

        setError("");
        setFormDialogOpen(true);
    };

    const handleCloseFormDialog =
        () => {
            if (saving) {
                return;
            }

            setFormDialogOpen(false);
            setSelectedKnowledgeBase(
                null
            );
            setForm(initialForm);
        };

    const handleSave = async () => {
        if (!form.name.trim()) {
            setError(
                "Bilgi bankası adı zorunludur."
            );
            return;
        }

        setSaving(true);
        setError("");

        try {
            if (selectedKnowledgeBase) {
                const updatedKnowledgeBase =
                    await updateKnowledgeBase(
                        selectedKnowledgeBase.id,
                        {
                            name:
                                form.name.trim(),
                            description:
                                form.description?.trim()
                        }
                    );

                setKnowledgeBases(
                    (current) =>
                        current.map((item) =>
                            item.id ===
                            updatedKnowledgeBase.id
                                ? {
                                      ...item,
                                      ...updatedKnowledgeBase
                                  }
                                : item
                        )
                );

                setSuccessMessage(
                    "Bilgi bankası başarıyla güncellendi."
                );
            } else {
                const createdKnowledgeBase =
                    await createKnowledgeBase(
                        {
                            name:
                                form.name.trim(),
                            description:
                                form.description?.trim()
                        }
                    );

                setKnowledgeBases(
                    (current) => [
                        createdKnowledgeBase,
                        ...current
                    ]
                );

                setSuccessMessage(
                    "Bilgi bankası başarıyla oluşturuldu."
                );
            }

            setFormDialogOpen(false);
            setSelectedKnowledgeBase(
                null
            );
            setForm(initialForm);
        } catch (exception) {
            setError(
                getApiErrorMessage(
                    exception,
                    "Bilgi bankası kaydedilemedi."
                )
            );
        } finally {
            setSaving(false);
        }
    };

    const handleOpenDelete = (
        knowledgeBase: KnowledgeBase
    ) => {
        setKnowledgeBaseToDelete(
            knowledgeBase
        );

        setError("");
        setDeleteDialogOpen(true);
    };

    const handleCloseDeleteDialog =
        () => {
            if (deleting) {
                return;
            }

            setDeleteDialogOpen(false);
            setKnowledgeBaseToDelete(
                null
            );
        };

    const handleConfirmDelete =
        async () => {
            if (
                !knowledgeBaseToDelete
            ) {
                return;
            }

            setDeleting(true);
            setError("");

            try {
                await deleteKnowledgeBase(
                    knowledgeBaseToDelete.id
                );

                setKnowledgeBases(
                    (current) =>
                        current.filter(
                            (item) =>
                                item.id !==
                                knowledgeBaseToDelete.id
                        )
                );

                setDeleteDialogOpen(
                    false
                );
                setKnowledgeBaseToDelete(
                    null
                );

                setSuccessMessage(
                    "Bilgi bankası başarıyla silindi."
                );
            } catch (exception) {
                setError(
                    getApiErrorMessage(
                        exception,
                        "Bilgi bankası silinemedi."
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
                    minHeight: 420,
                    display: "flex",
                    justifyContent:
                        "center",
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
                    gap: 2,
                    marginBottom: 3
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
                        Bilgi Bankaları
                    </Typography>

                    <Typography
                        color="text.secondary"
                        sx={{
                            marginTop: 0.5
                        }}
                    >
                        AI ajanlarının
                        kullanacağı firma
                        bilgilerini yönetin.
                    </Typography>
                </Box>

                <Button
                    variant="contained"
                    size="large"
                    startIcon={
                        <AddIcon />
                    }
                    onClick={
                        handleOpenCreate
                    }
                    sx={{
                        borderRadius: 3,
                        paddingX: 3,
                        minHeight: 48,
                        boxShadow:
                            "0 10px 25px rgba(37,99,235,0.25)"
                    }}
                >
                    Yeni Bilgi Bankası
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

            <Box
                sx={{
                    display: "grid",
                    gridTemplateColumns: {
                        xs: "1fr",
                        sm: "repeat(2, 1fr)",
                        xl: "repeat(4, 1fr)"
                    },
                    gap: 2.5,
                    marginBottom: 3
                }}
            >
                <Card
                    sx={{
                        borderRadius: 4,
                        color: "common.white",
                        background:
                            "linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)",
                        boxShadow:
                            "0 14px 30px rgba(79,70,229,0.25)"
                    }}
                >
                    <CardContent>
                        <Avatar
                            sx={{
                                marginBottom: 2,
                                backgroundColor:
                                    "rgba(255,255,255,0.18)"
                            }}
                        >
                            <MenuBookIcon />
                        </Avatar>

                        <Typography
                            variant="body2"
                            sx={{
                                opacity: 0.9
                            }}
                        >
                            Toplam Bilgi Bankası
                        </Typography>

                        <Typography
                            variant="h3"
                            sx={{
                                fontWeight: 800,
                                marginTop: 0.5
                            }}
                        >
                            {summary.totalCount}
                        </Typography>
                    </CardContent>
                </Card>

                <Card
                    sx={{
                        borderRadius: 4,
                        color: "common.white",
                        background:
                            "linear-gradient(135deg, #059669 0%, #10B981 100%)",
                        boxShadow:
                            "0 14px 30px rgba(16,185,129,0.22)"
                    }}
                >
                    <CardContent>
                        <Avatar
                            sx={{
                                marginBottom: 2,
                                backgroundColor:
                                    "rgba(255,255,255,0.18)"
                            }}
                        >
                            <CheckCircleIcon />
                        </Avatar>

                        <Typography
                            variant="body2"
                            sx={{
                                opacity: 0.9
                            }}
                        >
                            Aktif
                        </Typography>

                        <Typography
                            variant="h3"
                            sx={{
                                fontWeight: 800,
                                marginTop: 0.5
                            }}
                        >
                            {summary.activeCount}
                        </Typography>
                    </CardContent>
                </Card>

                <Card
                    sx={{
                        borderRadius: 4,
                        color: "common.white",
                        background:
                            "linear-gradient(135deg, #64748B 0%, #94A3B8 100%)",
                        boxShadow:
                            "0 14px 30px rgba(100,116,139,0.22)"
                    }}
                >
                    <CardContent>
                        <Avatar
                            sx={{
                                marginBottom: 2,
                                backgroundColor:
                                    "rgba(255,255,255,0.18)"
                            }}
                        >
                            <FolderOpenIcon />
                        </Avatar>

                        <Typography
                            variant="body2"
                            sx={{
                                opacity: 0.9
                            }}
                        >
                            Pasif
                        </Typography>

                        <Typography
                            variant="h3"
                            sx={{
                                fontWeight: 800,
                                marginTop: 0.5
                            }}
                        >
                            {summary.passiveCount}
                        </Typography>
                    </CardContent>
                </Card>

                <Card
                    sx={{
                        borderRadius: 4,
                        color: "common.white",
                        background:
                            "linear-gradient(135deg, #0284C7 0%, #0EA5E9 100%)",
                        boxShadow:
                            "0 14px 30px rgba(14,165,233,0.22)"
                    }}
                >
                    <CardContent>
                        <Avatar
                            sx={{
                                marginBottom: 2,
                                backgroundColor:
                                    "rgba(255,255,255,0.18)"
                            }}
                        >
                            <VisibilityIcon />
                        </Avatar>

                        <Typography
                            variant="body2"
                            sx={{
                                opacity: 0.9
                            }}
                        >
                            Görüntülenen
                        </Typography>

                        <Typography
                            variant="h3"
                            sx={{
                                fontWeight: 800,
                                marginTop: 0.5
                            }}
                        >
                            {
                                filteredKnowledgeBases.length
                            }
                        </Typography>
                    </CardContent>
                </Card>
            </Box>

            <Card
                sx={{
                    borderRadius: 4,
                    border: 1,
                    borderColor: "divider",
                    boxShadow:
                        "0 10px 30px rgba(15,23,42,0.06)",
                    marginBottom: 3
                }}
            >
                <CardContent>
                    <Box
                        sx={{
                            display: "grid",
                            gridTemplateColumns: {
                                xs: "1fr",
                                md: "minmax(0, 2fr) minmax(180px, 1fr) minmax(180px, 1fr)"
                            },
                            gap: 2
                        }}
                    >
                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1
                            }}
                        >
                            <SearchIcon
                                color="action"
                            />

                            <TextField
                                fullWidth
                                placeholder="Bilgi bankası ara..."
                                value={searchText}
                                onChange={(
                                    event
                                ) =>
                                    setSearchText(
                                        event.target
                                            .value
                                    )
                                }
                            />
                        </Box>

                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1
                            }}
                        >
                            <FilterListIcon
                                color="action"
                            />

                            <Select
                                fullWidth
                                value={
                                    statusFilter
                                }
                                onChange={(
                                    event
                                ) =>
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
                        </Box>

                        <Box
                            sx={{
                                display: "flex",
                                alignItems: "center",
                                gap: 1
                            }}
                        >
                            <SortByAlphaIcon
                                color="action"
                            />

                            <Select
                                fullWidth
                                value={
                                    sortOption
                                }
                                onChange={(
                                    event
                                ) =>
                                    setSortOption(
                                        event.target
                                            .value as SortOption
                                    )
                                }
                            >
                                <MenuItem value="name-asc">
                                    Ada göre A-Z
                                </MenuItem>

                                <MenuItem value="name-desc">
                                    Ada göre Z-A
                                </MenuItem>
                            </Select>
                        </Box>
                    </Box>

                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{
                            marginTop: 2
                        }}
                    >
                        {
                            filteredKnowledgeBases.length
                        }{" "}
                        bilgi bankası
                        gösteriliyor
                    </Typography>
                </CardContent>
            </Card>

            {knowledgeBases.length === 0 ? (
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
                                width: 82,
                                height: 82,
                                marginX: "auto",
                                marginBottom: 2,
                                background:
                                    "linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%)"
                            }}
                        >
                            <MenuBookIcon
                                sx={{
                                    fontSize: 42
                                }}
                            />
                        </Avatar>

                        <Typography
                            variant="h6"
                            sx={{
                                fontWeight: 800
                            }}
                        >
                            Henüz bilgi
                            bankası yok
                        </Typography>

                        <Typography
                            color="text.secondary"
                            sx={{
                                marginTop: 1,
                                marginBottom: 3
                            }}
                        >
                            PDF ve metin
                            belgeleri yüklemek
                            için ilk bilgi
                            bankanızı oluşturun.
                        </Typography>

                        <Button
                            variant="contained"
                            startIcon={
                                <AddIcon />
                            }
                            onClick={
                                handleOpenCreate
                            }
                        >
                            İlk Bilgi Bankasını
                            Oluştur
                        </Button>
                    </CardContent>
                </Card>
            ) : filteredKnowledgeBases.length ===
              0 ? (
                <Alert severity="info">
                    Arama ve filtrelere uygun
                    bilgi bankası bulunamadı.
                </Alert>
            ) : (
                <Box
                    sx={{
                        display: "grid",
                        gridTemplateColumns: {
                            xs: "1fr",
                            md: "repeat(2, minmax(0, 1fr))",
                            xl: "repeat(3, minmax(0, 1fr))"
                        },
                        gap: 3
                    }}
                >
                    {filteredKnowledgeBases.map(
                        (knowledgeBase) => (
                            <Card
                                key={
                                    knowledgeBase.id
                                }
                                sx={{
                                    height: "100%",
                                    display: "flex",
                                    flexDirection:
                                        "column",
                                    borderRadius: 4,
                                    border: 1,
                                    borderColor:
                                        "divider",
                                    overflow:
                                        "hidden",
                                    boxShadow:
                                        "0 10px 30px rgba(15,23,42,0.07)",
                                    transition:
                                        "transform 0.2s ease, box-shadow 0.2s ease",
                                    "&:hover": {
                                        transform:
                                            "translateY(-5px)",
                                        boxShadow:
                                            "0 20px 45px rgba(15,23,42,0.13)"
                                    }
                                }}
                            >
                                <Box
                                    sx={{
                                        height: 7,
                                        background:
                                            knowledgeBase.isActive
                                                ? "linear-gradient(90deg, #4F46E5, #7C3AED)"
                                                : "linear-gradient(90deg, #64748B, #94A3B8)"
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
                                            justifyContent:
                                                "space-between",
                                            alignItems:
                                                "flex-start",
                                            gap: 2,
                                            marginBottom: 2
                                        }}
                                    >
                                        <Avatar
                                            variant="rounded"
                                            sx={{
                                                width: 56,
                                                height: 56,
                                                background:
                                                    knowledgeBase.isActive
                                                        ? "linear-gradient(135deg, #4F46E5, #7C3AED)"
                                                        : "linear-gradient(135deg, #64748B, #94A3B8)"
                                            }}
                                        >
                                            <MenuBookIcon />
                                        </Avatar>

                                        <Chip
                                            label={
                                                knowledgeBase.isActive
                                                    ? "Aktif"
                                                    : "Pasif"
                                            }
                                            color={
                                                knowledgeBase.isActive
                                                    ? "success"
                                                    : "default"
                                            }
                                            size="small"
                                            sx={{
                                                fontWeight: 700
                                            }}
                                        />
                                    </Box>

                                    <Typography
                                        component="h2"
                                        variant="h6"
                                        sx={{
                                            fontWeight: 800,
                                            marginBottom: 1,
                                            wordBreak:
                                                "break-word"
                                        }}
                                    >
                                        {
                                            knowledgeBase.name
                                        }
                                    </Typography>

                                    <Typography
                                        color="text.secondary"
                                        sx={{
                                            minHeight: 72,
                                            lineHeight: 1.6,
                                            wordBreak:
                                                "break-word"
                                        }}
                                    >
                                        {knowledgeBase.description ||
                                            "Bu bilgi bankası için açıklama bulunmuyor."}
                                    </Typography>
                                </CardContent>

                                <Divider />

                                <Box
                                    sx={{
                                        padding: 2
                                    }}
                                >
                                    <Button
                                        fullWidth
                                        variant="contained"
                                        startIcon={
                                            <DescriptionIcon />
                                        }
                                        onClick={() =>
                                            navigate(
                                                `/knowledge-bases/${knowledgeBase.id}/documents`
                                            )
                                        }
                                        sx={{
                                            minHeight: 44,
                                            borderRadius: 2.5,
                                            marginBottom: 1.25
                                        }}
                                    >
                                        Belgeleri Aç
                                    </Button>

                                    <Box
                                        sx={{
                                            display: "grid",
                                            gridTemplateColumns:
                                                "1fr 1fr",
                                            gap: 1.25
                                        }}
                                    >
                                        <Button
                                            variant="outlined"
                                            startIcon={
                                                <EditIcon />
                                            }
                                            onClick={() =>
                                                handleOpenEdit(
                                                    knowledgeBase
                                                )
                                            }
                                            sx={{
                                                minHeight: 42,
                                                borderRadius: 2.5
                                            }}
                                        >
                                            Düzenle
                                        </Button>

                                        <Button
                                            variant="outlined"
                                            color="error"
                                            startIcon={
                                                <DeleteIcon />
                                            }
                                            onClick={() =>
                                                handleOpenDelete(
                                                    knowledgeBase
                                                )
                                            }
                                            sx={{
                                                minHeight: 42,
                                                borderRadius: 2.5
                                            }}
                                        >
                                            Sil
                                        </Button>
                                    </Box>
                                </Box>
                            </Card>
                        )
                    )}
                </Box>
            )}

            <Dialog
                open={formDialogOpen}
                onClose={
                    saving
                        ? undefined
                        : handleCloseFormDialog
                }
                fullWidth
                maxWidth="sm"
            >
                <DialogTitle
                    sx={{
                        fontWeight: 800
                    }}
                >
                    {selectedKnowledgeBase
                        ? "Bilgi Bankasını Düzenle"
                        : "Yeni Bilgi Bankası"}
                </DialogTitle>

                <DialogContent>
                    <TextField
                        autoFocus
                        required
                        fullWidth
                        label="Bilgi Bankası Adı"
                        value={form.name}
                        onChange={(
                            event
                        ) =>
                            setForm(
                                (current) => ({
                                    ...current,
                                    name:
                                        event.target
                                            .value
                                })
                            )
                        }
                        sx={{
                            marginTop: 1,
                            marginBottom: 2
                        }}
                    />

                    <TextField
                        fullWidth
                        multiline
                        minRows={4}
                        label="Açıklama"
                        value={
                            form.description ??
                            ""
                        }
                        onChange={(
                            event
                        ) =>
                            setForm(
                                (current) => ({
                                    ...current,
                                    description:
                                        event.target
                                            .value
                                })
                            )
                        }
                        helperText={`${
                            form.description?.length ??
                            0
                        } karakter`}
                    />
                </DialogContent>

                <DialogActions
                    sx={{
                        padding: 3
                    }}
                >
                    <Button
                        onClick={
                            handleCloseFormDialog
                        }
                        disabled={saving}
                    >
                        İptal
                    </Button>

                    <Button
                        variant="contained"
                        onClick={
                            handleSave
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
                open={deleteDialogOpen}
                onClose={
                    deleting
                        ? undefined
                        : handleCloseDeleteDialog
                }
                fullWidth
                maxWidth="xs"
            >
                <DialogTitle
                    sx={{
                        fontWeight: 800
                    }}
                >
                    Bilgi Bankasını Sil
                </DialogTitle>

                <DialogContent>
                    <DialogContentText>
                        <strong>
                            {knowledgeBaseToDelete
                                ?.name ?? ""}
                        </strong>{" "}
                        adlı bilgi bankasını
                        silmek istediğinize emin
                        misiniz? Bilgi bankasına
                        bağlı belgeler de
                        silinebilir.
                    </DialogContentText>
                </DialogContent>

                <DialogActions>
                    <Button
                        onClick={
                            handleCloseDeleteDialog
                        }
                        disabled={deleting}
                    >
                        Vazgeç
                    </Button>

                    <Button
                        color="error"
                        variant="contained"
                        onClick={
                            handleConfirmDelete
                        }
                        disabled={deleting}
                    >
                        {deleting
                            ? "Siliniyor..."
                            : "Sil"}
                    </Button>
                </DialogActions>
            </Dialog>

            <Snackbar
                open={Boolean(
                    successMessage
                )}
                autoHideDuration={3000}
                onClose={() =>
                    setSuccessMessage("")
                }
                message={successMessage}
            />
        </Box>
    );
}

export default KnowledgeBasesPage;