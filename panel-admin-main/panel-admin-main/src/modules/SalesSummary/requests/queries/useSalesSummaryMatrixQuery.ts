import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesSummaryEndpoints } from '../../domain/endpoints'
import type { SalesSummaryMatrixResponse } from '../../domain/types'

export const SALES_SUMMARY_MATRIX_KEY = 'sales-summary-matrix'

export function useSalesSummaryMatrixQuery(
  year: Ref<number>,
  month: Ref<number>,
  branchId: Ref<number | null>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_SUMMARY_MATRIX_KEY,
      auth.user?.id ?? 'anon',
      year.value,
      month.value,
      branchId.value ?? 'all',
    ]),
    enabled: computed(() => enabled.value === true),
    queryFn: async () => {
      const params: Record<string, number> = {
        year: year.value,
        month: month.value,
      }
      if (branchId.value != null && branchId.value > 0) params.branchId = branchId.value
      const { data } = await axiosInstance.get<SalesSummaryMatrixResponse>(
        SalesSummaryEndpoints.matrix,
        { params }
      )
      return data
    },
  })
}
