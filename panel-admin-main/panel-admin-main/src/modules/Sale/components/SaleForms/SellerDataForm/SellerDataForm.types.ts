import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'

export interface Props {
  creationAddress?: string
  saleDate?: string | null
  saleTime?: string | null
  sellerId?: number
  orderListId?: number

  sellerFormMode: FormMode
}
