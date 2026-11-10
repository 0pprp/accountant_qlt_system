import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import { SALES_REQUESTS_QUERY_KEY } from '../queries/useSalesRequestsQuery'
import { SALES_REQUEST_COUNTS_KEY } from '../queries/useSalesRequestCountsQuery'
import { SALES_REQUEST_BY_ID_KEY } from '../queries/useSalesRequestByIdQuery'
import { SALES_REQUEST_TIMELINE_KEY } from '../queries/useSalesRequestTimelineQuery'

export function useRestoreMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: { requestId: number; reason?: string }) =>
      axiosInstance.post(SalesRequestEndpoints.restore(payload.requestId), {
        reason: payload.reason,
      }),
    onSuccess: (_d, vars) => {
      void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
      void qc.invalidateQueries({
        queryKey: [SALES_REQUEST_BY_ID_KEY, vars.requestId],
      })
      void qc.invalidateQueries({
        queryKey: [SALES_REQUEST_TIMELINE_KEY, vars.requestId],
      })
    },
  })
}
