import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { ProblemDetails } from '../../../core/models/problem-details.model';
import {
  SUPPORT_TYPE_OPTIONS,
  SupportRequestDetail,
  SupportTypeIdValue,
  UpdateSupportRequestRequest,
  UpdateSupportRequestResponse,
} from '../../../core/models/support-request.model';
import { NotificationService } from '../../../core/services/notification.service';
import { SupportRequestService } from '../../../core/services/support-request.service';

/**
 * Dialog used from the support-request detail screen to update the editable business fields
 * (US-016 nota #1). The current detail is passed in as the dialog data so no extra
 * round-trip is required.
 */
@Component({
  selector: 'app-edit-support-request-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './edit-support-request-dialog.component.html',
  styleUrl: './edit-support-request-dialog.component.scss',
})
export class EditSupportRequestDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SupportRequestService);
  private readonly notifier = inject(NotificationService);
  private readonly dialogRef =
    inject<
      MatDialogRef<
        EditSupportRequestDialogComponent,
        UpdateSupportRequestResponse
      >
    >(MatDialogRef);

  protected readonly data = inject<SupportRequestDetail>(MAT_DIALOG_DATA);
  protected readonly supportTypeOptions = SUPPORT_TYPE_OPTIONS;
  protected readonly submitting = signal(false);
  protected readonly descriptionMaxLength = 1000;

  protected readonly form = this.fb.nonNullable.group({
    supportType: [
      this.data.supportType as SupportTypeIdValue,
      [Validators.required],
    ],
    requestedAmount: [
      this.data.requestedAmount as number | null,
      [Validators.required, Validators.min(1)],
    ],
    description: [
      this.data.description,
      [Validators.required, Validators.maxLength(this.descriptionMaxLength)],
    ],
  });

  cancel(): void {
    this.dialogRef.close();
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

    const payload: UpdateSupportRequestRequest = {
      supportType: raw.supportType,
      requestedAmount: raw.requestedAmount,
      description: raw.description.trim(),
    };

    this.submitting.set(true);
    this.service.update(this.data.id, payload).subscribe({
      next: (response) => {
        this.notifier.success('Solicitud actualizada exitosamente.');
        this.dialogRef.close(response);
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
      error.status === 409 &&
      code === 'supportRequests.update.finalized'
    ) {
      this.notifier.error(
        'La solicitud ya fue aprobada o rechazada y no puede modificarse.',
      );
      this.dialogRef.close();
      return;
    }

    if (error.status === 404 && code === 'supportRequests.notFound') {
      this.notifier.error('La solicitud ya no existe.');
      this.dialogRef.close();
    }
  }
}
