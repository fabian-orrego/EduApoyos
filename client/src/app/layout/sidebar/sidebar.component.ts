import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, map, startWith } from 'rxjs';

import { UserRoleId, UserRoleIdValue } from '../../core/models/auth.model';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  /** How the current URL is matched to decide the active highlight. */
  match: 'exact' | 'section';
  allowedRoles?: readonly UserRoleIdValue[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [MatListModule, MatIconModule, RouterLink],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects.split('?')[0] ?? event.urlAfterRedirects),
      startWith(this.router.url.split('?')[0] ?? this.router.url),
    ),
    { initialValue: this.router.url.split('?')[0] ?? this.router.url },
  );

  private readonly allNavItems: readonly NavItem[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      path: '/dashboard',
      match: 'exact',
    },
    {
      label: 'Estudiantes',
      icon: 'group',
      path: '/estudiantes',
      match: 'section',
      allowedRoles: [UserRoleId.Advisor],
    },
    {
      label: 'Solicitudes de apoyo',
      icon: 'assignment',
      path: '/solicitudes',
      match: 'section',
      allowedRoles: [UserRoleId.Advisor],
    },
    {
      label: 'Mis solicitudes',
      icon: 'assignment',
      path: '/solicitudes',
      match: 'section',
      allowedRoles: [UserRoleId.Student],
    },
    {
      label: 'Nueva solicitud',
      icon: 'note_add',
      path: '/solicitudes/nueva',
      match: 'exact',
      allowedRoles: [UserRoleId.Student],
    },
  ];

  protected readonly navItems = computed<readonly NavItem[]>(() => {
    const currentRole = this.auth.currentUser()?.roleId;
    return this.allNavItems.filter(
      (item) =>
        !item.allowedRoles ||
        (currentRole !== undefined && item.allowedRoles.includes(currentRole)),
    );
  });

  protected isActive(item: NavItem): boolean {
    const url = this.currentUrl();
    if (item.match === 'exact') {
      return url === item.path;
    }

    // "section" matches the list and nested detail routes, but never a sibling path that
    // starts with the same prefix (e.g. /solicitudes/nueva must not activate Mis solicitudes).
    if (url === item.path) {
      return true;
    }

    if (!url.startsWith(`${item.path}/`)) {
      return false;
    }

    const remainder = url.slice(item.path.length + 1);
    // Exclude known sibling create routes under the same parent segment.
    return remainder !== 'nueva' && !remainder.startsWith('nueva/');
  }
}
