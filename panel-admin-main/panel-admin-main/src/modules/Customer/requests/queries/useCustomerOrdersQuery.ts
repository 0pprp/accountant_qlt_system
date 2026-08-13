import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CustomerOrdersResponse } from '@/modules/Customer/types/api'

export const QUERY_KEY = 'customer-orders'
const ENDPOINT = '/admin/Customers/{id}/orders'

export function fetchCustomerOrders(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<CustomerOrdersResponse>(endpoint)
    .then((response) => response.data)
}

export function useCustomerOrdersQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchCustomerOrders>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchCustomerOrders(id.value),
    enabled: () => !!id.value,
    refetchOnMount: true,
    ...options,
  })
}
