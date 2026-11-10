export interface FollowerCollectionListRow {
  listId: number
  listName: string
  mandobUserId: number
  mandobName: string | null
  followerCollectionEnabled: boolean
  assignedAtUtc: string | null
  collectionToggleVersion: number
  lastDelegatePaymentAt: string | null
  lastDelegatePaymentAmount: number
  lastFollowerPaymentAt: string | null
  lastFollowerPaymentAmount: number
}

export interface FollowerCollectionPaymentRow {
  paymentId: number
  orderId: number
  customerId: number
  customerName: string
  listId: number
  listName: string | null
  amount: number
  currency: string
  paymentDate: string
  collectedByUserId: number | null
  collectedByName: string | null
  source: string
}

export interface FollowerCollectionTotals {
  paymentsPageAmount: number
  paymentsFilteredAmount: number
  paymentsFilteredCount: number
  currency: string
}

export interface FollowerCollectionResponse {
  lists: FollowerCollectionListRow[]
  payments: FollowerCollectionPaymentRow[]
  totals: FollowerCollectionTotals
  paymentsTotalCount: number
  paymentsPage: number
  paymentsPageSize: number
  paymentsTotalPages: number
  fromDate: string | null
  toDate: string | null
  timeZoneId: string
  generatedAtUtc: string
  followerId: number
  followerName: string | null
}

export interface SetFollowerCollectionEnabledResult {
  listId: number
  listName: string
  enabled: boolean
  assignedAtUtc: string | null
  collectionToggleVersion: number
  changed: boolean
}
