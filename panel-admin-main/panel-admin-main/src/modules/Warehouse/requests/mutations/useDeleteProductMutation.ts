import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import {
  QUERY_KEY_PRODUCT_CATEGORY_LIST,
  QUERY_KEY_PRODUCT_LIST,
  QUERY_KEY_ALL_PRODUCT_LIST,
} from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'

const ENDPOINT = '/Admin/Products/{id}'

export function deleteProduct(id: number): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance.delete(endpoint)
}

export function useDeleteProductMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id }) => deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_LIST],
      })
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_CATEGORY_LIST],
      })
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ALL_PRODUCT_LIST],
      })
    },
    meta: {
      success: {
        title: 'warehouse.product.deleteTitleSuccess',
        description: 'warehouse.product.deleteDescriptionSuccess',
      },
    },
    ...options,
  })
}
