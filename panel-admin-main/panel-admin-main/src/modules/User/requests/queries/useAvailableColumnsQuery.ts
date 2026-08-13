import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { AvailableColumnsResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'user-available-columns'
const ENDPOINT = '/admin/Users/available-columns'

export function fetchAvailableColumns() {
  return axiosInstance
    .get<AvailableColumnsResponse>(ENDPOINT)
    .then((response) => response.data)
}

export function useAvailableColumnsQuery(
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchAvailableColumns>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY],
    queryFn: () => fetchAvailableColumns(),
    ...options,
  })
}
