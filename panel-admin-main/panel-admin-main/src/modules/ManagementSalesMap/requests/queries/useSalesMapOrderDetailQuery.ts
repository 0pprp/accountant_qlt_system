import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementSalesMapEndpoints } from '../../domain/endpoints'
import type { SalesMapOrderDetail } from '../../domain/types'

export const SALES_MAP_ORDER_DETAIL_KEY = 'management-sales-map-order-detail'

export function useSalesMapOrderDetailQuery(orderId: Ref<number | null>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_MAP_ORDER_DETAIL_KEY,
      auth.branchGeneration,
      orderId.value,
    ]),
    enabled: computed(() => orderId.value != null && orderId.value > 0),
    staleTime: 15_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<SalesMapOrderDetail>(
        ManagementSalesMapEndpoints.orderDetail(orderId.value!)
      )
      return data
    },
  })
}
