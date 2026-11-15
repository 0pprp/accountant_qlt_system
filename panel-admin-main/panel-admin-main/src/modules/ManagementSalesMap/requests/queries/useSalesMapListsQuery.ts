import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementSalesMapEndpoints } from '../../domain/endpoints'
import type { SalesMapListOption } from '../../domain/types'

export const SALES_MAP_LISTS_KEY = 'management-sales-map-lists'

export function useSalesMapListsQuery(
  provinceId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_MAP_LISTS_KEY,
      auth.branchGeneration,
      provinceId.value,
    ]),
    enabled: computed(
      () => enabled.value && provinceId.value != null && provinceId.value > 0
    ),
    staleTime: 60_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<{ items: SalesMapListOption[] }>(
        ManagementSalesMapEndpoints.lists(provinceId.value!)
      )
      return data.items ?? []
    },
  })
}
