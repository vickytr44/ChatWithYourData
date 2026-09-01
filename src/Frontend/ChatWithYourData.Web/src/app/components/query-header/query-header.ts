import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'app-query-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './query-header.html',
  styleUrl: './query-header.css'
})
export class QueryHeaderComponent {
  readonly dataService = inject(DataService);
  
  queryInput = signal('');
  selectedCategory = signal('All');
  selectedStatus = signal('All');

  readonly suggestionChips = [
    { label: 'Critical Low Stock', query: 'critical' },
    { label: 'Pending PO Approvals', query: 'pending' },
    { label: 'Datacenter & AI Hardware', query: 'hardware' },
    { label: 'Finance Ledger & Tax', query: 'tax' }
  ];

  onSearchSubmit() {
    this.dataService.simulateLoading(400);
    this.dataService.setSearchQuery(this.queryInput());
  }

  applySuggestion(suggestion: string) {
    this.queryInput.set(suggestion);
    this.dataService.simulateLoading(350);
    this.dataService.setSearchQuery(suggestion);
  }

  onCategoryChange(event: Event) {
    const val = (event.target as HTMLSelectElement).value;
    this.selectedCategory.set(val);
    this.dataService.setCategoryFilter(val);
  }

  onStatusChange(event: Event) {
    const val = (event.target as HTMLSelectElement).value;
    this.selectedStatus.set(val);
    this.dataService.setStatusFilter(val);
  }

  clearSearch() {
    this.queryInput.set('');
    this.dataService.setSearchQuery('');
  }
}
