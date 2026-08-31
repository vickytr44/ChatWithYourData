import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { HttpAgent } from '@ag-ui/client';
import { provideCopilotKit } from '@copilotkit/angular';

const AG_UI_BACKEND_URL = 'http://localhost:5005/ag-ui';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideHttpClient(withFetch()),
    provideCopilotKit({
      agents: {
        'ChatWithYourDataERP': new HttpAgent({
          url: AG_UI_BACKEND_URL,
          description: 'Enterprise ERP AI Assistant connecting Inventory, Sales, Procurement, and Finance via Federated MCP tools.'
        }),
        'default': new HttpAgent({
          url: AG_UI_BACKEND_URL,
          description: 'Enterprise ERP AI Assistant connecting Inventory, Sales, Procurement, and Finance via Federated MCP tools.'
        })
      },
      enableInspector: false,
      defaultToolRendering: true
    })
  ]
};
