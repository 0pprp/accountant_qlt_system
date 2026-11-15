import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type {
  CreateManagementTaskFanoutPayload,
  ManagementTaskFanoutResult,
} from '../../domain/types'
import { MANAGEMENT_TASKS_LIST_KEY } from '../queries/useManagementTasksQuery'

export function useCreateManagementTaskFanoutMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: CreateManagementTaskFanoutPayload) => {
      const { data } = await axiosInstance.post<ManagementTaskFanoutResult>(
        ManagementTaskEndpoints.fanout,
        payload
      )
      return data
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASKS_LIST_KEY] })
    },
  })
}
