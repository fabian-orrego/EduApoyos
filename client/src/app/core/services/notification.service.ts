import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly defaults: MatSnackBarConfig = {
    duration: 4000,
    horizontalPosition: 'end',
    verticalPosition: 'top',
  };

  success(message: string): void {
    this.snackBar.open(message, 'Close', {
      ...this.defaults,
      panelClass: ['snackbar-success'],
    });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Close', {
      ...this.defaults,
      duration: 6000,
      panelClass: ['snackbar-error'],
    });
  }

  info(message: string): void {
    this.snackBar.open(message, 'Close', {
      ...this.defaults,
      panelClass: ['snackbar-info'],
    });
  }
}
