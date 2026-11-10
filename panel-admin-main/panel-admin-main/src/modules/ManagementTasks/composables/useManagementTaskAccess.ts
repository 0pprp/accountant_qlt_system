import { computed } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useManagementTaskCurrentUserId } from './useManagementTaskCurrentUserId'
import {
  ManagementTaskStatus,
  type ManagementTaskListItem,
} from '../domain/types'

export interface ManagementTaskAvailableActions {
  accept: boolean
  reject: boolean
  complete: boolean
  transfer: boolean
  confirm: boolean
  resend: boolean
}

/**
 * صلاحيات Task.* + قواعد الحالة. عند عدم معرفة المستخدم الحالي يُكتفى بالصلاحية والحالة
 * والخادم يرفض أي إجراء لغير الحامل.
 */
export function useManagementTaskAccess() {
  const { can } = usePermission()
  const currentUserId = useManagementTaskCurrentUserId()

  const canRead = computed(() => can('Task', 'Read'))
  const canCreate = computed(() => can('Task', 'Create'))
  const canAssign = computed(() => can('Task', 'Assign'))
  const canReadAttachment = computed(() => can('Task', 'ReadAttachment'))

  /** يظهر زر «مهامي» إن وُجدت أي صلاحية إجراء */
  const canUseInbox = computed(
    () =>
      can('Task', 'Accept') ||
      can('Task', 'Reject') ||
      can('Task', 'Complete') ||
      can('Task', 'Confirm') ||
      can('Task', 'Transfer')
  )

  function isHolder(task: ManagementTaskListItem): boolean {
    const me = currentUserId.value
    return me == null || task.currentHolderUserId === me
  }

  function isAssignee(task: ManagementTaskListItem): boolean {
    const me = currentUserId.value
    return me == null || task.assigneeUserId === me
  }

  function actionsFor(task: ManagementTaskListItem): ManagementTaskAvailableActions {
    const s = task.status
    const pending =
      s === ManagementTaskStatus.Sent || s === ManagementTaskStatus.Transferred
    const holder = isHolder(task)
    return {
      accept: pending && holder && can('Task', 'Accept'),
      reject: pending && holder && can('Task', 'Reject'),
      complete:
        s === ManagementTaskStatus.Accepted && holder && can('Task', 'Complete'),
      transfer:
        s === ManagementTaskStatus.Accepted &&
        holder &&
        isAssignee(task) &&
        can('Task', 'Transfer'),
      confirm:
        s === ManagementTaskStatus.AwaitingDelegatedConfirm &&
        isAssignee(task) &&
        can('Task', 'Confirm'),
      resend: s === ManagementTaskStatus.Returned && can('Task', 'Assign'),
    }
  }

  return {
    currentUserId,
    canRead,
    canCreate,
    canAssign,
    canReadAttachment,
    canUseInbox,
    actionsFor,
  }
}
