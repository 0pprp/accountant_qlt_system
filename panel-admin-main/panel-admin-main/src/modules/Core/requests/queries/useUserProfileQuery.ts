import type { Ref } from 'vue'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserProfileResponse } from '@/modules/Core/types/api/dashboard'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'

export const QUERY_KEY = 'dashboard-user-profile'
const ENDPOINT = '/admin/Users/profile'

export function fetchUserProfile(branchId: number, signal?: AbortSignal) {
  return axiosInstance
    .get<UserProfileResponse>(ENDPOINT, {
      params: { branchId },
      signal,
    })
    .then((response) => response.data)
}

export function useUserProfileQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchUserProfile>>>,
    'queryKey' | 'queryFn'
  > & { enabled?: MaybeRefOrGetter<boolean> }
) {
  const authStore = useAuthStore()
  const { can, permissionsReady } = usePermission()
  const extraEnabled = computed(() => toValue(options?.enabled) ?? true)

  return useQuery({
    ...options,
    queryKey: [
      QUERY_KEY,
      branchId,
      computed(() => authStore.branchGeneration),
      computed(() => authStore.sessionGeneration),
    ],
    queryFn: ({ signal }) => fetchUserProfile(branchId.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        branchId.value > 0 &&
        can('User', 'Read') &&
        extraEnabled.value
    ),
  })
}
