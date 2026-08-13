export interface PurchaseItemModel {
  id?: number
  quantity: number
  amount: number
  productId?: number | null
  product?: { id: number; name: string } | null
  foreignProductName?: string | null
}

// export interface PurchaseItemFormModel {
//   quantity: number
//   amount: number
//   productId: number
// }

export interface SinglePurchase {
  id: number
  factorNumber: string
  safeType: SafeType
  totalAmount: number
  purchaseItemsCount: number
  purchaseItems: Array<PurchaseItemModel>
  createdAt: string
  attachments: Array<PurchaseAttachment>
}

export interface PurchaseAttachment {
  id: number
  originalFileName: string
  relativePath: string
  fileSizeInByte: number
  type: number
}

export enum SafeType {
  Branch,
  Main,
}
export interface PurchasesForm {
  factorNumber?: number
  safeType?: SafeType
  branchId?: number
  purchaseItems: Array<PurchaseItemModel>
  attachments?: File
}

export interface LastFactorNumberResponse {
  lastFactorNumber: number
}
