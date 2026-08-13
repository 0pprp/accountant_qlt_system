import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import {
  QUERY_KEY_PRODUCT_LIST,
  QUERY_KEY_ALL_PRODUCT_LIST,
} from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductCreatePayload } from '@/modules/Warehouse/types/api'

const ENDPOINT = `/Admin/Products/{id}`

export function updateProduct(
  id: number,
  payload: ProductCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateProductMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: ProductCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateProduct(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ALL_PRODUCT_LIST],
      })
    },
    meta: {
      success: {
        title: 'warehouse.product.updateTitleSuccess',
        description: 'warehouse.product.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
