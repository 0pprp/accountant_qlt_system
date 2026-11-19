import { computed, toValue, type MaybeRefOrGetter, type Ref } from 'vue'
import { useQuery, type UseQueryOptions } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'
import type { SafeMovement, SafeMovementKind } from '../../types/model'

export const QUERY_KEY_SAFE_MOVEMENTS = 'safe-movements'
const ENDPOINT = '/admin/Safes/{id}/movements'

export interface SafeMovementsParams {
  pageIndex: number
  pageSize: number
  kind: SafeMovementKind | null
  startDate: string | null
  endDate: string | null
}

export function fetchSafeMovements(safeId: number, params: SafeMovementsParams, signal?: AbortSignal) {
  return axiosInstance
    .get<ServerSuccessPaginatedResponse<Array<SafeMovement>>>(
      ENDPOINT.replace('{id}', String(safeId)),
      {
        params: {
          pageIndex: params.pageIndex,
          pageSize: params.pageSize,
          kind: params.kind ?? undefined,
          startDate: params.startDate || undefined,
          endDate: params.endDate || undefined,
        },
        signal,
      },
    )
    .then((response) => response.data)
}

export function useSafeMovementsQuery(
  safeId: Ref<number>,
  params: Ref<SafeMovementsParams>,
  options?: { enabled?: MaybeRefOrGetter<boolean> },
) {
  const authStore = useAuthStore()
  const { can, permissionsReady } = usePermission()
  const extraEnabled = computed(() => toValue(options?.enabled) ?? true)

  return useQuery({
    queryKey: [
      QUERY_KEY_SAFE_MOVEMENTS,
      safeId,
      params,
      computed(() => authStore.branchGeneration),
      computed(() => authStore.sessionGeneration),
    ],
    queryFn: ({ signal }) => fetchSafeMovements(safeId.value, params.value, signal),
    enabled: computed(
      () =>
        permissionsReady.value &&
        safeId.value > 0 &&
        can('Safe', 'Read') &&
        extraEnabled.value,
    ),
  })
}
