import type {
  DashboardUsersReportResponse,
  MonthlyAmount,
  SafeReportResponse,
  UserProfileResponse,
  YearlyFinancialReportResponse,
} from '@/modules/Core/types/api/dashboard'
import type {
  DashboardFinancialRow,
  DashboardMonthlyChartData,
  DashboardStatCard,
  DashboardStats,
  DashboardTrend,
  DashboardUser,
} from '@/modules/Core/types/model/dashboard'

const MONTH_LABELS = Array.from({ length: 12 }, (_, i) => String(i + 1))

function buildMonthlySeries(amounts: MonthlyAmount[]): number[] {
  const series = Array<number>(12).fill(0)
  amounts.forEach(({ month, value }) => {
    if (month >= 1 && month <= 12) {
      series[month - 1] = value
    }
  })
  return series
}

function mapChangeToTrend(changePercent: number): DashboardTrend {
  if (changePercent > 0) return 'up'
  if (changePercent < 0) return 'down'
  return 'neutral'
}

function mapStatWithChange(stat: {
  count: number
  changePercent: number
}): DashboardStatCard {
  return {
    count: stat.count,
    change: stat.changePercent,
    trend: mapChangeToTrend(stat.changePercent),
  }
}

export function mapProfileToDashboardUser(
  profile: UserProfileResponse
): DashboardUser {
  const baseFileUrl = import.meta.env.VITE_FILE_BASE_URL

  return {
    fullName: profile.fullName,
    role: profile.roles.join('، ') || '',
    avatarUrl: profile.profilePicture?.relativePath
      ? `${baseFileUrl}${profile.profilePicture.relativePath}`
      : null,
  }
}

export function mapYearlyFinancialToChartData(
  report: YearlyFinancialReportResponse
): DashboardMonthlyChartData {
  return {
    labels: MONTH_LABELS,
    sales: buildMonthlySeries(report.sellAmounts),
    purchases: buildMonthlySeries(report.purchaseAmounts),
    payments: buildMonthlySeries(report.installmentPaymentAmounts),
    totals: {
      sales: report.totalSellAmount,
      purchases: report.totalPurchaseAmount,
      payments: report.totalInstallmentPaymentAmount,
    },
  }
}

export function mapUsersReportToStats(
  report: DashboardUsersReportResponse
): DashboardStats {
  return {
    customers: mapStatWithChange(report.customers),
    representatives: mapStatWithChange(report.mandobUsers),
    users: mapStatWithChange(report.allUsers),
  }
}

export interface SafeReportPeriodLabels {
  today: string
  yesterday: string
  lastWeek: string
  lastMonth: string
  lastYear: string
}

const SAFE_REPORT_PERIODS = [
  'today',
  'yesterday',
  'lastWeek',
  'lastMonth',
  'lastYear',
] as const

export function mapSafeReportToFinancialRows(
  report: SafeReportResponse,
  periodLabels: SafeReportPeriodLabels
): DashboardFinancialRow[] {
  return SAFE_REPORT_PERIODS.map((key, index) => {
    const period = report[key]

    return {
      id: index + 1,
      period: periodLabels[key],
      cashSafes: period.remainingCashAmount,
      withdrawals: period.totalWithdrawalAmount,
      transfer: period.totalTransferredAmount,
      additions: period.totalEarnedAmount,
    }
  })
}
