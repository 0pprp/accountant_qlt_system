import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { BranchListServerSuccessResponse } from '@/modules/Branch/types/api'

export const QUERY_KEY = 'branch-list'
const ENDPOINT = '/admin/Branches'

export function fetchBranch(
  pageIndex: number,
  pageSize: number,
  searchTerm: Array<string> | null = null
) {
  return axiosInstance.get<BranchListServerSuccessResponse>(ENDPOINT, {
    params: {
      pageSize,
      pageIndex,
      searchTerm,
    },

    paramsSerializer: (params) => {
      const searchParams = new URLSearchParams()

      Object.entries(params).forEach(([key, value]) => {
        if (Array.isArray(value)) {
          value.forEach((item) => {
            if (item !== null && item !== undefined) {
              searchParams.append(key, String(item))
            }
          })
        } else if (value !== null && value !== undefined) {
          searchParams.append(key, String(value))
        }
      })

      return searchParams.toString()
    },
  })
}

export function useBranchDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,

  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchBranch>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, searchTerm],

    queryFn: () =>
      fetchBranch(pageIndex.value, pageSize.value, searchTerm?.value ?? null),
    ...options,
  })
}
