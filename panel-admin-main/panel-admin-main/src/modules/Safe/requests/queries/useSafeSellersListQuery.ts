import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeSellersListServerSuccessResponse } from '@/modules/Safe/types/api'

export const QUERY_KEY = 'safe-seller-list'
const ENDPOINT = '/admin/Safes/{id}/sellers'

export function fetchSafeSellers(
  pageIndex: number,
  pageSize: number,
  safeId: number,
  searchTerm?: string | null
) {
  const endpoint = ENDPOINT.replace('{id}', String(safeId))
  return axiosInstance
    .get<SafeSellersListServerSuccessResponse>(endpoint, {
      params: {
        pageSize,
        pageIndex,
        searchTerm,
      },
    })
    .then((response) => response.data)
}

export function useSafeSellersDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  safeId: Ref<number>,
  searchTerm?: Ref<string | null>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchSafeSellers>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, safeId, searchTerm],
    queryFn: () =>
      fetchSafeSellers(
        pageIndex.value,
        pageSize.value,
        safeId.value,
        searchTerm?.value
      ),
    ...options,
  })
}
