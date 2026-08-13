import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { AllProductListServerSuccessResponse } from '@/modules/Warehouse/types/api'

export const QUERY_KEY = 'all-product-list'
const ENDPOINT = '/admin/Products'

export function fetchAllProduct(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm: Array<string> | null = null
) {
  return axiosInstance
    .get<AllProductListServerSuccessResponse>(ENDPOINT, {
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

export function useAllProductDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,

  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchAllProduct>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, branchId, searchTerm],

    queryFn: () =>
      fetchAllProduct(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value ?? null
      ),
    ...options,
  })
}
