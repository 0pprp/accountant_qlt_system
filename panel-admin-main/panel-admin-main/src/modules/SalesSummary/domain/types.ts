export interface SalesSummaryMatrixResponse {
  year: number
  month: number
  daysInMonth: number
  timeZone: string
  currency: string
  metric: string
  metricLabel: string
  dayBoundNote: string
  branches: Array<{ id: number; name: string }>
  cells: Array<{ day: number; branchId: number; amount: number }>
  dayTotals: Array<{ day: number; amount: number }>
  branchTotals: Array<{ branchId: number; amount: number }>
  monthTotal: number
  generatedAt: string
}

export interface SalesSummaryDetailsResponse {
  year: number
  month: number
  day: number
  branchId: number
  branchName: string
  metric: string
  metricLabel: string
  cellTotal: number
  currency: string
  dayBoundNote: string
  items: Array<{
    orderId: number
    title: string
    subtitle?: string | null
    amount: number
    occurredAt: string
  }>
  page: number
  pageSize: number
  totalCount: number
  generatedAt: string
}
