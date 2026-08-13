import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeByIdServerSuccessResponse } from '@/modules/Safe/types/api'

export const QUERY_KEY = 'safe-by-id'
const ENDPOINT = '/admin/Safes/{id}'

export function fetchSingleSafe(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<SafeByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useSafeByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchSingleSafe>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchSingleSafe(id.value),
    refetchOnMount: true,
    ...options,
  })
}
