import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_EXPENSES_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ExpensesCreatePayload } from '@/modules/Expenses/types/api'

const ENDPOINT = `/admin/Expenses/{id}`

export function updateExpenses(
  id: number,
  payload: ExpensesCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateExpensesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    {
      id: number
      payload: ExpensesCreatePayload
      orderId?: number
      branchId?: number
    },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateExpenses(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_EXPENSES_LIST],
      })
    },
    meta: {
      success: {
        title: 'successUpdate',
        description: 'successUpdateDescription',
      },
    },
    ...options,
  })
}
