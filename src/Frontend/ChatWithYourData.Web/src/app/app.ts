import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CopilotSidebar } from '@copilotkit/angular';
import { NavbarComponent } from './components/navbar/navbar';
import { QueryHeaderComponent } from './components/query-header/query-header';
import { DataGridComponent } from './components/data-grid/data-grid';
import { EmptyStateComponent } from './components/empty-state/empty-state';
import { LoadingStateComponent } from './components/loading-state/loading-state';
import { ErrorStateComponent } from './components/error-state/error-state';
import { DataService } from './services/data.service';
import { LineItem } from './models/data.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    NavbarComponent,
    QueryHeaderComponent,
    DataGridComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    ErrorStateComponent,
    CopilotSidebar
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly dataService = inject(DataService);
  readonly sidebarOpen = signal(false);
  readonly selectedItemContext = signal<any | null>(null);

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  onAskItemCopilot(item: any) {
    this.selectedItemContext.set(item);
    this.sidebarOpen.set(true);
  }

  clearContext() {
    this.selectedItemContext.set(null);
  }
}
