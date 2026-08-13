import type { FormsState } from '@/modules/Core/types/model/forms'
import type { PurchaseItemModel } from '@/modules/Purchases/types/model'

export interface Props {
  purchaseItems: Array<PurchaseItemModel>
  formState: FormsState
}

export interface Emits {
  (event: 'update', value: Array<PurchaseItemModel>): void
}
