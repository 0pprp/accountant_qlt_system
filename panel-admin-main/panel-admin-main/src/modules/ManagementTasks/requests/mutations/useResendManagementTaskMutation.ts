import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type {
  ManagementTaskActionResult,
  ResendManagementTaskPayload,
} from '../../domain/types'
import { MANAGEMENT_TASKS_LIST_KEY } from '../queries/useManagementTasksQuery'
import { MANAGEMENT_TASK_DETAIL_KEY } from '../queries/useManagementTaskDetailQuery'

export function useResendManagementTaskMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: {
      taskId: number
      payload: ResendManagementTaskPayload
    }) => {
      const { data } = await axiosInstance.post<ManagementTaskActionResult>(
        ManagementTaskEndpoints.resend(args.taskId),
        args.payload
      )
      return data
    },
    onSettled: (_data, _error, vars) => {
      void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASKS_LIST_KEY] })
      void qc.invalidateQueries({
        queryKey: [MANAGEMENT_TASK_DETAIL_KEY],
        predicate: (q) => q.queryKey.includes(vars.taskId),
      })
    },
  })
}
