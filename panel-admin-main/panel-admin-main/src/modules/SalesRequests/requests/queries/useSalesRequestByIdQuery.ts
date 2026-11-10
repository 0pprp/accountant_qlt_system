import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import type { SalesRequestListItem } from '@/modules/SalesRequests/types'

export const SALES_REQUEST_BY_ID_KEY = 'sales-request-by-id'

export function fetchSalesRequestById(id: number) {
  return axiosInstance
    .get<SalesRequestListItem>(SalesRequestEndpoints.byId(id))
    .then((r) => r.data)
}

export function useSalesRequestByIdQuery(
  id: Ref<number | null>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [SALES_REQUEST_BY_ID_KEY, id],
    queryFn: () => fetchSalesRequestById(id.value!),
    enabled,
  })
}
