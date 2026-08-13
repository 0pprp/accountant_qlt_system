import type { NotificationActionType } from '../constants/enums'

export interface Notification {
  id: number
  title: string
  description: string
  actionType: NotificationActionType
  actorUserFullName: string
  createdAt: string
  hasRead: boolean
}
