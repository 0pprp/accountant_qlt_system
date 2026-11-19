/** نوع الخزينة — يطابق Domain.Entities.SafeAggregate.Enums.SafeKind */
export enum SafeKind {
  Branch = 0,
  Expenses = 1,
  OrderList = 2,
  Employee = 3,
  Misc = 4,
}

/** نوع الحركة — يطابق Domain.Entities.SafeAggregate.Enums.SafeMovementKind */
export enum SafeMovementKind {
  Opening = 0,
  Deposit = 1,
  Withdrawal = 2,
  TransferIn = 3,
  TransferOut = 4,
  Collection = 5,
  Prepayment = 6,
  ExpenseOut = 7,
  PurchaseOut = 8,
  Refund = 9,
}

export interface Safe {
  id: number
  name: string
  kind: SafeKind
  branchId: number
  orderListId: number | null
  orderListName: string | null
  userId: number | null
  userFullName: string | null
  balance: number
  isSystem: boolean
  isActive: boolean
  movementsCount: number
}

export interface SafeMovement {
  id: number
  kind: SafeMovementKind
  /** موقّع: موجب داخل، سالب خارج */
  amount: number
  balanceAfter: number
  note: string | null
  counterpartySafeId: number | null
  counterpartySafeName: string | null
  performedByUserId: number
  performedByFullName: string | null
  createdAt: string | null
  installmentPaymentId: number | null
  customerFullName: string | null
}

export interface BackfillPaymentCollectionsResult {
  dryRun: boolean
  count: number
  amount: number
  lists: { orderListId: number; orderListName: string; count: number; amount: number }[]
  skippedImported: number
  skippedNotEnteredAsPayment: number
  alreadyPosted: number
}

export interface SafeVerification {
  safeId: number
  safeName: string
  storedBalance: number
  ledgerBalance: number
  difference: number
  movementsCount: number
  isBalanced: boolean
  inconsistentSnapshots: number
}

export interface SafeAmountForm {
  amount: number
  note: string | null
  operationKey: string
}

export interface SafeTransferForm {
  sourceSafeId: number
  destinationSafeId: number
  amount: number
  note: string | null
  operationKey: string
}

export interface CreateSafeForm {
  branchId: number
  orderListId: number | null
  userId: number | null
}
