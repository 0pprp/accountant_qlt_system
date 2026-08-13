import type { OrderItem } from '@/modules/Sale/types/model'
import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  orderItems?: Array<OrderItem>
  customerId?: number
  attachments?: Array<Attachment>
  saleFormMode: FormMode
}
