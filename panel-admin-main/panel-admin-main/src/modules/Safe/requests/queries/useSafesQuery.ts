import { computed, toValue, type MaybeRefOrGetter, type Ref } from 'vue'
import { useQuery, type UseQueryOptions } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'
import type { SafesServerSuccessResponse } from '../../types/api'

export const QUERY_KEY_SAFES = 'safes'
const ENDPOINT = '/admin/Safes'

export function fetchSafes(branchId: number, signal?: AbortSignal) {
  return axiosInstance
    .get<SafesServerSuccessResponse>(ENDPOINT, { params: { branchId }, signal })
    .then((response) => response.data)
}

export function useSafesQuery(
  branchId: Ref<number>,
  options?: Omit<UseQueryOptions<SafesServerSuccessResponse>, 'queryKey' | 'queryFn'> & {
    enabled?: MaybeRefOrGetter<boolean>
  },
) {
  const authStore = useAuthStore()
  const { can, permissionsReady } = usePermission()
  const extraEnabled = computed(() => toValue(options?.enabled) ?? true)

  return useQuery({
    ...options,
    queryKey: [
      QUERY_KEY_SAFES,
      branchId,
      computed(() => authStore.branchGeneration),
      computed(() => authStore.sessionGeneration),
    ],
    queryFn: ({ signal }) => fetchSafes(branchId.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        branchId.value > 0 &&
        can('Safe', 'Read') &&
        extraEnabled.value,
    ),
  })
}
