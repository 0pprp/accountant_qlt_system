/** ByteEnum — يطابق ManagementTaskStatus في الخادم (ليست حالات طلبات البيع) */
export const ManagementTaskStatus = {
  Sent: 0,
  Transferred: 1,
  Accepted: 2,
  Returned: 3,
  AwaitingDelegatedConfirm: 4,
  Completed: 5,
} as const

export type ManagementTaskStatusValue =
  (typeof ManagementTaskStatus)[keyof typeof ManagementTaskStatus]

export const MANAGEMENT_TASK_STATUS_ORDER: readonly ManagementTaskStatusValue[] = [
  ManagementTaskStatus.Sent,
  ManagementTaskStatus.Transferred,
  ManagementTaskStatus.Accepted,
  ManagementTaskStatus.Returned,
  ManagementTaskStatus.AwaitingDelegatedConfirm,
  ManagementTaskStatus.Completed,
]

/** مفاتيح الترجمة ضمن managementTasks.status.* */
export const MANAGEMENT_TASK_STATUS_KEYS: Record<ManagementTaskStatusValue, string> = {
  0: 'sent',
  1: 'transferred',
  2: 'accepted',
  3: 'returned',
  4: 'awaitingDelegatedConfirm',
  5: 'completed',
}

export const ManagementTaskPriority = {
  High: 1,
  Medium: 2,
  Low: 3,
} as const

export type ManagementTaskPriorityValue =
  (typeof ManagementTaskPriority)[keyof typeof ManagementTaskPriority]

export const MANAGEMENT_TASK_PRIORITY_KEYS: Record<ManagementTaskPriorityValue, string> = {
  1: 'high',
  2: 'medium',
  3: 'low',
}

/** ManagementTaskEventType */
export const MANAGEMENT_TASK_EVENT_KEYS: Record<number, string> = {
  1: 'created',
  2: 'updated',
  3: 'accepted',
  4: 'rejected',
  5: 'transferred',
  6: 'completed',
  7: 'confirmed',
}

export interface ManagementTaskListItem {
  id: number
  title: string
  description: string
  status: ManagementTaskStatusValue
  priority: ManagementTaskPriorityValue
  dueDate: string
  dueAtUtc: string | null
  isOverdue: boolean
  branchId: number
  branchName: string | null
  createdByUserId: number
  createdByDisplayName: string | null
  assigneeUserId: number
  assigneeDisplayName: string
  currentHolderUserId: number
  currentHolderDisplayName: string
  rejectNote: string | null
  customerId: number | null
  orderId: number | null
  expectedVersion: number
  createdAt: string | null
  statusChangedAtUtc: string | null
  transferredAtUtc: string | null
  branchCompletedAtUtc: string | null
  assigneeCompletedAtUtc: string | null
  confirmedAtUtc: string | null
}

export interface ManagementTaskAttachment {
  id: number
  fileName: string
  contentType: string
  sizeBytes: number
  uploadedAtUtc: string
  uploadedByUserId: number
}

export interface ManagementTaskEvent {
  id: number
  eventType: number
  atUtc: string
  actorUserId: number | null
  actorDisplayName: string | null
  targetUserId: number | null
  targetDisplayName: string | null
  note: string | null
  previousStatus: ManagementTaskStatusValue | null
  newStatus: ManagementTaskStatusValue | null
}

export interface ManagementTaskDetail {
  task: ManagementTaskListItem
  events: ManagementTaskEvent[]
  attachments: ManagementTaskAttachment[]
}

export interface ManagementTaskActionResult {
  id: number
  status: ManagementTaskStatusValue
  expectedVersion: number
}

export interface ManagementTaskAssigneeOption {
  userId: number
  displayName: string
  username: string
  branchIds: number[]
}

export interface ManagementTaskPaginated<T> {
  items: T[]
  totalCount: number
  pageIndex: number
  pageSize: number
}

export interface ManagementTaskListFilters {
  branchId: number | null
  status: ManagementTaskStatusValue | null
  search: string
  inboxForMe: boolean
}

export interface CreateManagementTaskPayload {
  title: string
  description: string
  priority: ManagementTaskPriorityValue
  dueDate: string
  dueAtUtc?: string | null
  branchId: number
  assigneeUserId: number
  customerId?: number | null
  orderId?: number | null
}

export interface ResendManagementTaskPayload {
  expectedVersion: number
  title: string
  description: string
  priority: ManagementTaskPriorityValue
  dueDate: string
  dueAtUtc?: string | null
  assigneeUserId: number
  customerId?: number | null
  orderId?: number | null
}

export type ManagementTaskSimpleAction = 'accept' | 'complete' | 'confirm'
