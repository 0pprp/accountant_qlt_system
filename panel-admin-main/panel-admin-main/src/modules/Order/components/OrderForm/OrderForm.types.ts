import type { Order, OrderForm } from '../../types/model'
import type { FormsState } from '@/modules/Core/types/model/forms'

export interface Props {
  selectedItem?: Order
  formState: FormsState
  loading: boolean
}

export interface Emits {
  (event: 'submit', values: OrderForm): void
}
