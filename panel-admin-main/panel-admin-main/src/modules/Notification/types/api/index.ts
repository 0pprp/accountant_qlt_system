import type { Notification } from '../model'

export type NotificationListServerSuccessResponse =
  ServerSuccessPaginatedResponse<Array<Notification>>

export interface NotificationUnreadCountResponse {
  unreadCount: number
}

export interface MarkAllNotificationsReadResponse {
  updatedCount: number
}

export interface NotificationListParams {
  pageIndex: number
  pageSize: number
  branchId: number
  hasRead?: boolean | null
  actionType?: number | null
  searchTerm?: string | null
  startDate?: string | null
  endDate?: string | null
  sortCriteria?: Array<{ property: string; direction: number }> | null
}
