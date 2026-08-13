import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { NotificationUnreadCountResponse } from '@/modules/Notification/types/api'

export const QUERY_KEY = 'notification-unread-count'
const ENDPOINT = '/admin/Notifications/unread-count'

export function fetchNotificationUnreadCount(branchId: number) {
  return axiosInstance
    .get<NotificationUnreadCountResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useNotificationUnreadCountQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchNotificationUnreadCount>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchNotificationUnreadCount(branchId.value),
    enabled: () => branchId.value > 0,
    ...options,
  })
}
