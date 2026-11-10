import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { CommitteeTasksEndpoints } from '../../domain/endpoints'
import { COMMITTEE_TASKS_LIST_KEY } from '../../domain/queryKeys'
import type { CommitteeTaskRow, CommitteeTaskUpsertBody } from '../../domain/types'

export function useCreateCommitteeTaskMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (body: CommitteeTaskUpsertBody) => {
      const { data } = await axiosInstance.post<CommitteeTaskRow>(
        CommitteeTasksEndpoints.create,
        body
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [...COMMITTEE_TASKS_LIST_KEY] })
    },
  })
}

export function useUpdateCommitteeTaskMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: { id: number; body: CommitteeTaskUpsertBody }) => {
      const { data } = await axiosInstance.put<CommitteeTaskRow>(
        CommitteeTasksEndpoints.update(args.id),
        args.body
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [...COMMITTEE_TASKS_LIST_KEY] })
    },
  })
}

export function useDeleteCommitteeTaskMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: { id: number; expectedVersion: number }) => {
      await axiosInstance.delete(CommitteeTasksEndpoints.remove(args.id), {
        params: { expectedVersion: args.expectedVersion },
      })
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [...COMMITTEE_TASKS_LIST_KEY] })
    },
  })
}
