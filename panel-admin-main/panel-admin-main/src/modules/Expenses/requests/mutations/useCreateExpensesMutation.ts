import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_EXPENSES_LIST } from '../queries'
import type { ExpensesCreatePayload } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

const ENDPOINT = '/admin/Expenses'

export function createExpenses(
  payload: ExpensesCreatePayload
): Promise<AxiosResponse> {
  const formData = new FormData()
  formData.append('factorNumber', payload.factorNumber!.toString())
  formData.append('safeType', payload.safeType!.toString())
  formData.append('branchId', payload.branchId?.toString() ?? '0')
  if (payload.attachments) {
    formData.append('Attachments', payload.attachments)
  }
  payload.expenseItems.forEach((item, index) => {
    formData.append(`expenseItems[${index}].name`, item.name!.toString())
    if (item.quantity !== null) {
      formData.append(
        `expenseItems[${index}].quantity`,
        item.quantity.toString()
      )
    }
    formData.append(`expenseItems[${index}].amount`, item.amount.toString())
  })

  return axiosInstance.post(ENDPOINT, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
}

export function useCreateExpensesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    ExpensesCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createExpenses,
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
