import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type { ManagementTaskDetail } from '../../domain/types'

export const MANAGEMENT_TASK_DETAIL_KEY = 'management-task-detail'

export function useManagementTaskDetailQuery(taskId: Ref<number | null>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      MANAGEMENT_TASK_DETAIL_KEY,
      auth.selectedBranch?.id ?? null,
      auth.branchGeneration,
      taskId.value,
    ]),
    enabled: computed(() => taskId.value != null && taskId.value > 0),
    staleTime: 0,
    queryFn: async () => {
      const { data } = await axiosInstance.get<ManagementTaskDetail>(
        ManagementTaskEndpoints.detail(taskId.value!)
      )
      return data
    },
  })
}
