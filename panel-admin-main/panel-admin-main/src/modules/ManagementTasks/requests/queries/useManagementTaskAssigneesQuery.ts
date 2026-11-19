import { computed, ref, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type { ManagementTaskAssigneeOption } from '../../domain/types'

export const MANAGEMENT_TASK_ASSIGNEES_KEY = 'management-task-assignees'

export function useManagementTaskAssigneesQuery(
  branchId: Ref<number | null>,
  enabled: Ref<boolean>,
  audience: Ref<string | null> = ref(null)
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      MANAGEMENT_TASK_ASSIGNEES_KEY,
      auth.branchGeneration,
      branchId.value,
      audience.value,
    ]),
    enabled: computed(
      () => enabled.value && branchId.value != null && branchId.value > 0
    ),
    staleTime: 60_000,
    queryFn: async () => {
      const transfer = audience.value === 'branchManager'
      const { data } = await axiosInstance.get<ManagementTaskAssigneeOption[]>(
        transfer ? ManagementTaskEndpoints.transferTargets : ManagementTaskEndpoints.assigneeOptions,
        {
          params: transfer
            ? { branchId: branchId.value }
            : { branchId: branchId.value, audience: audience.value || undefined },
        }
      )
      return data
    },
  })
}
