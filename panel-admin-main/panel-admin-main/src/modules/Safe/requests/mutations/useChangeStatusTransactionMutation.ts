import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import {
  QUERY_KEY_SAFE_TRANSACTIONS_LIST,
  QUERY_KEY_SAFE_CURRENT,
} from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ChangeStatusPayload } from '@/modules/Safe/types/api'

const ENDPOINT = `/admin/Transactions/{id}/change-status`

export function changeStatusTransaction(
  id: number,
  payload: ChangeStatusPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useChangeStatusTransactionMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: ChangeStatusPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => changeStatusTransaction(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_TRANSACTIONS_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_CURRENT],
      })
    },
    meta: {
      success: {
        title: 'safe.updateTitleSuccess',
        description: 'safe.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
