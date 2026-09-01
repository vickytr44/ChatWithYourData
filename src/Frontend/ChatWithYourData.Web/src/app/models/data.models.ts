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

export interface TableColumn {
  key: string;
  label: string;
  type: 'string' | 'number' | 'currency' | 'badge' | 'date';
}

export interface TableData {
  tableName: string;
  description?: string | null;
  parentKeyName?: string | null;
  columns: TableColumn[];
  rows: Record<string, any>[];
}

export interface DynamicDataQueryResponse {
  success: boolean;
  summary: string;
  tables: TableData[];
  rawJson?: string | null;
  errorMessage?: string | null;
}
