import { Component, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataService } from '../../services/data.service';
import { TableColumn } from '../../models/data.models';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-grid.html',
  styleUrl: './data-grid.css'
})
export class DataGridComponent {
  readonly dataService = inject(DataService);
  readonly askCopilot = output<any>();

  selectedRowIds = signal<Set<string>>(new Set());

  get activeColumns(): TableColumn[] {
    return this.dataService.activeTable()?.columns || [];
  }

  getRowId(row: Record<string, any>, index: number): string {
    return String(row['id'] || row['sku'] || row['poNumber'] || row['orderNumber'] || row['invoiceId'] || index);
  }

  toggleSelectAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      const allIds = new Set(this.dataService.paginatedRows().map((r, i) => this.getRowId(r, i)));
      this.selectedRowIds.set(allIds);
    } else {
      this.selectedRowIds.set(new Set());
    }
  }

  toggleSelectRow(id: string) {
    const set = new Set(this.selectedRowIds());
    if (set.has(id)) {
      set.delete(id);
    } else {
      set.add(id);
    }
    this.selectedRowIds.set(set);
  }

  isRowSelected(id: string): boolean {
    return this.selectedRowIds().has(id);
  }

  get allCurrentPageSelected(): boolean {
    const current = this.dataService.paginatedRows();
    if (current.length === 0) return false;
    return current.every((r, i) => this.selectedRowIds().has(this.getRowId(r, i)));
  }

  formatCurrency(value: any): string {
    const num = Number(value);
    if (isNaN(num)) return String(value ?? '—');
    return '$' + num.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  getBadgeClass(val: any): string {
    const str = String(val || '').toLowerCase();
    if (str.includes('stock') || str.includes('fulfill') || str.includes('settled') || str.includes('active') || str.includes('success')) {
      return 'badge-success';
    }
    if (str.includes('crit') || str.includes('error') || str.includes('failed') || str.includes('overdue')) {
      return 'badge-error';
    }
    if (str.includes('pend') || str.includes('warn') || str.includes('approv') || str.includes('review')) {
      return 'badge-warning';
    }
    return 'badge-info';
  }

  onAskRowCopilot(row: Record<string, any>) {
    this.askCopilot.emit(row);
  }

  onPageSizeChange(event: Event) {
    const val = parseInt((event.target as HTMLSelectElement).value, 10);
    this.dataService.setPageSize(val);
  }

  get pages(): number[] {
    const total = this.dataService.totalPages();
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  get startRange(): number {
    const f = this.dataService.filters();
    return (f.page - 1) * f.pageSize + 1;
  }

  get endRange(): number {
    const f = this.dataService.filters();
    const total = this.dataService.totalFilteredCount();
    return Math.min(f.page * f.pageSize, total);
  }
}
