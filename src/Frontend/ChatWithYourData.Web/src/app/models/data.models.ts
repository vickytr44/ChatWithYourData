export type SubgraphModule = 'All' | 'Inventory' | 'Sales' | 'Procurement' | 'Finance';

export type ItemStatus = 'in_stock' | 'critical_low' | 'pending_po' | 'fulfilled' | 'on_hold';

export interface LineItem {
  id: string;
  sku: string;
  name: string;
  category: string;
  subgraph: SubgraphModule;
  quantity: number;
  unitPrice: number;
  currency: string;
  status: ItemStatus;
  statusLabel: string;
  reorderPoint: number;
  lastUpdated: string;
}

export interface KpiSummary {
  activeLineItems: number;
  totalValue: number;
  pendingOrders: number;
  criticalAlerts: number;
}

export interface FilterState {
  searchQuery: string;
  subgraph: SubgraphModule;
  category: string;
  status: string;
  page: number;
  pageSize: number;
}

export type ViewState = 'ready' | 'loading' | 'empty' | 'error';
