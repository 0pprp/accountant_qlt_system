import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ActivityLogByIdServerSuccessResponse } from '@/modules/ActivityLog/types/api'

export const QUERY_KEY = 'activity-log-by-id'
const ENDPOINT = '/admin/ActivityLogs/{id}'

export function fetchActivityLogById(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<ActivityLogByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useActivityLogByIdQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchActivityLogById>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchActivityLogById(id.value),
    enabled: () => id.value > 0,
    ...options,
  })
}
