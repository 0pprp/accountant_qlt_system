import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { liveLocationsKey } from '../../domain/queryKeys'
import type { FollowerLiveLocationsResponse } from '../../domain/trackingTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerLiveLocationsQuery(params: {
  branchId: Ref<number | null>
  followerId: Ref<number | null>
  enabled: Ref<boolean>
  refetchIntervalMs?: number
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      liveLocationsKey({
        branchId: params.branchId.value,
        followerId: params.followerId.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(
      () =>
        params.enabled.value &&
        auth.permissionsReady === true &&
        params.branchId.value != null
    ),
    refetchInterval: params.refetchIntervalMs ?? false,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<FollowerLiveLocationsResponse>(
        FollowerWorkEndpoints.liveLocations,
        {
          signal,
          params: {
            branchId: params.branchId.value,
            followerId: params.followerId.value ?? undefined,
          },
        }
      )
      return data
    },
  })
}
