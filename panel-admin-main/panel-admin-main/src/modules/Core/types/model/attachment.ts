export enum AttachmentType {
  NationalCard,
  ResidenceCard,
  RationCard,
  PersonalPicture,
  PurchaseReceipt,
  TrustReceipt,
  SaleContract,
  ProfilePicture,
}

export interface AttachmentForm {
  file: File
  type: AttachmentType
  userId?: number
  orderId?: number
  customerId?: number
}
