import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroupDirective,
  NgForm,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';

import {
  USER_ROLE_OPTIONS,
  UserRoleId,
  type UserRoleIdValue,
} from '../../../core/models/auth.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

const passwordComplexityValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = control.value ?? '';
  if (!value) {
    return null;
  }

  if (!/[A-Z]/.test(value)) return { complexity: 'uppercase' };
  if (!/[a-z]/.test(value)) return { complexity: 'lowercase' };
  if (!/[0-9]/.test(value)) return { complexity: 'digit' };

  return null;
};

const passwordsMatchValidator: ValidatorFn = (
  group: AbstractControl,
): ValidationErrors | null => {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;

  if (!confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordsMismatch: true };
};

/**
 * Shows an error on the confirm-password field when the group-level `passwordsMismatch`
 * validator fails. Without this, `<mat-error>` would never render because the mismatch is
 * a group-level error and Material only inspects the individual FormControl by default.
 */
class ConfirmPasswordErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(
    control: AbstractControl | null,
    form: FormGroupDirective | NgForm | null,
  ): boolean {
    if (!control) return false;
    const touched = control.dirty || control.touched || !!form?.submitted;
    const ownError = control.invalid && touched;
    const groupMismatch =
      !!control.parent?.hasError('passwordsMismatch') && touched;
    return ownError || groupMismatch;
  }
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifier = inject(NotificationService);

  protected readonly roleOptions = USER_ROLE_OPTIONS;
  protected readonly hidePassword = signal(true);
  protected readonly hideConfirm = signal(true);
  protected readonly submitting = signal(false);
  protected readonly confirmMatcher = new ConfirmPasswordErrorStateMatcher();

  protected readonly form = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.maxLength(150)]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          passwordComplexityValidator,
        ],
      ],
      confirmPassword: ['', [Validators.required]],
      roleId: [UserRoleId.Student as UserRoleIdValue, [Validators.required]],
    },
    { validators: [passwordsMatchValidator] },
  );

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const payload = this.form.getRawValue();

    this.auth.register(payload).subscribe({
      next: () => {
        this.notifier.success(
          '¡Registro exitoso! Ya puedes iniciar sesión con tus credenciales.',
        );
        this.router.navigate(['/login']);
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        if (error.status === 409) {
          this.form.controls.email.setErrors({ duplicated: true });
        }
      },
      complete: () => this.submitting.set(false),
    });
  }
}
