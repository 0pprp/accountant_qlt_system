import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import type { AttachmentCreatePayload } from '../../types/api/attachment'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { QUERY_KEY_USER_BY_ID } from '@/modules/User/requests/queries'

const ENDPOINT = `/Admin/Attachments/{id}`

export function updateAttachment(
  id: number,
  payload: AttachmentCreatePayload
): Promise<AxiosResponse> {
  const endpoint = ENDPOINT.replace('{id}', String(id))

  return axiosInstance.put(endpoint, payload, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
}

export function useUpdateAttachmentMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    { id: number; payload: AttachmentCreatePayload },
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }) => updateAttachment(id, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_BY_ID],
      })
    },
    ...options,
  })
}
