import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { FOLLOWER_WORK_KEY } from '../../domain/queryKeys'

export function useCancelLegalFollowupMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      caseId: number
      reason: string
      expectedVersion: number
    }) => {
      const { data } = await axiosInstance.post(
        `/admin/LegalFollowups/${payload.caseId}/cancel`,
        {
          reason: payload.reason,
          expectedVersion: payload.expectedVersion,
        }
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [FOLLOWER_WORK_KEY, 'old-customers'] })
    },
  })
}
