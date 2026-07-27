import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';

import { UserRoleId } from '../../../core/models/auth.model';
import { ProblemDetails } from '../../../core/models/problem-details.model';
import {
  SUPPORT_TYPE_OPTIONS,
  SupportTypeId,
  SupportTypeIdValue,
} from '../../../core/models/support-request.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SupportRequestService } from '../../../core/services/support-request.service';

/**
 * Form used to register a new support request (US-013). Accessible to both Advisors and
 * Students. When the caller is a Student the email is locked to their own account so they
 * cannot create requests on behalf of someone else.
 */
@Component({
  selector: 'app-create-support-request',
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
  templateUrl: './create-support-request.component.html',
  styleUrl: './create-support-request.component.scss',
})
export class CreateSupportRequestComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SupportRequestService);
  private readonly auth = inject(AuthService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly supportTypeOptions = SUPPORT_TYPE_OPTIONS;
  protected readonly submitting = signal(false);
  protected readonly descriptionMaxLength = 1000;

  protected readonly isStudent = computed(
    () => this.auth.currentUser()?.roleId === UserRoleId.Student,
  );

  protected readonly form = this.fb.nonNullable.group({
    studentEmail: [
      this.isStudent() ? (this.auth.currentUser()?.email ?? '') : '',
      [Validators.required, Validators.email],
    ],
    supportType: [
      SupportTypeId.Scholarship as SupportTypeIdValue,
      [Validators.required],
    ],
    requestedAmount: [
      null as number | null,
      [Validators.required, Validators.min(1)],
    ],
    description: [
      '',
      [Validators.required, Validators.maxLength(this.descriptionMaxLength)],
    ],
  });

  constructor() {
    if (this.isStudent()) {
      this.form.controls.studentEmail.disable({ emitEvent: false });
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (raw.requestedAmount === null) {
      return;
    }

    this.submitting.set(true);
    this.service
      .create({
        studentEmail: raw.studentEmail.trim(),
        supportType: raw.supportType,
        requestedAmount: raw.requestedAmount,
        description: raw.description.trim(),
      })
      .subscribe({
        next: (response) => {
          this.notifier.success('Solicitud registrada exitosamente.');
          this.router.navigate(['/solicitudes', response.id]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          this.applyServerError(error);
        },
      });
  }

  private applyServerError(error: HttpErrorResponse): void {
    const problem = error.error as ProblemDetails | undefined;
    const code = typeof problem?.['code'] === 'string' ? problem['code'] : '';

    if (
      error.status === 404 &&
      code === 'supportRequests.student.notFound'
    ) {
      this.form.controls.studentEmail.setErrors({ studentNotFound: true });
    }
  }
}
