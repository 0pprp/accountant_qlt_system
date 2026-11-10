import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { MatchSummaryItem } from '@/modules/SalesRequests/types'

export const MATCH_SUMMARY_KEY = 'sales-requests-match-summary'

export function fetchMatchSummary(requestIds: number[]) {
  return axiosInstance
    .post<{ items: MatchSummaryItem[] }>(
      '/admin/SalesRequests/match-summary',
      { requestIds }
    )
    .then((r) => r.data)
}

export function useMatchSummaryQuery(
  requestIds: Ref<number[]>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [MATCH_SUMMARY_KEY, requestIds],
    queryFn: () => fetchMatchSummary(requestIds.value),
    enabled,
  })
}
