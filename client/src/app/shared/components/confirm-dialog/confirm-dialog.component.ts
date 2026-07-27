import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

/**
 * Data payload accepted by <c>ConfirmDialogComponent</c>. All fields are optional and fall back
 * to a sensible confirmation copy so trivial cases can invoke the dialog without extra config.
 */
export interface ConfirmDialogData {
  title?: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  /** When set to <c>warn</c> the confirm button uses the destructive palette. */
  confirmColor?: 'primary' | 'warn';
}

/**
 * Small reusable confirmation dialog. Returns <c>true</c> when the user confirms and
 * <c>false</c> (or <c>undefined</c>) when the dialog is dismissed.
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
})
export class ConfirmDialogComponent {
  protected readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef =
    inject<MatDialogRef<ConfirmDialogComponent, boolean>>(MatDialogRef);

  protected readonly title = this.data.title ?? 'Confirmar acción';
  protected readonly confirmLabel = this.data.confirmLabel ?? 'Confirmar';
  protected readonly cancelLabel = this.data.cancelLabel ?? 'Cancelar';
  protected readonly confirmColor = this.data.confirmColor ?? 'primary';

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.dialogRef.close(true);
  }
}
