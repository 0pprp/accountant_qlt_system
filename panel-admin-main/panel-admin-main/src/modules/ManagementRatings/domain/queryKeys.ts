import type { RatingsScope } from './types'

export const MANAGEMENT_RATINGS_KEY = 'management-ratings'

export function branchesKey(branchGeneration: number, sessionGeneration: number) {
  return [MANAGEMENT_RATINGS_KEY, 'branches', branchGeneration, sessionGeneration] as const
}

export function catalogKey(
  branchId: number,
  branchGeneration: number,
  sessionGeneration: number
) {
  return [
    MANAGEMENT_RATINGS_KEY,
    'catalog',
    branchId,
    branchGeneration,
    sessionGeneration,
  ] as const
}

export function trendKey(params: {
  branchId: number
  scope: RatingsScope
  resourceId: number
  from: string
  to: string
  branchGeneration: number
  sessionGeneration: number
}) {
  return [MANAGEMENT_RATINGS_KEY, 'trend', params] as const
}

export function customersKey(params: {
  branchId: number
  scope: RatingsScope
  resourceId: number
  level: string
  from: string
  to: string
  page: number
  q: string
  branchGeneration: number
  sessionGeneration: number
}) {
  return [MANAGEMENT_RATINGS_KEY, 'customers', params] as const
}
