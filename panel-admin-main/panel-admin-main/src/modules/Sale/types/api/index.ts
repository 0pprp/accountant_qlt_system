import type {
  Sale,
  SaleForm,
  SaleFormWithFiles,
  SaleInfo,
  SellerInfoForm,
} from '../model'

export interface AvailableColumn {
  key: string
  displayName: string
  dataType: string
}

export type AvailableColumnsResponse = Array<AvailableColumn>

export type SaleListServerSuccessResponse = {
  paginatedOrders: ServerSuccessPaginatedResponse<Array<Sale>>
  totalBuyAmount: number
  totalSellAmount: number
}

export type SaleCreatePayload = SaleFormWithFiles

export type SaleUpdatePayload = SaleForm

export type SellerInfoPayload = SellerInfoForm

export type ChangeOrderApprovalStatusPayload = {
  approvalStatus: number
}

export type SaleByIdServerSuccessResponse = SaleInfo
