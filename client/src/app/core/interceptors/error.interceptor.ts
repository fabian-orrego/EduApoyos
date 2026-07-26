import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { ProblemDetails } from '../models/problem-details.model';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notifier = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.logout();
        router.navigate(['/login']);
      }

      const message = buildErrorMessage(error);
      if (message) {
        notifier.error(message);
      }

      return throwError(() => error);
    }),
  );
};

function buildErrorMessage(error: HttpErrorResponse): string | null {
  if (error.status === 0) {
    return 'Unable to reach the server. Please verify your connection.';
  }

  const problem = error.error as ProblemDetails | undefined;
  if (problem?.detail) {
    return problem.detail;
  }
  if (problem?.title) {
    return problem.title;
  }
  return error.message ?? 'Unexpected error.';
}
