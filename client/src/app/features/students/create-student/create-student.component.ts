import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
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

import { NotificationService } from '../../../core/services/notification.service';
import { StudentService } from '../../../core/services/student.service';
import {
  ACADEMIC_PROGRAM_OPTIONS,
  DOCUMENT_TYPE_OPTIONS,
  DocumentTypeId,
  DocumentTypeIdValue,
  SEMESTER_OPTIONS,
} from '../../../core/models/student.model';
import { ProblemDetails } from '../../../core/models/problem-details.model';

@Component({
  selector: 'app-create-student',
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
  templateUrl: './create-student.component.html',
  styleUrl: './create-student.component.scss',
})
export class CreateStudentComponent {
  private readonly fb = inject(FormBuilder);
  private readonly studentService = inject(StudentService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly documentTypeOptions = DOCUMENT_TYPE_OPTIONS;
  protected readonly programOptions = ACADEMIC_PROGRAM_OPTIONS;
  protected readonly semesterOptions = SEMESTER_OPTIONS;
  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    documentType: [
      DocumentTypeId.NationalId as DocumentTypeIdValue,
      [Validators.required],
    ],
    documentNumber: [
      '',
      [Validators.required, Validators.maxLength(20)],
    ],
    academicProgram: [
      '',
      [Validators.required, Validators.maxLength(150)],
    ],
    semester: [null as number | null, [Validators.required]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (raw.semester === null) {
      return;
    }

    this.submitting.set(true);
    this.studentService
      .create({
        email: raw.email.trim(),
        documentType: raw.documentType,
        documentNumber: raw.documentNumber.trim(),
        academicProgram: raw.academicProgram,
        semester: raw.semester,
      })
      .subscribe({
        next: () => {
          this.notifier.success('Estudiante registrado exitosamente.');
          this.router.navigate(['/dashboard']);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          this.applyServerError(error);
        },
        complete: () => this.submitting.set(false),
      });
  }

  private applyServerError(error: HttpErrorResponse): void {
    const problem = error.error as ProblemDetails | undefined;
    const code = typeof problem?.['code'] === 'string' ? problem['code'] : '';

    // Map known backend error codes to the specific control so the user sees the message inline.
    if (error.status === 400 && code === 'students.user.notFound') {
      this.form.controls.email.setErrors({ userNotFound: true });
      return;
    }

    if (error.status === 400 && code === 'students.user.invalidRole') {
      this.form.controls.email.setErrors({ invalidRole: true });
      return;
    }

    if (error.status === 409 && code === 'students.user.alreadyLinked') {
      this.form.controls.email.setErrors({ alreadyLinked: true });
      return;
    }

    if (error.status === 409 && code === 'students.document.duplicated') {
      this.form.controls.documentNumber.setErrors({ duplicated: true });
    }
  }
}
