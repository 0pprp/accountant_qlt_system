import type { Ref } from 'vue'
import { computed } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserProfileResponse } from '@/modules/Core/types/api/dashboard'

export const QUERY_KEY = 'dashboard-user-profile'
const ENDPOINT = '/admin/Users/profile'

export function fetchUserProfile(branchId: number) {
  return axiosInstance
    .get<UserProfileResponse>(ENDPOINT, {
      params: { branchId },
    })
    .then((response) => response.data)
}

export function useUserProfileQuery(
  branchId: Ref<number>,
  options?: Omit<
    UseQueryOptions<Awaited<ReturnType<typeof fetchUserProfile>>>,
    'queryKey' | 'queryFn' | 'enabled'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, branchId],
    queryFn: () => fetchUserProfile(branchId.value),
    enabled: computed(() => branchId.value > 0),
    ...options,
  })
}
