import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'app-error-state',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './error-state.html',
  styleUrl: './error-state.css'
})
export class ErrorStateComponent {
  readonly dataService = inject(DataService);

  retry() {
    this.dataService.simulateLoading(500);
  }
}
