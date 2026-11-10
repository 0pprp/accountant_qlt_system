export type FollowerWorkSort = 'name' | 'rating' | 'lists'

export type MetricsAvailability = 'available' | 'unavailable'

export interface FollowerBranchRef {
  branchId: number
  branchName: string
}

export interface FollowerCardMetrics {
  phantomRating: number | null
  phantomRatingLabel: string | null
  phantomRatingWeekAgo: number | null
  scoreTrend: string | null
  ratedCustomerCount: number | null
  ratedCustomerCountWeekAgo: number | null
  countTrend: string | null
  availability: MetricsAvailability
}

export interface FollowerCard {
  followerId: number
  followerName: string
  branches: FollowerBranchRef[]
  assignedListCount: number
  metrics: FollowerCardMetrics | null
}

export interface FollowersResponse {
  items: FollowerCard[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  timeZone: string
  asOfLocalDate: string
  generatedAt: string
}

export type FollowerSectionStatus = 'ready' | 'not_implemented'

export interface FollowerSectionDef {
  id: string
  pathSegment: string
  labelKey: string
  permissionModule: string
  permissionAction: string
  status: FollowerSectionStatus
}
