import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type {
  CreateManagementTaskPayload,
  ManagementTaskActionResult,
} from '../../domain/types'
import { MANAGEMENT_TASKS_LIST_KEY } from '../queries/useManagementTasksQuery'

export function useCreateManagementTaskMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: CreateManagementTaskPayload) => {
      const { data } = await axiosInstance.post<ManagementTaskActionResult>(
        ManagementTaskEndpoints.list,
        payload
      )
      return data
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASKS_LIST_KEY] })
    },
  })
}
