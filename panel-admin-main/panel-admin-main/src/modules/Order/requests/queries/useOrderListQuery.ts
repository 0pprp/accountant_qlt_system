import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type { OrderListServerSuccessResponse } from '@/modules/Order/types/api'

export const QUERY_KEY = 'order-list'
const ENDPOINT = '/admin/OrderLists'

export function fetchOrder(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm: Array<string> | null = null,
  sortCriteria?: Array<{
    property: string
    direction: number
  }> | null
) {
  return axiosInstance
    .get<OrderListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
        branchId,
        searchTerm,
        sortCriteria,
      },
      paramsSerializer: serializeParamsWithSortCriteria,
    })
    .then((response) => response.data)
}

export function useOrderDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,
  sortCriteria?: Ref<Array<{
    property: string
    direction: number
  }> | null>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchOrder>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      pageSize,
      pageIndex,
      branchId,
      searchTerm,
      sortCriteria,
    ],

    queryFn: () =>
      fetchOrder(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value ?? null,
        sortCriteria?.value
      ),
    ...options,
  })
}
