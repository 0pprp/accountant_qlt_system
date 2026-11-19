import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'

export interface DelegateSummary {
  unreadComplaints: number | null
  unreadComplaintsAvailable: boolean
  pendingExceptions: number | null
  pendingExceptionsAvailable: boolean
  unreadWrongSales: number | null
  unreadWrongSalesAvailable: boolean
  approvedExceptions: number | null
  rejectedExceptions: number | null
  cancelledExceptions: number | null
  totalExceptions: number | null
  exceptionsSummaryAvailable: boolean
  pendingWaivers: number | null
  awaitingAccountantWaivers: number | null
  approvedWaivers: number | null
  rejectedWaivers: number | null
  totalWaivers: number | null
  waiversAvailable: boolean
  openTasks: number | null
  openTasksAvailable: boolean
}

export const DELEGATE_SUMMARY_KEY = 'delegate-home'

export function useDelegateSummary(branchId: Ref<number | null>) {
  const auth = useAuthStore()
  const canRead = computed(() => auth.hasPermission('DelegateHome', 'Read') === true)
  return useQuery({
    queryKey: computed(() => [DELEGATE_SUMMARY_KEY, auth.sessionGeneration, branchId.value]),
    enabled: computed(() => canRead.value && auth.permissionsReady),
    refetchOnWindowFocus: true,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<DelegateSummary>('/admin/DelegateHome/summary', {
        signal,
        params: branchId.value == null ? undefined : { branchId: branchId.value },
      })
      return data
    },
  })
}

export function delegateBranchId(raw: unknown): number | null {
  const n = Number(Array.isArray(raw) ? raw[0] : raw)
  return Number.isInteger(n) && n > 0 ? n : null
}

export function useDelegateBranchId() {
  const route = useRoute()
  const auth = useAuthStore()
  return computed(() => {
    const id = delegateBranchId(route.query.branchId)
    return id != null && (auth.branches ?? []).some((b) => b.id === id) ? id : null
  })
}
