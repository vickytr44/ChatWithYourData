import { Component, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataService } from '../../services/data.service';
import { LineItem } from '../../models/data.models';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-grid.html',
  styleUrl: './data-grid.css'
})
export class DataGridComponent {
  readonly dataService = inject(DataService);
  readonly askCopilot = output<LineItem>();

  selectedItems = signal<Set<string>>(new Set());

  toggleSelectAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      const allIds = new Set(this.dataService.paginatedItems().map(i => i.id));
      this.selectedItems.set(allIds);
    } else {
      this.selectedItems.set(new Set());
    }
  }

  toggleSelectItem(id: string) {
    const set = new Set(this.selectedItems());
    if (set.has(id)) {
      set.delete(id);
    } else {
      set.add(id);
    }
    this.selectedItems.set(set);
  }

  isItemSelected(id: string): boolean {
    return this.selectedItems().has(id);
  }

  get allCurrentPageSelected(): boolean {
    const current = this.dataService.paginatedItems();
    if (current.length === 0) return false;
    return current.every(i => this.selectedItems().has(i.id));
  }

  onAskItemCopilot(item: LineItem) {
    this.askCopilot.emit(item);
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
