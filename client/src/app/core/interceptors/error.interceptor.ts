import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { API_ROUTES } from '../constants/api-routes';
import { ProblemDetails } from '../models/problem-details.model';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notifier = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall =
        req.url.endsWith(API_ROUTES.auth.login) ||
        req.url.endsWith(API_ROUTES.auth.register);

      // Session expired during an authenticated request: force the user back to login. The auth
      // endpoints themselves must be exempted — a 401 from /login is a wrong-credentials response,
      // not an expired session, so the login component handles it inline (US-005 RN-004).
      if (error.status === 401 && !isAuthCall) {
        auth.logout();
        router.navigate(['/login']);
      }

      // Suppress the global snackbar on auth endpoints because those components render inline
      // errors themselves.
      if (!isAuthCall) {
        const message = buildErrorMessage(error);
        if (message) {
          notifier.error(message);
        }
      }

      return throwError(() => error);
    }),
  );
};

function buildErrorMessage(error: HttpErrorResponse): string | null {
  if (error.status === 0) {
    return 'No se pudo contactar al servidor. Verifica tu conexión.';
  }

  const problem = error.error as ProblemDetails | undefined;
  if (problem?.detail) {
    return problem.detail;
  }
  if (problem?.title) {
    return problem.title;
  }
  return error.message ?? 'Ocurrió un error inesperado.';
}
