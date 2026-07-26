import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import { STORAGE_KEYS } from '../constants/storage-keys';
import {
  AuthTokens,
  CurrentUser,
  LoginRequest,
  RegisterRequest,
  RegisterResponse,
} from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _currentUser = signal<CurrentUser | null>(this.loadUser());
  private readonly _accessToken = signal<string | null>(
    localStorage.getItem(STORAGE_KEYS.accessToken),
  );

  readonly currentUser = this._currentUser.asReadonly();
  readonly accessToken = this._accessToken.asReadonly();
  readonly isAuthenticated = computed(() => this._accessToken() !== null);

  login(request: LoginRequest): Observable<AuthTokens> {
    return this.http
      .post<AuthTokens>(API_ROUTES.auth.login, request)
      .pipe(tap((tokens) => this.storeTokens(tokens)));
  }

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(API_ROUTES.auth.register, request);
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.accessToken);
    localStorage.removeItem(STORAGE_KEYS.refreshToken);
    localStorage.removeItem(STORAGE_KEYS.currentUser);
    this._accessToken.set(null);
    this._currentUser.set(null);
  }

  private storeTokens(tokens: AuthTokens): void {
    localStorage.setItem(STORAGE_KEYS.accessToken, tokens.accessToken);
    localStorage.setItem(STORAGE_KEYS.refreshToken, tokens.refreshToken);
    this._accessToken.set(tokens.accessToken);
  }

  private loadUser(): CurrentUser | null {
    const raw = localStorage.getItem(STORAGE_KEYS.currentUser);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}
