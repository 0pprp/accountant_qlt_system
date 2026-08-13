import type { Ref } from 'vue'
import { computed } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductsReportResponse } from '@/modules/Core/types/api/dashboard'

export const QUERY_KEY = 'dashboard-products-report'
const ENDPOINT = '/admin/Reports/products'

export function fetchProductsReport(branchId: number) {
  return axiosInstance
    .get<ProductsReportResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useProductsReportQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchProductsReport>>>,
    'queryKey' | 'queryFn' | 'enabled'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchProductsReport(branchId.value),
    enabled: computed(() => branchId.value > 0),
    ...options,
  })
}
