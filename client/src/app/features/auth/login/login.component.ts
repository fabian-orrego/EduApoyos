import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { ROLE_HOME_ROUTES } from '../../../core/models/auth.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly notifier = inject(NotificationService);

  protected readonly hidePassword = signal(true);
  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.auth.login(this.form.getRawValue()).subscribe({
      next: (response) => {
        this.notifier.success(`¡Bienvenido, ${response.fullName}!`);
        const target =
          this.route.snapshot.queryParamMap.get('returnUrl') ??
          ROLE_HOME_ROUTES[response.roleId] ??
          '/';
        this.router.navigateByUrl(target);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        // RN-004: never disclose whether the email or the password was wrong. Any 401 becomes
        // the same generic error on the form.
        if (error.status === 401) {
          this.form.setErrors({ invalidCredentials: true });
        }
      },
      complete: () => this.submitting.set(false),
    });
  }
}
