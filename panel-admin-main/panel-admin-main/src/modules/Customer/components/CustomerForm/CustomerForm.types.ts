import type { CustomerCreatePayload } from '@/modules/Customer/types/api'
import type { CustomerForm } from '@/modules/Customer/types/model'

export interface Props {
  isEditMode: boolean
  loading?: boolean
  initialValues?: CustomerForm
}

export interface Emits {
  (event: 'submit', value: CustomerCreatePayload): void
}
