import type { FormMode } from '../../UserTabs/UserTabs.types'
import type { RolePermission } from '@/modules/User/types/model'

export interface Props {
  roleId: number
  selectedPermissions?: Array<RolePermission>
  userFormMode?: FormMode
}
