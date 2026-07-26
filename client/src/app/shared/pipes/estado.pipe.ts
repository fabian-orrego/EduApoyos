import { Pipe, PipeTransform } from '@angular/core';

export type EstadoSolicitud = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | string;

const LABELS: Record<string, string> = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Cancelled: 'Cancelled',
};

@Pipe({ name: 'estado', standalone: true })
export class EstadoPipe implements PipeTransform {
  transform(value: EstadoSolicitud | null | undefined): string {
    if (value == null) {
      return '';
    }
    return LABELS[value] ?? String(value);
  }
}
