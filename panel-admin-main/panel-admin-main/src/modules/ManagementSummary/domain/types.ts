export type ManagementSummaryMetricKey =
  | 'receipts'
  | 'expenses'
  | 'salePrice'
  | 'purchasePrice'
  | 'buys'

export interface ManagementSummaryBranch {
  id: number
  name: string
}

export interface ManagementSummaryCell {
  day: number
  branchId: number
  amount: number
  availability: string
}

export interface ManagementSummaryDayTotal {
  day: number
  amount: number
}

export interface ManagementSummaryBranchTotal {
  branchId: number
  amount: number
}

export interface ManagementSummaryBoxCard {
  key: string
  label: string
  amount: number | null
  availability: string
  reason: string | null
}

export interface ManagementSummaryMatrixResponse {
  year: number
  month: number
  daysInMonth: number
  timeZone: string
  currency: string
  metric: ManagementSummaryMetricKey
  metricLabel: string
  branches: ManagementSummaryBranch[]
  cells: ManagementSummaryCell[]
  dayTotals: ManagementSummaryDayTotal[]
  branchTotals: ManagementSummaryBranchTotal[]
  monthTotal: number
  boxCards: ManagementSummaryBoxCard[]
  generatedAt: string
}

export interface ManagementSummaryDetailRow {
  id: string
  title: string
  subtitle: string | null
  amount: number
  occurredAt: string
  reference: string | null
}

export interface ManagementSummaryDetailsResponse {
  year: number
  month: number
  day: number
  branchId: number
  branchName: string
  metric: ManagementSummaryMetricKey
  metricLabel: string
  cellTotal: number
  currency: string
  items: ManagementSummaryDetailRow[]
  page: number
  pageSize: number
  totalCount: number
  generatedAt: string
}

export interface SummaryFiltersState {
  year: number
  month: number
  metric: ManagementSummaryMetricKey
  branchId: number | null
}
