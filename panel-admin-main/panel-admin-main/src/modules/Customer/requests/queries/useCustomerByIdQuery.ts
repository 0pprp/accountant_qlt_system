import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CustomerByIdServerSuccessResponse } from '@/modules/Customer/types/api'

export const QUERY_KEY = 'customer-by-id'
const ENDPOINT = '/admin/Customers/{id}'

export function fetchSingleCustomer(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<CustomerByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useCustomerByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchSingleCustomer>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchSingleCustomer(id.value),
    refetchOnMount: true,
    ...options,
  })
}
