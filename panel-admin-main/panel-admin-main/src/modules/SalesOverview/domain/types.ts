export type SalesOverviewAvailability = 'Available' | 'Unavailable' | 'Forbidden'

export interface SalesOverviewLink {
  routeName: string
  path: string
  requiredPermission: string
  allowed: boolean
  query?: Record<string, string> | null
}

export interface SalesOverviewMetric {
  key: string
  labelAr: string
  unit: string
  entityOwner: string
  availability: SalesOverviewAvailability
  value: number | null
  unavailableReason?: string | null
  periodNote?: string | null
  link?: SalesOverviewLink | null
}

export interface SalesOverviewPeriod {
  businessDateIraq: string
  timeZoneId: string
  windowStartUtc: string
  windowEndUtc: string
  generatedAtUtc: string
}

export interface SalesOverviewResponse {
  period: SalesOverviewPeriod
  metrics: SalesOverviewMetric[]
  branchId: number | null
  branchScope: number[]
}
