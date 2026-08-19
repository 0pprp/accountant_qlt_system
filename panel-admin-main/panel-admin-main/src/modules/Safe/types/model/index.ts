export enum TransactionType {
  SellerPayment,
  Purchase,
  Expense,
  SafeTransfer,
}
export enum TransactionStatus {
  Pending,
  Approved,
  Rejected,
}

export enum TransactionDirection {
  In,
  Out,
}

export interface SafeCurrent {
  id: number
  name: string
  remainingCashAmount: number
  netBalance: number
  totalAmount: number
  totalUndeliveredCashAmount: number | null
  totalBranchesRemainingCashAmount: number | null
  branchId: number | null
}

export interface Safe {
  id: number | null
  name: string
  branchId: number | null
}

export interface SafeSellers {
  id: number
  fullName: string
  roleName: string | null
  orderListName: string | null
  deliveredCashAmount: number
  undeliveredCashAmount: number
  lastCashDeliveryDate: string
  lastCashDeliveryDescription: string | null
}

export interface SafeTransactions {
  id: number
  source: string
  destination: string
  amount: number
  type: TransactionType
  status: TransactionStatus
  statusDescription: string | null
  direction: TransactionDirection
  createdAt: string
}

export interface ChangeStatusForm {
  status: TransactionStatus
  statusDescription: null | string
}

export interface SomeSellersCashDeliveriesForm {
  sellerIds: Array<number>
}

export interface SellerCashDeliveriesForm {
  amount: number
  date: string
  description: null | string
}
export interface TransferForm {
  amount: number
  sourceSafeId: number | null
  destinationSafeId: number
  description: string | null
}
