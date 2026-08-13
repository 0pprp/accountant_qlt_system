export type DashboardTrend = 'up' | 'down' | 'neutral'

export interface DashboardUser {
  fullName: string
  role: string
  avatarUrl: string | null
}

export interface DashboardMonthlyChartData {
  labels: string[]
  sales: number[]
  purchases: number[]
  payments: number[]
  totals: {
    sales: number
    purchases: number
    payments: number
  }
}

export interface DashboardStatCard {
  count: number
  change: number
  trend: DashboardTrend
}

export interface DashboardStats {
  customers: DashboardStatCard
  representatives: DashboardStatCard
  users: DashboardStatCard
}

export interface DashboardFinancialRow {
  id: number
  period: string
  cashSafes: number
  withdrawals: number
  transfer: number
  additions: number
}

export interface DashboardData {
  user: DashboardUser
  monthlyChart: DashboardMonthlyChartData
  stats: DashboardStats
  financialRows: DashboardFinancialRow[]
}
