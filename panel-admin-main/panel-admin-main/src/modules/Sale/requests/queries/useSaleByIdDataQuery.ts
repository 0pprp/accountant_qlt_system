import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SaleByIdServerSuccessResponse } from '@/modules/Sale/types/api'

export const QUERY_KEY = 'sale-by-id'
const ENDPOINT = '/admin/Orders/{id}'

export function fetchSale(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<SaleByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useSaleByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchSale>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],

    queryFn: () => fetchSale(id.value),
    ...options,
  })
}
