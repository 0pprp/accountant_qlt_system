import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_USER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserCreatePayload } from '@/modules/User/types/api'

const ENDPOINT = '/admin/Users'

export function createUser(payload: UserCreatePayload): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload)
}

export function useCreateUserMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    UserCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createUser,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_LIST],
      })
    },
    ...options,
  })
}
