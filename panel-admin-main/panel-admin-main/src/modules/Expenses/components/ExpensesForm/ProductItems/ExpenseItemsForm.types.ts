import type { FormsState } from '@/modules/Core/types/model/forms'
import type { ExpenseItemModel } from '@/modules/Expenses/types/model'

export interface Props {
  expenseItems: Array<ExpenseItemModel>
  formState: FormsState
}

export interface Emits {
  (event: 'update', value: Array<ExpenseItemModel>): void
}
