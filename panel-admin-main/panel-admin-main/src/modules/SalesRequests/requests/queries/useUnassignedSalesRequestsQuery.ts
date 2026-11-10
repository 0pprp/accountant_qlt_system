import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UnassignedSalesRequest } from '@/modules/SalesRequests/types'

export const UNASSIGNED_QUERY_KEY = 'sales-requests-unassigned'

export interface UnassignedFilters {
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

export function fetchUnassigned(
  pageIndex: number,
  pageSize: number,
  filters: UnassignedFilters
) {
  return axiosInstance
    .get<Paginated<UnassignedSalesRequest>>('/admin/SalesRequests/unassigned', {
      params: {
        pageIndex,
        pageSize,
        branchId: filters.branchId || undefined,
        provinceId: filters.provinceId || undefined,
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

export function useUnassignedSalesRequestsQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  filters: Ref<UnassignedFilters>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [UNASSIGNED_QUERY_KEY, pageIndex, pageSize, filters],
    queryFn: ({ signal }) =>
      fetchUnassigned(pageIndex.value, pageSize.value, filters.value).then(
        (data) => data
      ),
    enabled,
  })
}
