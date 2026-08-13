import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeTransactionsListServerSuccessResponse } from '@/modules/Safe/types/api'

export const QUERY_KEY = 'safe-transactions-list'
const ENDPOINT = '/admin/Safes/{id}/transactions'

export function fetchSafeTransactions(
  pageIndex: number,
  pageSize: number,
  safeId: number,
  searchTerm?: string | null
) {
  const endpoint = ENDPOINT.replace('{id}', String(safeId))
  return axiosInstance
    .get<SafeTransactionsListServerSuccessResponse>(endpoint, {
      params: {
        pageSize,
        pageIndex,
        searchTerm,
      },
    })
    .then((response) => response.data)
}

export function useSafeTransactionsDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  safeId: Ref<number>,
  searchTerm?: Ref<string | null>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchSafeTransactions>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, safeId, searchTerm],
    queryFn: () =>
      fetchSafeTransactions(
        pageIndex.value,
        pageSize.value,
        safeId.value,
        searchTerm?.value
      ),
    ...options,
  })
}
