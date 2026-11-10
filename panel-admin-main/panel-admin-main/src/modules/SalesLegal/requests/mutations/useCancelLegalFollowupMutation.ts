import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesLegalEndpoints } from '../../domain/endpoints'
import { LEGAL_FOLLOWUPS_KEY } from '../queries/useLegalFollowupsQuery'

export function useCancelLegalFollowupMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      caseId: number
      reason: string
      expectedVersion?: number
    }) => {
      const { data } = await axiosInstance.post(
        SalesLegalEndpoints.cancel(payload.caseId),
        {
          reason: payload.reason,
          expectedVersion: payload.expectedVersion,
        }
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [LEGAL_FOLLOWUPS_KEY] })
    },
  })
}
