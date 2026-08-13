import type { ActivityLogDetail, ActivityLogListItem } from '../model'
import type { ActivityType, TargetEntityType } from '../../constants/enums'

export interface ActivityLogFilterParams {
  branchId: number
  searchTerm?: string | null
  startDate?: string | null
  endDate?: string | null
  activityType?: ActivityType | null
  targetEntityType?: TargetEntityType | null
  userId?: number | null
}

export type ActivityLogListServerSuccessResponse =
  ServerSuccessPaginatedResponse<Array<ActivityLogListItem>>

export type ActivityLogByIdServerSuccessResponse = ActivityLogDetail
