export interface FollowerDecliningRow {
  customerId: number
  customerName: string
  phone: string | null
  listId: number
  listName: string | null
  fromLabel: string
  toLabel: string
  ratingLabel: string | null
  fromScore: number
  toScore: number
  daysBetween: number
  priorCaptureDate: string
  currentCaptureDate: string
  /** null = ملاحظات غير منفَّذة (دفعة08) */
  hasNote: boolean | null
  lastNoteAtUtc: string | null
  hasSaleAuth: boolean
}

export interface FollowerDecliningResponse {
  items: FollowerDecliningRow[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  lookbackRule: string
  lookbackDays: number
  minScoreDrop: number
  targetTransitions: string[]
  comparisonMode: string
  notesAvailability: string
  timeZoneId: string
  asOfLocalDate: string
  generatedAtUtc: string
  followerName: string | null
  followerId: number
}

export function scoreDelta(row: FollowerDecliningRow): number {
  return row.toScore - row.fromScore
}

export function declineDirectionLabel(row: FollowerDecliningRow): string {
  const d = scoreDelta(row)
  if (d < 0) return 'تراجع'
  if (d > 0) return 'تحسّن'
  return 'بدون تغير'
}
