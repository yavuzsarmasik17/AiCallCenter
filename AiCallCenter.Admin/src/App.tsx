import type { ReactNode } from "react";
import {
    BrowserRouter,
    Navigate,
    Route,
    Routes
} from "react-router-dom";

import AdminLayout from "./layouts/AdminLayout";
import AiAgentsPage from "./pages/AiAgentsPage";
import CallOperatorsPage from "./pages/CallOperatorsPage";
import CallQueuesPage from "./pages/CallQueuesPage";
import ChatsPage from "./pages/ChatsPage";
import CreateCallPage from "./pages/CreateCallPage";
import DashboardPage from "./pages/DashboardPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import KnowledgeBaseDocumentsPage from "./pages/KnowledgeBaseDocumentsPage";
import KnowledgeBasesPage from "./pages/KnowledgeBasesPage";
import LiveCallsPage from "./pages/LiveCallsPage";
import LoginPage from "./pages/LoginPage";
import OperatorPanelPage from "./pages/OperatorPanelPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import UsersPage from "./pages/UsersPage";
import VerifyResetCodePage from "./pages/VerifyResetCodePage";

interface ProtectedRouteProps {
    children: ReactNode;
}

function ProtectedRoute({
    children
}: ProtectedRouteProps) {
    return localStorage.getItem("accessToken")
        ? children
        : <Navigate to="/login" replace />;
}

function ProtectedPage({
    children
}: ProtectedRouteProps) {
    return (
        <ProtectedRoute>
            <AdminLayout>
                {children}
            </AdminLayout>
        </ProtectedRoute>
    );
}

function HomeRedirect() {
    return (
        <Navigate
            to={
                localStorage.getItem("accessToken")
                    ? "/dashboard"
                    : "/login"
            }
            replace
        />
    );
}

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/forgot-password" element={<ForgotPasswordPage />} />
                <Route path="/verify-reset-code" element={<VerifyResetCodePage />} />
                <Route path="/reset-password" element={<ResetPasswordPage />} />

                <Route path="/dashboard" element={
                    <ProtectedPage><DashboardPage /></ProtectedPage>
                } />

                <Route path="/ai-agents" element={
                    <ProtectedPage><AiAgentsPage /></ProtectedPage>
                } />

                <Route path="/knowledge-bases" element={
                    <ProtectedPage><KnowledgeBasesPage /></ProtectedPage>
                } />

                <Route path="/knowledge-bases/:knowledgeBaseId/documents" element={
                    <ProtectedPage><KnowledgeBaseDocumentsPage /></ProtectedPage>
                } />

                <Route path="/conversations" element={
                    <ProtectedPage><ChatsPage /></ProtectedPage>
                } />

                <Route path="/call-operators" element={
                    <ProtectedPage><CallOperatorsPage /></ProtectedPage>
                } />

                <Route path="/operator-panel" element={
                    <ProtectedPage><OperatorPanelPage /></ProtectedPage>
                } />

                <Route path="/call-queues" element={
                    <ProtectedPage><CallQueuesPage /></ProtectedPage>
                } />

                <Route path="/create-call" element={
                    <ProtectedPage><CreateCallPage /></ProtectedPage>
                } />

                <Route path="/live-calls" element={
                    <ProtectedPage><LiveCallsPage /></ProtectedPage>
                } />

                <Route path="/users" element={
                    <ProtectedPage><UsersPage /></ProtectedPage>
                } />

                <Route path="/" element={<HomeRedirect />} />
                <Route path="*" element={<HomeRedirect />} />
            </Routes>
        </BrowserRouter>
    );
}
