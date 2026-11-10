import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import type { SalesRequestListItem } from '@/modules/SalesRequests/types'

export const SALES_REQUESTS_QUERY_KEY = 'sales-requests-list'

export interface SalesRequestListFilters {
  tab: string
  branchId?: number | null
  provinceId?: number | null
  employeeId?: number | null
  sourceType?: string | null
  fromDate?: string | null
  toDate?: string | null
  search?: string | null
}

interface Paginated<T> {
  items: T[]
  totalCount: number
  pageIndex: number
  pageSize: number
}

export function fetchSalesRequests(
  pageIndex: number,
  pageSize: number,
  filters: SalesRequestListFilters
) {
  return axiosInstance
    .get<Paginated<SalesRequestListItem>>(SalesRequestEndpoints.list, {
      params: {
        pageIndex,
        pageSize,
        tab: filters.tab,
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

export function useSalesRequestsQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  filters: Ref<SalesRequestListFilters>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [SALES_REQUESTS_QUERY_KEY, pageIndex, pageSize, filters],
    queryFn: () =>
      fetchSalesRequests(pageIndex.value, pageSize.value, filters.value),
    enabled,
  })
}
