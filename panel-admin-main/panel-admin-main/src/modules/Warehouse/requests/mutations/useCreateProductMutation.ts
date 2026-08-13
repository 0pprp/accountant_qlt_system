import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import {
  QUERY_KEY_ALL_PRODUCT_LIST,
  QUERY_KEY_PRODUCT_CATEGORY_LIST,
  QUERY_KEY_PRODUCT_LIST,
} from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductCreatePayload } from '@/modules/Warehouse/types/api'

const ENDPOINT = '/admin/Products'

export function createProduct(
  payload: ProductCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateProductMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    ProductCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createProduct,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_CATEGORY_LIST],
      })

      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_ALL_PRODUCT_LIST],
      })

      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_LIST],
      })
    },
    meta: {
      success: {
        title: 'warehouse.product.createTitleSuccess',
        description: 'warehouse.product.createDescriptionSuccess',
      },
    },
    ...options,
  })
}
