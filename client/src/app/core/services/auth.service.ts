import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import { STORAGE_KEYS } from '../constants/storage-keys';
import {
  CurrentUser,
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
} from '../models/auth.model';

/**
 * Central authentication state for the SPA. Persists the JWT + profile in local storage so a
 * page refresh keeps the user logged in until the token expires. There is intentionally no
 * refresh-token support (US-005 RN-003).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _accessToken = signal<string | null>(this.readAccessToken());
  private readonly _currentUser = signal<CurrentUser | null>(this.readUser());

  readonly accessToken = this._accessToken.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._accessToken() !== null);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(API_ROUTES.auth.login, request)
      .pipe(tap((response) => this.storeSession(response)));
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(API_ROUTES.auth.register, request);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.accessToken);
    localStorage.removeItem(STORAGE_KEYS.currentUser);
    this._accessToken.set(null);
    this._currentUser.set(null);
  }

  private storeSession(response: LoginResponse): void {
    const user: CurrentUser = {
      fullName: response.fullName,
      email: response.email,
      roleId: response.roleId,
      expiresAt: response.expiresAt,
    };

    localStorage.setItem(STORAGE_KEYS.accessToken, response.token);
    localStorage.setItem(STORAGE_KEYS.currentUser, JSON.stringify(user));
    this._accessToken.set(response.token);
    this._currentUser.set(user);
  }

  private readAccessToken(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEYS.accessToken);
    } catch {
      return null;
    }
  }

  private readUser(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEYS.currentUser);
      if (!raw) {
        return null;
      }

      const parsed = JSON.parse(raw) as CurrentUser;
      // Older sessions may lack email; recover it from the JWT email claim when possible.
      if (!parsed.email) {
        const emailFromToken = this.readEmailFromToken();
        if (emailFromToken) {
          parsed.email = emailFromToken;
          localStorage.setItem(STORAGE_KEYS.currentUser, JSON.stringify(parsed));
        }
      }

      return parsed;
    } catch {
      return null;
    }
  }

  private readEmailFromToken(): string | null {
    const token = this.readAccessToken();
    if (!token) {
      return null;
    }

    try {
      const payload = token.split('.')[1];
      if (!payload) {
        return null;
      }

      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const json = atob(normalized);
      const claims = JSON.parse(json) as Record<string, unknown>;
      const email =
        (typeof claims['email'] === 'string' && claims['email']) ||
        (typeof claims[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
        ] === 'string' &&
          (claims[
            'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
          ] as string)) ||
        null;

      return email;
    } catch {
      return null;
    }
  }
}
