import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PAYMENT_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PaymentUpdatePayload } from '@/modules/Payment/types/api'

const ENDPOINT = `/admin/InstallmentPayments/{id}`

export function updatePayment(
  id: number,
  payload: PaymentUpdatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdatePaymentMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      id: number
      payload: PaymentUpdatePayload
      orderId?: number
      branchId?: number
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updatePayment(id, payload),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PAYMENT_LIST, variables.orderId],
      })
    },
    meta: {
      success: {
        title: 'successUpdate',
        description: 'successUpdateDescription',
      },
      error: {
        title: 'payment.orderNotInProgressTitle',
        description: 'payment.orderNotInProgressDescription',
      },
    },
    ...options,
  })
}
