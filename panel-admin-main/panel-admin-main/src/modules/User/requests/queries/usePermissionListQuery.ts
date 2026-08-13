import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PermissionListServerSuccessResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'Permission-list'
const ENDPOINT = '/admin/Permissions'

export function fetchPermission() {
  return axiosInstance
    .get<PermissionListServerSuccessResponse>(ENDPOINT, {})
    .then((response) => response.data)
}

export function usePermissionDataQuery(
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchPermission>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [],

    queryFn: () => fetchPermission(),
    ...options,
  })
}
