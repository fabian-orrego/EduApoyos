export const UserRoleId = {
  Advisor: 1,
  Student: 2,
} as const;

export type UserRoleIdValue = (typeof UserRoleId)[keyof typeof UserRoleId];

export interface RoleOption {
  id: UserRoleIdValue;
  label: string;
}

export const USER_ROLE_OPTIONS: readonly RoleOption[] = [
  { id: UserRoleId.Advisor, label: 'Asesor' },
  { id: UserRoleId.Student, label: 'Estudiante' },
] as const;

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  roleId: UserRoleIdValue;
}

export interface RegisterResponse {
  id: string;
  email: string;
  fullName: string;
  roleId: UserRoleIdValue;
  registeredAt: string;
}
