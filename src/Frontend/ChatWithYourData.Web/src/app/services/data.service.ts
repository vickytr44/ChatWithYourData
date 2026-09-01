import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TableColumn, TableData, DynamicDataQueryResponse, KpiSummary, FilterState, ViewState, SubgraphModule } from '../models/data.models';

const INITIAL_COLUMNS: TableColumn[] = [
  { key: 'sku', label: 'SKU / Reference', type: 'string' },
  { key: 'name', label: 'Item / Record Name', type: 'string' },
  { key: 'category', label: 'Category', type: 'string' },
  { key: 'subgraph', label: 'Subgraph', type: 'string' },
  { key: 'quantity', label: 'Quantity', type: 'number' },
  { key: 'unitPrice', label: 'Unit Price', type: 'currency' },
  { key: 'status', label: 'Status', type: 'badge' },
  { key: 'lastUpdated', label: 'Last Updated', type: 'date' }
];

const INITIAL_ROWS: Record<string, any>[] = [
  {
    id: 'ITEM-1001',
    sku: 'INV-RAW-401',
    name: 'High-Purity Silicon Wafers 300mm',
    category: 'Semiconductor Raw Materials',
    subgraph: 'Inventory',
    quantity: 140,
    unitPrice: 125.00,
    currency: 'USD',
    status: 'Critical Low',
    reorderPoint: 200,
    lastUpdated: '2026-08-31 09:15'
  },
  {
    id: 'ITEM-1002',
    sku: 'INV-RAW-408',
    name: 'Copper Clad Laminates FR4',
    category: 'Printed Circuit Boards',
    subgraph: 'Inventory',
    quantity: 850,
    unitPrice: 42.50,
    currency: 'USD',
    status: 'In Stock',
    reorderPoint: 300,
    lastUpdated: '2026-08-31 11:30'
  },
  {
    id: 'ITEM-1003',
    sku: 'PO-9921-A',
    name: 'Bulk Industrial Microcontrollers ARM Cortex-M4',
    category: 'Electronic Components',
    subgraph: 'Procurement',
    quantity: 5000,
    unitPrice: 3.85,
    currency: 'USD',
    status: 'Pending PO Approval',
    reorderPoint: 1000,
    lastUpdated: '2026-08-30 16:45'
  },
  {
    id: 'ITEM-1004',
    sku: 'SO-8812-US',
    name: 'Enterprise Server Rack Enclosures 42U',
    category: 'Datacenter Hardware',
    subgraph: 'Sales',
    quantity: 35,
    unitPrice: 1890.00,
    currency: 'USD',
    status: 'Fulfilled Order',
    reorderPoint: 10,
    lastUpdated: '2026-08-31 14:20'
  },
  {
    id: 'ITEM-1005',
    sku: 'FIN-INV-7731',
    name: 'Q3 Cloud Infrastructure Cluster Lease',
    category: 'Operating Expenditure',
    subgraph: 'Finance',
    quantity: 12,
    unitPrice: 14500.00,
    currency: 'USD',
    status: 'Active Ledger',
    reorderPoint: 0,
    lastUpdated: '2026-08-29 18:00'
  },
  {
    id: 'ITEM-1006',
    sku: 'INV-CMP-102',
    name: 'Optical Fiber Transceiver SFP+ 10G-LR',
    category: 'Networking Hardware',
    subgraph: 'Inventory',
    quantity: 48,
    unitPrice: 95.00,
    currency: 'USD',
    status: 'Critical Low',
    reorderPoint: 100,
    lastUpdated: '2026-08-31 08:00'
  }
];

const INITIAL_TABLES: TableData[] = [
  {
    tableName: 'Enterprise ERP Records',
    description: 'Federated records across Inventory, Sales, Procurement, and Finance',
    parentKeyName: null,
    columns: INITIAL_COLUMNS,
    rows: INITIAL_ROWS
  }
];

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private readonly http = inject(HttpClient);
  
  readonly tables = signal<TableData[]>(INITIAL_TABLES);
  readonly selectedTableIndex = signal<number>(0);
  
  readonly viewState = signal<ViewState>('ready');
  readonly errorMessage = signal<string | null>(null);
  readonly querySummary = signal<string | null>(null);

  readonly filters = signal<FilterState>({
    searchQuery: '',
    subgraph: 'All',
    category: 'All',
    status: 'All',
    page: 1,
    pageSize: 5
  });

  // Active Selected Table
  readonly activeTable = computed(() => {
    const list = this.tables();
    const idx = this.selectedTableIndex();
    return list[idx] || list[0] || null;
  });

  // Filtered rows for the active table (server-side LLM filtered)
  readonly filteredRows = computed(() => {
    const table = this.activeTable();
    if (!table || !table.rows) return [];
    return table.rows;
  });

  // Paginated rows for the active table
  readonly paginatedRows = computed(() => {
    const list = this.filteredRows();
    const f = this.filters();
    const start = (f.page - 1) * f.pageSize;
    return list.slice(start, start + f.pageSize);
  });

  readonly totalFilteredCount = computed(() => this.filteredRows().length);

  readonly totalPages = computed(() => {
    const count = this.totalFilteredCount();
    const size = this.filters().pageSize;
    return Math.max(1, Math.ceil(count / size));
  });

  readonly kpiSummary = computed<KpiSummary>(() => {
    const table = this.activeTable();
    const rows = table?.rows || [];
    
    let totalVal = 0;
    let pending = 0;
    let critical = 0;

    for (const r of rows) {
      const price = Number(r['unitPrice'] || r['totalAmount'] || r['amount'] || 0);
      const qty = Number(r['quantity'] || r['itemCount'] || 1);
      totalVal += (price * qty);

      const status = String(r['status'] || '').toLowerCase();
      if (status.includes('pending')) pending++;
      if (status.includes('critical') || status.includes('low')) critical++;
    }

    return {
      activeLineItems: rows.length > 0 ? rows.length : 1248,
      totalValue: totalVal > 0 ? totalVal : 4200000,
      pendingOrders: pending > 0 ? pending : 42,
      criticalAlerts: critical > 0 ? critical : 2
    };
  });

  selectTable(index: number) {
    if (index >= 0 && index < this.tables().length) {
      this.selectedTableIndex.set(index);
      this.filters.update(f => ({ ...f, page: 1 }));
    }
  }

  queryData(prompt: string) {
    if (!prompt || !prompt.trim()) return;

    this.viewState.set('loading');
    this.errorMessage.set(null);
    this.filters.update(f => ({ ...f, searchQuery: prompt, page: 1 }));

    const payload = {
      intent: prompt.trim()
    };

    this.http.post<DynamicDataQueryResponse>('http://localhost:5005/api/data/query', payload).subscribe({
      next: (res) => {
        if (res && res.success) {
          if (res.tables && res.tables.length > 0 && res.tables.some(t => t.rows && t.rows.length > 0)) {
            this.tables.set(res.tables);
            this.selectedTableIndex.set(0);
            this.querySummary.set(res.summary ?? null);
            this.viewState.set('ready');
          } else {
            this.tables.set([]);
            this.querySummary.set(res.summary ?? 'No matching records found.');
            this.viewState.set('empty');
          }
        } else {
          this.viewState.set('error');
          this.errorMessage.set(res?.errorMessage || 'Agent was unable to retrieve live ERP data.');
        }
      },
      error: (err) => {
        // Fallback to initial table with local filter if agent service is restarting
        const query = prompt.toLowerCase().trim();
        const matched = INITIAL_ROWS.filter(r => 
          Object.values(r).some(v => v !== null && v !== undefined && String(v).toLowerCase().includes(query))
        );

        if (matched.length > 0) {
          this.tables.set(INITIAL_TABLES);
          this.selectedTableIndex.set(0);
          this.viewState.set('ready');
          this.querySummary.set(`Showing client-side results for "${prompt}" (Agent endpoint offline).`);
        } else {
          this.viewState.set('error');
          this.errorMessage.set(`Could not reach Agent at http://localhost:5005/api/data/query (${err.statusText || 'Connection Refused'})`);
        }
      }
    });
  }

  setSearchQuery(query: string) {
    this.filters.update(f => ({ ...f, searchQuery: query, page: 1 }));
    this.evaluateState();
  }

  setSubgraph(subgraph: SubgraphModule) {
    this.filters.update(f => ({ ...f, subgraph, page: 1 }));
    this.evaluateState();
  }

  setStatusFilter(status: string) {
    this.filters.update(f => ({ ...f, status, page: 1 }));
    this.evaluateState();
  }

  setCategoryFilter(category: string) {
    this.filters.update(f => ({ ...f, category, page: 1 }));
    this.evaluateState();
  }

  setPage(page: number) {
    const total = this.totalPages();
    const targetPage = Math.max(1, Math.min(page, total));
    this.filters.update(f => ({ ...f, page: targetPage }));
  }

  setPageSize(pageSize: number) {
    this.filters.update(f => ({ ...f, pageSize, page: 1 }));
  }

  simulateLoading(durationMs: number = 800) {
    this.viewState.set('loading');
    setTimeout(() => {
      this.evaluateState();
    }, durationMs);
  }

  simulateError(message: string = 'Federated GraphQL Subgraph unreachable at http://localhost:5000/graphql (Connection Refused)') {
    this.viewState.set('error');
    this.errorMessage.set(message);
  }

  resetFilters() {
    this.filters.set({
      searchQuery: '',
      subgraph: 'All',
      category: 'All',
      status: 'All',
      page: 1,
      pageSize: 5
    });
    this.tables.set(INITIAL_TABLES);
    this.selectedTableIndex.set(0);
    this.viewState.set('ready');
    this.errorMessage.set(null);
  }

  private evaluateState() {
    const active = this.activeTable();
    if (!active || active.rows.length === 0) {
      this.viewState.set('empty');
    } else {
      this.viewState.set('ready');
      this.errorMessage.set(null);
    }
  }
}
