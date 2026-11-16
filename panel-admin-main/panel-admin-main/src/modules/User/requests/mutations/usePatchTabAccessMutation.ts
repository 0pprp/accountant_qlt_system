import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_USER_BY_ID, QUERY_KEY_USER_LIST } from '../queries'
import { QUERY_KEY_USER_TAB_ACCESS } from '../queries/useUserTabAccessQuery'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PatchTabAccessPayload } from '@/modules/User/types/tabAccess'

export function patchUserTabAccess(
  id: number,
  payload: PatchTabAccessPayload
): Promise<AxiosResponse> {
  return axiosInstance.patch(`/admin/Users/${id}/tab-access`, {
    expectedVersion: payload.expectedVersion,
    changes: payload.changes,
  })
}

export function usePatchTabAccessMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: PatchTabAccessPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => patchUserTabAccess(id, payload),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({ queryKey: [QUERY_KEY_USER_LIST] })
      await queryClient.invalidateQueries({ queryKey: [QUERY_KEY_USER_BY_ID] })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_TAB_ACCESS, variables.id],
      })
    },
    meta: {
      success: {
        title: 'user.updatePermissionsTitleSuccess',
        description: 'user.permissionOverridesSavedSessionsInvalidated',
      },
      error: {
        title: 'user.updatePermissionsTitleError',
        description: 'user.updatePermissionsDescriptionError',
      },
    },
    ...options,
  })
}
