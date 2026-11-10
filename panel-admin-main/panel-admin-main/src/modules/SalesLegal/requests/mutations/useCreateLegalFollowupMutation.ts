import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesLegalEndpoints } from '../../domain/endpoints'
import { LEGAL_FOLLOWUPS_KEY } from '../queries/useLegalFollowupsQuery'
import type { LegalFollowupCaseRow } from '../../domain/types'

export function useCreateLegalFollowupMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (payload: {
      salesRequestId: number
      followerUserId: number
      note?: string
      expectedConcurrencyStamp?: number
    }) => {
      const { data } = await axiosInstance.post<LegalFollowupCaseRow>(
        SalesLegalEndpoints.create,
        payload
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [LEGAL_FOLLOWUPS_KEY] })
    },
  })
}
