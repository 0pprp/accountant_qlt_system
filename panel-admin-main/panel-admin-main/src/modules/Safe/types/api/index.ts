import type {
  ChangeStatusForm,
  SellerCashDeliveriesForm,
  SomeSellersCashDeliveriesForm,
  Safe,
  SafeSellers,
  SafeTransactions,
  TransferForm,
  SafeCurrent,
} from '../model'

export type SafeSellersListServerSuccessResponse =
  ServerSuccessPaginatedResponse<Array<SafeSellers>>

export type SafeServerSuccessResponse = Array<Safe>

export type SafeCurrentServerSuccessResponse = SafeCurrent

export type SafeTransactionsListServerSuccessResponse =
  ServerSuccessPaginatedResponse<Array<SafeTransactions>>

export type ChangeStatusPayload = ChangeStatusForm

export type CreateSomeSellersCashDeliveriesPayload =
  SomeSellersCashDeliveriesForm

export type CreateSellerCashDeliveriesPayload = SellerCashDeliveriesForm

export type CreateTransferFormPayload = TransferForm
