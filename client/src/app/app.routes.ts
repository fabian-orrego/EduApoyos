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
        path: 'estudiantes',
        pathMatch: 'full',
        canActivate: [roleGuard([UserRoleId.Advisor])],
        loadComponent: () =>
          import(
            './features/students/list-students/list-students.component'
          ).then((m) => m.ListStudentsComponent),
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
        path: 'solicitudes',
        pathMatch: 'full',
        canActivate: [roleGuard([UserRoleId.Advisor, UserRoleId.Student])],
        loadComponent: () =>
          import(
            './features/support-requests/list-support-requests/list-support-requests.component'
          ).then((m) => m.ListSupportRequestsComponent),
      },
      {
        path: 'solicitudes/nueva',
        canActivate: [roleGuard([UserRoleId.Advisor, UserRoleId.Student])],
        loadComponent: () =>
          import(
            './features/support-requests/create-support-request/create-support-request.component'
          ).then((m) => m.CreateSupportRequestComponent),
      },
      {
        path: 'solicitudes/:id',
        canActivate: [roleGuard([UserRoleId.Advisor, UserRoleId.Student])],
        loadComponent: () =>
          import(
            './features/support-requests/detail-support-request/detail-support-request.component'
          ).then((m) => m.DetailSupportRequestComponent),
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
