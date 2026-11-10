import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesEmployeesEndpoints } from '../../domain/endpoints'
import type { SalesEmployeeRatingSummary } from '../../domain/types'

export const SALES_EMPLOYEE_RATING_KEY = 'sales-employee-rating'

export function useSalesEmployeeRatingSummaryQuery(
  employeeId: Ref<number | null>,
  branchId: Ref<number | null>,
  bucket: Ref<string | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_EMPLOYEE_RATING_KEY,
      auth.user?.id ?? 'anon',
      employeeId.value,
      branchId.value ?? 'all',
      bucket.value ?? 'none',
    ]),
    enabled: computed(
      () => enabled.value === true && employeeId.value != null && employeeId.value > 0
    ),
    queryFn: async () => {
      const id = employeeId.value!
      const params: Record<string, string | number> = {}
      if (branchId.value != null && branchId.value > 0) params.branchId = branchId.value
      if (bucket.value) params.bucket = bucket.value
      const { data } = await axiosInstance.get<SalesEmployeeRatingSummary>(
        SalesEmployeesEndpoints.ratingSummary(id),
        { params }
      )
      return data
    },
  })
}
