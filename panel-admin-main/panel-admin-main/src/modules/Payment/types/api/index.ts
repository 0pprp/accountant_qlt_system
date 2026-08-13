import type { OrderListReport, Payment, PaymentUpdateForm } from '../model'

export type OrderListReportServerSuccessResponse = {
  items: ServerSuccessPaginatedResponse<Array<OrderListReport>>
  totalInstallmentPaymentCount: number
  totalCollectedAmount: number
}

export type PaymentListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<Payment>
>

export type PaymentUpdatePayload = PaymentUpdateForm

export interface PaymentCreatePayload {
  orderId: number
  date: string
  amount: number
  description: string | null
}
