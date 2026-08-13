import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { LastFactorNumberSuccessResponse } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const QUERY_KEY = 'last-factor-number'
const ENDPOINT = '/admin/Expenses/last-factor-number'

export function fetchLastFactorNumber() {
  return axiosInstance
    .get<LastFactorNumberSuccessResponse>(ENDPOINT)
    .then((response) => response.data)
}

export function useLastFactorNumberDataQuery(
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchLastFactorNumber>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY],

    queryFn: () => fetchLastFactorNumber(),
    ...options,
  })
}
