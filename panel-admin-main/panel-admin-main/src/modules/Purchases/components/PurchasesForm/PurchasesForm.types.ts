import type { PurchasesCreatePayload } from '../../types/api'
import type { SinglePurchase } from '../../types/model'
import type { FormsState } from '@/modules/Core/types/model/forms'

export interface Props {
  formState: FormsState
  loading?: boolean
  initialValues?: SinglePurchase | undefined
}

export interface Emits {
  (event: 'submit', value: PurchasesCreatePayload): void
  (event: 'cancel'): void
}
