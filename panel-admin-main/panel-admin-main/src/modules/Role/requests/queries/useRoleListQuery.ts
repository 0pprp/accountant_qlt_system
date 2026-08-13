import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { RoleListServerSuccessResponse } from '@/modules/Role/types/api'

export const QUERY_KEY = 'role-list'
const ENDPOINT = '/admin/Roles'

export function fetchRole() {
  return axiosInstance
    .get<RoleListServerSuccessResponse>(ENDPOINT, {})
    .then((response) => response.data)
}

export function useRoleDataQuery(
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchRole>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY],

    queryFn: () => fetchRole(),
    ...options,
  })
}
