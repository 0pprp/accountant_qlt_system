import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { DuplicateRequestHit } from '@/modules/SalesRequests/types'

export const DUPLICATES_KEY = 'sales-requests-duplicates'

interface Paginated<T> {
  items: T[]
  totalCount: number
}

export function fetchDuplicates(
  requestId: number,
  pageIndex: number,
  pageSize: number
) {
  return axiosInstance
    .get<Paginated<DuplicateRequestHit>>(
      `/admin/SalesRequests/${requestId}/duplicate-requests`,
      { params: { pageIndex, pageSize } }
    )
    .then((r) => r.data)
}

export function useDuplicateRequestsQuery(
  requestId: Ref<number | null>,
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [DUPLICATES_KEY, requestId, pageIndex, pageSize],
    queryFn: () =>
      fetchDuplicates(requestId.value!, pageIndex.value, pageSize.value),
    enabled,
  })
}
