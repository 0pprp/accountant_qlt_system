import type { FormMode } from '../../UserTabs/UserTabs.types'
import type { RolePermission } from '@/modules/User/types/model'
import type { TabAccessChange } from '@/modules/User/types/tabAccess'

export interface Props {
  roleId: number
  userId?: number | null
  selectedPermissions?: Array<RolePermission>
  userFormMode?: FormMode
  /** عند الإنشاء بعد نجاح الحساب — إظهار تنبيه إعادة الحفظ */
  createRetryNotice?: string | null
}

export type PowersFormExpose = {
  /** توافق المسار السابق — فارغ دائمًا (لم يعد يُستخدم CRUD) */
  permissionIds: number[]
  permissionsVersion: number
  getChanges: () => TabAccessChange[]
  dirtyCount: number
  exceptionCount: number
  reloadFromServer: () => Promise<void>
  compareWithServer: () => Promise<void>
  markConflict: (message?: string) => void
  hasConflictDraft: boolean
  hasPendingServerAdoption: boolean
  unresolvedConflictCount: number
  canSaveOverrides: boolean
  baselineVersion: number
  serverLatestVersion: number | null
}
