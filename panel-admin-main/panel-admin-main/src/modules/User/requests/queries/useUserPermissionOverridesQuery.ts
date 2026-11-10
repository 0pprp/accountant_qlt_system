import { useQuery } from '@tanstack/vue-query'
import { computed, type MaybeRefOrGetter, toValue } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserPermissionOverridesResponse } from '@/modules/User/types/permissionOverrides'

export const QUERY_KEY_USER_PERMISSION_OVERRIDES = 'user-permission-overrides'

function normalizeMode(mode: unknown): 'inherit' | 'allow' | 'deny' {
  if (mode === 1 || mode === 'Allow' || mode === 'allow') return 'allow'
  if (mode === 2 || mode === 'Deny' || mode === 'deny') return 'deny'
  return 'inherit'
}

export async function fetchUserPermissionOverrides(
  userId: number
): Promise<UserPermissionOverridesResponse> {
  const { data } = await axiosInstance.get(
    `/admin/Users/${userId}/permission-overrides`
  )
  return {
    ...data,
    rows: (data.rows ?? []).map(
      (row: UserPermissionOverridesResponse['rows'][number]) => ({
        ...row,
        mode: normalizeMode(row.mode),
      })
    ),
  }
}

export function useUserPermissionOverridesQuery(
  userId: MaybeRefOrGetter<number | null | undefined>,
  enabled?: MaybeRefOrGetter<boolean>
) {
  return useQuery({
    queryKey: computed(() => [
      QUERY_KEY_USER_PERMISSION_OVERRIDES,
      toValue(userId),
    ]),
    queryFn: () => fetchUserPermissionOverrides(toValue(userId)!),
    enabled: computed(() => {
      const id = toValue(userId)
      const extra = enabled == null ? true : toValue(enabled)
      return Boolean(id) && extra !== false
    }),
  })
}
