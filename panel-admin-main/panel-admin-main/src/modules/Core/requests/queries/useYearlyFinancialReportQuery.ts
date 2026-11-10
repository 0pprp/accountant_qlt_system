import type { Ref } from 'vue'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { YearlyFinancialReportResponse } from '@/modules/Core/types/api/dashboard'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'

export const QUERY_KEY = 'dashboard-yearly-financial'
const ENDPOINT = '/admin/Reports/yearly-financial'

export function fetchYearlyFinancialReport(
  branchId: number,
  signal?: AbortSignal
) {
  return axiosInstance
    .get<YearlyFinancialReportResponse>(ENDPOINT, {
      params: { branchId },
      signal,
    })
    .then((response) => response.data)
}

export function useYearlyFinancialReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      Awaited<ReturnType<typeof fetchYearlyFinancialReport>>
    >,
    'queryKey' | 'queryFn'
  > & { enabled?: MaybeRefOrGetter<boolean> }
) {
  const authStore = useAuthStore()
  const { can, permissionsReady } = usePermission()
  const extraEnabled = computed(() => toValue(options?.enabled) ?? true)

  return useQuery({
    ...options,
    queryKey: [
      QUERY_KEY,
      branchId,
      computed(() => authStore.branchGeneration),
      computed(() => authStore.sessionGeneration),
    ],
    queryFn: ({ signal }) =>
      fetchYearlyFinancialReport(branchId.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        branchId.value > 0 &&
        can('Report', 'Read') &&
        extraEnabled.value
    ),
  })
}
