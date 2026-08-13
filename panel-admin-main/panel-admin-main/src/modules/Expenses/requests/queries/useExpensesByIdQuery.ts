import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { ExpenseByIdServerSuccessResponse } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const QUERY_KEY = 'expenses-by-id'
const ENDPOINT = '/admin/Expenses/{id}'

export function fetchExpensesById(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance
    .get<ExpenseByIdServerSuccessResponse>(endpoint)
    .then((response) => response.data)
}

export function useExpensesByIdDataQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchExpensesById>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],

    queryFn: () => fetchExpensesById(id.value),
    ...options,
  })
}
