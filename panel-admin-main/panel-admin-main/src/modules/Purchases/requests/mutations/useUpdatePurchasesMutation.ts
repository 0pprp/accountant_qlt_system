import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PURCHASES_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PurchasesCreatePayload } from '@/modules/Purchases/types/api'

const ENDPOINT = `/admin/Purchases/{id}`

export function updatePurchases(
  id: number,
  payload: PurchasesCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdatePurchasesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      id: number
      payload: PurchasesCreatePayload
      orderId?: number
      branchId?: number
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updatePurchases(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PURCHASES_LIST],
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
