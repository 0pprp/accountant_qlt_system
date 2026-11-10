import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesLegalEndpoints } from '../../domain/endpoints'

export async function downloadLegalFollowupDocument(
  caseId: number,
  docId: number,
  fileName: string
) {
  const response = await axiosInstance.get<Blob>(
    SalesLegalEndpoints.documentFile(caseId, docId),
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

export async function previewLegalFollowupDocument(
  caseId: number,
  docId: number
): Promise<{ url: string; contentType: string }> {
  const response = await axiosInstance.get<Blob>(
    SalesLegalEndpoints.documentFile(caseId, docId),
    { responseType: 'blob' }
  )
  const contentType = response.headers['content-type'] ?? response.data.type ?? ''
  return { url: URL.createObjectURL(response.data), contentType }
}
