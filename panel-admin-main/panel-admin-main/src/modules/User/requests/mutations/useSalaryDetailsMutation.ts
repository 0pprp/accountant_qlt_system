import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_USER_LIST } from '../queries'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SalaryDetailsPayload } from '@/modules/User/types/api'

const ENDPOINT = '/admin/Users/{id}/salary-detail'

export function salaryDetails(
  id: number,
  payload: SalaryDetailsPayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))
  return axiosInstance.put(endpoint, payload)
}

export function useSalaryDetailsMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: SalaryDetailsPayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => salaryDetails(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_LIST],
      })
    },
    ...options,
  })
}
