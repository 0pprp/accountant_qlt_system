import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_SALE_LIST, QUERY_KEY_SALE_BY_ID } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ChangeOrderApprovalStatusPayload } from '@/modules/Sale/types/api'
import { QUERY_KEY as QUERY_KEY_CUSTOMER_ORDERS } from '@/modules/Customer/requests/queries/useCustomerOrdersQuery'
import { QUERY_KEY as QUERY_KEY_ORDER_INSTALLMENT_PAYMENTS } from '@/modules/Order/requests/queries/useOrderInstallmentPaymentsQuery'

const ENDPOINT = `/Admin/Orders/{id}/approval-status`

export function updateOrderApprovalStatus(
  id: number,
  payload: ChangeOrderApprovalStatusPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.patch(endpoint, payload)
}

export function useUpdateOrderApprovalStatusMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: ChangeOrderApprovalStatusPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateOrderApprovalStatus(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SALE_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SALE_BY_ID],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_CUSTOMER_ORDERS],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ORDER_INSTALLMENT_PAYMENTS],
      })
      await queryClient.invalidateQueries({
        queryKey: ['orders-by-customer'],
      })
    },
    meta: {
      success: {
        title: 'sale.approvalActions.successTitle',
        description: 'sale.approvalActions.successDescription',
      },
    },
    ...options,
  })
}
