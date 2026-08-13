import type { FormsState } from '@/modules/Core/types/model/forms'
import type { PurchaseAttachment } from '@/modules/Purchases/types/model'

export interface Props {
  formMode?: FormsState
  file?: PurchaseAttachment[]
  disabled?: boolean
}

export interface Emits {
  (event: 'uploadFile', files: File[]): void
}
