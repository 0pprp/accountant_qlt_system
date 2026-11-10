import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesFollowupEndpoints } from '../../domain/endpoints'
import type { SalesFollowupDetail } from '../../domain/types'

export const SALES_FOLLOWUP_DETAIL_KEY = 'sales-followup-detail'

export function useSalesFollowupDetailQuery(
  employeeId: Ref<number | null>,
  branchId: Ref<number | null>
) {
  return useQuery({
    queryKey: computed(() => [
      SALES_FOLLOWUP_DETAIL_KEY,
      employeeId.value,
      branchId.value,
    ]),
    enabled: computed(
      () =>
        employeeId.value != null &&
        employeeId.value > 0 &&
        branchId.value != null &&
        branchId.value > 0
    ),
    staleTime: 60_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<SalesFollowupDetail>(
        SalesFollowupEndpoints.detail(employeeId.value!),
        { params: { branchId: branchId.value } }
      )
      return data
    },
  })
}
