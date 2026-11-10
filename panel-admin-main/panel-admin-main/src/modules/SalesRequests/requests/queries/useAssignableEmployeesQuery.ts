import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import type { AssignableEmployee } from '@/modules/SalesRequests/types'

export const ASSIGNABLE_EMPLOYEES_KEY = 'sales-request-assignable'

export function fetchAssignableEmployees(branchId: number, search?: string) {
  return axiosInstance
    .get<AssignableEmployee[]>(SalesRequestEndpoints.assignableEmployees, {
      params: {
        branchId,
        search: search || undefined,
      },
    })
    .then((r) => r.data)
}

export function useAssignableEmployeesQuery(
  branchId: Ref<number | null>,
  search: Ref<string>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [ASSIGNABLE_EMPLOYEES_KEY, branchId, search],
    queryFn: () =>
      fetchAssignableEmployees(branchId.value!, search.value || undefined),
    enabled,
  })
}
