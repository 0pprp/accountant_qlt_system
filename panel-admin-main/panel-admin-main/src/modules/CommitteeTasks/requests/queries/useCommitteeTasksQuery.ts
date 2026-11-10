import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { CommitteeTasksEndpoints } from '../../domain/endpoints'
import { COMMITTEE_TASKS_LIST_KEY } from '../../domain/queryKeys'
import type { CommitteeTaskRow } from '../../domain/types'

export function useCommitteeTasksQuery(enabled: Ref<boolean>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      ...COMMITTEE_TASKS_LIST_KEY,
      auth.branchGeneration,
      auth.sessionGeneration,
    ]),
    enabled: computed(
      () => enabled.value === true && auth.permissionsReady === true
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<CommitteeTaskRow[]>(
        CommitteeTasksEndpoints.list,
        { signal }
      )
      return data
    },
  })
}
