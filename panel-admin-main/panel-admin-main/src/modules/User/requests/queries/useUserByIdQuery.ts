import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserByIdServerSuccessResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'user-by-id'
const ENDPOINT = '/admin/Users/{id}'

export function fetchUser(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<UserByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useUserByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchUser>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],

    queryFn: () => fetchUser(id.value),
    ...options,
  })
}
