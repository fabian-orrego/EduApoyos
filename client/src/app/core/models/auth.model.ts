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

export const ROLE_LABELS: Readonly<Record<UserRoleIdValue, string>> = {
  [UserRoleId.Advisor]: 'Asesor',
  [UserRoleId.Student]: 'Estudiante',
};

/**
 * Home route associated to each role. Used by the login flow to redirect the user right after
 * authentication (US-005). Both roles land on the dashboard for now, but the mapping is kept
 * explicit so future routes can be added without touching the login component.
 */
export const ROLE_HOME_ROUTES: Readonly<Record<UserRoleIdValue, string>> = {
  [UserRoleId.Advisor]: '/dashboard',
  [UserRoleId.Student]: '/dashboard',
};

export interface LoginRequest {
  email: string;
  password: string;
}

/** Response shape returned by <c>POST /api/auth/login</c> (US-005). */
export interface LoginResponse {
  token: string;
  expiresAt: string;
  fullName: string;
  roleId: UserRoleIdValue;
}

export interface CurrentUser {
  fullName: string;
  roleId: UserRoleIdValue;
  expiresAt: string;
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
