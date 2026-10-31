import { useCallback, useEffect, useMemo, useState } from "react";
import axios from "axios";
import {
  Alert,
  Avatar,
  Box,
  Button,
  Card,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControlLabel,
  IconButton,
  InputAdornment,
  MenuItem,
  Switch,
  TextField,
  Tooltip,
  Typography,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import KeyIcon from "@mui/icons-material/Key";
import PeopleIcon from "@mui/icons-material/People";
import SearchIcon from "@mui/icons-material/Search";

import {
  changeUserPassword,
  changeUserRole,
  createUser,
  deleteUser,
  getStoredCompanyId,
  getUserRoles,
  getUsers,
  setUserStatus,
  updateUser,
} from "../services/userService";

import type { CreateUserRequest, User, UserRoleOption } from "../types/user";

interface UserFormState {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: number;
}

const emptyForm: UserFormState = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  role: 0,
};

function formatDate(value: string | null): string {
  if (!value) {
    return "Henüz giriş yapmadı";
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return new Intl.DateTimeFormat("tr-TR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(date);
}

function getApiErrorMessage(exception: unknown, fallback: string): string {
  if (!axios.isAxiosError(exception)) {
    return exception instanceof Error ? exception.message : fallback;
  }

  const data = exception.response?.data as
    | {
        detail?: string;
        title?: string;
        message?: string;
      }
    | undefined;

  return (
    data?.detail ??
    data?.title ??
    data?.message ??
    exception.message ??
    fallback
  );
}

function UsersPage() {
  const companyId = getStoredCompanyId();

  const [users, setUsers] = useState<User[]>([]);

  const [roles, setRoles] = useState<UserRoleOption[]>([]);

  const [searchText, setSearchText] = useState("");

  const [loading, setLoading] = useState(true);

  const [saving, setSaving] = useState(false);

  const [error, setError] = useState("");

  const [success, setSuccess] = useState("");

  const [userDialogOpen, setUserDialogOpen] = useState(false);

  const [passwordDialogOpen, setPasswordDialogOpen] = useState(false);

  const [editingUser, setEditingUser] = useState<User | null>(null);

  const [passwordUser, setPasswordUser] = useState<User | null>(null);

  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const [deletingUser, setDeletingUser] = useState<User | null>(null);

  const [form, setForm] = useState<UserFormState>(emptyForm);

  const [newPassword, setNewPassword] = useState("");

  const loadData = useCallback(async () => {
    if (!companyId) {
      setError("Firma kimliği bulunamadı. Yeniden giriş yapın.");
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError("");

      const [userData, roleData] = await Promise.all([
        getUsers(companyId),
        getUserRoles(companyId),
      ]);

      setUsers(userData);
      setRoles(roleData);

      if (roleData.length > 0) {
        setForm((current) => {
          if (current.role !== 0) {
            return current;
          }

          const defaultRole =
            roleData.find((role) => role.name === "Operator")?.value ??
            roleData[0].value;

          return {
            ...current,
            role: defaultRole,
          };
        });
      }
    } catch (exception) {
      setError(getApiErrorMessage(exception, "Kullanıcılar yüklenemedi."));
    } finally {
      setLoading(false);
    }
  }, [companyId]);

  useEffect(() => {
    queueMicrotask(() => {
      void loadData();
    });
  }, [loadData]);

  const roleMap = useMemo(
    () => new Map(roles.map((role) => [role.value, role.displayName])),
    [roles],
  );

  const filteredUsers = useMemo(() => {
    const query = searchText.trim().toLocaleLowerCase("tr-TR");

    if (!query) {
      return users;
    }

    return users.filter(
      (user) =>
        user.fullName.toLocaleLowerCase("tr-TR").includes(query) ||
        user.email.toLocaleLowerCase("tr-TR").includes(query) ||
        (roleMap.get(user.role) ?? "")
          .toLocaleLowerCase("tr-TR")
          .includes(query),
    );
  }, [users, searchText, roleMap]);

  const openCreateDialog = () => {
    const defaultRole =
      roles.find((role) => role.name === "Operator")?.value ??
      roles[0]?.value ??
      0;

    setEditingUser(null);
    setForm({
      ...emptyForm,
      role: defaultRole,
    });
    setUserDialogOpen(true);
    setError("");
    setSuccess("");
  };

  const openEditDialog = (user: User) => {
    setEditingUser(user);
    setForm({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      password: "",
      role: user.role,
    });
    setUserDialogOpen(true);
    setError("");
    setSuccess("");
  };

  const handleSaveUser = async () => {
    if (!companyId) {
      return;
    }

    if (!form.firstName.trim() || !form.lastName.trim() || !form.email.trim()) {
      setError("Ad, soyad ve e-posta alanları zorunludur.");
      return;
    }

    if (!editingUser && form.password.length < 8) {
      setError("Parola en az 8 karakter olmalıdır.");
      return;
    }

    setSaving(true);
    setError("");
    setSuccess("");

    try {
      if (editingUser) {
        const updated = await updateUser(companyId, editingUser.id, {
          firstName: form.firstName,
          lastName: form.lastName,
          email: form.email,
        });

        const withRole =
          updated.role === form.role
            ? updated
            : await changeUserRole(companyId, editingUser.id, form.role);

        setUsers((current) =>
          current.map((user) => (user.id === withRole.id ? withRole : user)),
        );

        setSuccess("Kullanıcı güncellendi.");
      } else {
        const request: CreateUserRequest = {
          firstName: form.firstName,
          lastName: form.lastName,
          email: form.email,
          password: form.password,
          role: form.role,
        };

        const created = await createUser(companyId, request);

        setUsers((current) => [created, ...current]);

        setSuccess("Kullanıcı oluşturuldu.");
      }

      setUserDialogOpen(false);
    } catch (exception) {
      setError(getApiErrorMessage(exception, "Kullanıcı kaydedilemedi."));
    } finally {
      setSaving(false);
    }
  };

  const handleStatusChange = async (user: User, isActive: boolean) => {
    if (!companyId) {
      return;
    }

    setError("");
    setSuccess("");

    try {
      const updated = await setUserStatus(companyId, user.id, isActive);

      setUsers((current) =>
        current.map((item) => (item.id === updated.id ? updated : item)),
      );

      setSuccess(
        isActive ? "Kullanıcı aktifleştirildi." : "Kullanıcı pasifleştirildi.",
      );
    } catch (exception) {
      setError(
        getApiErrorMessage(exception, "Kullanıcı durumu değiştirilemedi."),
      );
    }
  };

  const openPasswordDialog = (user: User) => {
    setPasswordUser(user);
    setNewPassword("");
    setPasswordDialogOpen(true);
    setError("");
    setSuccess("");
  };

  const handlePasswordSave = async () => {
    if (!companyId || !passwordUser) {
      return;
    }

    if (newPassword.length < 8) {
      setError("Yeni parola en az 8 karakter olmalıdır.");
      return;
    }

    setSaving(true);
    setError("");

    try {
      await changeUserPassword(companyId, passwordUser.id, newPassword);

      setPasswordDialogOpen(false);
      setSuccess("Kullanıcı parolası değiştirildi.");
    } catch (exception) {
      setError(getApiErrorMessage(exception, "Parola değiştirilemedi."));
    } finally {
      setSaving(false);
    }
  };

  const openDeleteDialog = (user: User) => {
    setDeletingUser(user);
    setDeleteDialogOpen(true);
    setError("");
    setSuccess("");
  };

  const closeDeleteDialog = () => {
    if (saving) {
      return;
    }

    setDeleteDialogOpen(false);
    setDeletingUser(null);
  };

  const handleDeleteUser = async () => {
    if (!companyId || !deletingUser) {
      return;
    }

    setSaving(true);
    setError("");
    setSuccess("");

    try {
      await deleteUser(companyId, deletingUser.id);

      setUsers((current) =>
        current.filter((user) => user.id !== deletingUser.id),
      );

      setDeleteDialogOpen(false);
      setDeletingUser(null);
      setSuccess("Kullanıcı başarıyla silindi.");
    } catch (exception) {
      setError(getApiErrorMessage(exception, "Kullanıcı silinemedi."));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: {
            xs: "flex-start",
            md: "center",
          },
          flexDirection: {
            xs: "column",
            md: "row",
          },
          gap: 2,
          marginBottom: 3,
        }}
      >
        <Box>
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            Kullanıcılar
          </Typography>

          <Typography color="text.secondary">
            Firma kullanıcılarını, rollerini ve erişim durumlarını yönetin.
          </Typography>
        </Box>

        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={openCreateDialog}
          sx={{ borderRadius: 3 }}
        >
          Yeni Kullanıcı
        </Button>
      </Box>

      {error && (
        <Alert
          severity="error"
          sx={{ marginBottom: 2 }}
          onClose={() => setError("")}
        >
          {error}
        </Alert>
      )}

      {success && (
        <Alert
          severity="success"
          sx={{ marginBottom: 2 }}
          onClose={() => setSuccess("")}
        >
          {success}
        </Alert>
      )}

      <Card
        sx={{
          borderRadius: 4,
          border: 1,
          borderColor: "divider",
          overflow: "hidden",
        }}
      >
        <Box
          sx={{
            padding: 2,
            display: "flex",
            gap: 2,
            alignItems: "center",
          }}
        >
          <PeopleIcon color="primary" />

          <TextField
            fullWidth
            size="small"
            placeholder="Ad, e-posta veya rol ara..."
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            slotProps={{
    input: {
        startAdornment: (
            <InputAdornment position="start">
                <SearchIcon />
            </InputAdornment>
        )
    }
}}
          />
        </Box>

        {loading ? (
          <Box
            sx={{
              display: "flex",
              justifyContent: "center",
              padding: 6,
            }}
          >
            <CircularProgress />
          </Box>
        ) : filteredUsers.length === 0 ? (
          <Box
            sx={{
              textAlign: "center",
              padding: 6,
            }}
          >
            <PeopleIcon
              sx={{
                fontSize: 56,
                color: "text.disabled",
              }}
            />

            <Typography
              variant="h6"
              sx={{
                marginTop: 1,
                fontWeight: 700,
              }}
            >
              Kullanıcı bulunamadı
            </Typography>
          </Box>
        ) : (
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: {
                xs: "1fr",
                lg: "1fr 1fr",
              },
              gap: 2,
              padding: 2,
              paddingTop: 0,
            }}
          >
            {filteredUsers.map((user) => (
              <Card
                key={user.id}
                variant="outlined"
                sx={{
                  padding: 2,
                  borderRadius: 3,
                }}
              >
                <Box
                  sx={{
                    display: "flex",
                    alignItems: "flex-start",
                    gap: 2,
                  }}
                >
                  <Avatar
                    sx={{
                      width: 48,
                      height: 48,
                      backgroundColor: "primary.main",
                    }}
                  >
                    {user.firstName.slice(0, 1).toUpperCase()}
                    {user.lastName.slice(0, 1).toUpperCase()}
                  </Avatar>

                  <Box
                    sx={{
                      flexGrow: 1,
                      minWidth: 0,
                    }}
                  >
                    <Typography
                      sx={{
                        fontWeight: 800,
                      }}
                    >
                      {user.fullName}
                    </Typography>

                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{
                        overflow: "hidden",
                        textOverflow: "ellipsis",
                      }}
                    >
                      {user.email}
                    </Typography>

                    <Box
                      sx={{
                        display: "flex",
                        flexWrap: "wrap",
                        gap: 1,
                        marginTop: 1,
                      }}
                    >
                      <Chip
                        size="small"
                        label={roleMap.get(user.role) ?? user.role}
                      />

                      <Chip
                        size="small"
                        color={user.isActive ? "success" : "default"}
                        label={user.isActive ? "Aktif" : "Pasif"}
                      />
                    </Box>

                    <Typography
                      variant="caption"
                      color="text.secondary"
                      sx={{
                        display: "block",
                        marginTop: 1,
                      }}
                    >
                      Son giriş: {formatDate(user.lastLoginAt)}
                    </Typography>
                  </Box>

                  <Box
                    sx={{
                      display: "flex",
                      flexDirection: "column",
                      alignItems: "center",
                    }}
                  >
                    <Tooltip title="Düzenle">
                      <IconButton onClick={() => openEditDialog(user)}>
                        <EditIcon />
                      </IconButton>
                    </Tooltip>

                    <Tooltip title="Parola değiştir">
                      <IconButton onClick={() => openPasswordDialog(user)}>
                        <KeyIcon />
                      </IconButton>
                    </Tooltip>

                    <Tooltip title="Kullanıcıyı sil">
                      <IconButton
                        color="error"
                        onClick={() => openDeleteDialog(user)}
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>

                    <FormControlLabel
                      control={
                        <Switch
                          size="small"
                          checked={user.isActive}
                          onChange={(event) =>
                            void handleStatusChange(user, event.target.checked)
                          }
                        />
                      }
                      label=""
                      sx={{
                        margin: 0,
                      }}
                    />
                  </Box>
                </Box>
              </Card>
            ))}
          </Box>
        )}
      </Card>

      <Dialog
        open={userDialogOpen}
        onClose={() => !saving && setUserDialogOpen(false)}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>
          {editingUser ? "Kullanıcı Düzenle" : "Yeni Kullanıcı"}
        </DialogTitle>

        <DialogContent>
          <Box
            sx={{
              display: "grid",
              gap: 2,
              paddingTop: 1,
            }}
          >
            <TextField
              label="Ad"
              value={form.firstName}
              onChange={(event) =>
                setForm({
                  ...form,
                  firstName: event.target.value,
                })
              }
              fullWidth
            />

            <TextField
              label="Soyad"
              value={form.lastName}
              onChange={(event) =>
                setForm({
                  ...form,
                  lastName: event.target.value,
                })
              }
              fullWidth
            />

            <TextField
              label="E-posta"
              type="email"
              value={form.email}
              onChange={(event) =>
                setForm({
                  ...form,
                  email: event.target.value,
                })
              }
              fullWidth
            />

            {!editingUser && (
              <TextField
                label="Parola"
                type="password"
                value={form.password}
                onChange={(event) =>
                  setForm({
                    ...form,
                    password: event.target.value,
                  })
                }
                helperText="En az 8 karakter"
                fullWidth
              />
            )}

            <TextField
              select
              label="Rol"
              value={form.role}
              onChange={(event) =>
                setForm({
                  ...form,
                  role: Number(event.target.value),
                })
              }
              fullWidth
            >
              {roles.map((role) => (
                <MenuItem key={role.value} value={role.value}>
                  {role.displayName}
                </MenuItem>
              ))}
            </TextField>
          </Box>
        </DialogContent>

        <DialogActions>
          <Button disabled={saving} onClick={() => setUserDialogOpen(false)}>
            Vazgeç
          </Button>

          <Button
            variant="contained"
            disabled={saving}
            onClick={() => void handleSaveUser()}
          >
            {saving ? <CircularProgress size={20} /> : "Kaydet"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={passwordDialogOpen}
        onClose={() => !saving && setPasswordDialogOpen(false)}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>Parola Değiştir</DialogTitle>

        <DialogContent>
          <Typography color="text.secondary" sx={{ marginBottom: 2 }}>
            {passwordUser?.fullName}
          </Typography>

          <TextField
            autoFocus
            fullWidth
            label="Yeni parola"
            type="password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            helperText="En az 8 karakter"
          />
        </DialogContent>

        <DialogActions>
          <Button
            disabled={saving}
            onClick={() => setPasswordDialogOpen(false)}
          >
            Vazgeç
          </Button>

          <Button
            variant="contained"
            disabled={saving}
            onClick={() => void handlePasswordSave()}
          >
            {saving ? <CircularProgress size={20} /> : "Parolayı Değiştir"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={deleteDialogOpen}
        onClose={closeDeleteDialog}
        fullWidth
        maxWidth="xs"
      >
        <DialogTitle>Kullanıcıyı Sil</DialogTitle>

        <DialogContent>
          <Typography>
            <strong>{deletingUser?.fullName}</strong> adlı kullanıcıyı silmek
            istediğinize emin misiniz?
          </Typography>

          <Alert severity="warning" sx={{ marginTop: 2 }}>
            Bu işlem geri alınamaz.
          </Alert>
        </DialogContent>

        <DialogActions>
          <Button disabled={saving} onClick={closeDeleteDialog}>
            Vazgeç
          </Button>

          <Button
            color="error"
            variant="contained"
            disabled={saving}
            onClick={() => void handleDeleteUser()}
          >
            {saving ? <CircularProgress size={20} color="inherit" /> : "Sil"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

export default UsersPage;