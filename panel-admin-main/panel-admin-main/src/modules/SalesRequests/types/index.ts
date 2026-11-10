export type MatchCriterion =
  | 'tripleName'
  | 'phone'
  | 'fatherGrandfather'
  | 'sonFather'

export type MatchCardState =
  | 'loading'
  | 'results'
  | 'noResults'
  | 'insufficientData'
  | 'failed'

export type SearchSource = 'priorAccounts' | 'duplicates'

/** عنصر قائمة إداري — عقد 04B. */
export interface SalesRequestListItem {
  id: number
  customerName: string
  customerPhone?: string | null
  address?: string | null
  requestedProduct?: string | null
  submissionNote?: string | null
  sourceType: number
  sourceTypeLabel: string
  originalExternalId?: string | null
  branchId: number
  branchName?: string | null
  provinceId?: number | null
  provinceName?: string | null
  status: number
  statusLabel: string
  tabKey: string
  targetEmployeeId?: number | null
  targetEmployeeName?: string | null
  linkedOrderId?: number | null
  existingCustomerId?: number | null
  createdAt?: string | null
  statusChangedAtUtc?: string | null
  lastStatusNote?: string | null
  rejectReason?: string | null
  createdByName?: string | null
  concurrencyStamp: number
  isDeleted?: boolean
  deletedAtUtc?: string | null
  deleteReason?: string | null
  exceptionState?: 'pending' | 'approved' | 'approved_consumed' | 'rejected' | null
  exceptionDecidedAtUtc?: string | null
  exceptionConsumedAtUtc?: string | null
  isSentSource?: boolean
}

/** توافق مع صفحة غير المسند السابقة. */
export type UnassignedSalesRequest = SalesRequestListItem & {
  statusLabel: string
}

export interface SalesRequestTabCount {
  tab: string
  label: string
  count: number
}

/** عرض إضافي — متداخل مع سير العمل ولا يدخل في «الكل». */
export interface SalesRequestExtraViewCount extends SalesRequestTabCount {
  overlapping: boolean
  includedInAll: boolean
  caveat?: string | null
}

export interface SalesRequestCountsResponse {
  tabs: SalesRequestTabCount[]
  allowedTabs: string[]
  extraViews?: SalesRequestExtraViewCount[]
  allowedExtraViews?: string[]
  extraViewsNotImplemented: string[]
}

export interface SalesRequestTimelineItem {
  id: number
  eventType: number
  eventLabel: string
  actorUserId: number
  actorName?: string | null
  occurredAtUtc: string
  previousStatus?: number | null
  previousStatusLabel?: string | null
  newStatus?: number | null
  newStatusLabel?: string | null
  previousEmployeeId?: number | null
  newEmployeeId?: number | null
  reason?: string | null
}

export interface AssignableEmployee {
  id: number
  fullName: string
  username?: string | null
}

export interface MatchCard {
  key: MatchCriterion | string
  label: string
  state: MatchCardState
  count: number | null
}

export interface MatchSummaryItem {
  requestId: number
  cards: MatchCard[]
}

export interface MatchHit {
  customerId: number
  customerName: string
  matchReason: string
  highlightedParts: string[]
  phone?: string | null
  branchName?: string | null
  ratingLabel?: string | null
  score?: number | null
  isLegal?: boolean | null
  sellTotal?: number | null
  paidTotal?: number | null
  remaining?: number | null
  paymentCount?: number | null
  paymentDays?: number | null
  saleDate?: string | null
  canOpenProfile: boolean
}

export interface DuplicateRequestHit {
  id: number
  sourceType: number
  sourceTypeLabel: string
  statusLabel: string
  createdAt?: string | null
  assignedEmployeeName?: string | null
}

export const MATCH_LABELS: Record<MatchCriterion, string> = {
  tripleName: 'تشابه الاسم الثلاثي',
  phone: 'تشابه رقم الهاتف',
  fatherGrandfather: 'تشابه اسم الأب والجد',
  sonFather: 'تطابق ابن مع أب',
}

export const SOURCE_OPTIONS = [
  { value: '', label: 'كل المصادر' },
  { value: '0', label: 'إدخال يدوي' },
  { value: '1', label: 'تقديم موظف' },
  { value: '2', label: 'مسؤول اللجنة' },
  { value: '3', label: 'مندوب' },
  { value: '4', label: 'استيراد Excel' },
  { value: '5', label: 'CRM خارجي' },
]

export function formatDurationInStatus(
  statusChangedAtUtc?: string | null,
  createdAt?: string | null,
  now = Date.now()
): string {
  const raw = statusChangedAtUtc || createdAt
  if (!raw) return '—'
  const start = new Date(raw).getTime()
  if (Number.isNaN(start)) return '—'
  const diffMs = Math.max(0, now - start)
  const mins = Math.floor(diffMs / 60000)
  if (mins < 60) return `${mins} د`
  const hours = Math.floor(mins / 60)
  if (hours < 48) return `${hours} س`
  const days = Math.floor(hours / 24)
  return `${days} ي`
}
