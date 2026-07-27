import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  inject,
  OnInit,
  signal,
  TemplateRef,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';

import { UserRoleId } from '../../../core/models/auth.model';
import {
  SUPPORT_REQUEST_STATUS_LABELS,
  SUPPORT_REQUEST_STATUS_OPTIONS,
  SUPPORT_TYPE_LABELS,
  SUPPORT_TYPE_OPTIONS,
  SupportRequestListFilters,
  SupportRequestListItem,
  SupportRequestStatusIdValue,
  SupportTypeIdValue,
} from '../../../core/models/support-request.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SupportRequestService } from '../../../core/services/support-request.service';
import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import {
  DataTableColumn,
  DataTablePageEvent,
} from '../../../shared/components/data-table/data-table.model';
import { EstadoPipe } from '../../../shared/pipes/estado.pipe';

/**
 * Shared grid for support requests. Advisors see the full catalog with filters (US-015).
 * Students see only their own requests (student portal) and can open the detail or download
 * the PDF certificate (US-018) from each row.
 */
@Component({
  selector: 'app-list-support-requests',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DataTableComponent,
    EstadoPipe,
  ],
  templateUrl: './list-support-requests.component.html',
  styleUrl: './list-support-requests.component.scss',
})
export class ListSupportRequestsComponent implements OnInit {
  private readonly service = inject(SupportRequestService);
  private readonly auth = inject(AuthService);
  private readonly notifier = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly pageSizeOptions = [5, 10, 25, 50];
  protected readonly statusOptions = SUPPORT_REQUEST_STATUS_OPTIONS;
  protected readonly supportTypeOptions = SUPPORT_TYPE_OPTIONS;

  protected readonly items = signal<SupportRequestListItem[]>([]);
  protected readonly totalItems = signal(0);
  protected readonly pageIndex = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly loading = signal(false);
  protected readonly downloadingId = signal<string | null>(null);

  protected readonly isStudent = computed(
    () => this.auth.currentUser()?.roleId === UserRoleId.Student,
  );

  protected readonly filtersForm = this.fb.nonNullable.group({
    status: [null as SupportRequestStatusIdValue | null],
    supportType: [null as SupportTypeIdValue | null],
    fromDate: [null as string | null],
    toDate: [null as string | null],
  });

  protected readonly actionsTemplate =
    viewChild<TemplateRef<{ $implicit: SupportRequestListItem }>>('actionsTpl');
  protected readonly statusTemplate =
    viewChild<TemplateRef<{ $implicit: SupportRequestListItem }>>('statusTpl');

  protected readonly columns = computed<
    DataTableColumn<SupportRequestListItem>[]
  >(() => {
    const actions = this.actionsTemplate();
    const status = this.statusTemplate();
    const studentColumns: DataTableColumn<SupportRequestListItem>[] =
      this.isStudent()
        ? []
        : [
            {
              key: 'studentFullName',
              header: 'Estudiante',
              value: (row) => row.studentFullName,
            },
            {
              key: 'studentDocumentNumber',
              header: 'Documento',
              value: (row) => row.studentDocumentNumber,
            },
          ];

    return [
      {
        key: 'id',
        header: 'N° Solicitud',
        value: (row) => row.id.slice(0, 8).toUpperCase(),
      },
      ...studentColumns,
      {
        key: 'supportType',
        header: 'Tipo',
        value: (row) => SUPPORT_TYPE_LABELS[row.supportType] ?? row.supportType,
      },
      {
        key: 'status',
        header: 'Estado',
        value: (row) =>
          SUPPORT_REQUEST_STATUS_LABELS[row.status] ?? row.status,
        cellTemplate: status,
      },
      {
        key: 'requestedAt',
        header: 'Fecha',
        value: (row) => new Date(row.requestedAt).toLocaleDateString('es-CO'),
      },
      {
        key: 'requestedAmount',
        header: 'Monto',
        value: (row) =>
          row.requestedAmount.toLocaleString('es-CO', {
            style: 'currency',
            currency: 'COP',
            minimumFractionDigits: 0,
          }),
        align: 'end',
      },
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
    this.filtersForm.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.load();
    });
    this.load();
  }

  onPageChange(event: DataTablePageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  clearFilters(): void {
    this.filtersForm.reset(
      {
        status: null,
        supportType: null,
        fromDate: null,
        toDate: null,
      },
      { emitEvent: true },
    );
  }

  openDetail(row: SupportRequestListItem): void {
    this.router.navigate(['/solicitudes', row.id]);
  }

  downloadCertificate(row: SupportRequestListItem): void {
    if (this.downloadingId()) {
      return;
    }

    this.downloadingId.set(row.id);
    this.service.downloadCertificate(row.id).subscribe({
      next: ({ blob, fileName }) => {
        this.downloadingId.set(null);
        this.saveBlob(blob, fileName);
        this.notifier.success('Constancia generada exitosamente.');
      },
      error: () => this.downloadingId.set(null),
    });
  }

  statusClass(status: SupportRequestStatusIdValue): string {
    return `status-chip status-chip--${status}`;
  }

  private load(): void {
    this.loading.set(true);
    const pageNumber = this.pageIndex() + 1;
    const raw = this.filtersForm.getRawValue();
    const filters: SupportRequestListFilters = {
      status: raw.status,
      supportType: raw.supportType,
      fromDate: raw.fromDate,
      toDate: raw.toDate,
    };

    this.service.list(pageNumber, this.pageSize(), filters).subscribe({
      next: (page) => {
        this.items.set(page.items);
        this.totalItems.set(page.totalItems);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private saveBlob(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
