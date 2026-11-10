import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { ManagementSummaryEndpoints } from '../../domain/endpoints'
import { detailsQueryKey } from '../../domain/queryKeys'
import type {
  ManagementSummaryDetailsResponse,
  ManagementSummaryMetricKey,
} from '../../domain/types'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useSummaryDetailsQuery(params: {
  year: Ref<number>
  month: Ref<number>
  day: Ref<number | null>
  metric: Ref<ManagementSummaryMetricKey>
  branchId: Ref<number | null>
  page: Ref<number>
  pageSize: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()

  return useQuery({
    queryKey: computed(() =>
      detailsQueryKey({
        year: params.year.value,
        month: params.month.value,
        day: params.day.value ?? 0,
        metric: params.metric.value,
        branchId: params.branchId.value ?? 0,
        page: params.page.value,
        pageSize: params.pageSize.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(
      () =>
        params.enabled.value &&
        auth.permissionsReady &&
        params.day.value != null &&
        params.branchId.value != null
    ),
    queryFn: async ({ signal }) => {
      const { data } =
        await axiosInstance.get<ManagementSummaryDetailsResponse>(
          ManagementSummaryEndpoints.details,
          {
            signal,
            params: {
              year: params.year.value,
              month: params.month.value,
              day: params.day.value,
              metric: params.metric.value,
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
