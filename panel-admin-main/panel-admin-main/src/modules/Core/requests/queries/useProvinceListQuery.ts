import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { ProvinceListServerSuccessResponse } from '@/modules/Core/types/api/province'

export const QUERY_KEY = 'province-list'
const ENDPOINT = '/admin/Provinces'

export function fetchProvince(pageIndex: number, pageSize: number) {
  return axiosInstance
    .get<ProvinceListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
      },
    })
    .then((response) => response.data)
}

export function useProvinceDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,

  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchProvince>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex],

    queryFn: () => fetchProvince(pageIndex.value, pageSize.value),
    ...options,
  })
}
