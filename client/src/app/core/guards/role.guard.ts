import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { UserRoleIdValue } from '../models/auth.model';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

/**
 * Restricts a route to a whitelist of roles. When the current user is authenticated but does not
 * hold any of the allowed roles the navigation is redirected back to the dashboard and a toast is
 * shown. When the user is not authenticated at all, the guard defers to <c>authGuard</c> by
 * redirecting to <c>/login</c>.
 */
export const roleGuard =
  (allowedRoles: readonly UserRoleIdValue[]): CanActivateFn =>
  (_route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);
    const notifier = inject(NotificationService);

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/login'], {
        queryParams: { returnUrl: state.url },
      });
    }

    const currentRole = auth.currentUser()?.roleId;
    if (currentRole !== undefined && allowedRoles.includes(currentRole)) {
      return true;
    }

    notifier.error('No tienes permisos para acceder a esta sección.');
    return router.createUrlTree(['/dashboard']);
  };
