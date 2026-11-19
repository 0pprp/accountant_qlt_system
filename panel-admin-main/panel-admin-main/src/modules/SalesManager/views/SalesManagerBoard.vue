<template>
  <div class="dfb fwb smb" dir="rtl">
    <header class="dfb-head">
      <div>
        <h1 class="dfb-title">المبيعات</h1>
        <p class="dfb-subtitle">اختر موظفاً من العمود أو «كل الطلبات»، وكل الأقسام تظهر بجانبه</p>
      </div>
      <div class="dfb-actions">
        <label v-if="branches.length > 1" class="fwb-select">
          <span class="sr-only">الفرع</span>
          <select :value="branchId ?? ''" class="dfb-search" aria-label="الفرع" @change="setBranch(($event.target as HTMLSelectElement).value)">
            <option value="">كل الفروع</option>
            <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <button type="button" class="dfb-button" @click="refresh">تحديث</button>
      </div>
    </header>

    <div class="fwb-layout">
      <aside class="fwb-rail" aria-label="موظفو المبيعات">
        <button
          type="button"
          class="fwb-follower smb-all"
          :class="{ 'is-active': employeeId == null }"
          :aria-pressed="employeeId == null"
          @click="selectEmployee(null)"
        >
          <span class="fwb-follower-name">كل الطلبات</span>
          <span class="dfb-cell-sub">كل الموظفين{{ currentBranch ? ` · ${currentBranch.name}` : '' }}</span>
        </button>

        <template v-if="canReadEmployees">
          <input v-model="railSearch" class="dfb-search" type="search" placeholder="بحث موظف بالاسم" aria-label="بحث موظف">
          <label class="fwb-select fwb-mobile-only">
            <span class="sr-only">الموظف</span>
            <select :value="employeeId ?? ''" class="dfb-search" aria-label="الموظف" @change="selectEmployee(positive(($event.target as HTMLSelectElement).value))">
              <option value="">كل الطلبات</option>
              <option v-for="e in railItems" :key="e.employeeId" :value="e.employeeId">{{ e.employeeName }}</option>
            </select>
          </label>
          <p v-if="employeesQuery.isPending.value" class="dfb-state" role="status">جاري تحميل الموظفين…</p>
          <div v-else-if="employeesQuery.isError.value" class="dfb-error" role="alert">
            تعذر تحميل الموظفين
            <button type="button" class="dfb-button is-small" @click="employeesQuery.refetch()">إعادة المحاولة</button>
          </div>
          <p v-else-if="!railItems.length" class="dfb-state">لا يوجد موظفو مبيعات</p>
          <ul v-else class="fwb-rail-list">
            <li v-for="e in railItems" :key="e.employeeId">
              <button
                type="button"
                class="fwb-follower"
                :class="{ 'is-active': e.employeeId === employeeId }"
                :aria-pressed="e.employeeId === employeeId"
                @click="selectEmployee(e.employeeId)"
              >
                <span class="fwb-follower-name">{{ e.employeeName }}</span>
                <span class="fwb-follower-score" :class="`tier-${e.ratingLabel ?? ''}`">
                  <span class="dfb-num">{{ fixed1(e.latestRating) }}</span>
                </span>
                <span class="dfb-cell-sub">
                  <span class="smb-dot" :class="`is-${locationTone(e)}`" aria-hidden="true" />
                  {{ shiftText(e) }} · اليوم {{ e.salesTodayCount }} · معلّق {{ e.pendingSalesCount }}
                </span>
              </button>
            </li>
          </ul>
        </template>
      </aside>

      <main class="fwb-main">
        <header v-if="selected" class="dfb-head">
          <div>
            <h2 class="dfb-title fwb-name">{{ selected.employeeName }}</h2>
            <p class="dfb-subtitle">
              التقييم الحقيقي <strong class="dfb-num">{{ fixed1(selected.latestRating) }}</strong>
              <template v-if="selected.ratingLabel"> ({{ selected.ratingLabel }})</template>
              · الوهمي <span class="dfb-num">{{ fixed1(selected.phantomLatestRating) }}</span>
              · {{ shiftText(selected) }} · {{ selected.branchName }}
            </p>
          </div>
        </header>

        <nav class="fwb-tabs" aria-label="أقسام المبيعات">
          <button
            v-for="s in visibleSections"
            :key="s.key"
            type="button"
            class="fwb-tab"
            :aria-pressed="s.key === active?.key"
            @click="setSection(s.routeName)"
          >
            {{ s.label }}
          </button>
        </nav>

        <p v-if="!visibleSections.length" class="dfb-state">لا تملك صلاحية أي قسم من أقسام المبيعات</p>
        <p v-else-if="active?.employeeScoped && employeeId == null" class="dfb-state">اختر موظفاً من العمود لعرض {{ active.label }}</p>
        <section v-else-if="active" class="dfb-section fwb-section smb-section" :aria-label="active.label">
          <SalesManagerRatingSection v-if="active.key === 'rating'" :employee-id="employeeId" :branch-id="branchId" />
          <component :is="activeComponent" v-else-if="activeComponent" :key="`${active.key}-${employeeId}-${branchId}`" />
        </section>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent, ref, watch, type Component } from 'vue'
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import { useQueryClient } from '@tanstack/vue-query'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import '@/modules/FollowerWork/components/board/follower-work-board.css'
import '../components/sales-manager-board.css'
import { useAuthStore } from '@/modules/Auth/store'
import { useSalesEmployeesQuery } from '@/modules/SalesEmployees/requests/queries/useSalesEmployeesQuery'
import type { SalesEmployeeCard } from '@/modules/SalesEmployees/domain/types'
import { useSalesRequestTabAccess } from '@/modules/SalesRequests/composables/useSalesRequestTabAccess'
import SalesManagerRatingSection from '../components/SalesManagerRatingSection.vue'
import { SALES_MANAGER_SECTIONS, sectionByRouteName, type SalesManagerSection } from '../domain/sections'

const BRANCH_KEY = 'sales-manager-branch'
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const qc = useQueryClient()
const tabAccess = useSalesRequestTabAccess()

const COMPONENTS: Partial<Record<SalesManagerSection, Component>> = {
  overview: defineAsyncComponent(() => import('@/modules/SalesOverview/views/SalesOverviewPage.vue')),
  requests: defineAsyncComponent(() => import('@/modules/SalesRequests/views/SalesRequestsWorkspace.vue')),
  followup: defineAsyncComponent(() => import('@/modules/SalesFollowup/views/SalesFollowupPage.vue')),
  'excel-search': defineAsyncComponent(() => import('@/modules/SalesExcelSearch/views/SalesExcelSearchPage.vue')),
  sales: defineAsyncComponent(() => import('@/modules/SalesRecords/views/SalesCompletedPage.vue')),
  'live-map': defineAsyncComponent(() => import('@/modules/SalesLiveMap/views/SalesLiveMapPage.vue')),
  routes: defineAsyncComponent(() => import('@/modules/SalesRoutes/views/SalesRoutesPage.vue')),
  summary: defineAsyncComponent(() => import('@/modules/SalesSummary/views/SalesSummaryPage.vue')),
  legal: defineAsyncComponent(() => import('@/modules/SalesLegal/views/SalesLegalPage.vue')),
}

function positive(v: unknown) {
  const n = Number(Array.isArray(v) ? v[0] : v)
  return Number.isInteger(n) && n > 0 ? n : null
}

const branches = computed(() => auth.branches ?? [])
const branchId = computed(() => {
  const id = positive(route.query.branchId)
  return id != null && branches.value.some((b) => b.id === id) ? id : null
})
const currentBranch = computed(() => branches.value.find((b) => b.id === branchId.value) ?? null)
const employeeId = computed(() => positive(route.query.employeeId))

const visibleSections = computed(() =>
  SALES_MANAGER_SECTIONS.filter((s) =>
    s.key === 'requests'
      ? tabAccess.canOpenWorkspace.value
      : s.permissions.some((p) => auth.hasPermission(p.module, p.action) === true)
  )
)
const active = computed(() => {
  const fromRoute = sectionByRouteName(route.name)
  return visibleSections.value.find((s) => s.key === fromRoute?.key) ?? visibleSections.value[0] ?? null
})
const activeComponent = computed(() => (active.value ? COMPONENTS[active.value.key] ?? null : null))

watch(
  () => [route.name, visibleSections.value.length] as const,
  () => {
    const fromRoute = sectionByRouteName(route.name)
    if (active.value && fromRoute?.key !== active.value.key)
      void router.replace({ name: active.value.routeName, query: route.query })
  },
  { immediate: true }
)

function patch(next: Record<string, string | null | undefined>, replace = true) {
  const query: LocationQueryRaw = { ...route.query }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete query[k]
    else query[k] = v
  }
  const target = { name: route.name ?? 'SaleOverviewRoute', query }
  void (replace ? router.replace(target) : router.push(target))
}

watch(
  branches,
  (list) => {
    if (route.query.branchId != null || list.length === 0) return
    let stored: number | null = null
    try {
      stored = positive(localStorage.getItem(BRANCH_KEY))
    } catch {
      stored = null
    }
    const fallback = stored != null && list.some((b) => b.id === stored) ? stored : list.length === 1 ? list[0].id : null
    if (fallback != null) patch({ branchId: String(fallback) })
  },
  { immediate: true }
)

function setBranch(raw: string) {
  const id = positive(raw)
  try {
    if (id == null) localStorage.removeItem(BRANCH_KEY)
    else localStorage.setItem(BRANCH_KEY, String(id))
  } catch {
    void 0
  }
  patch({ branchId: id == null ? null : String(id), employeeId: null, sellerId: null, page: null, requestId: null }, false)
}

const canReadEmployees = computed(() => auth.hasPermission('SalesEmployee', 'Read') === true)
const railSearch = ref('')
const employeesQuery = useSalesEmployeesQuery(
  computed(() => ({ branchId: branchId.value, search: '', shiftState: '', page: 1, pageSize: 100, sort: 'name' })),
  canReadEmployees
)
const allEmployees = computed<SalesEmployeeCard[]>(() => employeesQuery.data.value?.items ?? [])
const railItems = computed(() => {
  const q = railSearch.value.trim()
  return q ? allEmployees.value.filter((e) => e.employeeName.includes(q)) : allEmployees.value
})
const selected = computed(() => allEmployees.value.find((e) => e.employeeId === employeeId.value) ?? null)

function selectEmployee(id: number | null) {
  if (id === employeeId.value) return
  const value = id == null ? null : String(id)
  patch({ employeeId: value, sellerId: value, page: null, requestId: null }, false)
}

function setSection(routeName: string) {
  if (routeName === route.name) return
  const keep: LocationQueryRaw = {}
  for (const k of ['branchId', 'employeeId', 'sellerId']) if (route.query[k] != null) keep[k] = route.query[k]
  void router.push({ name: routeName, query: keep })
}

function refresh() {
  void qc.invalidateQueries()
}

function fixed1(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : n.toFixed(1)
}

function shiftText(e: SalesEmployeeCard) {
  return e.shiftStatus === 'Active' ? 'في الدوام' : 'خارج الدوام'
}

function locationTone(e: SalesEmployeeCard) {
  if (e.shiftStatus !== 'Active') return 'off'
  return e.locationStatus === 'Live' ? 'live' : e.locationStatus === 'Stale' ? 'stale' : 'off'
}
</script>
