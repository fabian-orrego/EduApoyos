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

import {
  ACADEMIC_PROGRAM_OPTIONS,
  DOCUMENT_TYPE_OPTIONS,
  DocumentTypeIdValue,
  SEMESTER_OPTIONS,
  StudentListItem,
  UpdateStudentRequest,
  UpdateStudentResponse,
} from '../../../core/models/student.model';
import { ProblemDetails } from '../../../core/models/problem-details.model';
import { NotificationService } from '../../../core/services/notification.service';
import { StudentService } from '../../../core/services/student.service';

/**
 * Dialog used from the students grid to update an existing student (US-009). The current row
 * is passed in as the dialog data so no additional round-trip is required (there is no GET-by-id
 * endpoint by design).
 */
@Component({
  selector: 'app-edit-student-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './edit-student-dialog.component.html',
  styleUrl: './edit-student-dialog.component.scss',
})
export class EditStudentDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly studentService = inject(StudentService);
  private readonly notifier = inject(NotificationService);
  private readonly dialogRef =
    inject<MatDialogRef<EditStudentDialogComponent, UpdateStudentResponse>>(
      MatDialogRef,
    );

  protected readonly data = inject<StudentListItem>(MAT_DIALOG_DATA);
  protected readonly documentTypeOptions = DOCUMENT_TYPE_OPTIONS;
  protected readonly programOptions = ACADEMIC_PROGRAM_OPTIONS;
  protected readonly semesterOptions = SEMESTER_OPTIONS;
  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    documentType: [
      this.data.documentType as DocumentTypeIdValue,
      [Validators.required],
    ],
    documentNumber: [
      this.data.documentNumber,
      [Validators.required, Validators.maxLength(20)],
    ],
    academicProgram: [
      this.data.academicProgram,
      [Validators.required, Validators.maxLength(150)],
    ],
    semester: [
      this.data.semester as number | null,
      [Validators.required],
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
    if (raw.semester === null) {
      return;
    }

    const payload: UpdateStudentRequest = {
      documentType: raw.documentType,
      documentNumber: raw.documentNumber.trim(),
      academicProgram: raw.academicProgram,
      semester: raw.semester,
    };

    this.submitting.set(true);
    this.studentService.update(this.data.id, payload).subscribe({
      next: (response) => {
        this.notifier.success('Estudiante actualizado exitosamente.');
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

    if (error.status === 409 && code === 'students.document.duplicated') {
      this.form.controls.documentNumber.setErrors({ duplicated: true });
      return;
    }

    if (error.status === 404 && code === 'students.notFound') {
      this.notifier.error('El estudiante ya no existe.');
      this.dialogRef.close();
    }
  }
}
