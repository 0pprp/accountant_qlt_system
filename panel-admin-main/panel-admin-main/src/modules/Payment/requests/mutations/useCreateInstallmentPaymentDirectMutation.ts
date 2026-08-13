import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PaymentCreatePayload } from '@/modules/Payment/types/api'

const ENDPOINT = `/admin/InstallmentPayments`

export function createInstallmentPaymentDirect(
  payload: PaymentCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateInstallmentPaymentDirectMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    PaymentCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload) => createInstallmentPaymentDirect(payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['order-list-report'],
      })
      await queryClient.invalidateQueries({
        queryKey: ['payment-list'],
      })
    },

    meta: {
      success: {
        title: 'payment.createTitleSuccess',
        description: 'payment.createDescriptionSuccess',
      },
      error: {
        title: 'payment.orderNotInProgressTitle',
        description: 'payment.orderNotInProgressDescription',
      },
    },
    ...options,
  })
}
