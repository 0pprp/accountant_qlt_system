import type { Ref } from 'vue'
import { computed } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { YearlyFinancialReportResponse } from '@/modules/Core/types/api/dashboard'

export const QUERY_KEY = 'dashboard-yearly-financial'
const ENDPOINT = '/admin/Reports/yearly-financial'

export function fetchYearlyFinancialReport(branchId: number) {
  return axiosInstance
    .get<YearlyFinancialReportResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useYearlyFinancialReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchYearlyFinancialReport>>>,
    'queryKey' | 'queryFn' | 'enabled'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchYearlyFinancialReport(branchId.value),
    enabled: computed(() => branchId.value > 0),
    ...options,
  })
}
