import type { Safe, SafeMovement, SafeVerification } from '../model'

export type SafesServerSuccessResponse = Array<Safe>
export type SafeMovementsServerSuccessResponse = SafeMovement
export type SafeVerificationServerSuccessResponse = SafeVerification

export interface SafeAmountPayload {
  amount: number
  note: string | null
  operationKey: string
}

export interface SafeTransferPayload {
  sourceSafeId: number
  destinationSafeId: number
  amount: number
  note: string | null
  operationKey: string
}

export interface CreateSafePayload {
  branchId: number
  orderListId: number | null
  userId: number | null
}

export interface CreateSafeResponse {
  id: number
}
