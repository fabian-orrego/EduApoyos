import { Pipe, PipeTransform } from '@angular/core';

import {
  SUPPORT_TYPE_LABELS,
  SupportTypeIdValue,
} from '../../core/models/support-request.model';

/**
 * Translates a <c>SupportType</c> numeric identifier (as emitted by the API) into its
 * user-facing Spanish label.
 */
@Pipe({ name: 'tipoApoyo', standalone: true })
export class TipoApoyoPipe implements PipeTransform {
  transform(value: SupportTypeIdValue | null | undefined): string {
    if (value === null || value === undefined) {
      return '';
    }
    return SUPPORT_TYPE_LABELS[value] ?? String(value);
  }
}
