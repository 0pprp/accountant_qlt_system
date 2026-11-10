import { computed, ref, watch, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { SalesRequestListFilters } from '../requests/queries/useSalesRequestsQuery'
import type { SalesRequestTabKey } from '../domain/tabs'

export interface WorkspaceUrlState {
  tab: SalesRequestTabKey
  page: number
  selectedId: number | null
  filters: Omit<SalesRequestListFilters, 'tab'>
  mobileFiltersOpen: boolean
}

function numOrNull(v: unknown): number | null {
  if (v == null || v === '') return null
  const n = Number(v)
  return Number.isFinite(n) ? n : null
}

export function useSalesRequestFilters(
  resolveTab: (preferred?: string | null) => SalesRequestTabKey
) {
  const route = useRoute()
  const router = useRouter()

  const tab = ref<SalesRequestTabKey>(
    resolveTab(typeof route.query.tab === 'string' ? route.query.tab : null)
  )
  const pageIndex = ref(numOrNull(route.query.page) || 1)
  const pageSize = ref(20)
  const selectedId = ref<number | null>(numOrNull(route.query.requestId))
  const mobileFiltersOpen = ref(false)

  const filters = ref<Omit<SalesRequestListFilters, 'tab'>>({
    branchId: numOrNull(route.query.branchId),
    provinceId: numOrNull(route.query.provinceId),
    employeeId: numOrNull(route.query.employeeId),
    sourceType:
      typeof route.query.sourceType === 'string' ? route.query.sourceType : '',
    fromDate:
      typeof route.query.fromDate === 'string' ? route.query.fromDate : null,
    toDate: typeof route.query.toDate === 'string' ? route.query.toDate : null,
    search: typeof route.query.search === 'string' ? route.query.search : null,
  })

  const listFilters = computed<SalesRequestListFilters>(() => ({
    tab: tab.value,
    ...filters.value,
  }))

  function syncUrl() {
    const q: Record<string, string> = {
      tab: tab.value,
      page: String(pageIndex.value),
    }
    if (selectedId.value) q.requestId = String(selectedId.value)
    if (filters.value.branchId) q.branchId = String(filters.value.branchId)
    if (filters.value.provinceId)
      q.provinceId = String(filters.value.provinceId)
    if (filters.value.employeeId)
      q.employeeId = String(filters.value.employeeId)
    if (filters.value.sourceType) q.sourceType = filters.value.sourceType
    if (filters.value.fromDate) q.fromDate = filters.value.fromDate
    if (filters.value.toDate) q.toDate = filters.value.toDate
    if (filters.value.search) q.search = filters.value.search

    void router.replace({ query: q })
  }

  watch(
    [tab, pageIndex, selectedId, filters],
    () => {
      syncUrl()
    },
    { deep: true }
  )

  function setTab(next: SalesRequestTabKey) {
    tab.value = next
    pageIndex.value = 1
  }

  function clearFilters() {
    filters.value = {
      branchId: null,
      provinceId: null,
      employeeId: null,
      sourceType: '',
      fromDate: null,
      toDate: null,
      search: null,
    }
    pageIndex.value = 1
  }

  return {
    tab,
    pageIndex,
    pageSize,
    selectedId,
    filters,
    listFilters,
    mobileFiltersOpen,
    setTab,
    clearFilters,
    syncUrl,
  }
}

export type SalesRequestFiltersApi = ReturnType<typeof useSalesRequestFilters>
