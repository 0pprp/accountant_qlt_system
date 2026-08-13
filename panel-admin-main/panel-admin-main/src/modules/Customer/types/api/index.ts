import type {
  Business,
  Customer,
  CustomerOrder,
  OrderSummary,
  SingleCustomer,
} from '../model'

export type CustomerListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<Customer>
>

export type CustomerByIdServerSuccessResponse = SingleCustomer

export type CustomerCreatePayload = {
  hasError?: boolean
  fullName: string
  motherName: string
  nationalCode: string
  birthDate: string | null
  phoneNumber: string
  whatsAppPhoneNumber: string
  business: Business
  branchId: number
}

export type CustomerOrdersResponse = Array<CustomerOrder>

export type OrderSummaryResponse = OrderSummary
