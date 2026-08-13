import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PAYMENT_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PaymentCreatePayload } from '@/modules/Payment/types/api'

const ENDPOINT = `/admin/Orders/{id}/installment-payments`

export function createInstallmentPayment(
  orderId: number,
  payload: PaymentCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(orderId))

  return axiosInstance.post(endpoint, payload)
}

export function useCreateInstallmentPaymentMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      orderId: number
      payload: PaymentCreatePayload
      branchId?: number
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ orderId, payload }) =>
      createInstallmentPayment(orderId, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({
        queryKey: [
          QUERY_KEY_PAYMENT_LIST,
          variables.orderId,
          variables.branchId,
        ],
      })
    },
    meta: {
      success: {
        title: 'successCreate',
        description: 'successCreateDescription',
      },
      error: {
        title: 'payment.orderNotInProgressTitle',
        description: 'payment.orderNotInProgressDescription',
      },
    },
    ...options,
  })
}
