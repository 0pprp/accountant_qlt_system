import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import type { SalesRequestCountsResponse } from '@/modules/SalesRequests/types'
import type { SalesRequestListFilters } from './useSalesRequestsQuery'

export const SALES_REQUEST_COUNTS_KEY = 'sales-requests-counts'

export function fetchSalesRequestCounts(
  filters: Omit<SalesRequestListFilters, 'tab'>
) {
  return axiosInstance
    .get<SalesRequestCountsResponse>(SalesRequestEndpoints.counts, {
      params: {
        branchId: filters.branchId || undefined,
        provinceId: filters.provinceId || undefined,
        employeeId: filters.employeeId || undefined,
        sourceType:
          filters.sourceType === '' || filters.sourceType == null
            ? undefined
            : Number(filters.sourceType),
        fromDate: filters.fromDate || undefined,
        toDate: filters.toDate || undefined,
        search: filters.search || undefined,
      },
    })
    .then((r) => r.data)
}

export function useSalesRequestCountsQuery(
  filters: Ref<Omit<SalesRequestListFilters, 'tab'>>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [SALES_REQUEST_COUNTS_KEY, filters],
    queryFn: () => fetchSalesRequestCounts(filters.value),
    enabled,
    staleTime: 30_000,
  })
}
