import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PAYMENT_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'

const ENDPOINT = `/admin/InstallmentPayments/{id}`

export function deletePayment(id: number): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance.delete(endpoint)
}

export function useDeletePaymentMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      id: number
      orderId?: number
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id }) => deletePayment(id),
    onSuccess: async (_, variables) => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PAYMENT_LIST, variables.orderId],
      })
    },
    meta: {
      success: {
        title: 'payment.successDelete',
        description: 'payment.successDeleteDescription',
      },
      error: {
        title: 'payment.orderNotInProgressTitle',
        description: 'payment.orderNotInProgressDescription',
      },
    },
    ...options,
  })
}
