import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { DelegateFollowupEndpoints } from '../../domain/endpoints'
import type {
  DelegateFollowupActionResult,
  UpsertBoardRatingPayload,
} from '../../domain/types'
import { DELEGATE_FOLLOWUP_KEY } from '../queries/useListsQuery'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export function useUpsertBoardRatingMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: UpsertBoardRatingPayload) => {
      const { data } = await axiosInstance.post<DelegateFollowupActionResult>(
        DelegateFollowupEndpoints.boardRating(args.listId),
        { score: args.score, note: args.note },
        { params: { branchId: args.branchId } }
      )
      return data
    },
    onSuccess: (_data, vars) => {
      void qc.invalidateQueries({
        queryKey: [DELEGATE_FOLLOWUP_KEY, 'summary', vars.listId, vars.branchId],
      })
    },
  })
}
