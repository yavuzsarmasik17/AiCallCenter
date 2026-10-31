import {
  Visibility,
  VisibilityOff,
} from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  IconButton,
  InputAdornment,
  LinearProgress,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import type { FormEvent } from "react";
import {
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  useLocation,
  useNavigate,
} from "react-router-dom";
import { resetPassword } from "../services/authService";

function calculatePasswordScore(password: string) {
  let score = 0;

  if (password.length >= 8) score += 20;
  if (password.length >= 12) score += 20;
  if (/[a-z]/.test(password)) score += 15;
  if (/[A-Z]/.test(password)) score += 15;
  if (/\d/.test(password)) score += 15;
  if (/[^A-Za-z0-9]/.test(password)) score += 15;

  return Math.min(100, score);
}

function getPasswordStrengthLabel(score: number) {
  if (score < 40) return "Zayıf";
  if (score < 70) return "Orta";
  if (score < 90) return "Güçlü";
  return "Çok güçlü";
}

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const locationState = location.state as
    | {
        email?: string;
        code?: string;
      }
    | null;

  const email =
    locationState?.email ??
    sessionStorage.getItem("passwordResetEmail") ??
    "";

  const code =
    locationState?.code ??
    sessionStorage.getItem("passwordResetCode") ??
    "";

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] =
    useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] =
    useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const passwordStrength = useMemo(
    () => calculatePasswordScore(newPassword),
    [newPassword],
  );

  useEffect(() => {
    if (!email || !code) {
      navigate("/forgot-password", { replace: true });
    }
  }, [email, code, navigate]);

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    setError("");
    setSuccess("");

    if (!email || !code) {
      setError(
        "Şifre sıfırlama bilgileri bulunamadı. İşleme yeniden başlayınız.",
      );
      return;
    }

    if (newPassword.length < 8) {
      setError("Yeni şifre en az 8 karakter olmalıdır.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Şifreler birbiriyle eşleşmiyor.");
      return;
    }

    try {
      setLoading(true);

      const response = await resetPassword({
        email,
        code,
        newPassword,
        confirmPassword,
      });

      setSuccess(
        response.message ??
          "Şifreniz başarıyla değiştirildi.",
      );

      [
        "passwordResetEmail",
        "passwordResetCode",
        "passwordResetExpiresAt",
      ].forEach((key) => sessionStorage.removeItem(key));

      window.setTimeout(() => {
        navigate("/login", {
          replace: true,
          state: {
            passwordResetSuccess: true,
          },
        });
      }, 1400);
    } catch {
      setError(
        "Şifre değiştirilemedi. Kodun süresi dolmuş olabilir.",
      );
    } finally {
      setLoading(false);
    }
  }

  function createPasswordAdornment(
    visible: boolean,
    onToggle: () => void,
    label: string,
  ) {
    return (
      <InputAdornment position="end">
        <IconButton
          edge="end"
          onClick={onToggle}
          disabled={loading}
          aria-label={label}
        >
          {visible ? <VisibilityOff /> : <Visibility />}
        </IconButton>
      </InputAdornment>
    );
  }

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        py: 4,
        background:
          "linear-gradient(135deg,#eef2ff 0%,#f8fafc 50%,#ecfeff 100%)",
      }}
    >
      <Container maxWidth="sm">
        <Paper
          elevation={8}
          sx={{
            p: { xs: 3, sm: 5 },
            borderRadius: 4,
          }}
        >
          <Typography
            component="h1"
            variant="h4"
            sx={{
              fontWeight: 800,
              textAlign: "center",
            }}
          >
            Yeni Şifre Belirle
          </Typography>

          <Typography
            color="text.secondary"
            sx={{
              mt: 1,
              mb: 4,
              textAlign: "center",
            }}
          >
            Hesabınız için güvenli bir şifre oluşturunuz.
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              fullWidth
              required
              autoFocus
              label="Yeni şifre"
              type={showPassword ? "text" : "password"}
              value={newPassword}
              onChange={(event) =>
                setNewPassword(event.target.value)
              }
              disabled={loading}
              autoComplete="new-password"
              slotProps={{
                input: {
                  endAdornment: createPasswordAdornment(
                    showPassword,
                    () =>
                      setShowPassword((value) => !value),
                    showPassword
                      ? "Şifreyi gizle"
                      : "Şifreyi göster",
                  ),
                },
              }}
            />

            <Box sx={{ mt: 1, mb: 2 }}>
              <LinearProgress
                variant="determinate"
                value={passwordStrength}
                sx={{
                  height: 8,
                  borderRadius: 4,
                }}
              />

              <Typography
                variant="caption"
                color="text.secondary"
              >
                Şifre gücü:{" "}
                {getPasswordStrengthLabel(
                  passwordStrength,
                )}
              </Typography>
            </Box>

            <TextField
              fullWidth
              required
              label="Yeni şifre tekrarı"
              type={
                showConfirmPassword ? "text" : "password"
              }
              value={confirmPassword}
              onChange={(event) =>
                setConfirmPassword(event.target.value)
              }
              disabled={loading}
              autoComplete="new-password"
              error={
                confirmPassword.length > 0 &&
                newPassword !== confirmPassword
              }
              helperText={
                confirmPassword.length > 0 &&
                newPassword !== confirmPassword
                  ? "Şifreler eşleşmiyor."
                  : " "
              }
              slotProps={{
                input: {
                  endAdornment: createPasswordAdornment(
                    showConfirmPassword,
                    () =>
                      setShowConfirmPassword(
                        (value) => !value,
                      ),
                    showConfirmPassword
                      ? "Şifre tekrarını gizle"
                      : "Şifre tekrarını göster",
                  ),
                },
              }}
            />

            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ mb: 2 }}
            >
              En az 8 karakter kullanın. Büyük harf, küçük
              harf, rakam ve özel karakter önerilir.
            </Typography>

            <Button
              fullWidth
              type="submit"
              variant="contained"
              size="large"
              disabled={loading || Boolean(success)}
              sx={{ py: 1.4 }}
            >
              {loading ? (
                <CircularProgress size={24} color="inherit" />
              ) : (
                "Şifreyi Kaydet"
              )}
            </Button>

            <Button
              fullWidth
              variant="text"
              onClick={() => navigate("/verify-reset-code")}
              sx={{ mt: 1 }}
            >
              Kod ekranına dön
            </Button>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
}