import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SaleListServerSuccessResponse } from '@/modules/Sale/types/api'

export const QUERY_KEY = 'sale-list'
const ENDPOINT = '/admin/Orders'

export function fetchSale(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm: Array<string> | null = null,
  columns?: Array<string> | null,
  sortCriteria?: Array<{
    property: string
    direction: number
  }> | null,
  approvalStatus?: number | null,
  startDate?: string | null,
  endDate?: string | null,
  step?: number | null
) {
  const params: Record<string, unknown> = {
    pageSize,
    pageIndex,
    branchId,
    searchTerm,
  }

  if (columns && columns.length > 0) {
    params.columns = columns
  }

  if (sortCriteria && sortCriteria.length > 0) {
    params.sortCriteria = sortCriteria
  }

  if (approvalStatus !== null && approvalStatus !== undefined) {
    params.ApprovalStatus = approvalStatus
  }

  if (startDate) {
    params.startDate = startDate
  }

  if (endDate) {
    params.endDate = endDate
  }

  if (step !== null && step !== undefined) {
    params.Step = step
  }

  return axiosInstance
    .get<SaleListServerSuccessResponse>(ENDPOINT, {
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

export function useSaleDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,
  columns?: Ref<Array<string> | null>,
  sortCriteria?: Ref<Array<{
    property: string
    direction: number
  }> | null>,
  approvalStatus?: Ref<number | null>,
  startDate?: Ref<string | null>,
  endDate?: Ref<string | null>,
  step?: Ref<number | null>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchSale>>>>,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      pageSize,
      pageIndex,
      branchId,
      searchTerm,
      columns,
      sortCriteria,
      approvalStatus,
      startDate,
      endDate,
      step,
    ],

    queryFn: () =>
      fetchSale(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value ?? null,
        columns?.value ?? null,
        sortCriteria?.value ?? null,
        approvalStatus?.value ?? null,
        startDate?.value ?? null,
        endDate?.value ?? null,
        step?.value ?? null
      ),
    ...options,
  })
}
