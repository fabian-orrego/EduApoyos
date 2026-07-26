export interface DataTableColumn<T> {
  key: string;
  header: string;
  value: (row: T) => unknown;
  align?: 'start' | 'center' | 'end';
}

export interface DataTablePageEvent {
  pageIndex: number;
  pageSize: number;
}
