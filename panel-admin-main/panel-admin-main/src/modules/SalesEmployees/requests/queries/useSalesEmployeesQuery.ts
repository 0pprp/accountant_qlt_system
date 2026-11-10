import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesEmployeesEndpoints } from '../../domain/endpoints'
import type { ListSalesEmployeesResponse } from '../../domain/types'

export const SALES_EMPLOYEES_KEY = 'sales-employees'

export type SalesEmployeesFilters = {
  branchId: number | null
  search: string
  shiftState: string
  page: number
  pageSize: number
  sort: string
}

export function useSalesEmployeesQuery(
  filters: Ref<SalesEmployeesFilters>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_EMPLOYEES_KEY,
      auth.user?.id ?? 'anon',
      filters.value.branchId ?? 'all',
      filters.value.search,
      filters.value.shiftState || 'any',
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
        sort: f.sort || 'name',
      }
      if (f.branchId != null && f.branchId > 0) params.branchId = f.branchId
      if (f.search.trim()) params.search = f.search.trim()
      if (f.shiftState) params.shiftState = f.shiftState
      const { data } = await axiosInstance.get<ListSalesEmployeesResponse>(
        SalesEmployeesEndpoints.list,
        { params }
      )
      return data
    },
  })
}
