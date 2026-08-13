import type { Ref } from 'vue'
import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type { CustomerListServerSuccessResponse } from '@/modules/Customer/types/api'

export const QUERY_KEY = 'customer-list'
const ENDPOINT = '/admin/Customers'

export function fetchCustomer(
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm?: string | null,
  startDate?: string | null,
  endDate?: string | null,
  sortCriteria?: Array<{
    property: string
    direction: number
  }> | null
) {
  return axiosInstance
    .get<CustomerListServerSuccessResponse>(ENDPOINT, {
      params: {
        pageSize,
        pageIndex,
        branchId,
        searchTerm,
        startDate,
        endDate,
        sortCriteria,
      },
      paramsSerializer: serializeParamsWithSortCriteria,
    })
    .then((response) => response.data)
}

export function useCustomerDataQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<string | null>,
  startDate?: Ref<string | null>,
  endDate?: Ref<string | null>,
  sortCriteria?: Ref<Array<{
    property: string
    direction: number
  }> | null>,
  options?: Omit<
    UseQueryOptions<UseQueryOptions<Awaited<ReturnType<typeof fetchCustomer>>>>,
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
      startDate,
      endDate,
      sortCriteria,
    ],
    queryFn: () =>
      fetchCustomer(
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value,
        startDate?.value,
        endDate?.value,
        sortCriteria?.value
      ),
    ...options,
  })
}
