import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { followersKey } from '../../domain/queryKeys'
import type { FollowersResponse, FollowerWorkSort } from '../../domain/types'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowersQuery(params: {
  branchId: Ref<number | null>
  search: Ref<string>
  page: Ref<number>
  pageSize: Ref<number>
  sort: Ref<FollowerWorkSort>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      followersKey({
        branchId: params.branchId.value,
        search: params.search.value,
        page: params.page.value,
        pageSize: params.pageSize.value,
        sort: params.sort.value,
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
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<FollowersResponse>(
        FollowerWorkEndpoints.followers,
        {
          signal,
          params: {
            branchId: params.branchId.value,
            search: params.search.value || undefined,
            page: params.page.value,
            pageSize: params.pageSize.value,
            sort: params.sort.value,
          },
        }
      )
      return data
    },
  })
}
