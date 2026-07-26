import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRoleId } from './core/models/auth.model';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register.component').then(
        (m) => m.RegisterComponent,
      ),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layout/main-layout/main-layout.component').then(
        (m) => m.MainLayoutComponent,
      ),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: 'estudiantes/nuevo',
        canActivate: [roleGuard([UserRoleId.Advisor])],
        loadComponent: () =>
          import(
            './features/students/create-student/create-student.component'
          ).then((m) => m.CreateStudentComponent),
      },
      {
        path: 'estudiantes',
        pathMatch: 'full',
        redirectTo: 'estudiantes/nuevo',
      },
    ],
  },
  {
    path: '**',
    loadComponent: () =>
      import('./layout/not-found/not-found.component').then(
        (m) => m.NotFoundComponent,
      ),
  },
];
