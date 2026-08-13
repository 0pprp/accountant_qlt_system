import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_BRANCH_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { BranchCreatePayload } from '@/modules/Branch/types/api'

const ENDPOINT = `/Admin/Branches/{id}`

export function updateBranch(
  id: number,
  payload: BranchCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateBranchMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: BranchCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateBranch(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_BRANCH_LIST],
      })
    },
    meta: {
      success: {
        title: 'branch.updateTitleSuccess',
        description: 'branch.updateDescriptionSuccess',
      },
    },
    ...options,
  })
}
