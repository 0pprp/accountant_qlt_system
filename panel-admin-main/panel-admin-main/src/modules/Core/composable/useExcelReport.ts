import { ref } from 'vue'
import type { ExcelReportParamValue } from '../components/base/ExcelReport/ExcelReport.types'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import useAuthStore from '@/modules/Auth/store'

export interface ExcelReportOptions {
  endpoint: string
  filename?: string
  params?: Record<string, ExcelReportParamValue>
  successMessage?: {
    title: string
    description?: string
  }
  errorMessage?: {
    title: string
    description?: string
  }
}

export function useExcelReport() {
  const isLoading = ref(false)
  const toastStore = useToastStore()
  const authStore = useAuthStore()
  const baseUrl = import.meta.env.VITE_API_BASE_URL

  async function downloadExcelReport(options: ExcelReportOptions) {
    isLoading.value = true

    try {
      // Build URL with parameters
      const url = new URL(`${baseUrl}${options.endpoint}`)

      if (options.params) {
        Object.entries(options.params).forEach(([key, value]) => {
          if (value !== null && value !== undefined) {
            if (Array.isArray(value)) {
              value.forEach((item) => {
                if (item !== null && item !== undefined) {
                  url.searchParams.append(key, String(item))
                }
              })
            } else {
              url.searchParams.append(key, String(value))
            }
          }
        })
      }

      const response = await fetch(url.toString(), {
        method: 'GET',
        headers: {
          Authorization: `Bearer ${authStore.accessToken}`,
        },
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      // Get filename from Content-Disposition header or use provided filename
      const contentDisposition = response.headers.get('Content-Disposition')
      let filename = options.filename || 'report.xlsx'

      if (contentDisposition && contentDisposition.includes('attachment')) {
        const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(
          contentDisposition
        )
        if (matches != null && matches[1]) {
          filename = matches[1].replace(/['"]/g, '')
        }
      }

      // Download the file
      const blob = await response.blob()

      if (!blob || blob.size === 0) {
        throw new Error('Received an empty file')
      }

      const blobUrl = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = blobUrl
      link.setAttribute('download', filename)

      document.body.appendChild(link)
      link.click()

      // Cleanup
      link.parentNode?.removeChild(link)
      window.URL.revokeObjectURL(blobUrl)

      // Show success message
      if (options.successMessage) {
        toastStore.setMassage({
          title: options.successMessage.title,
          description: options.successMessage.description,
          dialogState: DialogState.Success,
          isOpen: true,
        })
      }
    } catch (error) {
      // Show error message
      const errorMsg = options.errorMessage || {
        title: 'خطأ في التصدير',
        description:
          error instanceof Error ? error.message : 'حدث خطأ أثناء تصدير الملف',
      }

      toastStore.setMassage({
        title: errorMsg.title,
        description: errorMsg.description,
        dialogState: DialogState.Error,
        isOpen: true,
      })
    } finally {
      isLoading.value = false
    }
  }

  return {
    downloadExcelReport,
    isLoading,
  }
}
