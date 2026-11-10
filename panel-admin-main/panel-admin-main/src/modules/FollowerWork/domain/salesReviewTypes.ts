export interface SalesReviewListCard {
  listId: number
  listName: string
  phantomRating: number | null
  phantomRatingLabel: string | null
  phantomRatingWeekAgo: number | null
  scoreTrend: string
  ratedCustomerCount: number
}

export interface SalesReviewRatingBucket {
  levelKey: string
  label: string
  customerCount: number
  customerCountYesterday: number
  countTrend: string
}

export interface SalesReviewSaleRow {
  orderId: number
  customerId: number
  customerName: string
  phone: string | null
  listId: number
  listName: string | null
  authenticated: boolean
  authenticatedAtUtc: string | null
  authRating: number | null
  authNote: string | null
  shopStockEstimatedValue: number | null
  estimatedDailyRevenue: number | null
  shopRating: number | null
  shopRatingNote: string | null
  delegateShopStockEstimatedValue: number | null
  delegateEstimatedDailyRevenue: number | null
  employeeShopStockEstimatedValue: number | null
  employeeEstimatedDailyRevenue: number | null
  currentRatingLabel: string | null
  salePrice: number | null
  saleDate: string | null
  dailyInstallment: number | null
  sellerName: string | null
  paidAmount: number | null
  remainingAmount: number | null
  isLowAuthRisk: boolean
}

export interface SalesReviewWorkspace {
  follower: { followerId: number; followerName: string } | null
  listCards: SalesReviewListCard[]
  ratingBuckets: SalesReviewRatingBucket[]
  ratedCustomerCount: number
  remainingLe100Count: number
  remainingLt200Count: number
  remainingLe100Trend: string
  remainingLt200Trend: string
  pendingSalesCount: number
  authenticatedSalesCount: number
  salesRows: SalesReviewSaleRow[]
  timeZone: string
  asOfLocalDate: string
  generatedAt: string
}

export interface RemainingCustomerRow {
  customerId: number
  customerName: string
  phone: string | null
  listId: number
  listName: string | null
  amountRemaining: number
  ratingLabel: string | null
}

export interface RemainingCustomersResponse {
  bucket: string
  items: RemainingCustomerRow[]
  totalCount: number
  timeZone: string
  asOfLocalDate: string
  generatedAt: string
}

export const REMAINING_BUCKETS = [
  'excellent',
  'good',
  'weak',
  'lagging',
  'bad',
  'le100',
  'lt200',
  'rated',
] as const

export type RemainingBucket = (typeof REMAINING_BUCKETS)[number]
