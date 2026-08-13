import type { Attachment } from '@/modules/User/types/model'

export interface UserProfileResponse {
  id: number
  fullName: string
  profilePicture: Attachment | null
  roles: string[]
}

export interface MonthlyAmount {
  month: number
  value: number
}

export interface YearlyFinancialReportResponse {
  totalSellAmount: number
  totalPurchaseAmount: number
  totalInstallmentPaymentAmount: number
  sellAmounts: MonthlyAmount[]
  purchaseAmounts: MonthlyAmount[]
  installmentPaymentAmounts: MonthlyAmount[]
}

export interface StatWithChange {
  count: number
  changePercent: number
}

export interface DashboardUsersReportResponse {
  customers: StatWithChange
  mandobUsers: StatWithChange
  allUsers: StatWithChange
}

export interface SafeReportPeriod {
  totalWithdrawalAmount: number
  totalTransferredAmount: number
  totalEarnedAmount: number
  remainingCashAmount: number
}

export interface SafeReportResponse {
  today: SafeReportPeriod
  yesterday: SafeReportPeriod
  lastWeek: SafeReportPeriod
  lastMonth: SafeReportPeriod
  lastYear: SafeReportPeriod
}

export interface ProductsReportResponse {
  productsCount: number
}
