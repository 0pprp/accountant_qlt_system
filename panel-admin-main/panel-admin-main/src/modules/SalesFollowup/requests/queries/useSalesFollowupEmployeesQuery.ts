import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesFollowupEndpoints } from '../../domain/endpoints'
import type { SalesFollowupEmployeeCard } from '../../domain/types'

export const SALES_FOLLOWUP_EMPLOYEES_KEY = 'sales-followup-employees'

interface EmployeesResponse {
  items: SalesFollowupEmployeeCard[]
  marker: string
}

/** branchId = null → كل الفروع المصرّح بها */
export function useSalesFollowupEmployeesQuery(branchId: Ref<number | null>) {
  return useQuery({
    queryKey: computed(() => [SALES_FOLLOWUP_EMPLOYEES_KEY, branchId.value ?? 'all']),
    queryFn: async () => {
      const params =
        branchId.value != null && branchId.value > 0
          ? { branchId: branchId.value }
          : {}
      const { data } = await axiosInstance.get<EmployeesResponse>(
        SalesFollowupEndpoints.employees,
        { params }
      )
      return data
    },
  })
}
