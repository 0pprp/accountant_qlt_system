export enum OrderExecutionStatus {
  NotStarted = 0,
  InProgress = 1,
  Completed = 2,
}

export enum OrderStep {
  Attachments = 0,
  SellerInfo = 1,
  Completed = 2,
}

export enum OrderApprovalStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
}

export interface Order {
  id: number
  buyAmount: number
  createdAt: string
  customerFullName: string
  customerNationalCode: string
  customerPhoneNumber: string
  dailyInstallmentAmount: number
  installmentsCount: number
  lastInstallmentDate: string
  orderListName: string
  prepaymentAmount: number
  productsSummary: string
  remainingAmount: number
  saleDate: string
  sellAmount: number
  sellerName: string
  executionStatus?: OrderExecutionStatus
  approvalStatus?: OrderApprovalStatus
  totalInstallmentsAmount: number
}

export interface Province {
  name: string
  id: number
}

export interface OrderForm {
  name: string
  mandobId: number
  motabaId: number
  branchId: number
}

export interface OrderWarehouses {
  id: number
  name: string
}

export interface OrderInstallmentPayment {
  id: number | null
  date: string
  amount: number | null
  description: string | null
  hasPayment: boolean
}
