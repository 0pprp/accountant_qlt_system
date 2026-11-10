/** Mirrors Application/DelegateFollowup/Common/DelegateFollowupDtos.cs (camelCase JSON, DateOnly → "yyyy-MM-dd"). */

export type DelegateFollowupTrend = 'up' | 'down' | 'flat'

export interface DelegateFollowupUnresolvedField {
  field: string
  note: string
}

export interface DelegateFollowupListCard {
  listId: number
  listName: string
  branchId: number
  delegateName: string | null
  followerName: string | null
  realRating: number
  realRatingLabel: string
  nonZeroedCustomerCount: number
  lastPaymentDate: string | null
}

export interface DelegateFollowupListsResponse {
  lists: DelegateFollowupListCard[]
}

export interface DelegateFollowupDayAmount {
  date: string
  amount: number
}

export interface DelegateFollowupDayRating {
  date: string
  score: number
  label: string
  boardOperatorScore: number | null
  boardOperatorNote: string | null
}

export interface DelegateFollowupTierMoney {
  label: string
  requiredDueAvailable: boolean
  requiredDue: number | null
  collectedYesterday: number
  collectedLast30Days: DelegateFollowupDayAmount[]
}

export interface DelegateFollowupConversionFrom {
  fromLabel: string
  count: number
}

export interface DelegateFollowupConversionCustomer {
  customerId: number
  customerName: string
  fromLabel: string
}

export interface DelegateFollowupConversionTargetDay {
  date: string
  arrivedCount: number
  fromBreakdown: DelegateFollowupConversionFrom[]
  customers: DelegateFollowupConversionCustomer[]
}

export interface DelegateFollowupConversionTarget {
  targetLabel: string
  arrivedCount: number
  detailText: string
  fromBreakdown: DelegateFollowupConversionFrom[]
  customers: DelegateFollowupConversionCustomer[]
  last30Days: DelegateFollowupConversionTargetDay[]
}

export interface DelegateFollowupConversion {
  improved: number
  declined: number
  unchanged: number
  summaryText: string
  legalTargetAvailable: boolean
  targets: DelegateFollowupConversionTarget[]
}

export interface DelegateFollowupBoardRating {
  listId: number
  ratingDate: string
  score: number | null
  note: string | null
  ratedByUserName: string | null
  ratedAtUtc: string | null
  hasRating: boolean
}

export interface DelegateFollowupCommitteeRating {
  available: boolean
  note: string
  latest: DelegateFollowupBoardRating | null
  last30Days: DelegateFollowupDayRating[]
}

export interface DelegateFollowupSummary {
  listId: number
  listName: string
  branchId: number
  asOfDate: string

  realRating: number
  realRatingLabel: string
  realRatingWeekAgo: number
  ratingTrend: DelegateFollowupTrend | string

  nonZeroedCustomerCount: number

  legalAvailable: boolean
  legalCustomerCount: number | null

  excellentCount: number
  goodCount: number
  weakCount: number
  laggingCount: number
  badCount: number

  remainingLt100Count: number
  remaining100To200Count: number
  remainingGe100Count: number

  newSaleUnpaid3DaysCount: number

  requiredDueAvailable: boolean
  requiredDueToday: number | null
  requiredDueUnavailableReason: string | null
  collectedYesterday: number
  collectedLastFriday: number
  lastFridayDate: string
  collectedLast30Days: DelegateFollowupDayAmount[]
  collectedLast30Fridays: DelegateFollowupDayAmount[]
  tierMoney: DelegateFollowupTierMoney[]

  realRatingLast30Days: DelegateFollowupDayRating[]
  boardOperatorLast30Days: DelegateFollowupDayRating[]
  followerCommittee: DelegateFollowupCommitteeRating
  supervisionCommittee: DelegateFollowupCommitteeRating
  salesDelegate: DelegateFollowupCommitteeRating

  conversion: DelegateFollowupConversion
  boardOperatorRating: DelegateFollowupBoardRating

  unresolvedNotes: DelegateFollowupUnresolvedField[]
}

export interface DelegateFollowupCustomer {
  customerId: number
  listId: number
  customerName: string
  address: string | null
  phoneNumber: string | null
  shopName: string | null
  saleName: string | null
  delegateName: string | null
  followerName: string | null
  dateSaleDevice: string | null
  amountTotalSales: number
  amountDaySales: number
  receiptsTotal: number
  amountRemaining: number
  itemsNames: string | null
  lastPaymentDate: string | null
  lastPaymentAmount: number | null
  countReceiptDevice: number
  numberOfDayDevice: number
  receiptRateDevice: number
  score: number | null
  realRatingLabel: string
  isLegal: boolean
  isNewSaleUnpaid3Days: boolean
  notes: string | null
  receiptName: string | null
}

export interface DelegateFollowupPayment {
  paymentId: number
  orderId: number
  customerId: number
  date: string
  amount: number
  description: string | null
}

export interface DelegateFollowupCustomerPayments {
  customerId: number
  listId: number
  branchId: number
  payments: DelegateFollowupPayment[]
  totalAmount: number
}

export interface DelegateFollowupCustomers {
  listId: number
  branchId: number
  legalAvailable: boolean
  newSaleUnpaid3DaysCount: number
  customers: DelegateFollowupCustomer[]
  unresolvedNotes: DelegateFollowupUnresolvedField[]
}

export interface DelegateFollowupActionResult {
  created: boolean
  rating: DelegateFollowupBoardRating
}

export interface UpsertBoardRatingPayload {
  listId: number
  branchId: number
  score: number
  note: string | null
}

export type CustomerTableFilter = 'all' | 'tiers' | 'lt100' | '100to200'

export const BOARD_SCORE_MIN = 0
export const BOARD_SCORE_MAX = 10
export const BOARD_NOTE_MAX = 500
export const BRANCH_STORAGE_KEY = 'delegate-followup-branch'
