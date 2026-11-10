import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { ManagementSummaryEndpoints } from '../../domain/endpoints'
import { matrixQueryKey } from '../../domain/queryKeys'
import type {
  ManagementSummaryMatrixResponse,
  ManagementSummaryMetricKey,
} from '../../domain/types'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export function useSummaryMatrixQuery(
  year: Ref<number>,
  month: Ref<number>,
  metric: Ref<ManagementSummaryMetricKey>,
  branchId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()

  return useQuery({
    queryKey: computed(() =>
      matrixQueryKey({
        year: year.value,
        month: month.value,
        metric: metric.value,
        branchId: branchId.value,
        branchGeneration: auth.branchGeneration,
        sessionGeneration: auth.sessionGeneration,
      })
    ),
    enabled: computed(() => enabled.value && auth.permissionsReady),
    placeholderData: undefined,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<ManagementSummaryMatrixResponse>(
        ManagementSummaryEndpoints.matrix,
        {
          signal,
          params: {
            year: year.value,
            month: month.value,
            metric: metric.value,
            ...(branchId.value != null ? { branchId: branchId.value } : {}),
          },
        }
      )
      return data
    },
  })
}
