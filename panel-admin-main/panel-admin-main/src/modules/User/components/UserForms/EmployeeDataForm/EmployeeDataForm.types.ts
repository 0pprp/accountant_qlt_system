import type { FormMode } from '../../UserTabs/UserTabs.types'

export interface Props {
  fullName?: string
  motherName?: string
  nationalCode?: string
  birthDate?: string
  roleId?: number
  branchIds?: Array<number>
  address?: string
  phoneNumber?: string

  userFormMode?: FormMode
}
