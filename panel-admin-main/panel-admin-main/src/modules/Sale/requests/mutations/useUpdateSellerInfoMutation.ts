import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_SALE_LIST, QUERY_KEY_SALE_BY_ID } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SellerInfoPayload } from '@/modules/Sale/types/api'
import { QUERY_KEY as QUERY_KEY_CUSTOMER_ORDERS } from '@/modules/Customer/requests/queries/useCustomerOrdersQuery'
import { QUERY_KEY as QUERY_KEY_ORDER_INSTALLMENT_PAYMENTS } from '@/modules/Order/requests/queries/useOrderInstallmentPaymentsQuery'

const ENDPOINT = `/Admin/Orders/{id}/seller-info`

export function updateSellerInfo(
  id: number,
  payload: SellerInfoPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateSellerInfoMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: SellerInfoPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateSellerInfo(id, payload),
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
        title: 'successUpdate',
        description: 'successUpdateDescription',
      },
    },
    ...options,
  })
}
