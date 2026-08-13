import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { UserListServerSuccessResponse } from '@/modules/User/types/api'

export const QUERY_KEY = 'user-list'
const ENDPOINT = '/admin/Users'

export function fetchUser(
  pageIndex: number,
  pageSize: number,
  roleIds: Array<number> | null,
  branchId: number | null,
  searchTerm: Array<string> | null = null,
  columns?: Array<string> | null,
  sortCriteria?: Array<{
    property: string
    direction: number
  }> | null,
  startDate?: string | null,
  endDate?: string | null
) {
  const params: Record<string, unknown> = {
    pageSize,
    pageIndex,
    roleIds,
    branchId,
    searchTerm,
  }

  if (columns && columns.length > 0) {
    params.columns = columns
  }

  if (sortCriteria && sortCriteria.length > 0) {
    params.sortCriteria = sortCriteria
  }

  if (startDate) {
    params.startDate = startDate
  }

  if (endDate) {
    params.endDate = endDate
  }

  return axiosInstance
    .get<UserListServerSuccessResponse>(ENDPOINT, {
      params,
      paramsSerializer: (params) => {
        const searchParams = new URLSearchParams()

        Object.entries(params).forEach(([key, value]) => {
          if (key === 'sortCriteria' && Array.isArray(value)) {
            value.forEach((item, index) => {
              if (item && typeof item === 'object') {
                Object.entries(item).forEach(([propKey, propValue]) => {
                  if (propValue !== null && propValue !== undefined) {
                    searchParams.append(
                      `${key}[${index}].${propKey}`,
                      String(propValue)
                    )
                  }
                })
              }
            })
          } else if (Array.isArray(value)) {
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
    .then((response) => response.data)
}

export function useUserDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  roleIds?: Ref<Array<number> | null>,
  branchId?: Ref<number | null>,
  searchTerm?: Ref<Array<string> | null>,
  columns?: Ref<Array<string> | null>,
  sortCriteria?: Ref<Array<{
    property: string
    direction: number
  }> | null>,
  startDate?: Ref<string | null>,
  endDate?: Ref<string | null>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchUser>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      pageSize,
      pageIndex,
      roleIds,
      branchId,
      searchTerm,
      columns,
      sortCriteria,
      startDate,
      endDate,
    ],

    queryFn: () =>
      fetchUser(
        pageIndex.value,
        pageSize.value,
        roleIds?.value ?? null,
        branchId?.value ?? null,
        searchTerm?.value ?? null,
        columns?.value ?? null,
        sortCriteria?.value,
        startDate?.value ?? null,
        endDate?.value ?? null
      ),
    ...options,
  })
}
