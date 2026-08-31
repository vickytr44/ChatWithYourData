import { Component, signal } from '@angular/core';
import { CopilotChat, CopilotSidebar } from '@copilotkit/angular';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CopilotChat, CopilotSidebar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly sidebarOpen = signal(false);
}


