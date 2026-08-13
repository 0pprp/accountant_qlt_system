import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_SAFE_SELLERS_LIST, QUERY_KEY_SAFE_CURRENT } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CreateSomeSellersCashDeliveriesPayload } from '@/modules/Safe/types/api'

const ENDPOINT = '/admin/Safes/{id}/cash-deliveries'

export function createSomeSellersCashDeliveries(
  id: number,
  payload: CreateSomeSellersCashDeliveriesPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance.post<{
    id: number
  }>(endpoint, payload)
}

export function useCreateSomeSellersCashDeliveriesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: CreateSomeSellersCashDeliveriesPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) =>
      createSomeSellersCashDeliveries(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_SELLERS_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_CURRENT],
      })
    },
    meta: {
      success: {
        title: 'branch.updateTitleSuccess',
        description: 'branch.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
