import type { Order, OrderForm, OrderInstallmentPayment } from '../model'
import type { Warehouse } from '@/modules/Warehouse/types/model'

export type OrderCreatePayload = OrderForm

export type OrderListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<Order>
>

export type OrderWarehousesServerSuccessResponse = Warehouse

export type OrderInstallmentPaymentsResponse = Array<OrderInstallmentPayment>
