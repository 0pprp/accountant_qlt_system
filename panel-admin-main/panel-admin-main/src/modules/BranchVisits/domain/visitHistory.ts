export const VISIT_HISTORY_PAGE_SIZE = 100

export interface VisitHistoryRating {
  orderListId: number
  listName: string
  score: number
  ratedAtUtc: string
}

export interface VisitHistoryAttachment {
  id: number
  originalFileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

export interface VisitHistoryItem {
  id: number
  branchId: number
  branchName?: string | null
  openedByUserId: number
  openedByName: string
  status: string
  arrivedAtUtc: string
  departedAtUtc: string | null
  departNote: string | null
  ratings: VisitHistoryRating[]
  members: { userId: number; displayName: string }[]
  attachments: VisitHistoryAttachment[]
}

export interface VisitHistoryResponse {
  items: VisitHistoryItem[]
  totalCount: number
}

const STATUS_LABELS: Record<string, { label: string; open: boolean }> = {
  open: { label: 'مفتوحة', open: true },
  departed: { label: 'مغلقة', open: false },
}

export function visitStatus(status: string | null | undefined): { label: string; open: boolean } {
  const key = String(status ?? '').trim().toLowerCase()
  return STATUS_LABELS[key] ?? { label: 'غير معروفة', open: false }
}

export function visitDateTime(value: string | null | undefined): string {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const map: Record<string, string> = {}
  const fmt = new Intl.DateTimeFormat('en-US', {
    timeZone: 'Asia/Baghdad',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
  })
  for (const p of fmt.formatToParts(d)) map[p.type] = p.value
  const hour = map.hour === '00' ? '12' : map.hour
  const suffix = (map.dayPeriod ?? '').toUpperCase() === 'PM' ? 'م' : 'ص'
  return `${map.year}/${map.month}/${map.day} ${hour}:${map.minute} ${suffix}`
}

export function visitBranchLabel(visit: Pick<VisitHistoryItem, 'branchId' | 'branchName'>, fallback?: string | null): string {
  const name = (visit.branchName ?? fallback ?? '').trim()
  return name === '' ? 'فرع غير معروف' : name
}

export function visitMembersLine(members: { displayName: string }[]): string {
  const names = members.map((m) => (m.displayName ?? '').trim()).filter((n) => n !== '')
  return names.length === 0 ? '—' : names.join('، ')
}
