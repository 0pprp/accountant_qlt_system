import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import {
  QUERY_KEY_NOTIFICATION_LIST,
  QUERY_KEY_NOTIFICATION_UNREAD_COUNT,
} from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { MarkAllNotificationsReadResponse } from '@/modules/Notification/types/api'

const ENDPOINT = '/admin/Notifications/read-all'

export function markAllNotificationsRead(branchId: number) {
  return axiosInstance
    .patch<MarkAllNotificationsReadResponse>(ENDPOINT, null, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useMarkAllNotificationsReadMutation(
  options?: UseMutationOptions<
    MarkAllNotificationsReadResponse,
    AxiosCustomError,
    number,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (branchId: number) => markAllNotificationsRead(branchId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_NOTIFICATION_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_NOTIFICATION_UNREAD_COUNT],
      })
    },
    meta: {
      success: {
        title: 'notification.markAllSuccessTitle',
        description: 'notification.markAllSuccess',
      },
    },
    ...options,
  })
}
