import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal, viewChild, TemplateRef, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';

import { ProblemDetails } from '../../../core/models/problem-details.model';
import {
  DOCUMENT_TYPE_LABELS,
  StudentListItem,
} from '../../../core/models/student.model';
import { NotificationService } from '../../../core/services/notification.service';
import { StudentService } from '../../../core/services/student.service';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import {
  DataTableColumn,
  DataTablePageEvent,
} from '../../../shared/components/data-table/data-table.model';
import { EditStudentDialogComponent } from '../edit-student-dialog/edit-student-dialog.component';

/**
 * Advisor grid that lists every registered student (US-011) and hosts the row-level actions
 * needed by US-009 (edit) and US-010 (delete). Pagination is server-side and clamped by the
 * project-wide maximum of 100 records per page.
 */
@Component({
  selector: 'app-list-students',
  standalone: true,
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    DataTableComponent,
  ],
  templateUrl: './list-students.component.html',
  styleUrl: './list-students.component.scss',
})
export class ListStudentsComponent implements OnInit {
  private readonly studentService = inject(StudentService);
  private readonly notifier = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  protected readonly pageSizeOptions = [5, 10, 25, 50];
  protected readonly items = signal<StudentListItem[]>([]);
  protected readonly totalItems = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly loading = signal(false);

  protected readonly actionsTemplate =
    viewChild<TemplateRef<{ $implicit: StudentListItem }>>('actionsTpl');

  protected readonly columns = computed<DataTableColumn<StudentListItem>[]>(() => {
    const actions = this.actionsTemplate();
    return [
      { key: 'fullName', header: 'Nombre completo', value: (row) => row.fullName },
      {
        key: 'document',
        header: 'Documento',
        value: (row) =>
          `${DOCUMENT_TYPE_LABELS[row.documentType]} · ${row.documentNumber}`,
      },
      {
        key: 'academicProgram',
        header: 'Programa',
        value: (row) => row.academicProgram,
      },
      {
        key: 'semester',
        header: 'Semestre',
        value: (row) => row.semester,
        align: 'center',
      },
      { key: 'email', header: 'Correo', value: (row) => row.email },
      {
        key: 'actions',
        header: 'Acciones',
        value: () => '',
        align: 'end',
        cellTemplate: actions,
      },
    ];
  });

  ngOnInit(): void {
    this.load();
  }

  onPageChange(event: DataTablePageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  openEdit(student: StudentListItem): void {
    const dialogRef = this.dialog.open(EditStudentDialogComponent, {
      data: student,
      width: '480px',
      autoFocus: 'first-tabbable',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((updated) => {
      if (updated) {
        this.load();
      }
    });
  }

  confirmDelete(student: StudentListItem): void {
    const data: ConfirmDialogData = {
      title: 'Eliminar estudiante',
      message: `¿Estás seguro de eliminar a ${student.fullName}? Esta acción no se puede deshacer.`,
      confirmLabel: 'Eliminar',
      cancelLabel: 'Cancelar',
      confirmColor: 'warn',
    };

    this.dialog
      .open(ConfirmDialogComponent, { data, width: '420px' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed) {
          this.delete(student);
        }
      });
  }

  private delete(student: StudentListItem): void {
    this.studentService.delete(student.id).subscribe({
      next: () => {
        this.notifier.success('Estudiante eliminado exitosamente.');
        this.load();
      },
      error: (error: HttpErrorResponse) => this.handleDeleteError(error),
    });
  }

  private handleDeleteError(error: HttpErrorResponse): void {
    // The global error interceptor already surfaces the ProblemDetails message. This method
    // exists only to keep any future error branches close to the delete flow.
    void (error.error as ProblemDetails | undefined);
  }

  private load(): void {
    this.loading.set(true);
    const pageNumber = this.pageIndex() + 1;
    this.studentService.list(pageNumber, this.pageSize()).subscribe({
      next: (page) => {
        this.items.set(page.items);
        this.totalItems.set(page.totalItems);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
