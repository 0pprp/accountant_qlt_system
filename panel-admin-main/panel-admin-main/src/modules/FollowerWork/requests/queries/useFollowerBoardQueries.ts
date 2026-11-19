import { computed, type Ref } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { FOLLOWER_WORK_KEY } from '../../domain/queryKeys'
import type {
  FollowerAccountPayload,
  FollowerCustomerProfile,
  FollowerSectionCounts,
  FollowerShopCapture,
} from '../../domain/boardTypes'

export function useFollowerSectionCountsQuery(followerId: Ref<number | null>, branchId: Ref<number | null>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [FOLLOWER_WORK_KEY, 'section-counts', followerId.value, branchId.value, auth.branchGeneration, auth.sessionGeneration]),
    enabled: computed(() => auth.permissionsReady === true && followerId.value != null && branchId.value != null),
    queryFn: async ({ signal }) => (await axiosInstance.get<FollowerSectionCounts>(
      FollowerWorkEndpoints.sectionCounts(followerId.value as number),
      { signal, params: { branchId: branchId.value } },
    )).data,
    staleTime: 60_000,
  })
}

export function useFollowerCustomerProfileQuery(
  followerId: Ref<number | null>,
  customerId: Ref<number | null>,
  branchId: Ref<number | null>,
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [FOLLOWER_WORK_KEY, 'customer-profile', followerId.value, customerId.value, branchId.value, auth.branchGeneration, auth.sessionGeneration]),
    enabled: computed(() => followerId.value != null && customerId.value != null && branchId.value != null),
    queryFn: async ({ signal }) => (await axiosInstance.get<FollowerCustomerProfile>(
      FollowerWorkEndpoints.customerProfile(followerId.value as number, customerId.value as number),
      { signal, params: { branchId: branchId.value } },
    )).data,
  })
}

export function useFollowerShopCaptureQuery(
  branchId: Ref<number | null>,
  followerId: Ref<number | null>,
  enabled: Ref<boolean>,
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [FOLLOWER_WORK_KEY, 'shop-capture', branchId.value, followerId.value, auth.branchGeneration, auth.sessionGeneration]),
    enabled: computed(() => enabled.value && branchId.value != null),
    queryFn: async ({ signal }) => (await axiosInstance.get<FollowerShopCapture>(
      FollowerWorkEndpoints.shopCapture,
      { signal, params: { branchId: branchId.value, followerId: followerId.value ?? undefined } },
    )).data,
  })
}

export function useManageFollowerMutations() {
  const qc = useQueryClient()
  const invalidate = () => qc.invalidateQueries({ queryKey: [FOLLOWER_WORK_KEY] })
  const create = useMutation({
    mutationFn: async (payload: FollowerAccountPayload) =>
      (await axiosInstance.post<number>(FollowerWorkEndpoints.manageFollowers, payload)).data,
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: async (args: { followerId: number; payload: FollowerAccountPayload }) =>
      (await axiosInstance.put(FollowerWorkEndpoints.manageFollower(args.followerId), args.payload)).data,
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: async (followerId: number) =>
      (await axiosInstance.delete(FollowerWorkEndpoints.manageFollower(followerId))).data,
    onSuccess: invalidate,
  })
  return { create, update, remove }
}
