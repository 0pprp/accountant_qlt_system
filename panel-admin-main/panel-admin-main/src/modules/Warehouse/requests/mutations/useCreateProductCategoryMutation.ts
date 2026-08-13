import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PRODUCT_CATEGORY_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductCategoryCreatePayload } from '@/modules/Warehouse/types/api'

const ENDPOINT = '/admin/ProductCategories'

export function createProductCategory(
  payload: ProductCategoryCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateProductCategoryMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    ProductCategoryCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createProductCategory,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_CATEGORY_LIST],
      })
    },
    ...options,
  })
}
