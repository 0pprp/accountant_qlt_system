import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SaleListServerSuccessResponse } from '@/modules/Sale/types/api'

export const QUERY_KEY = 'orders-by-customer'
const ENDPOINT = '/admin/Orders'

const DEFAULT_COLUMNS = ['ExecutionStatus']

export function fetchOrdersByCustomer(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  customerId: number,
  columns: Array<string> = DEFAULT_COLUMNS
) {
  return axiosInstance
    .get<SaleListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
        branchId,
        customerId,
        columns,
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
    .then((response) => response.data)
}

export function useOrdersByCustomerQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  customerId: Ref<number | null>,
  columns?: Ref<Array<string> | null>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchOrdersByCustomer>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, pageSize, pageIndex, branchId, customerId, columns],
    queryFn: () =>
      fetchOrdersByCustomer(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        customerId.value!,
        columns?.value?.length ? columns.value : DEFAULT_COLUMNS
      ),
    enabled: () => !!customerId.value,
    ...options,
  })
}
