import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SALES_REQUESTS_QUERY_KEY } from '../queries/useSalesRequestsQuery'
import { SALES_REQUEST_COUNTS_KEY } from '../queries/useSalesRequestCountsQuery'
import { SALES_REQUEST_BY_ID_KEY } from '../queries/useSalesRequestByIdQuery'
import { SALES_REQUEST_TIMELINE_KEY } from '../queries/useSalesRequestTimelineQuery'
import { UNASSIGNED_QUERY_KEY } from '../queries/useUnassignedSalesRequestsQuery'

export interface SalesManagerActionPayload {
  url: string
  body: Record<string, unknown>
  requestIds: number[]
}

export function useSalesManagerActionMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: SalesManagerActionPayload) => axiosInstance.post(payload.url, payload.body).then((r) => r.data),
    onSuccess: (_d, vars) => {
      void qc.invalidateQueries({ queryKey: [UNASSIGNED_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
      for (const id of vars.requestIds) {
        void qc.invalidateQueries({ queryKey: [SALES_REQUEST_BY_ID_KEY, id] })
        void qc.invalidateQueries({ queryKey: [SALES_REQUEST_TIMELINE_KEY, id] })
      }
    },
  })
}
