import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import type { SalesRequestTimelineItem } from '@/modules/SalesRequests/types'

export const SALES_REQUEST_TIMELINE_KEY = 'sales-request-timeline'

export function fetchSalesRequestTimeline(id: number) {
  return axiosInstance
    .get<SalesRequestTimelineItem[]>(SalesRequestEndpoints.timeline(id))
    .then((r) => r.data)
}

export function useSalesRequestTimelineQuery(
  id: Ref<number | null>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [SALES_REQUEST_TIMELINE_KEY, id],
    queryFn: () => fetchSalesRequestTimeline(id.value!),
    enabled,
  })
}
