import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeServerSuccessResponse } from '@/modules/Safe/types/api'

export const QUERY_KEY = 'safe-data'
const ENDPOINT = '/admin/Safes'

export function fetchSafe() {
  return axiosInstance
    .get<SafeServerSuccessResponse>(ENDPOINT, {})
    .then((response) => response.data)
}

export function useSafeDataQuery(
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchSafe>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY],
    queryFn: () => fetchSafe(),
    ...options,
  })
}
