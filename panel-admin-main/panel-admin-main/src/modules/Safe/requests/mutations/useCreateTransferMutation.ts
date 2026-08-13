import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_SAFE_SELLERS_LIST, QUERY_KEY_SAFE_CURRENT } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CreateTransferFormPayload } from '@/modules/Safe/types/api'

const ENDPOINT = '/admin/Safes/safe-transfer'

export function createTransfer(
  payload: CreateTransferFormPayload
): Promise<AxiosResponse> {
  return axiosInstance.post<{
    id: number
  }>(ENDPOINT, payload)
}

export function useCreateTransferMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    CreateTransferFormPayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createTransfer,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_SELLERS_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_SAFE_CURRENT],
      })
    },
    meta: {
      success: {
        title: 'branch.updateTitleSuccess',
        description: 'branch.updateTitleSuccess',
      },
    },
    ...options,
  })
}
