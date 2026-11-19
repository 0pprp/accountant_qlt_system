export type FollowerBoardTab =
  | 'sales'
  | 'declining'
  | 'notes'
  | 'old-customers'
  | 'delegate-notes'
  | 'collection'
  | 'shop-capture'
  | 'live-map'
  | 'routes'

export interface FollowerSectionCounts {
  pendingSales: number | null
  declining: number | null
  notes: number | null
  oldCustomers: number | null
  delegateNotes: number | null
  collectionEnabledLists: number | null
}

export interface FollowerCustomerAssessment {
  rating: number | null
  note: string | null
  shopStockEstimatedValue: number | null
  estimatedDailyRevenue: number | null
  ratedByName: string | null
  ratedAtUtc: string | null
  isLowRisk: boolean
}

export interface FollowerCustomerProfile {
  general: {
    customerId: number
    customerName: string
    phoneNumber: string | null
    shopName: string | null
    address: string | null
    listId: number
    listName: string | null
    isLegal: boolean
    ratingLabel: string | null
    sellAmount: number
    saleDate: string | null
    dailyInstallment: number
    sellerName: string | null
    paid: number
    remaining: number
    lastPaymentDate: string | null
    hasLocation: boolean
    latitude: number | null
    longitude: number | null
  }
  followerAssessment: FollowerCustomerAssessment | null
  delegateAssessment: FollowerCustomerAssessment | null
  salesEmployee: {
    rating: number | null
    note: string | null
    ratedByName: string | null
    ratedAtUtc: string | null
    shopName: string | null
    businessType: string | null
    stockEstimatedValue: number | null
    estimatedDailyRevenue: number | null
    shopArea: number | null
  } | null
  asOfLocalDate: string
}

export interface FollowerShopCaptureList {
  listId: number
  listName: string
  followerId: number
  followerName: string | null
  totalCustomers: number
  withLocation: number
  withPhoto: number
  complete: number
}

export interface FollowerShopCaptureCustomer {
  customerId: number
  customerName: string
  shopName: string | null
  phoneNumber: string | null
  listId: number
  amountRemaining: number
  hasLocation: boolean
  latitude: number | null
  longitude: number | null
  hasPhoto: boolean
  photoId: number | null
  photoSource: 'imported' | 'app' | null
  photoCapturedAtUtc: string | null
}

export interface FollowerShopCapture {
  totalCustomers: number
  withLocation: number
  withPhoto: number
  complete: number
  lists: FollowerShopCaptureList[]
  customers: FollowerShopCaptureCustomer[]
  asOfLocalDate: string
}

export interface FollowerAccountPayload {
  fullName: string
  username?: string
  password?: string
  newPassword?: string
  phoneNumber?: string | null
  branchIds?: number[]
  listIds?: number[]
  assignListIds?: number[]
}
