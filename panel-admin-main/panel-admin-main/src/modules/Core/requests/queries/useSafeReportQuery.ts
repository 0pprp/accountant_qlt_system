import type { Ref } from 'vue'
import { computed } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SafeReportResponse } from '@/modules/Core/types/api/dashboard'

export const QUERY_KEY = 'dashboard-safe-report'
const ENDPOINT = '/admin/Reports/safe'

export function fetchSafeReport(branchId: number) {
  return axiosInstance
    .get<SafeReportResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useSafeReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchSafeReport>>>,
    'queryKey' | 'queryFn' | 'enabled'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchSafeReport(branchId.value),
    enabled: computed(() => branchId.value > 0),
    ...options,
  })
}
