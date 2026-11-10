export interface AppNoteSummary {
  id: number
  sourceApp: string
  sourceType: string | null
  senderDisplayName: string
  senderRole: string
  branchId: number | null
  branchName: string | null
  subject: string
  bodyPreview: string
  status: string
  isUnread: boolean
  createdAtUtc: string
  readAtUtc: string | null
  expectedVersion: number
}

export interface AppNoteDetail extends AppNoteSummary {
  senderUserId: number
  body: string
}

export interface AppNoteListResponse {
  items: AppNoteSummary[]
  totalCount: number
  page: number
  pageSize: number
  unreadCount: number
}
