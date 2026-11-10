import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesOverviewEndpoints } from '../../domain/endpoints'
import type { SalesOverviewResponse } from '../../domain/types'

export const SALES_OVERVIEW_KEY = 'sales-overview'

export function useSalesOverviewQuery(branchId: Ref<number | null>, enabled: Ref<boolean>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_OVERVIEW_KEY,
      auth.user?.id ?? 'anon',
      branchId.value ?? 'all',
      (auth.branches ?? []).map((b) => b.id).join(','),
    ]),
    enabled: computed(() => enabled.value === true),
    queryFn: async () => {
      const params =
        branchId.value != null && branchId.value > 0
          ? { branchId: branchId.value }
          : {}
      const { data } = await axiosInstance.get<SalesOverviewResponse>(
        SalesOverviewEndpoints.overview,
        { params }
      )
      return data
    },
  })
}
