export interface OrderListReport {
  id: number
  name: string
  installmentPaymentsCount: number
  totalPaidInstallmentsAmount: number
  mandob?: {
    id: number
    fullName: string
  }
  customersCount?: number
  createdAt: string
}

export interface Payment {
  id: number
  customerFullName: string
  productName: string
  date: string
  dailyInstallmentAmount: number
  sellerFullName: string
  amount: string
  description: string | null
}

export interface PaymentUpdateForm {
  amount: number
  description: string | null
}
