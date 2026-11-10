import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesRoutesEndpoints } from '../../domain/endpoints'
import type { SalesEmployeeRouteResponse } from '../../domain/types'

export const SALES_EMPLOYEE_ROUTE_KEY = 'sales-employee-route'

export function useSalesEmployeeRouteQuery(params: {
  employeeId: Ref<number | null>
  branchId: Ref<number | null>
  date: Ref<string>
  page: Ref<number>
  pageSize: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_EMPLOYEE_ROUTE_KEY,
      auth.user?.id ?? 'anon',
      params.employeeId.value ?? 'none',
      params.branchId.value ?? 'all',
      params.date.value,
      params.page.value,
      params.pageSize.value,
    ]),
    enabled: computed(
      () =>
        enabledSafe(params.enabled) &&
        params.employeeId.value != null &&
        params.employeeId.value > 0 &&
        !!params.date.value
    ),
    queryFn: async () => {
      const employeeId = params.employeeId.value!
      const query: Record<string, string | number> = {
        date: params.date.value,
        page: params.page.value,
        pageSize: params.pageSize.value,
      }
      if (params.branchId.value != null && params.branchId.value > 0) {
        query.branchId = params.branchId.value
      }
      const { data } = await axiosInstance.get<SalesEmployeeRouteResponse>(
        SalesRoutesEndpoints.route(employeeId),
        { params: query }
      )
      return data
    },
  })
}

function enabledSafe(enabled: Ref<boolean>) {
  return enabled.value === true
}
