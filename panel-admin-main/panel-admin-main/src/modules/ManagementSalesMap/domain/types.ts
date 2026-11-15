export const SalesMapStatusFilter = {
  All: 0,
  Continuous: 1,
  Stopped: 2,
  Legal: 3,
} as const

export type SalesMapStatusFilterValue =
  (typeof SalesMapStatusFilter)[keyof typeof SalesMapStatusFilter]

export interface SalesMapProvinceCard {
  provinceId: number
  provinceName: string
  listCount: number
  salesWithLocation: number
  salesWithoutLocation: number
}

export interface SalesMapListOption {
  listId: number
  listName: string
  branchId: number
  letter: string
  salesWithLocation: number
  salesWithoutLocation: number
}

export interface SalesMapPoint {
  orderId: number
  customerId: number
  customerName: string
  listId: number
  listName: string
  listLetter: string
  latitude: number
  longitude: number
  statusCode: number | null
  statusLabel: string
  ratingTier: string | null
  ratingLabel: string | null
}

export interface SalesMapOrderDetail {
  orderId: number
  customerId: number
  customerName: string
  listId: number
  listName: string
  listLetter: string
  statusLabel: string
  ratingLabel: string | null
  sellAmount: number
  paidAmount: number
  remainingAmount: number
  saleDate: string | null
  lastPaymentDate: string | null
  lastPaymentAmount: number | null
  lastPaymentDisplay: string | null
}

export const RATING_COLORS: Record<string, string> = {
  excellent: '#0b6b3a',
  good: '#006a64',
  weak: '#b45309',
  lagging: '#c2410c',
  bad: '#b91c1c',
  unknown: '#6f797a',
}

export function pointColor(tier: string | null | undefined): string {
  if (!tier) return '#94a3b8'
  return RATING_COLORS[tier] ?? '#94a3b8'
}

export function statusGlyph(code: number | null): string {
  if (code === 1) return '١'
  if (code === 2) return '٢'
  if (code === 3) return '٣'
  return '؟'
}
