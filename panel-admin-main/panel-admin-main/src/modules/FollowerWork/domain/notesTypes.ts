export interface FollowerNoteRow {
  noteId: number
  customerId: number
  customerName: string
  phone: string | null
  listId: number | null
  listName: string | null
  noteText: string
  authorName: string | null
  authorUserId: number | null
  createdAtUtc: string
  source: string
}

export interface FollowerNotesResponse {
  items: FollowerNoteRow[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  timeZoneId: string
  generatedAtUtc: string
  followerId: number
  followerName: string | null
}
