import type { Product, ProductForm } from '../../types/model'
import type { FormsState } from '@/modules/Core/types/model/forms'

export interface Props {
  selectedProduct?: Product
  formState: FormsState
  categoryId?: number
  warehouseId?: number
  loading: boolean
}

export interface Emits {
  (event: 'submit', values: ProductForm): void
}
