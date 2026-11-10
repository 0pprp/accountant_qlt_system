import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesRecordsEndpoints } from '../../domain/endpoints'
import type { ListSalesRecordsResponse } from '../../domain/types'

export const SALES_RECORDS_KEY = 'sales-records'

export type SalesRecordsFilters = {
  branchId: number | null
  sellerId: number | null
  search: string
  saleDateFrom: string
  saleDateTo: string
  page: number
  pageSize: number
  sort: string
}

export function useSalesRecordsQuery(
  filters: Ref<SalesRecordsFilters>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_RECORDS_KEY,
      auth.user?.id ?? 'anon',
      filters.value.branchId ?? 'all',
      filters.value.sellerId ?? 'all',
      filters.value.search,
      filters.value.saleDateFrom || 'any',
      filters.value.saleDateTo || 'any',
      filters.value.page,
      filters.value.pageSize,
      filters.value.sort,
    ]),
    enabled: computed(() => enabled.value === true),
    queryFn: async () => {
      const f = filters.value
      const params: Record<string, string | number> = {
        page: f.page,
        pageSize: f.pageSize,
        sort: f.sort || 'saleDate',
      }
      if (f.branchId != null && f.branchId > 0) params.branchId = f.branchId
      if (f.sellerId != null && f.sellerId > 0) params.sellerId = f.sellerId
      if (f.search.trim()) params.search = f.search.trim()
      if (f.saleDateFrom) params.saleDateFrom = f.saleDateFrom
      if (f.saleDateTo) params.saleDateTo = f.saleDateTo
      const { data } = await axiosInstance.get<ListSalesRecordsResponse>(
        SalesRecordsEndpoints.list,
        { params }
      )
      return data
    },
  })
}
