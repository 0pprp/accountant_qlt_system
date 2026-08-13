import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductCategoryListServerSuccessResponse } from '@/modules/Warehouse/types/api'

export const QUERY_KEY = 'product-category-list'
const ENDPOINT = '/admin/ProductCategories'

export function fetchProductCategory(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm: Array<string> | null = null
) {
  return axiosInstance
    .get<ProductCategoryListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
        branchId,
        searchTerm,
      },

      paramsSerializer: (params) => {
        const searchParams = new URLSearchParams()

        Object.entries(params).forEach(([key, value]) => {
          if (Array.isArray(value)) {
            value.forEach((item) => {
              if (item !== null && item !== undefined) {
                searchParams.append(key, String(item))
              }
            })
          } else if (value !== null && value !== undefined) {
            searchParams.append(key, String(value))
          }
        })

        return searchParams.toString()
      },
    })
    .then((response) => response.data)
}

export function useProductCategoryDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,

  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchProductCategory>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, branchId, searchTerm],

    queryFn: () =>
      fetchProductCategory(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value ?? null
      ),
    ...options,
  })
}
