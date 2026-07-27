import { Pipe, PipeTransform } from '@angular/core';

import {
  SUPPORT_REQUEST_STATUS_LABELS,
  SupportRequestStatusIdValue,
} from '../../core/models/support-request.model';

/**
 * Translates a <c>SupportRequestStatus</c> numeric identifier (as emitted by the API) into
 * its user-facing Spanish label (US-011 shared UX for the support requests module).
 */
@Pipe({ name: 'estado', standalone: true })
export class EstadoPipe implements PipeTransform {
  transform(value: SupportRequestStatusIdValue | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }
    return SUPPORT_REQUEST_STATUS_LABELS[value] ?? String(value);
  }
}
