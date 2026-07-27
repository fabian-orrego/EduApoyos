import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
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
  ChangeSupportRequestStatusResponse,
  SUPPORT_REQUEST_STATUS_LABELS,
  SUPPORT_REQUEST_STATUS_TRANSITIONS,
  SupportRequestDetail,
  SupportRequestStatusId,
  SupportRequestStatusIdValue,
} from '../../../core/models/support-request.model';
import { NotificationService } from '../../../core/services/notification.service';
import { SupportRequestService } from '../../../core/services/support-request.service';

/**
 * Dialog that lets an advisor transition the support request to one of the states allowed by
 * US-016. The list of target statuses is computed from the current status so the UI can only
 * offer valid transitions. Notes are required when moving to <c>Rejected</c> (RN-7).
 */
@Component({
  selector: 'app-change-status-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './change-status-dialog.component.html',
  styleUrl: './change-status-dialog.component.scss',
})
export class ChangeStatusDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SupportRequestService);
  private readonly notifier = inject(NotificationService);
  private readonly dialogRef =
    inject<
      MatDialogRef<
        ChangeStatusDialogComponent,
        ChangeSupportRequestStatusResponse
      >
    >(MatDialogRef);

  protected readonly data = inject<SupportRequestDetail>(MAT_DIALOG_DATA);
  protected readonly submitting = signal(false);
  protected readonly notesMaxLength = 500;

  protected readonly statusLabels = SUPPORT_REQUEST_STATUS_LABELS;
  protected readonly allowedStatuses: readonly SupportRequestStatusIdValue[] =
    SUPPORT_REQUEST_STATUS_TRANSITIONS[
      this.data.status as SupportRequestStatusIdValue
    ] ?? [];

  protected readonly form = this.fb.nonNullable.group({
    newStatus: [
      (this.allowedStatuses[0] ?? null) as SupportRequestStatusIdValue | null,
      [Validators.required],
    ],
    notes: ['' as string, [Validators.maxLength(this.notesMaxLength)]],
  });

  protected readonly notesRequired = computed(() => {
    const selected = this.form.controls.newStatus.value;
    return selected === SupportRequestStatusId.Rejected;
  });

  constructor() {
    // Toggle the "notes required" validator dynamically based on the chosen transition
    // (US-016 RN-7). Kept inside the constructor because the effect depends on both the
    // signal and the reactive form values.
    this.form.controls.newStatus.valueChanges.subscribe((next) => {
      const control = this.form.controls.notes;
      if (next === SupportRequestStatusId.Rejected) {
        control.addValidators(Validators.required);
      } else {
        control.removeValidators(Validators.required);
      }
      control.updateValueAndValidity({ emitEvent: false });
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (raw.newStatus === null) {
      return;
    }

    this.submitting.set(true);
    this.service
      .changeStatus(this.data.id, {
        newStatus: raw.newStatus,
        notes: raw.notes.trim() === '' ? null : raw.notes.trim(),
      })
      .subscribe({
        next: (response) => {
          this.notifier.success('Estado actualizado exitosamente.');
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
      (code === 'supportRequests.status.finalized' ||
        code === 'supportRequests.status.invalidTransition')
    ) {
      this.dialogRef.close();
    }
  }
}
