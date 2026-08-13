import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type {
  ActivityLogFilterParams,
  ActivityLogListServerSuccessResponse,
} from '@/modules/ActivityLog/types/api'

export const QUERY_KEY = 'activity-log-list'
const ENDPOINT = '/admin/ActivityLogs'

export function fetchActivityLogList(
  pageIndex: number,
  pageSize: number,
  filter: ActivityLogFilterParams,
  sortCriteria?: Array<{ property: string; direction: number }> | null
) {
  const params: Record<string, unknown> = {
    pageIndex,
    pageSize,
    filter,
  }

  if (sortCriteria && sortCriteria.length > 0) {
    params.sortCriteria = sortCriteria
  }

  return axiosInstance
    .get<ActivityLogListServerSuccessResponse>(ENDPOINT, {
      params,
      paramsSerializer: serializeParamsWithSortCriteria,
    })
    .then((response) => response.data)
}

export function useActivityLogListQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  filter: Ref<ActivityLogFilterParams>,
  sortCriteria?: Ref<Array<{ property: string; direction: number }> | null>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchActivityLogList>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, filter, sortCriteria],
    queryFn: () =>
      fetchActivityLogList(
        pageIndex.value,
        pageSize.value,
        filter.value,
        sortCriteria?.value
      ),
    enabled: () => filter.value.branchId > 0,
    ...options,
  })
}
