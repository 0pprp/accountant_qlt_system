export interface SalesEmployeeLiveLocationRow {
  employeeId: number
  employeeName: string
  branchId: number
  branchName: string
  shiftStatus: string
  locationStatus: string
  latitude: number | null
  longitude: number | null
  accuracyMeters: number | null
  capturedAtUtc: string | null
  receivedAtUtc: string | null
  hasCoordinates: boolean
}

export interface SalesEmployeeLiveLocationsResponse {
  items: SalesEmployeeLiveLocationRow[]
  timeZoneId: string
  generatedAtUtc: string
  liveThresholdSeconds: number
  staleThresholdMinutes: number
}
