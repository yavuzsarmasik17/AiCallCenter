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
import type {
  ChangeEvent,
  ClipboardEvent,
  KeyboardEvent,
} from "react";
import {
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import {
  useLocation,
  useNavigate,
} from "react-router-dom";
import {
  forgotPassword,
  verifyResetCode,
} from "../services/authService";

const CODE_LENGTH = 6;
const CODE_DURATION_SECONDS = 600;
const RESEND_DURATION_SECONDS = 60;

function formatTime(seconds: number) {
  return `${String(Math.floor(seconds / 60)).padStart(
    2,
    "0",
  )}:${String(seconds % 60).padStart(2, "0")}`;
}

export default function VerifyResetCodePage() {
  const navigate = useNavigate();
  const location = useLocation();

  const locationState = location.state as
    | { email?: string }
    | null;

  const email = useMemo(
    () =>
      locationState?.email ??
      sessionStorage.getItem("passwordResetEmail") ??
      "",
    [locationState?.email],
  );

  const [digits, setDigits] = useState<string[]>(
    Array(CODE_LENGTH).fill(""),
  );
  const [loading, setLoading] = useState(false);
  const [resendLoading, setResendLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [remaining, setRemaining] = useState(
    CODE_DURATION_SECONDS,
  );
  const [resendRemaining, setResendRemaining] = useState(
    RESEND_DURATION_SECONDS,
  );

  const inputRefs = useRef<Array<HTMLInputElement | null>>([]);

  useEffect(() => {
    if (!email) {
      navigate("/forgot-password", { replace: true });
      return;
    }

    sessionStorage.setItem("passwordResetEmail", email);

    const storedExpiresAt = Number(
      sessionStorage.getItem("passwordResetExpiresAt"),
    );

    const expiresAt =
      storedExpiresAt > Date.now()
        ? storedExpiresAt
        : Date.now() + CODE_DURATION_SECONDS * 1000;

    sessionStorage.setItem(
      "passwordResetExpiresAt",
      String(expiresAt),
    );

    const updateRemaining = () => {
      setRemaining(
        Math.max(
          0,
          Math.ceil((expiresAt - Date.now()) / 1000),
        ),
      );
    };

    updateRemaining();

    const timerId = window.setInterval(updateRemaining, 1000);

    return () => window.clearInterval(timerId);
  }, [email, navigate]);

  useEffect(() => {
    if (resendRemaining <= 0) {
      return;
    }

    const timerId = window.setInterval(() => {
      setResendRemaining((value) => Math.max(0, value - 1));
    }, 1000);

    return () => window.clearInterval(timerId);
  }, [resendRemaining]);

  const code = digits.join("");

  function focusInput(index: number) {
    inputRefs.current[index]?.focus();
    inputRefs.current[index]?.select();
  }

  function handleDigitChange(
    index: number,
    event: ChangeEvent<HTMLInputElement>,
  ) {
    const value = event.target.value
      .replace(/\D/g, "")
      .slice(-1);

    const nextDigits = [...digits];
    nextDigits[index] = value;

    setDigits(nextDigits);
    setError("");

    if (value && index < CODE_LENGTH - 1) {
      focusInput(index + 1);
    }
  }

  function handleKeyDown(
    index: number,
    event: KeyboardEvent<HTMLInputElement>,
  ) {
    if (
      event.key === "Backspace" &&
      !digits[index] &&
      index > 0
    ) {
      focusInput(index - 1);
    }

    if (event.key === "ArrowLeft" && index > 0) {
      event.preventDefault();
      focusInput(index - 1);
    }

    if (
      event.key === "ArrowRight" &&
      index < CODE_LENGTH - 1
    ) {
      event.preventDefault();
      focusInput(index + 1);
    }
  }

  function handlePaste(event: ClipboardEvent<HTMLDivElement>) {
    const value = event.clipboardData
      .getData("text")
      .replace(/\D/g, "")
      .slice(0, CODE_LENGTH);

    if (!value) {
      return;
    }

    event.preventDefault();

    const nextDigits = Array<string>(CODE_LENGTH).fill("");

    value.split("").forEach((digit, index) => {
      nextDigits[index] = digit;
    });

    setDigits(nextDigits);

    focusInput(
      Math.max(
        0,
        Math.min(value.length, CODE_LENGTH) - 1,
      ),
    );
  }

  async function handleVerify() {
    setError("");
    setSuccess("");

    if (remaining <= 0) {
      setError("Kodun süresi doldu. Yeni kod isteyiniz.");
      return;
    }

    if (code.length !== CODE_LENGTH) {
      setError("6 haneli doğrulama kodunu giriniz.");
      return;
    }

    try {
      setLoading(true);

      const response = await verifyResetCode({
        email,
        code,
      });

      if (!response.isValid) {
        setError(
          response.message ?? "Doğrulama kodu geçersiz.",
        );
        return;
      }

      sessionStorage.setItem("passwordResetCode", code);

      setSuccess(
        "Kod doğrulandı. Yeni şifre ekranına yönlendiriliyorsunuz.",
      );

      window.setTimeout(() => {
        navigate("/reset-password", {
          state: { email, code },
        });
      }, 700);
    } catch {
      setError(
        "Kod geçersiz, süresi dolmuş veya deneme sınırı aşılmış.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function handleResendCode() {
    if (resendRemaining > 0 || resendLoading) {
      return;
    }

    setError("");
    setSuccess("");

    try {
      setResendLoading(true);

      await forgotPassword({ email });

     const expiresAt =
     new Date().getTime() + CODE_DURATION_SECONDS * 1000;

      sessionStorage.setItem(
        "passwordResetExpiresAt",
        String(expiresAt),
      );
      sessionStorage.removeItem("passwordResetCode");

      setRemaining(CODE_DURATION_SECONDS);
      setResendRemaining(RESEND_DURATION_SECONDS);
      setDigits(Array(CODE_LENGTH).fill(""));
      setSuccess(
        "Yeni doğrulama kodu e-posta adresinize gönderildi.",
      );

      focusInput(0);
    } catch {
      setError(
        "Yeni kod gönderilemedi. Lütfen tekrar deneyiniz.",
      );
    } finally {
      setResendLoading(false);
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
            Doğrulama Kodu
          </Typography>

          <Typography
            color="text.secondary"
            sx={{
              mt: 1,
              textAlign: "center",
            }}
          >
            <strong>{email}</strong> adresine gönderilen 6
            haneli kodu giriniz.
          </Typography>

          <Typography
            variant="h6"
            color={
              remaining <= 0
                ? "error.main"
                : "primary.main"
            }
            sx={{
              mt: 2,
              mb: 3,
              fontWeight: 700,
              textAlign: "center",
            }}
          >
            {remaining <= 0
              ? "Kodun süresi doldu"
              : `Kalan süre: ${formatTime(remaining)}`}
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

          <Box
            onPaste={handlePaste}
            sx={{
              display: "flex",
              justifyContent: "center",
              gap: { xs: 0.7, sm: 1.2 },
              mb: 3,
            }}
          >
            {digits.map((digit, index) => (
              <TextField
                key={index}
                inputRef={(element: HTMLInputElement | null) => {
                  inputRefs.current[index] = element;
                }}
                value={digit}
                onChange={(event) =>
                  handleDigitChange(
                    index,
                    event as ChangeEvent<HTMLInputElement>,
                  )
                }
                onKeyDown={(event) =>
                  handleKeyDown(
                    index,
                    event as KeyboardEvent<HTMLInputElement>,
                  )
                }
                disabled={loading || remaining <= 0}
                slotProps={{
                  htmlInput: {
                    inputMode: "numeric",
                    maxLength: 1,
                    style: {
                      textAlign: "center",
                      fontSize: "1.5rem",
                      fontWeight: 700,
                      padding: "14px 4px",
                    },
                    "aria-label": `${index + 1}. kod hanesi`,
                  },
                }}
                sx={{
                  width: { xs: 43, sm: 54 },
                }}
              />
            ))}
          </Box>

          <Button
            fullWidth
            variant="contained"
            size="large"
            onClick={handleVerify}
            disabled={
              loading ||
              remaining <= 0 ||
              code.length !== CODE_LENGTH
            }
            sx={{ py: 1.4 }}
          >
            {loading ? (
              <CircularProgress size={24} color="inherit" />
            ) : (
              "Kodu Doğrula"
            )}
          </Button>

          <Button
            fullWidth
            variant="text"
            onClick={handleResendCode}
            disabled={resendRemaining > 0 || resendLoading}
            sx={{ mt: 1 }}
          >
            {resendLoading
              ? "Kod gönderiliyor..."
              : resendRemaining > 0
                ? `Kodu tekrar gönder (${resendRemaining})`
                : "Kodu tekrar gönder"}
          </Button>

          <Button
            fullWidth
            variant="text"
            onClick={() => navigate("/forgot-password")}
          >
            E-posta adresini değiştir
          </Button>
        </Paper>
      </Container>
    </Box>
  );
}