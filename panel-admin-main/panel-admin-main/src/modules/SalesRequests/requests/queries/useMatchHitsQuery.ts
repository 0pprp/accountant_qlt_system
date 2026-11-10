import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { MatchCriterion, MatchHit } from '@/modules/SalesRequests/types'

export const MATCH_HITS_KEY = 'sales-requests-match-hits'

interface Paginated<T> {
  items: T[]
  totalCount: number
}

export function fetchMatchHits(
  requestId: number,
  criterion: MatchCriterion,
  pageIndex: number,
  pageSize: number
) {
  return axiosInstance
    .get<Paginated<MatchHit>>(
      `/admin/SalesRequests/${requestId}/match-hits`,
      { params: { criterion, pageIndex, pageSize } }
    )
    .then((r) => r.data)
}

export function useMatchHitsQuery(
  requestId: Ref<number | null>,
  criterion: Ref<MatchCriterion | null>,
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [MATCH_HITS_KEY, requestId, criterion, pageIndex, pageSize],
    queryFn: () =>
      fetchMatchHits(
        requestId.value!,
        criterion.value!,
        pageIndex.value,
        pageSize.value
      ),
    enabled,
  })
}
