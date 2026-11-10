import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesSummaryEndpoints } from '../../domain/endpoints'
import type { SalesSummaryDetailsResponse } from '../../domain/types'

export const SALES_SUMMARY_DETAILS_KEY = 'sales-summary-details'

export function useSalesSummaryDetailsQuery(params: {
  year: Ref<number>
  month: Ref<number>
  day: Ref<number | null>
  branchId: Ref<number | null>
  page: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_SUMMARY_DETAILS_KEY,
      auth.user?.id ?? 'anon',
      params.year.value,
      params.month.value,
      params.day.value,
      params.branchId.value,
      params.page.value,
    ]),
    enabled: computed(
      () =>
        params.enabled.value === true &&
        params.day.value != null &&
        params.day.value > 0 &&
        params.branchId.value != null &&
        params.branchId.value > 0
    ),
    queryFn: async () => {
      const { data } = await axiosInstance.get<SalesSummaryDetailsResponse>(
        SalesSummaryEndpoints.details,
        {
          params: {
            year: params.year.value,
            month: params.month.value,
            day: params.day.value,
            branchId: params.branchId.value,
            page: params.page.value,
            pageSize: 25,
          },
        }
      )
      return data
    },
  })
}
