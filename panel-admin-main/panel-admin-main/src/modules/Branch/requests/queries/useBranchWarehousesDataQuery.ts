import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import type { BranchWarehousesServerSuccessResponse } from '@/modules/Branch/types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const QUERY_KEY = 'breach-of-trust-by-id'
const ENDPOINT = `/admin/Branches/{id}/warehouses`

export function fetchBranchWarehouses(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<BranchWarehousesServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useBranchWarehousesDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchBranchWarehouses>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchBranchWarehouses(id.value),
    ...options,
  })
}
