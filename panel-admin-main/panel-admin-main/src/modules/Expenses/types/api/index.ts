import type {
  ExpensesForm,
  SingleExpense,
  LastFactorNumberResponse,
} from '../model'

export type ExpensesListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<SingleExpense>
>

export type ExpenseByIdServerSuccessResponse = SingleExpense

export type ExpensesCreatePayload = ExpensesForm

export type LastFactorNumberSuccessResponse = LastFactorNumberResponse
