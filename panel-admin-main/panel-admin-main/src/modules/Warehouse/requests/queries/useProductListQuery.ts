import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { ref, type Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProductListServerSuccessResponse } from '@/modules/Warehouse/types/api'

export const QUERY_KEY = 'product-list'
const ENDPOINT = '/admin/Products'

export function fetchProduct(
  categoryId: number | null,
  branchId: number,
  pageIndex: number = 1,
  pageSize: number = 100,
  searchTerm: Array<string> | null = null
) {
  const params: Record<string, unknown> = {
    branchId,
    pageIndex,
    pageSize,
  }

  if (categoryId !== null && categoryId !== -1) {
    params.CategoryId = categoryId
  }

  if (searchTerm) {
    params.searchTerm = searchTerm
  }

  return axiosInstance
    .get<ProductListServerSuccessResponse>(ENDPOINT, {
      params,
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

export function useProductDataQuery(
  categoryId: Ref<number>,
  branchId: Ref<number>,
  pageIndex: Ref<number> = ref(1),
  pageSize: Ref<number> = ref(100),
  searchTerm?: Ref<Array<string> | null>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchProduct>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      categoryId,
      branchId,
      pageIndex,
      pageSize,
      searchTerm,
    ],

    queryFn: () =>
      fetchProduct(
        categoryId.value,
        branchId.value,
        pageIndex.value,
        pageSize.value,
        searchTerm?.value ?? null
      ),
    ...options,
  })
}
