export interface FollowerLiveLocationRow {
  followerId: number
  followerName: string
  branchId: number | null
  branchName: string | null
  shiftId: number | null
  hasActiveShift: boolean
  latitude: number | null
  longitude: number | null
  accuracyMeters: number | null
  capturedAtUtc: string | null
  receivedAtUtc: string | null
  locationStatus: string
}

export interface FollowerLiveLocationsResponse {
  items: FollowerLiveLocationRow[]
  timeZoneId: string
  generatedAtUtc: string
  liveThresholdSeconds: number
  staleThresholdMinutes: number
}

export interface FollowerRoutePoint {
  pointId: number
  latitude: number
  longitude: number
  accuracyMeters: number | null
  capturedAtUtc: string
  receivedAtUtc: string
  isLive: boolean
  shiftId: number | null
}

export interface FollowerRouteResponse {
  followerId: number
  followerName: string | null
  dateIraq: string
  fromUtc: string
  toUtc: string
  points: FollowerRoutePoint[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  truncated: boolean
  timeZoneId: string
  generatedAtUtc: string
}

export const locationStatusLabel: Record<string, string> = {
  Live: 'مباشر',
  Stale: 'قديم',
  NoLocation: 'بلا موقع',
  NoShift: 'بلا دوام',
}
