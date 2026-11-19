import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { DelegateFollowupEndpoints } from '../../domain/endpoints'
import type { DelegateSalesOrders } from '../../domain/types'
import { DELEGATE_FOLLOWUP_KEY } from './useListsQuery'
import { useAuthStore } from '@/modules/Auth/store'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export function useSalesOrdersQuery(
  listId: Ref<number | null>,
  branchId: Ref<number | null>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      DELEGATE_FOLLOWUP_KEY,
      'sales-orders',
      listId.value,
      branchId.value,
      auth.branchGeneration,
    ]),
    enabled: computed(
      () => (listId.value ?? 0) > 0 && (branchId.value ?? 0) > 0
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<DelegateSalesOrders>(
        DelegateFollowupEndpoints.salesOrders(listId.value as number),
        { params: { branchId: branchId.value }, signal }
      )
      return data
    },
  })
}
