export interface User {
    id: string;
    companyId: string;
    firstName: string;
    lastName: string;
    fullName: string;
    email: string;
    role: number;
    isActive: boolean;
    createdAt: string;
    lastLoginAt: string | null;
}

export interface UserRoleOption {
    value: number;
    name: string;
    displayName: string;
}

export interface CreateUserRequest {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    role: number;
}

export interface UpdateUserRequest {
    firstName: string;
    lastName: string;
    email: string;
}
