import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeCurrentServerSuccessResponse } from '@/modules/Safe/types/api'

export const QUERY_KEY = 'safe-current-data'
const ENDPOINT = '/admin/Safes/current'

export function fetchSafeCurrent(branchId: number | null) {
  const params: { branchId?: number } = {}
  if (branchId !== null) {
    params.branchId = branchId
  }
  return axiosInstance
    .get<SafeCurrentServerSuccessResponse>(ENDPOINT, {
      params,
    })
    .then((response) => response.data)
}

export function useSafeCurrentDataQuery(
  branchId: Ref<number | null>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchSafeCurrent>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchSafeCurrent(branchId.value),
    ...options,
  })
}
