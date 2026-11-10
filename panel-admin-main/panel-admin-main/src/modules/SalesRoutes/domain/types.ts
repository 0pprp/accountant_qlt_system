export interface SalesEmployeeRoutePoint {
  pointId: number
  latitude: number
  longitude: number
  accuracyMeters?: number | null
  capturedAtUtc: string
  receivedAtUtc: string
  isLive: boolean
  shiftId?: number | null
  branchId?: number | null
}

export interface SalesEmployeeRouteGap {
  afterPointId: number
  beforePointId: number
  gapMinutes: number
}

export interface SalesEmployeeRouteResponse {
  employeeId: number
  employeeName: string
  dateIraq: string
  fromUtc: string
  toUtc: string
  points: SalesEmployeeRoutePoint[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  truncated: boolean
  timeZoneId: string
  generatedAtUtc: string
  gaps: SalesEmployeeRouteGap[]
  dayBoundNote: string
}
