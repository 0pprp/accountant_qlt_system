import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import type { ExpensesListServerSuccessResponse } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const QUERY_KEY = 'purchase-list'
const ENDPOINT = '/admin/Expenses'

export function fetchPurchase(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm?: string | null,
  startDate?: string | null,
  endDate?: string | null
) {
  return axiosInstance
    .get<ExpensesListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
        branchId,
        searchTerm,
        startDate,
        endDate,
      },
    })
    .then((response) => response.data)
}

export function useExpensesDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<string | null>,
  startDate?: Ref<string | null>,
  endDate?: Ref<string | null>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchPurchase>>>>,
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
      startDate,
      endDate,
    ],

    queryFn: () =>
      fetchPurchase(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value,
        startDate?.value,
        endDate?.value
      ),
    ...options,
  })
}
