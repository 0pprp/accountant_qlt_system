import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { PurchaseByIdServerSuccessResponse } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const QUERY_KEY = 'purchases-by-id'
const ENDPOINT = '/admin/Purchases/{id}'

export function fetchPurchasesById(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<PurchaseByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function usePurchasesByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchPurchasesById>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],

    queryFn: () => fetchPurchasesById(id.value),
    ...options,
  })
}
