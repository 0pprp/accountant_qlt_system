import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementSalesMapEndpoints } from '../../domain/endpoints'
import type { SalesMapProvinceCard } from '../../domain/types'

export const SALES_MAP_PROVINCES_KEY = 'management-sales-map-provinces'

export function useSalesMapProvincesQuery(enabled: { value: boolean }) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_MAP_PROVINCES_KEY,
      auth.branchGeneration,
    ]),
    enabled: computed(() => enabled.value),
    staleTime: 60_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<{ items: SalesMapProvinceCard[] }>(
        ManagementSalesMapEndpoints.provinces
      )
      return data.items ?? []
    },
  })
}
