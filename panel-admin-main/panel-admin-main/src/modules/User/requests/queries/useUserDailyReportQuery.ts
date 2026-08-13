import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type { UserDailyReportServerSuccessResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'user-daily-report'
const ENDPOINT = '/admin/Users/{id}/daily-installment-report'

export function fetchUserDailyReport(
  id: number,
  startDate: string,
  endDate: string
) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<UserDailyReportServerSuccessResponse>(endpoint, {
      params: {
        id,
        startDate,
        endDate,
      },
      paramsSerializer: serializeParamsWithSortCriteria,
    })
    .then((response) => response.data)
}

export function useUserDailyReportDataQuery(
  id: Ref<number>,
  startDate: Ref<string>,
  endDate: Ref<string>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchUserDailyReport>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id, startDate, endDate],

    queryFn: () =>
      fetchUserDailyReport(id.value, startDate.value, endDate.value),
    ...options,
  })
}
