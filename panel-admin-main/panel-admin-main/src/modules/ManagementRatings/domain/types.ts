export type RatingsScope = 'branch' | 'list' | 'follower' | 'employee'
export type RatingsLevel =
  | 'excellent'
  | 'good'
  | 'weak'
  | 'rejected'
  | 'bad'
  | 'ممتاز'
  | 'جيد'
  | 'ضعيف'
  | 'متلكئ'
  | 'سيء'

export interface MonthBucket {
  yearMonth: number
  newCustomersCount: number
  newCustomersSaleSum: number
  newCustomersDaySalesSum: number
  zeroedCustomersCount: number
  zeroedCustomersSaleSum: number
  zeroedCustomersDaySalesSum: number
  netCustomersCount: number
  netSaleSum: number
  netDaySalesSum: number
}

export interface RatingsBranchCard {
  branchId: number
  name: string
  currentMonth: MonthBucket | null
  previousMonth: MonthBucket | null
  availability: string
}

export interface RatingsBranchesResponse {
  timeZone: string
  branches: RatingsBranchCard[]
  generatedAt: string
}

export interface RatingsEntityOption {
  id: number
  name: string
  assignedListIds: number[] | null
}

export interface RatingsCatalogResponse {
  branchId: number
  branchName: string
  lists: RatingsEntityOption[]
  followers: RatingsEntityOption[]
  employees: RatingsEntityOption[]
  generatedAt: string
}

export interface RatingsTrendPoint {
  date: string
  value: number
}

export interface RatingsTrendResponse {
  scope: RatingsScope
  resourceId: number
  name: string
  metricLabel: string
  saleFrom: string
  saleTo: string
  chartFrom: string
  chartTo: string
  timeZone: string
  points: RatingsTrendPoint[]
  startValue: number
  endValue: number
  changeAbsolute: number
  changePercent: number
  direction: 'up' | 'down' | 'flat' | string
  periodIgnored: boolean
  periodEndValue: number | null
  periodRatingLabel: string | null
  periodCustomerCount: number | null
  ratingLabel: string
  customerCount: number
  excellentCount: number
  goodCount: number
  weakCount: number
  laggingCount: number
  badCount: number
  phantomEndValue: number | null
  phantomRatingLabel: string | null
  installmentSum: number | null
  availability: string
  unavailableReason: string | null
  generatedAt: string
}

export interface RatingsCustomerCard {
  customerId: number
  customerName: string
  phone: string | null
  level: string
  ratingLabel: string
  score: number
  saleDate: string | null
  lastPaymentDate: string | null
  amountPaid: number | null
  amountRemaining: number | null
  amountTotal: number | null
  isLegal: boolean
  hidePaidRemaining: boolean
}

export interface RatingsCustomersResponse {
  scope: string
  resourceId: number
  level: string
  page: number
  pageSize: number
  totalCount: number
  items: RatingsCustomerCard[]
  generatedAt: string
}
