import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { collectionKey } from '../../domain/queryKeys'
import type { FollowerCollectionResponse } from '../../domain/collectionTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerCollectionQuery(params: {
  followerId: Ref<number | null>
  branchId: Ref<number | null>
  fromDate: Ref<string | null>
  toDate: Ref<string | null>
  paymentsPage: Ref<number>
  paymentsPageSize: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      collectionKey({
        followerId: params.followerId.value ?? 0,
        branchId: params.branchId.value,
        fromDate: params.fromDate.value,
        toDate: params.toDate.value,
        paymentsPage: params.paymentsPage.value,
        paymentsPageSize: params.paymentsPageSize.value,
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
      const { data } = await axiosInstance.get<FollowerCollectionResponse>(
        FollowerWorkEndpoints.collection(params.followerId.value!),
        {
          signal,
          params: {
            branchId: params.branchId.value,
            fromDate: params.fromDate.value || undefined,
            toDate: params.toDate.value || undefined,
            paymentsPage: params.paymentsPage.value,
            paymentsPageSize: params.paymentsPageSize.value,
          },
        }
      )
      return data
    },
  })
}
