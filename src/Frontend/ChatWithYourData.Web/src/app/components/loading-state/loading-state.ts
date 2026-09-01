import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-state.html',
  styleUrl: './loading-state.css'
})
export class LoadingStateComponent {
  readonly skeletonRows = Array.from({ length: 5 });
}
