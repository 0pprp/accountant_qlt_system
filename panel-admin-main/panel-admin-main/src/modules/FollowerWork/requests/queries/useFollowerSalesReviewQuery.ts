import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { salesReviewKey } from '../../domain/queryKeys'
import type { SalesReviewWorkspace } from '../../domain/salesReviewTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerSalesReviewQuery(params: {
  followerId: Ref<number | null>
  branchId: Ref<number | null>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      salesReviewKey({
        followerId: params.followerId.value ?? 0,
        branchId: params.branchId.value,
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
      const { data } = await axiosInstance.get<SalesReviewWorkspace>(
        FollowerWorkEndpoints.salesReview(params.followerId.value!),
        {
          signal,
          params: { branchId: params.branchId.value },
        }
      )
      return data
    },
  })
}
