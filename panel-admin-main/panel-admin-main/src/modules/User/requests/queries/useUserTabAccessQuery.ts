import { useQuery } from '@tanstack/vue-query'
import { computed, type MaybeRefOrGetter, toValue } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type {
  TabAccessOverride,
  UserTabAccessResponse,
} from '@/modules/User/types/tabAccess'

export const QUERY_KEY_USER_TAB_ACCESS = 'user-tab-access'

function normalizeOverride(value: unknown): TabAccessOverride {
  if (value === 'allow' || value === 'Allow' || value === 1) return 'allow'
  if (value === 'deny' || value === 'Deny' || value === 2) return 'deny'
  return null
}

export async function fetchUserTabAccess(
  userId: number
): Promise<UserTabAccessResponse> {
  const { data } = await axiosInstance.get(`/admin/Users/${userId}/tab-access`)
  return {
    userId: data.userId,
    fullName: data.fullName ?? '',
    roleDisplayName: data.roleDisplayName ?? '',
    isProtectedAccount: Boolean(data.isProtectedAccount),
    permissionsVersion: data.permissionsVersion ?? 0,
    canEdit: Boolean(data.canEdit),
    tabs: (data.tabs ?? []).map(
      (row: UserTabAccessResponse['tabs'][number]) => ({
        tabKey: row.tabKey,
        parentKey: row.parentKey ?? null,
        displayName: row.displayName,
        roleDefaultEnabled: Boolean(row.roleDefaultEnabled),
        effectiveEnabled: Boolean(row.effectiveEnabled),
        override: normalizeOverride(row.override),
      })
    ),
  }
}

export function useUserTabAccessQuery(
  userId: MaybeRefOrGetter<number | null | undefined>,
  enabled?: MaybeRefOrGetter<boolean>
) {
  return useQuery({
    queryKey: computed(() => [QUERY_KEY_USER_TAB_ACCESS, toValue(userId)]),
    queryFn: () => fetchUserTabAccess(toValue(userId)!),
    enabled: computed(() => {
      const id = toValue(userId)
      const extra = enabled == null ? true : toValue(enabled)
      return Boolean(id) && extra !== false
    }),
  })
}
