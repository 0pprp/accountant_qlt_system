import type { Attachment } from '@/modules/User/types/model'
import type { Warehouse } from '@/modules/Warehouse/types/model'

export enum CustomerUpsertTabSteps {
  CustomerForm,
  CustomerDocuments,
  CustomerOrders,
}

export interface Customer {
  id: number
  fullName: string
  businessName: string
  businessAddress: string
  orderListName: string | null
  ordersCount: number
  lastInstallmentPaymentDate: string | null
}

export interface CustomerForm {
  hasError?: boolean
  fullName: string
  motherName: string
  nationalCode: string
  birthDate: string | null
  phoneNumber: string
  whatsAppPhoneNumber: string
  branchId: number
  business: {
    name: string
    address: string
    nearestKnownLocation: string
  }
}

export interface Business {
  name: string
  address: string
  nearestKnownLocation: string
}

export interface SingleCustomer {
  id: number
  fullName: string
  motherName: string
  nationalCode: string
  birthDate: string
  phoneNumber: string
  whatsAppPhoneNumber: string
  branch: Warehouse
  business: Business
  attachments: Array<Attachment>
}

export interface CustomerOrder {
  id: number
  sellAmount: number
  sellerFullName: string
  orderListName: string
  orderItems: Array<OrderItem>
  createdAt: string
}

export interface OrderItem {
  id: number
  productName: string
  productType: number
  quantity: number
  buyAmount: number
  sellAmount: number
}

export interface OrderSummary {
  id: number
  buyAmount: number
  sellAmount: number
  paidAmount: number
  overdueAmount: number
  dailyInstallmentAmount: number
  unpaidAmount: number
}
