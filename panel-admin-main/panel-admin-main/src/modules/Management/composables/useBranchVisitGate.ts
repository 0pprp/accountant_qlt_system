import { computed, ref, watch } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { useRoute, useRouter } from 'vue-router'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export type ActiveBranchVisit = {
  id: number
  branchId: number
  status: string
}

/**
 * بوابة زيارة لأعمال إدارة الفروع فقط.
 *
 * - تُفرض على تبويبات `/management/branch` عدا visit-workspace.
 * - لا تُفرض على oversight ولا على تقارير المفوض (delegate/history).
 * - نقص BranchVisit.Read لا يمنح تجاوزًا؛ الاستعلام يحتاج Read أو Open فقط للفحص.
 * - الزيارة مربوطة بالفاعل + selectedBranch؛ زيارة A لا تفتح أعمال B.
 */
export function useBranchVisitGate() {
  const route = useRoute()
  const router = useRouter()
  const auth = useAuthStore()

  /** meta من أعمق سجل مطابق — لا تعتمد على دمج الأب فقط. */
  const leafMeta = computed(() => {
    const matched = route.matched
    return matched.length > 0
      ? matched[matched.length - 1]!.meta
      : route.meta
  })
  const isBranchSection = computed(
    () => leafMeta.value.managementSectionId === 'branch'
  )
  const isVisitWorkspace = computed(
    () => leafMeta.value.managementTabId === 'branchVisitWorkspace'
  )
  /** صلاحية فحص الزيارة النشطة — ليست شرط التجاوز. */
  const canProbeVisit = computed(
    () =>
      auth.hasPermission('BranchVisit', 'Read') === true ||
      auth.hasPermission('BranchVisit', 'Open') === true
  )
  const selectedBranchId = computed(() => auth.selectedBranch?.id ?? null)

  const activeQuery = useQuery({
    queryKey: computed(() => [
      'branch-visit-active',
      auth.sessionGeneration,
      auth.branchGeneration,
      selectedBranchId.value,
    ]),
    enabled: computed(
      () =>
        isBranchSection.value &&
        canProbeVisit.value &&
        auth.permissionsReady === true
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<ActiveBranchVisit | null>(
        '/admin/BranchVisits/active',
        { signal }
      )
      return data
    },
    staleTime: 0,
    retry: false,
    refetchOnWindowFocus: false,
  })

  const hasMatchingOpenVisit = computed(() => {
    const visit = activeQuery.data.value
    if (!visit || selectedBranchId.value == null) return false
    if (String(visit.status).toLowerCase() !== 'open') return false
    return visit.branchId === selectedBranchId.value
  })

  /** تحميل أولي فقط — لا تُفرَّغ الأعمال أثناء إعادة جلب خلفية لزيارة مطابقة. */
  const gateLoading = computed(
    () =>
      isBranchSection.value &&
      !isVisitWorkspace.value &&
      canProbeVisit.value &&
      activeQuery.isPending.value &&
      activeQuery.data.value === undefined
  )

  const gateError = computed(
    () =>
      isBranchSection.value &&
      !isVisitWorkspace.value &&
      canProbeVisit.value &&
      activeQuery.isError.value
  )

  /**
   * يركّب RouterView للأعمال فقط عند زيارة مطابقة جاهزة.
   * صفحة الزيارة نفسها تُركَّب دائمًا (بلا حلقة توجيه).
   */
  const allowOpsContent = computed(() => {
    if (!isBranchSection.value) return true
    if (isVisitWorkspace.value) return true
    if (!canProbeVisit.value) return false
    if (gateLoading.value || gateError.value) return false
    return hasMatchingOpenVisit.value
  })

  /** إعادة توجيه لأعمال الفرع عند عدم استيفاء الزيارة (بعد انتهاء الفحص أو تعذّره). */
  const needsVisitRedirect = computed(() => {
    if (!isBranchSection.value || isVisitWorkspace.value) return false
    if (!canProbeVisit.value) return true
    if (gateLoading.value) return false
    if (gateError.value) return true
    return !hasMatchingOpenVisit.value
  })

  const redirecting = ref(false)

  watch(
    needsVisitRedirect,
    (need) => {
      if (!need || redirecting.value) return
      if (route.name === 'ManagementBranchVisitWorkspaceRoute') return
      redirecting.value = true
      void router
        .replace({ name: 'ManagementBranchVisitWorkspaceRoute' })
        .finally(() => {
          redirecting.value = false
        })
    },
    { immediate: true }
  )

  const gateMessage = computed(() => {
    if (gateLoading.value) return 'جاري التحقق من الزيارة…'
    if (gateError.value)
      return 'تعذر التحقق من الزيارة. افتح زيارة الفرع أو أعد المحاولة.'
    if (!canProbeVisit.value)
      return 'لا صلاحية لفحص/فتح الزيارة — أعمال الفرع مقفلة حتى تُستوفى الزيارة.'
    if (
      activeQuery.data.value &&
      String(activeQuery.data.value.status).toLowerCase() === 'open' &&
      activeQuery.data.value.branchId !== selectedBranchId.value
    ) {
      return 'الزيارة المفتوحة لفرع آخر. بدّل الفرع أو أغلق الزيارة وافتح زيارة الفرع المحدد.'
    }
    return 'افتح زيارة فرع قبل دخول أعمال إدارة الفروع.'
  })

  return {
    isBranchSection,
    isVisitWorkspace,
    hasMatchingOpenVisit,
    allowOpsContent,
    needsVisitRedirect,
    activeVisit: computed(() => activeQuery.data.value),
    loading: gateLoading,
    error: gateError,
    gateMessage,
  }
}
