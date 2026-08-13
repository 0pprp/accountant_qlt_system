import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_ORDER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { OrderCreatePayload } from '@/modules/Order/types/api'

const ENDPOINT = `/Admin/OrderLists/{id}`

export function updateOrder(
  id: number,
  payload: OrderCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateOrderMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: OrderCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateOrder(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ORDER_LIST],
      })
    },
    meta: {
      success: {
        title: 'order.updateTitleSuccess',
        description: 'order.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
