import { type UseMutationOptions, useMutation } from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { axiosInstance } from '@/modules/Core/plugins/axios'

const ENDPOINT = `/Admin/Users/{id}/personal-documents/complete`

export function updateUserDocumentsStep(id: number): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint)
}

export function useUpdateUserDocumentsStepMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number },
    unknown
  >
) {
  return useMutation({
    mutationFn: ({ id }) => updateUserDocumentsStep(id),
    onSuccess: async () => {},
    ...options,
  })
}
