export const WRONG_SALE_PAGE_SIZE = 20

export type DelegateMailbox = 'unread' | 'seen' | 'reviewed'

export interface WrongSaleRow {
  id: number
  orderId: number
  customerId: number
  branchId: number
  customerName: string
  customerPhone: string | null
  rating: number
  note: string | null
  shopStockEstimatedValue: number
  estimatedDailyRevenue: number
  reasonBlock: string
  sourceApp: string
  status: string
  followerName: string | null
  createdAtUtc: string
  isDelegateUnread: boolean
  isBranchUnreviewed: boolean
  delegateReadAtUtc: string | null
  branchManagerReviewedAtUtc: string | null
  branchManagerReviewNote: string | null
  expectedVersion: number
}

export interface WrongSaleListResponse {
  items: WrongSaleRow[]
  totalCount: number
  unreadCount: number
  unreviewedCount: number
  seenCount?: number
  reviewedCount?: number
  page: number
  pageSize: number
}

export interface WrongSaleDetail {
  notice: WrongSaleRow
  review: { reviewerName: string | null; reviewedAtUtc: string; note: string | null } | null
  customer: {
    name: string
    phone: string | null
    branchId: number
    branchName: string | null
    address: string | null
    listId: number | null
    listName: string | null
  }
  sale: {
    sellAmount: number
    dailyInstallment: number
    paidOnOrder: number
    remainingOnOrder: number
    customerRemaining: number
    saleDate: string | null
  }
  evaluation: {
    source: string
    title: string
    evaluatorLabel: string
    evaluatorName: string | null
    listName: string | null
    isEvaluated: boolean
    rating: number
    note: string | null
    shopStockEstimatedValue: number
    estimatedDailyRevenue: number
    saleAmountAtEvaluation: number | null
  }
  paymentsCount: number
  payments: { id: number; date: string; amount: number; description: string | null }[]
  matches: {
    customerId: number
    name: string
    phone: string | null
    matchReasons: string[]
    lastSaleDate: string | null
    totalSales: number
    totalPaid: number
    remaining: number
  }[]
}

export function normalizeDelegateMailbox(raw: unknown): DelegateMailbox {
  const v = String(Array.isArray(raw) ? raw[0] : raw ?? '').trim().toLowerCase()
  if (v === 'seen' || v === 'read') return 'seen'
  if (v === 'reviewed') return 'reviewed'
  return 'unread'
}

const BAGHDAD = 'Asia/Baghdad'

function parts(d: Date, withTime: boolean) {
  const fmt = new Intl.DateTimeFormat('en-US', {
    timeZone: BAGHDAD,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    ...(withTime ? { hour: '2-digit', minute: '2-digit', hour12: true } : {}),
  })
  const map: Record<string, string> = {}
  for (const p of fmt.formatToParts(d)) map[p.type] = p.value
  return map
}

export function wrongSaleDateTime(value: string | null | undefined): string {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const p = parts(d, true)
  const hour = p.hour === '00' ? '12' : p.hour
  const suffix = (p.dayPeriod ?? '').toUpperCase() === 'PM' ? 'م' : 'ص'
  return `${p.year}/${p.month}/${p.day} ${hour}:${p.minute} ${suffix}`
}

export function wrongSaleDate(value: string | null | undefined): string {
  if (!value) return '—'
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value)
  if (m) return `${m[1]}/${m[2]}/${m[3]}`
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const p = parts(d, false)
  return `${p.year}/${p.month}/${p.day}`
}
