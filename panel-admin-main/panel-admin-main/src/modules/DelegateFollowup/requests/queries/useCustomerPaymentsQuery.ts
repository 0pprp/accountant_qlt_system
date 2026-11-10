import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { useAuthStore } from '@/modules/Auth/store'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { DelegateFollowupEndpoints } from '../../domain/endpoints'
import type { DelegateFollowupCustomerPayments } from '../../domain/types'
import { DELEGATE_FOLLOWUP_KEY } from './useListsQuery'

export function useCustomerPaymentsQuery(
  listId: Ref<number | null>,
  branchId: Ref<number | null>,
  customerId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      DELEGATE_FOLLOWUP_KEY,
      'payments',
      listId.value,
      branchId.value,
      customerId.value,
      auth.branchGeneration,
    ]),
    enabled: computed(
      () =>
        enabled.value === true &&
        (listId.value ?? 0) > 0 &&
        (branchId.value ?? 0) > 0 &&
        (customerId.value ?? 0) > 0
    ),
    queryFn: async () => {
      const { data } = await axiosInstance.get<DelegateFollowupCustomerPayments>(
        DelegateFollowupEndpoints.customerPayments(
          listId.value as number,
          customerId.value as number
        ),
        { params: { branchId: branchId.value } }
      )
      return data
    },
  })
}
