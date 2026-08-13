import type { OrderItem } from './../../../../types/model/index'
import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'

export interface Props {
  formState: FormMode
  orderItems?: Array<OrderItem>
}
