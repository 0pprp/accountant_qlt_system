import type { FormMode } from '../../UserTabs/UserTabs.types'
import type { RolePermission } from '@/modules/User/types/model'
import type { PermissionOverrideChange } from '@/modules/User/types/permissionOverrides'

export interface Props {
  roleId: number
  userId?: number | null
  selectedPermissions?: Array<RolePermission>
  userFormMode?: FormMode
}

export type PowersFormExpose = {
  /** توافق المسار السابق — معرفات الفعّالة الظاهرة */
  permissionIds: number[]
  permissionsVersion: number
  getChanges: () => PermissionOverrideChange[]
  dirtyCount: number
  exceptionCount: number
  reloadFromServer: () => Promise<void>
  hasConflictDraft: boolean
  hasPendingServerAdoption: boolean
  unresolvedConflictCount: number
  /** false عند وجود serverLatest غير معتمد أو تعارضات غير محسومة */
  canSaveOverrides: boolean
}
