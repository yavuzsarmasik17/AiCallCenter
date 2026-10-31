import axios from "axios";

import type {
    CreateUserRequest,
    UpdateUserRequest,
    User,
    UserRoleOption
} from "../types/user";

const apiBaseUrl =
    import.meta.env.VITE_API_BASE_URL ?? "";

function getHeaders() {
    const token =
        localStorage.getItem("accessToken");

    return token
        ? {
              Authorization: `Bearer ${token}`
          }
        : {};
}

export function getStoredCompanyId(): string | null {
    const directCompanyId =
        localStorage.getItem("companyId");

    if (directCompanyId) {
        return directCompanyId;
    }

    const storedUser =
        localStorage.getItem("user");

    if (!storedUser) {
        return null;
    }

    try {
        const user = JSON.parse(
            storedUser
        ) as {
            companyId?: string;
            CompanyId?: string;
        };

        return (
            user.companyId ??
            user.CompanyId ??
            null
        );
    } catch {
        return null;
    }
}

export async function getUsers(
    companyId: string
): Promise<User[]> {
    const response = await axios.get(
        `${apiBaseUrl}/api/companies/${companyId}/users`,
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function getUserRoles(
    companyId: string
): Promise<UserRoleOption[]> {
    const response = await axios.get(
        `${apiBaseUrl}/api/companies/${companyId}/users/roles`,
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function createUser(
    companyId: string,
    request: CreateUserRequest
): Promise<User> {
    const response = await axios.post(
        `${apiBaseUrl}/api/companies/${companyId}/users`,
        request,
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function updateUser(
    companyId: string,
    userId: string,
    request: UpdateUserRequest
): Promise<User> {
    const response = await axios.put(
        `${apiBaseUrl}/api/companies/${companyId}/users/${userId}`,
        request,
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function changeUserRole(
    companyId: string,
    userId: string,
    role: number
): Promise<User> {
    const response = await axios.put(
        `${apiBaseUrl}/api/companies/${companyId}/users/${userId}/role`,
        { role },
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function changeUserPassword(
    companyId: string,
    userId: string,
    newPassword: string
): Promise<void> {
    await axios.put(
        `${apiBaseUrl}/api/companies/${companyId}/users/${userId}/password`,
        { newPassword },
        {
            headers: getHeaders()
        }
    );
}

export async function setUserStatus(
    companyId: string,
    userId: string,
    isActive: boolean
): Promise<User> {
    const response = await axios.put(
        `${apiBaseUrl}/api/companies/${companyId}/users/${userId}/status`,
        { isActive },
        {
            headers: getHeaders()
        }
    );

    return response.data;
}

export async function deleteUser(
    companyId: string,
    userId: string
): Promise<void> {
    await axios.delete(
        `${apiBaseUrl}/api/companies/${companyId}/users/${userId}`,
        {
            headers: getHeaders()
        }
    );
}
