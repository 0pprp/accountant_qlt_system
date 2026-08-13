import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_SAFE_SELLERS_LIST, QUERY_KEY_SAFE_CURRENT } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CreateSellerCashDeliveriesPayload } from '@/modules/Safe/types/api'

const ENDPOINT = '/admin/Safes/{id}/sellers/{sellerId}/cash-deliveries'

export function createSellerCashDeliveries(
  id: number,
  sellerId: number,
  payload: CreateSellerCashDeliveriesPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id)).replace(
    '{sellerId}',
    String(sellerId)
  )

  return axiosInstance.post<{ id: number; sellerId: number }>(endpoint, payload)
}

export function useCreateSellerCashDeliveriesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      id: number
      sellerId: number
      payload: CreateSellerCashDeliveriesPayload
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, sellerId, payload }) =>
      createSellerCashDeliveries(id, sellerId, payload),
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
