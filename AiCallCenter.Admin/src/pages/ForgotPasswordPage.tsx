import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import type { FormEvent } from "react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { forgotPassword } from "../services/authService";

export default function ForgotPasswordPage() {
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    const value = email.trim().toLowerCase();

    if (!value) {
      setError("E-posta adresini giriniz.");
      return;
    }

    try {
      setLoading(true);

      await forgotPassword({ email: value });

      sessionStorage.setItem("passwordResetEmail", value);
      sessionStorage.setItem(
        "passwordResetExpiresAt",
        String(Date.now() + 600_000),
      );

      navigate("/verify-reset-code", {
        state: { email: value },
      });
    } catch {
      setError(
        "Doğrulama kodu gönderilemedi. Lütfen tekrar deneyiniz.",
      );
    } finally {
      setLoading(false);
    }
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
            Şifremi Unuttum
          </Typography>

          <Typography
            color="text.secondary"
            sx={{
              mt: 1,
              mb: 4,
              textAlign: "center",
            }}
          >
            E-posta adresinizi girin. Size 6 haneli doğrulama
            kodu gönderelim.
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit}>
            <TextField
              fullWidth
              required
              autoFocus
              type="email"
              label="E-posta adresi"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              disabled={loading}
              autoComplete="email"
            />

            <Button
              fullWidth
              type="submit"
              variant="contained"
              size="large"
              disabled={loading}
              sx={{
                mt: 3,
                py: 1.4,
              }}
            >
              {loading ? (
                <CircularProgress size={24} color="inherit" />
              ) : (
                "Doğrulama Kodu Gönder"
              )}
            </Button>

            <Button
              fullWidth
              variant="text"
              onClick={() => navigate("/login")}
              sx={{ mt: 1 }}
            >
              Giriş ekranına dön
            </Button>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
}