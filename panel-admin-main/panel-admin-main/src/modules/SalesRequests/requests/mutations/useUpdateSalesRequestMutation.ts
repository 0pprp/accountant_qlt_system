import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import { UNASSIGNED_QUERY_KEY } from '../queries/useUnassignedSalesRequestsQuery'
import { SALES_REQUESTS_QUERY_KEY } from '../queries/useSalesRequestsQuery'
import { SALES_REQUEST_COUNTS_KEY } from '../queries/useSalesRequestCountsQuery'
import { SALES_REQUEST_BY_ID_KEY } from '../queries/useSalesRequestByIdQuery'
import { SALES_REQUEST_TIMELINE_KEY } from '../queries/useSalesRequestTimelineQuery'

export function useUpdateSalesRequestMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: {
      requestId: number
      customerName: string
      customerPhone: string
      address?: string
      requestedProduct?: string
      branchId: number
      provinceId?: number | null
      employeeId?: number | null
      reason?: string
    }) =>
      axiosInstance.put(SalesRequestEndpoints.update(payload.requestId), {
        customerName: payload.customerName,
        customerPhone: payload.customerPhone,
        address: payload.address,
        requestedProduct: payload.requestedProduct,
        branchId: payload.branchId,
        provinceId: payload.provinceId ?? undefined,
        employeeId: payload.employeeId ?? undefined,
        reason: payload.reason,
      }),
    onSuccess: (_data, vars) => {
      void qc.invalidateQueries({ queryKey: [UNASSIGNED_QUERY_KEY] })
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
