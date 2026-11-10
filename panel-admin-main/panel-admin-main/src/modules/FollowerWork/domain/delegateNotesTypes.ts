export interface FollowerDelegateNoteRow {
  noteId: number
  orderListId: number
  listName: string | null
  mandobUserId: number
  mandobName: string | null
  noteText: string
  authorUserId: number
  authorName: string | null
  authorRole: string
  createdAtUtc: string
  source: string
}

export interface FollowerDelegateNotesResponse {
  items: FollowerDelegateNoteRow[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  timeZoneId: string
  generatedAtUtc: string
  followerId: number
  followerName: string | null
}
