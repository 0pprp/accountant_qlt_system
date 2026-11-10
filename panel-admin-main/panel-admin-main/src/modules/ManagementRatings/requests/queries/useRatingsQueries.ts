import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { ManagementRatingsEndpoints } from '../../domain/endpoints'
import {
  branchesKey,
  catalogKey,
  customersKey,
  trendKey,
} from '../../domain/queryKeys'
import type {
  RatingsBranchesResponse,
  RatingsCatalogResponse,
  RatingsCustomersResponse,
  RatingsScope,
  RatingsTrendResponse,
} from '../../domain/types'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useRatingsBranchesQuery(enabled: Ref<boolean>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      branchesKey(auth.branchGeneration, auth.sessionGeneration)
    ),
    enabled: computed(() => enabled.value && auth.permissionsReady),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<RatingsBranchesResponse>(
        ManagementRatingsEndpoints.branches,
        { signal }
      )
      return data
    },
  })
}

export function useRatingsCatalogQuery(
  branchId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      catalogKey(
        branchId.value ?? 0,
        auth.branchGeneration,
        auth.sessionGeneration
      )
    ),
    enabled: computed(
      () => enabled.value && auth.permissionsReady && branchId.value != null
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<RatingsCatalogResponse>(
        ManagementRatingsEndpoints.catalog,
        { signal, params: { branchId: branchId.value } }
      )
      return data
    },
  })
}

export function useRatingsTrendQuery(params: {
  branchId: Ref<number | null>
  scope: Ref<RatingsScope>
  resourceId: Ref<number>
  from: Ref<string>
  to: Ref<string>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      trendKey({
        branchId: params.branchId.value ?? 0,
        scope: params.scope.value,
        resourceId: params.resourceId.value,
        from: params.from.value,
        to: params.to.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(() => {
      if (!params.enabled.value || !auth.permissionsReady) return false
      if (params.branchId.value == null) return false
      if (params.scope.value !== 'branch' && params.resourceId.value <= 0)
        return false
      return true
    }),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<RatingsTrendResponse>(
        ManagementRatingsEndpoints.trend,
        {
          signal,
          params: {
            branchId: params.branchId.value,
            scope: params.scope.value,
            resourceId: params.resourceId.value,
            fromDate: params.from.value,
            toDate: params.to.value,
          },
        }
      )
      return data
    },
  })
}

export function useRatingsCustomersQuery(params: {
  branchId: Ref<number | null>
  scope: Ref<RatingsScope>
  resourceId: Ref<number>
  level: Ref<string | null>
  from: Ref<string>
  to: Ref<string>
  page: Ref<number>
  q: Ref<string>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() =>
      customersKey({
        branchId: params.branchId.value ?? 0,
        scope: params.scope.value,
        resourceId: params.resourceId.value,
        level: params.level.value ?? '',
        from: params.from.value,
        to: params.to.value,
        page: params.page.value,
        q: params.q.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(
      () =>
        params.enabled.value &&
        auth.permissionsReady &&
        params.branchId.value != null &&
        !!params.level.value
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<RatingsCustomersResponse>(
        ManagementRatingsEndpoints.customers,
        {
          signal,
          params: {
            branchId: params.branchId.value,
            scope: params.scope.value,
            resourceId: params.resourceId.value,
            level: params.level.value,
            fromDate: params.from.value,
            toDate: params.to.value,
            page: params.page.value,
            pageSize: 100,
            q: params.q.value || undefined,
          },
        }
      )
      return data
    },
  })
}
