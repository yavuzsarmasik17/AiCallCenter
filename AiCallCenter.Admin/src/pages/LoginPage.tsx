import {
  useState,
  type FormEvent,
} from "react";
import {
  useLocation,
  useNavigate,
} from "react-router-dom";
import axios from "axios";
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Checkbox,
  CircularProgress,
  Divider,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import {
  AutoAwesome,
  LockOutlined,
  Visibility,
  VisibilityOff,
} from "@mui/icons-material";
import { login } from "../services/authService";

interface LoginLocationState {
  passwordResetSuccess?: boolean;
}

const REMEMBERED_EMAIL_KEY =
  "aiCallCenterRememberedEmail";

function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const locationState =
    location.state as LoginLocationState | null;

  const rememberedEmail =
    localStorage.getItem(REMEMBERED_EMAIL_KEY) ?? "";

  const [email, setEmail] = useState(
    rememberedEmail || "fatih@aicallcenter.com",
  );

  const [password, setPassword] = useState(
    rememberedEmail ? "" : "Test1234",
  );

  const [rememberMe, setRememberMe] = useState(
    Boolean(rememberedEmail),
  );

  const [showPassword, setShowPassword] =
    useState(false);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(
    locationState?.passwordResetSuccess
      ? "Şifreniz başarıyla değiştirildi. Yeni şifrenizle giriş yapabilirsiniz."
      : "",
  );

  const handleSubmit = async (
    event: FormEvent<HTMLFormElement>,
  ) => {
    event.preventDefault();

    setError("");
    setSuccess("");

    const normalizedEmail =
      email.trim().toLowerCase();

    if (!normalizedEmail) {
      setError("E-posta adresini giriniz.");
      return;
    }

    if (!password) {
      setError("Şifrenizi giriniz.");
      return;
    }

    setLoading(true);

    try {
      const response = await login({
        email: normalizedEmail,
        password,
      });

      localStorage.setItem(
        "accessToken",
        response.accessToken,
      );

      localStorage.setItem(
        "user",
        JSON.stringify({
          userId: response.userId,
          companyId: response.companyId,
          fullName: response.fullName,
          email: response.email,
          role: response.role,
        }),
      );

      if (rememberMe) {
        localStorage.setItem(
          REMEMBERED_EMAIL_KEY,
          normalizedEmail,
        );
      } else {
        localStorage.removeItem(
          REMEMBERED_EMAIL_KEY,
        );
      }

      window.location.replace("/dashboard");
    } catch (exception) {
      if (axios.isAxiosError(exception)) {
        const responseData =
          exception.response?.data as
            | {
                detail?: string;
                title?: string;
              }
            | undefined;

        setError(
          responseData?.detail ??
            responseData?.title ??
            "E-posta adresi veya şifre hatalı.",
        );
      } else {
        setError(
          "Giriş işlemi sırasında beklenmeyen bir hata oluştu.",
        );
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: "100vh",
        position: "relative",
        overflow: "hidden",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        px: 2,
        py: 4,
        background:
          "linear-gradient(135deg, #e0e7ff 0%, #f8fafc 48%, #cffafe 100%)",
        "&::before": {
          content: '""',
          position: "absolute",
          width: 360,
          height: 360,
          borderRadius: "50%",
          background:
            "rgba(99, 102, 241, 0.14)",
          top: -120,
          left: -90,
          filter: "blur(6px)",
          animation:
            "floatCircle 8s ease-in-out infinite",
        },
        "&::after": {
          content: '""',
          position: "absolute",
          width: 320,
          height: 320,
          borderRadius: "50%",
          background:
            "rgba(6, 182, 212, 0.13)",
          bottom: -120,
          right: -80,
          filter: "blur(8px)",
          animation:
            "floatCircle 10s ease-in-out infinite reverse",
        },
        "@keyframes floatCircle": {
          "0%, 100%": {
            transform: "translate3d(0, 0, 0)",
          },
          "50%": {
            transform:
              "translate3d(18px, -22px, 0)",
          },
        },
      }}
    >
      <Card
        elevation={16}
        sx={{
          position: "relative",
          zIndex: 1,
          width: "100%",
          maxWidth: 450,
          borderRadius: 5,
          border:
            "1px solid rgba(255,255,255,0.8)",
          background:
            "rgba(255,255,255,0.92)",
          backdropFilter: "blur(18px)",
          animation:
            "loginCardEnter .55s ease-out",
          "@keyframes loginCardEnter": {
            from: {
              opacity: 0,
              transform:
                "translateY(18px) scale(.98)",
            },
            to: {
              opacity: 1,
              transform:
                "translateY(0) scale(1)",
            },
          },
        }}
      >
        <CardContent
          sx={{
            p: {
              xs: 3,
              sm: 5,
            },
          }}
        >
          <Stack
            spacing={1.5}
            sx={{
              mb: 3,
              alignItems: "center",
            }}
          >
            <Box
              sx={{
                width: 64,
                height: 64,
                display: "grid",
                placeItems: "center",
                borderRadius: 3,
                color: "white",
                background:
                  "linear-gradient(135deg, #4f46e5, #0891b2)",
                boxShadow:
                  "0 14px 32px rgba(79, 70, 229, .28)",
              }}
            >
              <AutoAwesome sx={{ fontSize: 32 }} />
            </Box>

            <Typography
              component="h1"
              variant="h4"
              sx={{
                fontWeight: 800,
                textAlign: "center",
                letterSpacing: -0.6,
              }}
            >
              AI Call Center
            </Typography>

            <Typography
              color="text.secondary"
              sx={{
                textAlign: "center",
              }}
            >
              Yönetim paneline güvenli giriş
            </Typography>
          </Stack>

          {success && (
            <Alert
              severity="success"
              sx={{ mb: 2.5 }}
            >
              {success}
            </Alert>
          )}

          {error && (
            <Alert
              severity="error"
              sx={{ mb: 2.5 }}
            >
              {error}
            </Alert>
          )}

          <Box
            component="form"
            onSubmit={handleSubmit}
            noValidate
          >
            <TextField
              label="E-posta"
              type="email"
              fullWidth
              required
              autoFocus
              autoComplete="email"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
              disabled={loading}
              sx={{ mb: 2 }}
            />

            <TextField
              label="Şifre"
              type={
                showPassword ? "text" : "password"
              }
              fullWidth
              required
              autoComplete="current-password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              disabled={loading}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockOutlined
                        fontSize="small"
                        color="action"
                      />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        edge="end"
                        aria-label={
                          showPassword
                            ? "Şifreyi gizle"
                            : "Şifreyi göster"
                        }
                        onClick={() =>
                          setShowPassword(
                            (value) => !value,
                          )
                        }
                        disabled={loading}
                      >
                        {showPassword ? (
                          <VisibilityOff />
                        ) : (
                          <Visibility />
                        )}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
            />

            <Stack
              direction="row"
              sx={{
                mt: 0.75,
                mb: 2.5,
                justifyContent: "space-between",
                alignItems: "center",
              }}
            >
              <FormControlLabel
                control={
                  <Checkbox
                    checked={rememberMe}
                    onChange={(event) =>
                      setRememberMe(
                        event.target.checked,
                      )
                    }
                    disabled={loading}
                    size="small"
                  />
                }
                label="Beni hatırla"
              />

              <Button
                variant="text"
                size="small"
                disabled={loading}
                onClick={() =>
                  navigate("/forgot-password")
                }
                sx={{
                  textTransform: "none",
                  fontWeight: 700,
                }}
              >
                Şifremi unuttum
              </Button>
            </Stack>

            <Button
              type="submit"
              variant="contained"
              size="large"
              fullWidth
              disabled={loading}
              sx={{
                minHeight: 50,
                borderRadius: 2.5,
                fontWeight: 800,
                textTransform: "none",
                background:
                  "linear-gradient(135deg, #4f46e5, #0891b2)",
                boxShadow:
                  "0 12px 24px rgba(79, 70, 229, .24)",
                "&:hover": {
                  background:
                    "linear-gradient(135deg, #4338ca, #0e7490)",
                },
              }}
            >
              {loading ? (
                <CircularProgress
                  size={24}
                  color="inherit"
                />
              ) : (
                "Giriş Yap"
              )}
            </Button>

            <Divider sx={{ my: 3 }} />

            <Typography
              variant="caption"
              color="text.secondary"
              sx={{
                textAlign: "center",
                display: "block",
              }}
            >
              Hesabınız ve giriş bilgileriniz güvenli
              biçimde korunur.
            </Typography>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}

export default LoginPage;