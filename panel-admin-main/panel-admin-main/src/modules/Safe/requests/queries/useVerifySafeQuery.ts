import { computed, toValue, type MaybeRefOrGetter, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'
import type { SafeVerificationServerSuccessResponse } from '../../types/api'

export const QUERY_KEY_SAFE_VERIFY = 'safe-verify'
const ENDPOINT = '/admin/Safes/{id}/verify'

export function fetchSafeVerification(safeId: number, signal?: AbortSignal) {
  return axiosInstance
    .get<SafeVerificationServerSuccessResponse>(ENDPOINT.replace('{id}', String(safeId)), { signal })
    .then((response) => response.data)
}

export function useVerifySafeQuery(
  safeId: Ref<number>,
  options?: { enabled?: MaybeRefOrGetter<boolean> },
) {
  const authStore = useAuthStore()
  const { can, permissionsReady } = usePermission()
  const extraEnabled = computed(() => toValue(options?.enabled) ?? true)

  return useQuery({
    queryKey: [
      QUERY_KEY_SAFE_VERIFY,
      safeId,
      computed(() => authStore.sessionGeneration),
    ],
    queryFn: ({ signal }) => fetchSafeVerification(safeId.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        safeId.value > 0 &&
        can('Safe', 'Read') &&
        extraEnabled.value,
    ),
  })
}
