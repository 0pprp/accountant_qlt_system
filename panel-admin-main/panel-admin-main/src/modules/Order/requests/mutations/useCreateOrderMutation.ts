import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_ORDER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { OrderCreatePayload } from '@/modules/Order/types/api'

const ENDPOINT = '/admin/OrderLists'

export function createOrder(
  payload: OrderCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateOrderMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    OrderCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createOrder,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ORDER_LIST],
      })
    },
    meta: {
      success: {
        title: 'order.createTitleSuccess',
        description: 'order.createDescriptionSuccess',
      },
    },
    ...options,
  })
}
