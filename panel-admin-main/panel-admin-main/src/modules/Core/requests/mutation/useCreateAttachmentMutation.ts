import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import type { AttachmentCreatePayload } from '../../types/api/attachment'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { QUERY_KEY_USER_BY_ID } from '@/modules/User/requests/queries'

const ENDPOINT = '/admin/Attachments'

export function createAttachment(
  payload: AttachmentCreatePayload
): Promise<AxiosResponse> {
  return axiosInstance.post(ENDPOINT, payload, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
}

export function useCreateAttachmentMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    AttachmentCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createAttachment,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_USER_BY_ID],
      })
    },
    ...options,
  })
}
