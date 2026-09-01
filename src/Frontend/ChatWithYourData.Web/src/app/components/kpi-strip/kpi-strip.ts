import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'app-kpi-strip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './kpi-strip.html',
  styleUrl: './kpi-strip.css'
})
export class KpiStripComponent {
  readonly dataService = inject(DataService);
}
