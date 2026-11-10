import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { remainingCustomersKey } from '../../domain/queryKeys'
import type { RemainingCustomersResponse } from '../../domain/salesReviewTypes'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useFollowerRemainingCustomersQuery(params: {
  followerId: Ref<number | null>
  branchId: Ref<number | null>
  bucket: Ref<string | null>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      remainingCustomersKey({
        followerId: params.followerId.value ?? 0,
        branchId: params.branchId.value,
        bucket: params.bucket.value ?? '',
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
        !!params.bucket.value
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<RemainingCustomersResponse>(
        FollowerWorkEndpoints.remainingCustomers(params.followerId.value!),
        {
          signal,
          params: {
            branchId: params.branchId.value,
            bucket: params.bucket.value,
          },
        }
      )
      return data
    },
  })
}
