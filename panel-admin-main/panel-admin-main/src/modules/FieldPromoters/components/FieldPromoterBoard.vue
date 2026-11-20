<template>
  <div class="dfb fwb dmb fpb" dir="rtl">
    <header class="dfb-head">
      <div>
        <h1 class="dfb-title">المروّجون الميدانيون</h1>
        <p class="dfb-subtitle">{{ subtitle }}</p>
      </div>
      <div class="dfb-actions">
        <label v-if="branches.length > 1" class="fwb-select">
          <span class="sr-only">المحافظة</span>
          <select :value="branchId ?? ''" class="dfb-search" aria-label="المحافظة" @change="setBranch(($event.target as HTMLSelectElement).value)">
            <option value="">كل المحافظات</option>
            <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <button type="button" class="dfb-button" @click="refresh">تحديث</button>
      </div>
    </header>

    <div class="fwb-layout dmb-layout">
      <aside class="fwb-rail dmb-rail" aria-label="أقسام المروّجين الميدانيين">
        <label class="fwb-select fwb-mobile-only">
          <span class="sr-only">القسم</span>
          <select :value="activeTabId" class="dfb-search" aria-label="القسم" @change="openById(($event.target as HTMLSelectElement).value)">
            <option v-for="item in navItems" :key="item.id" :value="item.tab?.id ?? ''" :disabled="!item.tab">
              {{ item.label }}{{ item.count ? ` (${item.count})` : '' }}
            </option>
          </select>
        </label>
        <ul class="dmb-nav">
          <li v-for="item in navItems" :key="item.id">
            <button
              type="button"
              class="dmb-nav-item"
              :class="{ 'is-active': item.tab?.id === activeTabId, 'is-locked': item.locked }"
              :disabled="!item.tab"
              :aria-current="item.tab?.id === activeTabId ? 'page' : undefined"
              @click="item.tab && open(item.tab)"
            >
              <i :class="['pi', item.locked ? 'pi-lock' : item.icon]" aria-hidden="true" />
              <span class="dmb-nav-label">{{ item.label }}</span>
              <span v-if="item.count" class="dmb-count" :class="`is-${item.tone}`">{{ num(item.count) }}</span>
              <small v-else-if="!item.tab" class="dmb-soon">غير متاح</small>
            </button>
          </li>
        </ul>
      </aside>

      <main class="fwb-main dmb-main">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import { useQueryClient } from '@tanstack/vue-query'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import '@/modules/FollowerWork/components/board/follower-work-board.css'
import '@/modules/DelegateManager/components/delegate-manager-board.css'
import './field-promoter-board.css'
import { useOrdersQuery, usePromotersLiveQuery } from '../requests/fieldPromoters'
import { num } from '../domain/format'
import { useAuthStore } from '@/modules/Auth/store'
import { useManagementAccess } from '@/modules/Management/composables/useManagementAccess'
import { getTabByIds, type ManagementTabDefinition } from '@/modules/Management/domain/sections'
import { APP_TABS } from '@/modules/Management/domain/workspaceNavigation'
import { delegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'

const BRANCH_KEY = 'field-promoter-branch'
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const qc = useQueryClient()
const { canOpenTab, isTabLocked } = useManagementAccess()

const branches = computed(() => auth.branches ?? [])
const branchId = computed(() => {
  const id = delegateBranchId(route.query.branchId)
  return id != null && branches.value.some((b) => b.id === id) ? id : null
})
const currentBranch = computed(() => branches.value.find((b) => b.id === branchId.value) ?? null)
const activeTabId = computed(() => String(route.meta.managementTabId ?? ''))

const live = usePromotersLiveQuery(branchId)
const orders = useOrdersQuery(
  computed(() => ({ branchId: branchId.value, userId: null, stage: 'inProgress' as const, from: '', to: '', page: 1 })),
  60_000
)

const subtitle = computed(() => `متابعة الدوام والمواقع والمحلات والطلبات · ${currentBranch.value?.name ?? 'كل المحافظات'}`)

type Tone = 'info' | 'warn' | 'danger'
const COUNTS: Record<string, { pick: () => number | undefined; tone: Tone }> = {
  fpAlerts: { pick: () => live.data.value?.unseenAlerts, tone: 'danger' },
  fpOrders: { pick: () => orders.data.value?.counts.inProgress, tone: 'warn' },
}

const navItems = computed(() =>
  APP_TABS.fieldPromoter.map((item) => {
    const tab = item.tabId ? getTabByIds('fieldPromoter', item.tabId) : undefined
    const rule = tab ? COUNTS[tab.id] : undefined
    const locked = tab ? isTabLocked(tab) : false
    const count = rule && !locked ? rule.pick() ?? 0 : 0
    return { ...item, tab, locked, count, tone: rule?.tone ?? 'info' }
  })
)

function keepQuery(): LocationQueryRaw {
  return branchId.value == null ? {} : { branchId: String(branchId.value) }
}

function open(tab: ManagementTabDefinition) {
  if (!canOpenTab(tab)) {
    void router.push({ name: 'AccessDeniedRoute', query: { from: tab.routeName } })
    return
  }
  if (tab.id === activeTabId.value) return
  void router.push({ name: tab.routeName, query: { ...keepQuery(), ...tab.defaultQuery } })
}

function openById(id: string) {
  const tab = navItems.value.find((i) => i.tab?.id === id)?.tab
  if (tab) open(tab)
}

function setBranch(raw: string) {
  const id = delegateBranchId(raw)
  try {
    if (id == null) localStorage.removeItem(BRANCH_KEY)
    else localStorage.setItem(BRANCH_KEY, String(id))
  } catch {
    void 0
  }
  const query: LocationQueryRaw = { ...route.query, page: undefined }
  if (id == null) delete query.branchId
  else query.branchId = String(id)
  void router.replace({ name: route.name ?? undefined, params: route.params, query })
}

watch(
  branches,
  (list) => {
    if (route.query.branchId != null || list.length === 0) return
    let stored: number | null = null
    try {
      stored = delegateBranchId(localStorage.getItem(BRANCH_KEY))
    } catch {
      stored = null
    }
    if (stored != null && list.some((b) => b.id === stored)) setBranch(String(stored))
  },
  { immediate: true }
)

function refresh() {
  void qc.invalidateQueries()
}
</script>
