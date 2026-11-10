import { axiosInstance } from '@/modules/Core/plugins/axios'
import { ManagementTaskEndpoints } from '../../domain/endpoints'

export async function downloadManagementTaskAttachment(
  taskId: number,
  fileId: number,
  fileName: string
) {
  const response = await axiosInstance.get<Blob>(
    ManagementTaskEndpoints.attachment(taskId, fileId),
    { responseType: 'blob' }
  )
  const url = URL.createObjectURL(response.data)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}
