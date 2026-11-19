/** مرايا لِـ ArchiveStage في الخادم — الجسم يرسل أرقاماً لأن الـAPI بلا محوّل تعداد نصّي. */
export const ArchiveStage = {
  SalesEmployee: 0,
  BranchManagement: 1,
  Follower: 2,
  Archive: 3,
} as const

export type ArchiveStageValue = (typeof ArchiveStage)[keyof typeof ArchiveStage]

export const ARCHIVE_STAGE_LABEL: Record<number, string> = {
  0: 'موظف المبيعات',
  1: 'إدارة الفرع',
  2: 'المتابع',
  3: 'الأرشيف',
}

export const ArchiveTransactionStatus = {
  WithSalesEmployee: 0,
  SentToBranch: 1,
  AtBranch: 2,
  SentToFollower: 3,
  AtFollower: 4,
  SentToArchive: 5,
  Archived: 6,
} as const

export interface ArchiveBundleListItem {
  id: number
  branchId: number
  branchName?: string | null
  salesEmployeeUserId: number
  salesEmployeeName: string
  periodYear: number
  periodMonth: number
  periodIndex: number
  periodStart: string
  periodEnd: string
  periodLabel: string
  isPeriodClosed: boolean
  canSend: boolean
  pendingSendCount: number
  itemCount: number
  withSalesEmployeeCount: number
  sentToBranchCount: number
  atBranchCount: number
  sentToFollowerCount: number
  atFollowerCount: number
  sentToArchiveCount: number
  archivedCount: number
  voidedCount: number
  openReturnCount: number
  receivedOnwardCount: number
  receivedByMeCount: number
  firstSentAtUtc?: string | null
  lastArchivedAtUtc?: string | null
  concurrencyStamp: number
}

export interface ArchiveTransactionRow {
  id: number
  orderId: number
  customerId: number
  customerName: string
  customerPhone?: string | null
  periodSourceDate: string
  status: number
  statusLabel: string
  holdingStage: number
  isInTransit: boolean
  isVoided: boolean
  voidReason?: string | null
  hasOpenReturn: boolean
  returnCount: number
  lastReturnReason?: string | null
  followerUserId?: number | null
  followerName?: string | null
  concurrencyStamp: number
  sentToBranchAtUtc?: string | null
  receivedByBranchAtUtc?: string | null
  sentToFollowerAtUtc?: string | null
  receivedByFollowerAtUtc?: string | null
  sentToArchiveAtUtc?: string | null
  archivedAtUtc?: string | null
  awaitsMyReceipt: boolean
}

export interface ArchiveBundleDetail {
  bundle: ArchiveBundleListItem
  transactions: ArchiveTransactionRow[]
  myStage?: number | null
  canSend: boolean
  canReceive: boolean
  canReturn: boolean
  canUndoSend: boolean
}

export interface ArchiveActionResult {
  bundleId: number
  affectedCount: number
  skippedCount: number
  message: string
}

export interface ArchiveFlowRow {
  bundleId: number
  salesEmployeeName: string
  branchName?: string | null
  periodLabel: string
  itemCount: number
  archivedCount: number
  openReturnCount: number
  whereLabel: string
  stuckAtStage?: number | null
  daysSinceFirstSend?: number | null
  averageHoursSalesToBranch?: number | null
  averageHoursBranchToFollower?: number | null
  averageHoursFollowerToArchive?: number | null
}
