import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type { ManagementTaskAssigneeOption } from '../../domain/types'

export const MANAGEMENT_TASK_ASSIGNEES_KEY = 'management-task-assignees'

export function useManagementTaskAssigneesQuery(
  branchId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      MANAGEMENT_TASK_ASSIGNEES_KEY,
      auth.branchGeneration,
      branchId.value,
    ]),
    enabled: computed(
      () => enabled.value && branchId.value != null && branchId.value > 0
    ),
    staleTime: 60_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<ManagementTaskAssigneeOption[]>(
        ManagementTaskEndpoints.assigneeOptions,
        { params: { branchId: branchId.value } }
      )
      return data
    },
  })
}
