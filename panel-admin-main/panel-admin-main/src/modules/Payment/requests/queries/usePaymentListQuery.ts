import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { serializeParamsWithSortCriteria } from '@/modules/Core/utils/paramsSerializer'
import type { PaymentListServerSuccessResponse } from '@/modules/Payment/types/api'

export const QUERY_KEY = 'payment-list'
const ENDPOINT = '/admin/OrderLists/{id}/installment-payments'

export function fetchPaymentList(
  id: number,
  pageIndex: number,
  pageSize: number,
  branchId: number,
  searchTerm: Array<string> | null = null,
  startDate: string | null = null,
  endDate: string | null = null,
  sortCriteria?: Array<{
    property: string
    direction: number
  }> | null
) {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance
    .get<PaymentListServerSuccessResponse>(endpoint, {
      params: {
        pageIndex,
        pageSize,
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

export function usePaymentListDataQuery(
  id: Ref<number>,
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  branchId: Ref<number>,
  searchTerm?: Ref<Array<string> | null>,
  startDate?: Ref<string | null>,
  endDate?: Ref<string | null>,
  sortCriteria?: Ref<Array<{
    property: string
    direction: number
  }> | null>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchPaymentList>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [
      QUERY_KEY,
      id,
      pageIndex,
      pageSize,
      branchId,
      searchTerm,
      startDate,
      endDate,
      sortCriteria,
    ],

    queryFn: () =>
      fetchPaymentList(
        id.value,
        pageIndex.value,
        pageSize.value,
        branchId.value,
        searchTerm?.value ?? null,
        startDate?.value ?? null,
        endDate?.value ?? null,
        sortCriteria?.value
      ),
    ...options,
  })
}
