import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { DelegateFollowupEndpoints } from '../../domain/endpoints'
import type { DelegateFollowupListsResponse } from '../../domain/types'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export const DELEGATE_FOLLOWUP_KEY = 'delegate-followup'

export function useListsQuery(branchId: Ref<number | null>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      DELEGATE_FOLLOWUP_KEY,
      'lists',
      branchId.value,
      auth.branchGeneration,
    ]),
    enabled: computed(() => branchId.value != null && branchId.value > 0),
    queryFn: async () => {
      const { data } = await axiosInstance.get<DelegateFollowupListsResponse>(
        DelegateFollowupEndpoints.lists,
        { params: { branchId: branchId.value } }
      )
      return data
    },
  })
}
