import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesLiveMapEndpoints } from '../../domain/endpoints'
import type { SalesEmployeeLiveLocationsResponse } from '../../domain/types'

export const SALES_LIVE_LOCATIONS_KEY = 'sales-employee-live-locations'

export function useSalesEmployeeLiveLocationsQuery(
  branchId: Ref<number | null>,
  employeeId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_LIVE_LOCATIONS_KEY,
      auth.user?.id ?? 'anon',
      branchId.value ?? 'all',
      employeeId.value ?? 'all',
    ]),
    enabled: computed(() => enabled.value === true),
    refetchInterval: false,
    queryFn: async () => {
      const params: Record<string, number> = {}
      if (branchId.value != null && branchId.value > 0) params.branchId = branchId.value
      if (employeeId.value != null && employeeId.value > 0) params.employeeId = employeeId.value
      const { data } = await axiosInstance.get<SalesEmployeeLiveLocationsResponse>(
        SalesLiveMapEndpoints.latest,
        { params }
      )
      return data
    },
  })
}
