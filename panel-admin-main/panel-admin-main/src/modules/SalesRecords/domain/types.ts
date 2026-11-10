export interface SalesRecordsTotals {
  totalSellAmount: number
  totalPrepaymentAmount: number
  totalReceipts: number
  totalRemaining: number
  scopeNote: string
}

export interface SalesRecordRow {
  orderId: number
  saleDate?: string | null
  approvalStatus: string
  customerId: number
  customerName: string
  customerPhone?: string | null
  sellerId?: number | null
  sellerName?: string | null
  branchId: number
  branchName: string
  sellAmount: number
  prepaymentAmount: number
  dailyInstallmentAmount: number
  receiptsTotal: number
  remainingAmount: number
  linkedSalesRequestId?: number | null
}

export interface ListSalesRecordsResponse {
  items: SalesRecordRow[]
  totalCount: number
  page: number
  pageSize: number
  totals: SalesRecordsTotals
  branchScope: number[]
  currencyNote: string
}
