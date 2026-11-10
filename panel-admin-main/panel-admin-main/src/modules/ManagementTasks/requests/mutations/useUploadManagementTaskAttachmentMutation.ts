import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type { ManagementTaskAttachment } from '../../domain/types'
import { MANAGEMENT_TASK_DETAIL_KEY } from '../queries/useManagementTaskDetailQuery'

export function uploadManagementTaskAttachment(taskId: number, file: File) {
  const form = new FormData()
  form.append('file', file)
  return axiosInstance
    .post<ManagementTaskAttachment>(
      ManagementTaskEndpoints.attachments(taskId),
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    )
    .then((r) => r.data)
}

export function useUploadManagementTaskAttachmentMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (args: { taskId: number; file: File }) =>
      uploadManagementTaskAttachment(args.taskId, args.file),
    onSettled: (_data, _error, vars) => {
      void qc.invalidateQueries({
        queryKey: [MANAGEMENT_TASK_DETAIL_KEY],
        predicate: (q) => q.queryKey.includes(vars.taskId),
      })
    },
  })
}
