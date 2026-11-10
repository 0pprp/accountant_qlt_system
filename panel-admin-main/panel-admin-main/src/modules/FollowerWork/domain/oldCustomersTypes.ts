export interface FollowerOldCustomerRow {
  id: number
  customerId: number | null
  salesRequestId: number | null
  customerName: string
  phone: string | null
  address: string | null
  status: string
  statusLabel: string
  note: string | null
  completionNote: string | null
  createdAtUtc: string
  completedAtUtc: string | null
  isDone: boolean
  canCancel: boolean
  rowVersion: number
}

export interface FollowerOldCustomersResponse {
  items: FollowerOldCustomerRow[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  definitionHint: string
  timeZoneId: string
  generatedAtUtc: string
  followerId: number
  followerName: string | null
}
