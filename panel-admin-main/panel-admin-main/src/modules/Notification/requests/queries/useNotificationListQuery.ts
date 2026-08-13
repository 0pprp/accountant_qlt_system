import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type {
  NotificationListParams,
  NotificationListServerSuccessResponse,
} from '@/modules/Notification/types/api'

export const QUERY_KEY = 'notification-list'
const ENDPOINT = '/admin/Notifications'

export function fetchNotificationList(params: NotificationListParams) {
  const {
    pageIndex,
    pageSize,
    branchId,
    hasRead,
    actionType,
    searchTerm,
    startDate,
    endDate,
    sortCriteria,
  } = params

  const queryParams: Record<string, unknown> = {
    pageIndex,
    pageSize,
    branchId,
  }

  if (hasRead !== null && hasRead !== undefined) {
    queryParams.hasRead = hasRead
  }
  if (actionType !== null && actionType !== undefined) {
    queryParams.actionType = actionType
  }
  if (searchTerm) {
    queryParams.searchTerm = searchTerm
  }
  if (startDate) {
    queryParams.startDate = startDate
  }
  if (endDate) {
    queryParams.endDate = endDate
  }
  if (sortCriteria && sortCriteria.length > 0) {
    queryParams.sortCriteria = sortCriteria
  }

  return axiosInstance
    .get<NotificationListServerSuccessResponse>(ENDPOINT, {
      params: queryParams,
      paramsSerializer: serializeParamsWithSortCriteria,
    })
    .then((response) => response.data)
}

export function useNotificationListQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm: Ref<string | null>,
  startDate: Ref<string | null>,
  endDate: Ref<string | null>,
  sortCriteria: Ref<Array<{ property: string; direction: number }> | null>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchNotificationList>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      pageSize,
      pageIndex,
      branchId,
      searchTerm,
      startDate,
      endDate,
      sortCriteria,
    ],
    queryFn: () =>
      fetchNotificationList({
        pageIndex: pageIndex.value,
        pageSize: pageSize.value,
        branchId: branchId.value,
        searchTerm: searchTerm.value,
        startDate: startDate.value,
        endDate: endDate.value,
        sortCriteria: sortCriteria.value,
      }),
    enabled: () => branchId.value > 0,
    ...options,
  })
}
