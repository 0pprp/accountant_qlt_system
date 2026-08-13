import type {
  PurchasesForm,
  SinglePurchase,
  LastFactorNumberResponse,
} from '../model'

export type PurchasesListServerSuccessResponse = ServerSuccessPaginatedResponse<
  Array<SinglePurchase>
>

export type PurchaseByIdServerSuccessResponse = SinglePurchase

export type PurchasesCreatePayload = PurchasesForm

export type LastFactorNumberSuccessResponse = LastFactorNumberResponse
