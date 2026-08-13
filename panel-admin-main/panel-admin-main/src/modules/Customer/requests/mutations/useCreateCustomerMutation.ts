import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_CUSTOMER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CustomerCreatePayload } from '@/modules/Customer/types/api'

const ENDPOINT = '/admin/Customers'

export function createCustomer(
  payload: CustomerCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post<{
    id: number
  }>(ENDPOINT, payload)
}

export function useCreateCustomerMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    CustomerCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createCustomer,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_CUSTOMER_LIST],
      })
    },
    ...options,
  })
}
