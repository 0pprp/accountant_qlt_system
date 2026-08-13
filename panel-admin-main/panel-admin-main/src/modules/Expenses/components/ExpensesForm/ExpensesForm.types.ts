import type { ExpensesCreatePayload } from '../../types/api'
import type { SingleExpense } from '../../types/model'
import type { FormsState } from '@/modules/Core/types/model/forms'

export interface Props {
  formState: FormsState
  loading?: boolean
  initialValues?: SingleExpense | undefined
}

export interface Emits {
  (event: 'submit', value: ExpensesCreatePayload): void
  (event: 'cancel'): void
}
