import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { RolePermissionListServerSuccessResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'role-permission-list'
const ENDPOINT = '/admin/Roles/{id}/permissions'

export function fetchRolePermission(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<RolePermissionListServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useRolePermissionDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchRolePermission>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],

    queryFn: () => fetchRolePermission(id.value),
    ...options,
  })
}
