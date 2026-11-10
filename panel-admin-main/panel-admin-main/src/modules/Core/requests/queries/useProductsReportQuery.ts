import type { Ref } from 'vue'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductsReportResponse } from '@/modules/Core/types/api/dashboard'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'

export const QUERY_KEY = 'dashboard-products-report'
const ENDPOINT = '/admin/Reports/products'

export function fetchProductsReport(branchId: number, signal?: AbortSignal) {
  return axiosInstance
    .get<ProductsReportResponse>(ENDPOINT, {
      params: { branchId },
      signal,
    })
    .then((response) => response.data)
}

export function useProductsReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchProductsReport>>>,
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
    queryFn: ({ signal }) => fetchProductsReport(branchId.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        branchId.value > 0 &&
        can('Report', 'Read') &&
        extraEnabled.value
    ),
  })
}
