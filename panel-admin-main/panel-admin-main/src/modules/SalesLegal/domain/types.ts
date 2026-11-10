export interface LegalFollowupDocument {
  id: number
  documentType: string
  typeLabel: string
  fileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
}

export interface LegalFollowupCaseRow {
  id: number
  salesRequestId?: number | null
  customerId?: number | null
  customerName: string
  customerPhone?: string | null
  customerAddress?: string | null
  followerUserId: number
  followerName?: string | null
  status: string
  statusLabel: string
  note?: string | null
  completionNote?: string | null
  createdAtUtc: string
  completedAtUtc?: string | null
  rowVersion: number
  canCancel: boolean
  branchId?: number | null
  shopLatitude?: number | null
  shopLongitude?: number | null
  documents?: LegalFollowupDocument[] | null
}

export interface ListLegalFollowupsResponse {
  items: LegalFollowupCaseRow[]
  totalCount: number
  page: number
  pageSize: number
  branchScope: number[]
}

export interface LegalFollowupFollowerOption {
  followerUserId: number
  followerName: string
  branchId: number
}
