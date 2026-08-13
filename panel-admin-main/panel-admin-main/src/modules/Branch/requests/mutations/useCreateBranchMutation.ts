import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_BRANCH_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { BranchCreatePayload } from '@/modules/Branch/types/api'

const ENDPOINT = '/admin/Branches'

export function createBranch(
  payload: BranchCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateBranchMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    BranchCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createBranch,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_BRANCH_LIST],
      })
    },
    meta: {
      success: {
        title: 'branch.createTitleSuccess',
        description: 'branch.createDescriptionSuccess',
      },
    },
    ...options,
  })
}
