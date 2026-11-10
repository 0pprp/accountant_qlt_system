import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesFollowupEndpoints } from '../../domain/endpoints'
import type { SalesFollowupAppBlock } from '../../domain/types'
import { SALES_FOLLOWUP_DETAIL_KEY } from '../queries/useSalesFollowupDetailQuery'

export function useSetAppBlockMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (args: {
      employeeId: number
      branchId: number
      isBlocked: boolean
    }) => {
      const { data } = await axiosInstance.put<SalesFollowupAppBlock>(
        SalesFollowupEndpoints.appBlock(args.employeeId),
        { isBlocked: args.isBlocked },
        { params: { branchId: args.branchId } }
      )
      return data
    },
    onSuccess: (_data, vars) => {
      void qc.invalidateQueries({
        queryKey: [SALES_FOLLOWUP_DETAIL_KEY, vars.employeeId, vars.branchId],
      })
    },
  })
}
