import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { UserRoleId } from '../../../core/models/auth.model';
import {
  isFinalizedStatus,
  SUPPORT_REQUEST_STATUS_TRANSITIONS,
  SupportRequestDetail,
  SupportRequestStatusIdValue,
} from '../../../core/models/support-request.model';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SupportRequestService } from '../../../core/services/support-request.service';
import { EstadoPipe } from '../../../shared/pipes/estado.pipe';
import { TipoApoyoPipe } from '../../../shared/pipes/tipo-apoyo.pipe';
import { ChangeStatusDialogComponent } from '../change-status-dialog/change-status-dialog.component';
import { EditSupportRequestDialogComponent } from '../edit-support-request-dialog/edit-support-request-dialog.component';

/**
 * Detail view for a single support request (US-014). Advisors get the "editar" and "cambiar
 * estado" actions (US-016), while students get the "descargar constancia" action for their
 * own requests (US-018). The component defensively handles 403/404 responses because the
 * route can be reached with an arbitrary id from a shared link.
 */
@Component({
  selector: 'app-detail-support-request',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    EstadoPipe,
    TipoApoyoPipe,
  ],
  templateUrl: './detail-support-request.component.html',
  styleUrl: './detail-support-request.component.scss',
})
export class DetailSupportRequestComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(SupportRequestService);
  private readonly auth = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly notifier = inject(NotificationService);

  protected readonly loading = signal(false);
  protected readonly downloading = signal(false);
  protected readonly detail = signal<SupportRequestDetail | null>(null);

  private readonly idParam = toSignal(this.route.paramMap, {
    initialValue: this.route.snapshot.paramMap,
  });

  protected readonly currentUserRole = computed(
    () => this.auth.currentUser()?.roleId ?? null,
  );

  protected readonly isAdvisor = computed(
    () => this.currentUserRole() === UserRoleId.Advisor,
  );

  protected readonly isStudent = computed(
    () => this.currentUserRole() === UserRoleId.Student,
  );

  protected readonly canEdit = computed(() => {
    const current = this.detail();
    return (
      this.isAdvisor() && current !== null && !isFinalizedStatus(current.status)
    );
  });

  protected readonly canChangeStatus = computed(() => {
    const current = this.detail();
    if (!this.isAdvisor() || current === null) {
      return false;
    }
    const allowed =
      SUPPORT_REQUEST_STATUS_TRANSITIONS[
        current.status as SupportRequestStatusIdValue
      ] ?? [];
    return allowed.length > 0;
  });

  constructor() {
    const id = this.idParam().get('id');
    if (id) {
      this.load(id);
    }
  }

  load(id: string): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (detail) => {
        this.detail.set(detail);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        if (error.status === 403 || error.status === 404) {
          this.router.navigate(['/solicitudes']);
        }
      },
    });
  }

  openEdit(): void {
    const current = this.detail();
    if (!current || !this.canEdit()) {
      return;
    }

    this.dialog
      .open(EditSupportRequestDialogComponent, {
        data: current,
        width: '520px',
        autoFocus: 'first-tabbable',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.load(current.id);
        }
      });
  }

  openChangeStatus(): void {
    const current = this.detail();
    if (!current || !this.canChangeStatus()) {
      return;
    }

    this.dialog
      .open(ChangeStatusDialogComponent, {
        data: current,
        width: '520px',
        autoFocus: 'first-tabbable',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.load(current.id);
        }
      });
  }

  downloadCertificate(): void {
    const current = this.detail();
    if (!current || this.downloading()) {
      return;
    }

    this.downloading.set(true);
    this.service.downloadCertificate(current.id).subscribe({
      next: ({ blob, fileName }) => {
        this.downloading.set(false);
        this.saveBlob(blob, fileName);
        this.notifier.success('Constancia generada exitosamente.');
      },
      error: () => this.downloading.set(false),
    });
  }

  formatShortId(id: string): string {
    return id.slice(0, 8).toUpperCase();
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
