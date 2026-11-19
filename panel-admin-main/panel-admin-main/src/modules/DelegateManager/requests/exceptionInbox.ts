import { computed, type Ref } from 'vue'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesRequestEndpoints } from '@/modules/SalesRequests/domain/endpoints'
import { SALES_REQUESTS_QUERY_KEY } from '@/modules/SalesRequests/requests/queries/useSalesRequestsQuery'
import { SALES_REQUEST_COUNTS_KEY } from '@/modules/SalesRequests/requests/queries/useSalesRequestCountsQuery'
import { SALES_REQUEST_BY_ID_KEY } from '@/modules/SalesRequests/requests/queries/useSalesRequestByIdQuery'
import { DELEGATE_SUMMARY_KEY } from '../composables/useDelegateSummary'

export type ExceptionInboxStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled'

export interface ExceptionInboxRow {
  id: number
  customerName: string
  customerPhone?: string | null
  branchId: number
  branchName?: string | null
  existingCustomerId?: number | null
  status: ExceptionInboxStatus
  requestStatus: string
  requestedAtUtc?: string | null
  requestedByName?: string | null
  requestReason?: string | null
  decidedAtUtc?: string | null
  decidedByName?: string | null
  decisionNote?: string | null
  isRead: boolean
  readAtUtc?: string | null
}

export interface ExceptionInboxResponse {
  items: ExceptionInboxRow[]
  totalCount: number
  page: number
  pageSize: number
  counts: { pending: number; approved: number; rejected: number; cancelled: number }
}

export const EXCEPTION_INBOX_KEY = 'delegate-exception-inbox'

export const EXCEPTION_STATUS_LABELS: Record<ExceptionInboxStatus, string> = {
  Pending: 'قيد الانتظار',
  Approved: 'موافق عليه',
  Rejected: 'مرفوض',
  Cancelled: 'ملغى',
}

export function useExceptionInboxQuery(params: Ref<{ status: ExceptionInboxStatus; branchId: number | null; search: string; page: number }>) {
  return useQuery({
    queryKey: computed(() => [EXCEPTION_INBOX_KEY, params.value]),
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const p = params.value
      const { data } = await axiosInstance.get<ExceptionInboxResponse>('/admin/SalesRequests/exception-inbox', {
        signal,
        params: {
          status: p.status,
          branchId: p.branchId ?? undefined,
          search: p.search.trim() || undefined,
          page: p.page,
          pageSize: 20,
        },
      })
      return data
    },
  })
}

export function useMarkExceptionReadMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => axiosInstance.post(`/admin/SalesRequests/${id}/exception-read`).then((r) => r.data),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: [EXCEPTION_INBOX_KEY] })
      void qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] })
    },
  })
}

export function useExceptionDecisionMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, approve, reason }: { id: number; approve: boolean; reason: string }) =>
      axiosInstance
        .post(approve ? SalesRequestEndpoints.approveException(id) : SalesRequestEndpoints.rejectException(id), { reason })
        .then((r) => r.data),
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: [EXCEPTION_INBOX_KEY] })
      void qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_BY_ID_KEY] })
    },
  })
}

export function decisionErrorMessage(e: unknown): string {
  const ax = e as { response?: { status?: number; data?: { title?: string; detail?: string; errors?: { code?: string; message?: string }[] } } }
  const status = ax.response?.status
  const body = ax.response?.data
  const code = String(body?.errors?.[0]?.code || body?.title || '')
  const msg = body?.errors?.[0]?.message || body?.detail
  if (code.includes('Self_Approval') || code.includes('SelfApproval')) return 'لا يمكن لطالب الاستثناء أن يقرر في طلبه.'
  if (status === 409) return 'تم اتخاذ القرار من مستخدم آخر. أعدنا تحميل الطلب.'
  if (status === 403) return msg || 'غير مصرح بهذا الإجراء'
  if (status === 404) return 'الطلب غير موجود'
  return msg || 'تعذر تنفيذ الإجراء'
}
