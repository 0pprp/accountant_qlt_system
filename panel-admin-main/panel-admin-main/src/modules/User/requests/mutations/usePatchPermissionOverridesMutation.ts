import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_USER_BY_ID, QUERY_KEY_USER_LIST } from '../queries'
import { QUERY_KEY_USER_PERMISSION_OVERRIDES } from '../queries/useUserPermissionOverridesQuery'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { PatchPermissionOverridesPayload } from '@/modules/User/types/permissionOverrides'

const MODE_TO_API: Record<'inherit' | 'allow' | 'deny', number> = {
  inherit: 0,
  allow: 1,
  deny: 2,
}

export function patchUserPermissionOverrides(
  id: number,
  payload: PatchPermissionOverridesPayload
): Promise<AxiosResponse> {
  return axiosInstance.patch(`/admin/Users/${id}/permission-overrides`, {
    expectedVersion: payload.expectedVersion,
    changes: payload.changes.map((c) => ({
      permissionId: c.permissionId,
      mode: MODE_TO_API[c.mode],
    })),
  })
}

export function usePatchPermissionOverridesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: PatchPermissionOverridesPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => patchUserPermissionOverrides(id, payload),
    onSuccess: async (_data, variables) => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_LIST],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_BY_ID],
      })
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_PERMISSION_OVERRIDES, variables.id],
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
