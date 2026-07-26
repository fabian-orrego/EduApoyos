import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

import { DataTableColumn, DataTablePageEvent } from './data-table.model';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent<T> {
  readonly columns = input.required<DataTableColumn<T>[]>();
  readonly rows = input.required<T[]>();
  readonly totalItems = input<number>(0);
  readonly pageIndex = input<number>(0);
  readonly pageSize = input<number>(10);
  readonly pageSizeOptions = input<number[]>([5, 10, 25, 50]);
  readonly showPaginator = input<boolean>(true);

  readonly pageChange = output<DataTablePageEvent>();

  readonly displayedColumns = computed(() => this.columns().map((c) => c.key));

  onPage(event: PageEvent): void {
    this.pageChange.emit({ pageIndex: event.pageIndex, pageSize: event.pageSize });
  }
}
