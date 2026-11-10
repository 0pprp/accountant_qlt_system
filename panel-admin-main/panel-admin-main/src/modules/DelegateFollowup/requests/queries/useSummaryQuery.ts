import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { DelegateFollowupEndpoints } from '../../domain/endpoints'
import type { DelegateFollowupSummary } from '../../domain/types'
import { DELEGATE_FOLLOWUP_KEY } from './useListsQuery'
import { useAuthStore } from '@/modules/Auth/store'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export function useSummaryQuery(
  listId: Ref<number | null>,
  branchId: Ref<number | null>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      DELEGATE_FOLLOWUP_KEY,
      'summary',
      listId.value,
      branchId.value,
      auth.branchGeneration,
    ]),
    enabled: computed(
      () =>
        listId.value != null &&
        listId.value > 0 &&
        branchId.value != null &&
        branchId.value > 0
    ),
    queryFn: async () => {
      const { data } = await axiosInstance.get<DelegateFollowupSummary>(
        DelegateFollowupEndpoints.summary(listId.value as number),
        { params: { branchId: branchId.value } }
      )
      return data
    },
  })
}
