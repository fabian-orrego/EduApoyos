import { Component, computed, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { UserRoleId, UserRoleIdValue } from '../../core/models/auth.model';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  path: string;
  allowedRoles?: readonly UserRoleIdValue[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [MatListModule, MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);

  private readonly allNavItems: readonly NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', path: '/dashboard' },
    {
      label: 'Estudiantes',
      icon: 'group',
      path: '/estudiantes',
      allowedRoles: [UserRoleId.Advisor],
    },
    {
      label: 'Solicitudes de apoyo',
      icon: 'assignment',
      path: '/solicitudes-apoyo',
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
}
