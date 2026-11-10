import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type { ManagementTaskActionResult } from '../../domain/types'
import { MANAGEMENT_TASKS_LIST_KEY } from '../queries/useManagementTasksQuery'
import { MANAGEMENT_TASK_DETAIL_KEY } from '../queries/useManagementTaskDetailQuery'

export type ManagementTaskActionKind =
  | 'accept'
  | 'complete'
  | 'confirm'
  | 'reject'
  | 'transfer'

export interface ManagementTaskActionArgs {
  taskId: number
  action: ManagementTaskActionKind
  expectedVersion: number
  note?: string
  targetUserId?: number
}

function endpointFor(action: ManagementTaskActionKind, taskId: number) {
  switch (action) {
    case 'accept':
      return ManagementTaskEndpoints.accept(taskId)
    case 'complete':
      return ManagementTaskEndpoints.complete(taskId)
    case 'confirm':
      return ManagementTaskEndpoints.confirm(taskId)
    case 'reject':
      return ManagementTaskEndpoints.reject(taskId)
    case 'transfer':
      return ManagementTaskEndpoints.transfer(taskId)
  }
}

function bodyFor(args: ManagementTaskActionArgs) {
  switch (args.action) {
    case 'reject':
      return { expectedVersion: args.expectedVersion, note: args.note ?? '' }
    case 'transfer':
      return {
        expectedVersion: args.expectedVersion,
        targetUserId: args.targetUserId,
        note: args.note || undefined,
      }
    default:
      return { expectedVersion: args.expectedVersion }
  }
}

export function useManagementTaskActionMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: ManagementTaskActionArgs) => {
      const { data } = await axiosInstance.post<ManagementTaskActionResult>(
        endpointFor(args.action, args.taskId),
        bodyFor(args)
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
