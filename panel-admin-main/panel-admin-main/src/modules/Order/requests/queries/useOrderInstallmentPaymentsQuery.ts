import { type UseQueryOptions, useQuery } from '@tanstack/vue-query'
import type { Ref } from 'vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { OrderInstallmentPaymentsResponse } from '@/modules/Order/types/api'

export const QUERY_KEY = 'order-installment-payments'
const ENDPOINT = '/admin/Orders/{id}/installment-payments'

export function fetchOrderInstallmentPayments(id: number) {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance
    .get<OrderInstallmentPaymentsResponse>(endpoint)
    .then((response) => response.data)
}

export function useOrderInstallmentPaymentsQuery(
  id: Ref<number>,
  options?: Omit<
    UseQueryOptions<
      UseQueryOptions<Awaited<ReturnType<typeof fetchOrderInstallmentPayments>>>
    >,
    'queryKey' | 'queryFn'
  >
) {
  return useQuery({
    queryKey: [QUERY_KEY, id],
    queryFn: () => fetchOrderInstallmentPayments(id.value),
    enabled: () => !!id.value,
    ...options,
  })
}
