import type { AttachmentForm } from '@/modules/Core/types/model/attachment'
import type {
  OrderApprovalStatus,
  OrderExecutionStatus,
  OrderStep,
} from '@/modules/Order/types/model'
import type { Attachment } from '@/modules/User/types/model'

export interface Sale extends Record<string, unknown> {
  id: number
}

export interface Business {
  name: string
  address: string
  nearestKnownLocation: string
}

export interface CustomerForm {
  hasError?: boolean
  fullName: string
  motherName: string
  nationalCode: string
  birthDate: string
  phoneNumber: string
  whatsAppPhoneNumber: string
  business: Business
  branchId: number
}

export enum ProductType {
  Warehouse,
  Foreign,
}

export enum CreateSaleSteps {
  customerData,
  customerDocuments,
  installmentDetails,
  sellerData,
}

export interface OrderItem {
  productType: ProductType
  productName: string | null
  productId: number | null
  quantity: number
  buyAmount: number
  sellAmount: number
  prepaymentAmount: number
  dailyInstallmentAmount: number
}

export interface SaleForm {
  hasError?: boolean
  orderItems: Array<OrderItem>
}

export interface SaleFormWithFiles extends SaleForm {
  attachments: Array<AttachmentForm>
  customerId: number
  branchId: number
  //
  creationAddress?: string
  saleDate?: string
  saleTime?: string
  sellerId?: number
  orderListId?: number
}

export interface SellerInfoForm {
  hasError: boolean
  creationAddress: string
  saleDate?: string | undefined
  saleTime?: string | undefined
  sellerId: number
  orderListId: number
}

export interface SaleInfo {
  orderInfo: OrderInfo
  sellerInfo: SellerInfo
  step?: OrderStep
  approvalStatus?: OrderApprovalStatus
  executionStatus?: OrderExecutionStatus
}

export interface SellerInfo {
  seller: {
    id: number
    name: string
  }
  orderList: {
    id: number
    name: string
  }
  creationAddress: string
  saleDate: string
  saleTime: string
}

export interface OrderInfo {
  id: number
  customerId?: number
  buyAmount: number
  sellAmount: number
  prepaymentAmount: number
  dailyInstallmentAmount: number
  attachments: Array<Attachment>
  orderItems: Array<OrderItem>
  step?: OrderStep
  approvalStatus?: OrderApprovalStatus
  executionStatus?: OrderExecutionStatus
}
