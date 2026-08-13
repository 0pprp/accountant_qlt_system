import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_CUSTOMER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { CustomerCreatePayload } from '@/modules/Customer/types/api'

const ENDPOINT = `/Admin/Customers/{id}`

export function updateCustomer(
  id: number,
  payload: CustomerCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload)
}

export function useUpdateCustomerMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: CustomerCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateCustomer(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_CUSTOMER_LIST],
      })
    },
    ...options,
  })
}
