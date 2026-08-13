import type {
  ActivityType,
  DeviceType,
  TargetEntityType,
} from '../../constants/enums'

export interface ActivityLogListItem {
  id: number
  activityType: ActivityType
  description: string
  userName: string
  userRoles: string
  targetEntityType: TargetEntityType
  targetEntityId: number | null
  createdAt: string
}

export interface ActivityLogDetail extends ActivityLogListItem {
  ipAddress: string | null
  userAgent: string | null
  deviceType: DeviceType
  browser: string | null
  operatingSystem: string | null
  branchId: number
  userId: number
}
