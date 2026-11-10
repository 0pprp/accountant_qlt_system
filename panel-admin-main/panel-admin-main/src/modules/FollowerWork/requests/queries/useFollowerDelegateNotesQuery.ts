import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { delegateNotesKey } from '../../domain/queryKeys'
import type { FollowerDelegateNotesResponse } from '../../domain/delegateNotesTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerDelegateNotesQuery(params: {
  followerId: Ref<number | null>
  branchId: Ref<number | null>
  page: Ref<number>
  pageSize: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      delegateNotesKey({
        followerId: params.followerId.value ?? 0,
        branchId: params.branchId.value,
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
        params.branchId.value != null
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<FollowerDelegateNotesResponse>(
        FollowerWorkEndpoints.delegateNotes(params.followerId.value!),
        {
          signal,
          params: {
            branchId: params.branchId.value,
            page: params.page.value,
            pageSize: params.pageSize.value,
          },
        }
      )
      return data
    },
  })
}
