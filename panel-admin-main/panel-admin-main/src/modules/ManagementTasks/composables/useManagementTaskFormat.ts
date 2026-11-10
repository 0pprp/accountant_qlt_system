import { MANAGEMENT_TASK_STATUS_KEYS, type ManagementTaskStatusValue } from '../domain/types'

export function useManagementTaskFormat() {
  const dateTime = new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
  const dateOnly = new Intl.DateTimeFormat('ar-IQ', { dateStyle: 'medium' })

  function formatDateTime(value: string | null | undefined): string {
    if (!value) return '—'
    const d = new Date(value)
    return Number.isNaN(d.getTime()) ? '—' : dateTime.format(d)
  }

  function formatDate(value: string | null | undefined): string {
    if (!value) return '—'
    const d = new Date(`${value.slice(0, 10)}T00:00:00`)
    return Number.isNaN(d.getTime()) ? '—' : dateOnly.format(d)
  }

  function formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
    return `${(bytes / 1024 / 1024).toFixed(1)} MB`
  }

  function statusKey(status: ManagementTaskStatusValue): string {
    return MANAGEMENT_TASK_STATUS_KEYS[status] ?? 'sent'
  }

  return { formatDateTime, formatDate, formatSize, statusKey }
}
