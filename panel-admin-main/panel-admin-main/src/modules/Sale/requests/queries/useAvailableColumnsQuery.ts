import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { AvailableColumnsResponse } from '@/modules/Sale/types/api'

export const QUERY_KEY = 'sale-available-columns'
const ENDPOINT = '/admin/Orders/available-columns'

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
