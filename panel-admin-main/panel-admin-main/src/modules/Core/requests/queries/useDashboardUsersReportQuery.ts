import type { Ref } from 'vue'
import { computed } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { DashboardUsersReportResponse } from '@/modules/Core/types/api/dashboard'

export const QUERY_KEY = 'dashboard-users-report'
const ENDPOINT = '/admin/Reports/users'

export function fetchDashboardUsersReport(branchId: number) {
  return axiosInstance
    .get<DashboardUsersReportResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useDashboardUsersReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchDashboardUsersReport>>>,
    'queryKey' | 'queryFn' | 'enabled'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchDashboardUsersReport(branchId.value),
    enabled: computed(() => branchId.value > 0),
    ...options,
  })
}
