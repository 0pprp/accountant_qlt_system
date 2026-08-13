export interface ExpenseItemModel {
  id?: number
  quantity: number | null
  amount: number
  name: string
}

// export interface ExpenseItemFormModel {
//   quantity: number
//   amount: number
//   name: string
// }

export interface SingleExpense {
  id: number
  factorNumber: string
  safeType: SafeType
  totalAmount: number
  expenseItemsCount: number
  expenseItems: Array<ExpenseItemModel>
  createdAt: string
  attachments: Array<ExpenseAttachment>
}

export interface ExpenseAttachment {
  id: number
  originalFileName: string
  relativePath: string
  fileSizeInByte: number
  type: number
}

export enum SafeType {
  Branch,
  Main,
}
export interface ExpensesForm {
  factorNumber?: number
  safeType?: SafeType
  branchId?: number
  expenseItems: Array<ExpenseItemModel>
  attachments?: File
}

export interface LastFactorNumberResponse {
  lastFactorNumber: number
}
