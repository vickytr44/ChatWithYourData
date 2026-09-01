import { Injectable, signal, computed } from '@angular/core';
import { LineItem, KpiSummary, FilterState, ViewState, SubgraphModule } from '../models/data.models';

const INITIAL_ITEMS: LineItem[] = [
  {
    id: 'ITEM-1001',
    sku: 'INV-RAW-401',
    name: 'High-Purity Silicon Wafers 300mm',
    category: 'Semiconductor Raw Materials',
    subgraph: 'Inventory',
    quantity: 140,
    unitPrice: 125.00,
    currency: 'USD',
    status: 'critical_low',
    statusLabel: 'Critical Low',
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
    status: 'in_stock',
    statusLabel: 'In Stock',
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
    status: 'pending_po',
    statusLabel: 'Pending PO Approval',
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
    status: 'fulfilled',
    statusLabel: 'Fulfilled Order',
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
    status: 'in_stock',
    statusLabel: 'Active Ledger',
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
    status: 'critical_low',
    statusLabel: 'Critical Low',
    reorderPoint: 100,
    lastUpdated: '2026-08-31 08:00'
  },
  {
    id: 'ITEM-1007',
    sku: 'PO-9945-B',
    name: 'Precision Aluminum Heat Sinks CNC Extruded',
    category: 'Thermal Management',
    subgraph: 'Procurement',
    quantity: 2400,
    unitPrice: 18.20,
    currency: 'USD',
    status: 'pending_po',
    statusLabel: 'Supplier Confirmation',
    reorderPoint: 500,
    lastUpdated: '2026-08-31 10:10'
  },
  {
    id: 'ITEM-1008',
    sku: 'SO-8840-EU',
    name: 'Industrial Smart Gateway Edge AI Unit',
    category: 'IoT Edge Computing',
    subgraph: 'Sales',
    quantity: 120,
    unitPrice: 620.00,
    currency: 'USD',
    status: 'in_stock',
    statusLabel: 'Processing Delivery',
    reorderPoint: 25,
    lastUpdated: '2026-08-31 13:45'
  },
  {
    id: 'ITEM-1009',
    sku: 'FIN-TAX-2026',
    name: 'Cross-Border VAT & Compliance Reserve',
    category: 'Tax & Compliance',
    subgraph: 'Finance',
    quantity: 1,
    unitPrice: 84000.00,
    currency: 'USD',
    status: 'fulfilled',
    statusLabel: 'Reconciled',
    reorderPoint: 0,
    lastUpdated: '2026-08-28 17:30'
  },
  {
    id: 'ITEM-1010',
    sku: 'INV-ASM-503',
    name: 'High-Density Lithium-Polymer Battery Pack 48V',
    category: 'Energy Storage',
    subgraph: 'Inventory',
    quantity: 18,
    unitPrice: 480.00,
    currency: 'USD',
    status: 'critical_low',
    statusLabel: 'Critical Low',
    reorderPoint: 50,
    lastUpdated: '2026-08-31 07:30'
  }
];

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private readonly allItems = signal<LineItem[]>(INITIAL_ITEMS);
  
  readonly viewState = signal<ViewState>('ready');
  readonly errorMessage = signal<string | null>(null);

  readonly filters = signal<FilterState>({
    searchQuery: '',
    subgraph: 'All',
    category: 'All',
    status: 'All',
    page: 1,
    pageSize: 5
  });

  // Filtered dataset
  readonly filteredItems = computed(() => {
    const items = this.allItems();
    const f = this.filters();
    const query = f.searchQuery.toLowerCase().trim();

    return items.filter(item => {
      // Subgraph filter
      if (f.subgraph !== 'All' && item.subgraph !== f.subgraph) {
        return false;
      }
      // Status filter
      if (f.status !== 'All' && item.status !== f.status) {
        return false;
      }
      // Category filter
      if (f.category !== 'All' && item.category !== f.category) {
        return false;
      }
      // Search query (matches SKU, name, or category)
      if (query) {
        const matchSku = item.sku.toLowerCase().includes(query);
        const matchName = item.name.toLowerCase().includes(query);
        const matchCat = item.category.toLowerCase().includes(query);
        const matchSub = item.subgraph.toLowerCase().includes(query);
        if (!matchSku && !matchName && !matchCat && !matchSub) {
          return false;
        }
      }
      return true;
    });
  });

  // Paginated view
  readonly paginatedItems = computed(() => {
    const list = this.filteredItems();
    const f = this.filters();
    const start = (f.page - 1) * f.pageSize;
    return list.slice(start, start + f.pageSize);
  });

  readonly totalFilteredCount = computed(() => this.filteredItems().length);

  readonly totalPages = computed(() => {
    const count = this.totalFilteredCount();
    const size = this.filters().pageSize;
    return Math.max(1, Math.ceil(count / size));
  });

  readonly kpiSummary = computed<KpiSummary>(() => {
    const items = this.allItems();
    const totalVal = items.reduce((acc, item) => acc + (item.quantity * item.unitPrice), 0);
    const pending = items.filter(i => i.status === 'pending_po').length;
    const critical = items.filter(i => i.status === 'critical_low').length;

    return {
      activeLineItems: 1248, // Global enterprise count
      totalValue: totalVal > 0 ? totalVal : 4200000,
      pendingOrders: 42,
      criticalAlerts: critical + 2
    };
  });

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
    this.viewState.set('ready');
    this.errorMessage.set(null);
  }

  private evaluateState() {
    if (this.filteredItems().length === 0) {
      this.viewState.set('empty');
    } else {
      this.viewState.set('ready');
      this.errorMessage.set(null);
    }
  }
}
