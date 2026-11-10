import type { FollowerWorkSort } from './types'

export const FOLLOWER_WORK_KEY = 'follower-work'

export function followersKey(params: {
  branchId: number | null
  search: string
  page: number
  pageSize: number
  sort: FollowerWorkSort
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'followers', params] as const
}

export function salesReviewKey(params: {
  followerId: number
  branchId: number | null
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'sales-review', params] as const
}

export function remainingCustomersKey(params: {
  followerId: number
  branchId: number | null
  bucket: string
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'remaining-customers', params] as const
}

export function decliningKey(params: {
  followerId: number
  branchId: number | null
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'declining', params] as const
}

export function notesKey(params: {
  followerId: number
  branchId: number | null
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'notes', params] as const
}

export function oldCustomersKey(params: {
  followerId: number
  branchId: number | null
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'old-customers', params] as const
}

export function delegateNotesKey(params: {
  followerId: number
  branchId: number | null
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'delegate-notes', params] as const
}

export function collectionKey(params: {
  followerId: number
  branchId: number | null
  fromDate: string | null
  toDate: string | null
  paymentsPage: number
  paymentsPageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'collection', params] as const
}

export function liveLocationsKey(params: {
  branchId: number | null
  followerId: number | null
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'live-locations', params] as const
}

export function routeKey(params: {
  followerId: number
  branchId: number | null
  date: string
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [FOLLOWER_WORK_KEY, 'route', params] as const
}
