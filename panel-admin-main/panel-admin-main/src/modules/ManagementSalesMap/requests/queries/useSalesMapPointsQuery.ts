import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementSalesMapEndpoints } from '../../domain/endpoints'
import type {
  SalesMapPoint,
  SalesMapStatusFilterValue,
} from '../../domain/types'

export const SALES_MAP_POINTS_KEY = 'management-sales-map-points'

export function useSalesMapPointsQuery(
  provinceId: Ref<number | null>,
  listIds: Ref<number[] | null>,
  status: Ref<SalesMapStatusFilterValue>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      SALES_MAP_POINTS_KEY,
      auth.branchGeneration,
      provinceId.value,
      listIds.value?.slice().sort((a, b) => a - b).join(',') ?? 'all',
      status.value,
    ]),
    enabled: computed(
      () =>
        enabled.value &&
        provinceId.value != null &&
        provinceId.value > 0
    ),
    staleTime: 30_000,
    queryFn: async ({ signal }) => {
      // قائمة فارغة صريحة = لا نقاط
      if (listIds.value && listIds.value.length === 0) return [] as SalesMapPoint[]
      const { data } = await axiosInstance.get<{ items: SalesMapPoint[] }>(
        ManagementSalesMapEndpoints.points(provinceId.value!),
        {
          signal,
          params: {
            status: status.value,
            listIds: listIds.value ?? undefined,
          },
          paramsSerializer: {
            indexes: null,
          },
        }
      )
      return data.items ?? []
    },
  })
}
