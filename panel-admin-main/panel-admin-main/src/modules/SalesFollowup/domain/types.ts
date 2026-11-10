export interface SalesFollowupEmployeeCard {
  employeeId: number
  employeeName: string
  branchId: number
  branchName?: string | null
  customerCount: number
  averageScore: number
  ratingLabel: string
  managerScore?: number | null
  managerNote?: string | null
  managerRatedAt?: string | null
}

export interface SalesFollowupRatingRow {
  score: number
  note?: string | null
  ratingDate: string
  listName?: string | null
  customerName?: string | null
  kind?: string | null
}

export interface SalesFollowupCustomer {
  customerId: number
  customerName: string
  phoneNumber?: string | null
  address?: string | null
  shopName?: string | null
  notes?: string | null
  amountTotalSales: number
  receiptsTotal: number
  amountRemaining: number
  amountDaySales: number
  dateSaleDevice?: string | null
  lastPaymentDate?: string | null
  lastPaymentAmount: number
  countReceiptDevice: number
  numberOfDayDevice: number
  receiptRateDevice: number
  score?: number | null
  ratingLabel: string
  isLegal: boolean
  itemsNames?: string | null
  saleName?: string | null
  receiptName?: string | null
  orderListId?: number | null
  listName?: string | null
}

export interface SalesFollowupDetail {
  employee: SalesFollowupEmployeeCard
  managerRating?: {
    score: number
    note?: string | null
    ratedAt: string
    ratedByUserName?: string | null
  } | null
  followerSaleAuthRatings: {
    high: SalesFollowupRatingRow[]
    low: SalesFollowupRatingRow[]
  }
  collectionDelegateRatings: {
    high: SalesFollowupRatingRow[]
    low: SalesFollowupRatingRow[]
  }
  systemRatingHistory: Array<{
    ratingDate: string
    averageScore: number
    ratingLabel: string
    customerCount: number
  }>
  zeroedCustomerCount: number
  customers: SalesFollowupCustomer[]
  salesByList: Array<{
    listId?: number | null
    listName: string
    customerCount: number
    totalSales: number
    totalRemaining: number
  }>
  classCounts: Array<{ label: string; count: number }>
  conversion: {
    improved: number
    declined: number
    unchanged: number
    summaryText: string
    targets: Array<{
      targetLabel: string
      arrivedCount: number
      detailText: string
      fromBreakdown: Array<{ fromLabel: string; count: number }>
      customers: Array<{
        customerId: number
        customerName: string
        fromLabel: string
      }>
      last30Days: Array<{
        date: string
        arrivedCount: number
        fromBreakdown: Array<{ fromLabel: string; count: number }>
        customers: Array<{
          customerId: number
          customerName: string
          fromLabel: string
        }>
      }>
    }>
  }
  isAppBlocked: boolean
  marker: string
}

export interface SalesFollowupAppBlock {
  employeeId: number
  isBlocked: boolean
  updatedAtUtc?: string | null
  updatedBy?: string | null
}
