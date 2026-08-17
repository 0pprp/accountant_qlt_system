import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_USER_BY_ID, QUERY_KEY_USER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserPermissionPayload } from '@/modules/User/types/api'

const ENDPOINT = '/admin/Users/{id}/permissions'

export function userPermission(
  id: number,
  payload: UserPermissionPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance.put(endpoint, payload)
}

export function useUserPermissionMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: UserPermissionPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => userPermission(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_BY_ID],
      })
    },

    meta: {
      success: {
        title: 'user.updatePermissionsTitleSuccess',
        description: 'user.updatePermissionsDescriptionSuccess',
      },
      error: {
        title: 'user.updatePermissionsTitleError',
        description: 'user.updatePermissionsDescriptionError',
      },
    },
    ...options,
  })
}
