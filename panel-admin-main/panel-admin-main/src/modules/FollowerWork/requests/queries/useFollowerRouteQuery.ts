import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { routeKey } from '../../domain/queryKeys'
import type { FollowerRouteResponse } from '../../domain/trackingTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerRouteQuery(params: {
  followerId: Ref<number | null>
  branchId: Ref<number | null>
  date: Ref<string>
  page: Ref<number>
  pageSize: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      routeKey({
        followerId: params.followerId.value ?? 0,
        branchId: params.branchId.value,
        date: params.date.value,
        page: params.page.value,
        pageSize: params.pageSize.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(
      () =>
        params.enabled.value &&
        auth.permissionsReady === true &&
        params.followerId.value != null &&
        params.branchId.value != null &&
        !!params.date.value
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<FollowerRouteResponse>(
        FollowerWorkEndpoints.route(params.followerId.value!),
        {
          signal,
          params: {
            branchId: params.branchId.value,
            date: params.date.value,
            page: params.page.value,
            pageSize: params.pageSize.value,
          },
        }
      )
      return data
    },
  })
}
