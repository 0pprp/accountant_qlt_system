import type { ManagementSummaryMetricKey } from './types'

export const MANAGEMENT_SUMMARY_KEY = 'management-summary'

export function matrixQueryKey(params: {
  year: number
  month: number
  metric: ManagementSummaryMetricKey
  branchId: number | null
  branchGeneration: number
  sessionGeneration: number
}) {
  return [
    MANAGEMENT_SUMMARY_KEY,
    'matrix',
    params.year,
    params.month,
    params.metric,
    params.branchId,
    params.branchGeneration,
    params.sessionGeneration,
  ] as const
}

export function detailsQueryKey(params: {
  year: number
  month: number
  day: number
  metric: ManagementSummaryMetricKey
  branchId: number
  page: number
  pageSize: number
  branchGeneration: number
  sessionGeneration: number
}) {
  return [
    MANAGEMENT_SUMMARY_KEY,
    'details',
    params.year,
    params.month,
    params.day,
    params.metric,
    params.branchId,
    params.page,
    params.pageSize,
    params.branchGeneration,
    params.sessionGeneration,
  ] as const
}
