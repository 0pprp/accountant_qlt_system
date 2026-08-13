import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PRODUCT_CATEGORY_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductCategoryCreatePayload } from '@/modules/Warehouse/types/api'

const ENDPOINT = `/admin/ProductCategories/{id}`

export function updateProductCategory(
  id: number,
  payload: ProductCategoryCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateProductCategoryMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: ProductCategoryCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateProductCategory(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PRODUCT_CATEGORY_LIST],
      })
    },
    meta: {
      success: {
        title: 'warehouse.productCategory.updateTitleSuccess',
        description: 'warehouse.productCategory.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
