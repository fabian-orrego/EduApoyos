import { TemplateRef } from '@angular/core';

/**
 * Column descriptor consumed by <c>DataTableComponent</c>. Text columns provide a
 * <c>value</c> accessor; custom columns (e.g. actions) pass a <c>cellTemplate</c> whose
 * implicit context is the row.
 */
export interface DataTableColumn<T> {
  key: string;
  header: string;
  value: (row: T) => unknown;
  align?: 'start' | 'center' | 'end';
  cellTemplate?: TemplateRef<{ $implicit: T }>;
}

export interface DataTablePageEvent {
  pageIndex: number;
  pageSize: number;
}
