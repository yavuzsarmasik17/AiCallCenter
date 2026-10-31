import api from "./api";

export interface LoginRequest { email: string; password: string; }
export interface LoginResponse {
  accessToken: string; expiresAt: string; userId: string; companyId: string;
  fullName: string; email: string; role: number;
}
export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>("/api/auth/login", request);
  return response.data;
}

export interface ForgotPasswordRequest { email: string; }
export interface ForgotPasswordResponse { message: string; }
export async function forgotPassword(request: ForgotPasswordRequest): Promise<ForgotPasswordResponse> {
  const response = await api.post<ForgotPasswordResponse>("/api/auth/forgot-password", request);
  return response.data;
}

export interface VerifyResetCodeRequest { email: string; code: string; }
export interface VerifyResetCodeResponse { isValid: boolean; message: string; }
export async function verifyResetCode(request: VerifyResetCodeRequest): Promise<VerifyResetCodeResponse> {
  const response = await api.post<VerifyResetCodeResponse>("/api/auth/verify-reset-code", request);
  return response.data;
}

export interface ResetPasswordRequest {
  email: string; code: string; newPassword: string; confirmPassword: string;
}
export interface ResetPasswordResponse { message: string; }
export async function resetPassword(request: ResetPasswordRequest): Promise<ResetPasswordResponse> {
  const response = await api.post<ResetPasswordResponse>("/api/auth/reset-password", request);
  return response.data;
}
