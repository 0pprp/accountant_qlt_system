import type { FormMode } from '../../UserTabs/UserTabs.types'
import type { SalaryType } from '@/modules/User/types/model'

export interface Props {
  type?: SalaryType
  amount?: number
  installmentSharePercent?: number | null
  saleSharePercent?: number | null

  userFormMode?: FormMode
}
