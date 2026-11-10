export type SalesEmployeeShiftState = 'Active' | 'NoShift'

export interface SalesEmployeeLink {
  key: string
  labelAr: string
  routeName: string
  path: string
  requiredPermission: string
  allowed: boolean
}

export interface SalesEmployeeCard {
  employeeId: number
  employeeName: string
  primaryBranchId: number
  branchName: string
  visibleBranchIds: number[]
  shiftStatus: string
  shiftStartedAtUtc?: string | null
  currentShiftId?: number | null
  locationStatus: string
  lastLocationAtUtc?: string | null
  latestRating?: number | null
  phantomLatestRating?: number | null
  ratingLabel?: string | null
  salesTodayCount: number
  pendingSalesCount: number
  isAccountActive: boolean
  links?: SalesEmployeeLink[] | null
}

export interface ListSalesEmployeesResponse {
  items: SalesEmployeeCard[]
  totalCount: number
  page: number
  pageSize: number
  branchScope: number[]
}

export interface SalesEmployeeTrendPoint {
  date: string
  value: number
}

export interface SalesEmployeeRatingCustomer {
  customerId: number
  customerName: string
  phone?: string | null
  dateSale?: string | null
  lastPaymentDate?: string | null
  amountPaid?: number | null
  amountRemaining?: number | null
  amountTotal?: number | null
  ratingLabel: string
  amountsRedacted: boolean
}

export interface SalesEmployeeRatingSummary {
  employeeId: number
  employeeName: string
  branchId: number
  latestRating: number | null
  phantomLatestRating: number | null
  ratingLabel: string | null
  excellentCount: number
  goodCount: number
  weakCount: number
  rejectedCount: number
  badCount: number
  points: SalesEmployeeTrendPoint[]
  financialVisible: boolean
  customers?: SalesEmployeeRatingCustomer[] | null
}
