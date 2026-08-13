import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { OrderSummaryResponse } from '@/modules/Customer/types/api'

export const QUERY_KEY = 'order-summary'
const ENDPOINT = '/admin/Orders/{id}/summary'

export function fetchOrderSummary(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<OrderSummaryResponse>(endpoint)
    .then((response) => response.data)
}

export function useOrderSummaryQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchOrderSummary>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchOrderSummary(id.value),
    enabled: () => !!id.value,
    refetchOnMount: true,
    ...options,
  })
}
