<template>
  <div class="dfb fwb" dir="rtl">
    <header class="dfb-head">
      <div>
        <h1 class="dfb-title">عمل المتابعين</h1>
        <p class="dfb-subtitle">اختر متابعاً من العمود، وأقسامه تظهر بجانبه مع عدد ما ينتظر في كل قسم</p>
      </div>
      <div class="dfb-actions">
        <label v-if="branches.length > 1" class="fwb-select">
          <span class="sr-only">الفرع</span>
          <select :value="branchId ?? ''" class="dfb-search" aria-label="الفرع" @change="setBranch(($event.target as HTMLSelectElement).value)">
            <option value="" disabled>اختر الفرع</option>
            <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <button type="button" class="dfb-button" :disabled="branchId == null" @click="refresh">تحديث</button>
        <button v-if="canManage && branchId != null" type="button" class="dfb-button is-primary" @click="openAccount('create')">+ إنشاء متابع</button>
      </div>
    </header>

    <p v-if="branchId == null" class="dfb-state">اختر الفرع لعرض متابعيه</p>

    <div v-else class="fwb-layout">
      <aside class="fwb-rail" aria-label="المتابعون">
        <input v-model="railSearch" class="dfb-search" type="search" placeholder="بحث متابع بالاسم أو الرقم" aria-label="بحث متابع">
        <label class="fwb-select fwb-mobile-only">
          <span class="sr-only">المتابع</span>
          <select :value="followerId ?? ''" class="dfb-search" aria-label="المتابع" @change="selectFollower(Number(($event.target as HTMLSelectElement).value))">
            <option v-for="f in railItems" :key="f.followerId" :value="f.followerId">{{ f.followerName }}</option>
          </select>
        </label>
        <p v-if="followersQuery.isPending.value" class="dfb-state" role="status">جاري تحميل المتابعين…</p>
        <div v-else-if="followersQuery.isError.value" class="dfb-error" role="alert">
          تعذر تحميل المتابعين
          <button type="button" class="dfb-button is-small" @click="followersQuery.refetch()">إعادة المحاولة</button>
        </div>
        <p v-else-if="!railItems.length" class="dfb-state">لا يوجد متابعون بقوائم في هذا الفرع</p>
        <ul v-else class="fwb-rail-list">
          <li v-for="f in railItems" :key="f.followerId">
            <button
              type="button"
              class="fwb-follower"
              :class="{ 'is-active': f.followerId === followerId }"
              :aria-pressed="f.followerId === followerId"
              @click="selectFollower(f.followerId)"
            >
              <span class="fwb-follower-name">{{ f.followerName }}</span>
              <span class="fwb-follower-score" :class="`tier-${f.metrics?.phantomRatingLabel ?? ''}`">
                <span :class="`dfb-trend-${f.metrics?.scoreTrend ?? 'flat'}`" aria-hidden="true">{{ arrow(f.metrics?.scoreTrend) }}</span>
                <span class="dfb-num">{{ fixed1(f.metrics?.phantomRating) }}</span>
              </span>
              <span class="dfb-cell-sub">
                {{ f.assignedListCount }} {{ f.assignedListCount === 1 ? 'قائمة' : 'قوائم' }}
                · {{ f.metrics?.ratedCustomerCount ?? '—' }} زبون مقيّم
              </span>
            </button>
          </li>
        </ul>
      </aside>

      <main class="fwb-main">
        <header v-if="selected" class="dfb-head">
          <div>
            <h2 class="dfb-title fwb-name">{{ selected.followerName }}</h2>
            <p class="dfb-subtitle">
              التقييم الوهمي <strong class="dfb-num">{{ fixed1(selected.metrics?.phantomRating) }}</strong>
              <template v-if="selected.metrics?.phantomRatingLabel"> ({{ selected.metrics.phantomRatingLabel }})</template>
              · قبل أسبوع <span class="dfb-num">{{ fixed1(selected.metrics?.phantomRatingWeekAgo) }}</span>
              <span :class="`dfb-trend-${selected.metrics?.scoreTrend ?? 'flat'}`"> ({{ trendText(selected.metrics?.scoreTrend) }})</span>
              · {{ selected.metrics?.ratedCustomerCount ?? '—' }} زبون مقيّم
            </p>
          </div>
          <button v-if="canManage" type="button" class="dfb-button" @click="openAccount('edit')">تعديل المتابع</button>
        </header>

        <nav class="fwb-tabs" aria-label="أقسام عمل المتابع">
          <button
            v-for="t in visibleTabs"
            :key="t.key"
            type="button"
            class="fwb-tab"
            :aria-pressed="t.key === activeTab"
            @click="setTab(t.key)"
          >
            {{ t.label }}
            <span v-if="t.count != null && t.count > 0" class="fwb-count" :class="{ 'is-warn': t.warn }">{{ t.count }}</span>
          </button>
        </nav>

        <p v-if="!visibleTabs.length" class="dfb-state">لا تملك صلاحية أي قسم من أقسام عمل المتابعين</p>
        <p v-else-if="needsFollower && followerId == null" class="dfb-state">اختر متابعاً من العمود</p>
        <section v-else class="dfb-section fwb-section" :aria-label="activeLabel">
          <FollowerShopCaptureSection
            v-if="activeTab === 'shop-capture'"
            :branch-id="branchId"
            :follower-id="followerId"
            @open-customer="openCustomer"
          />
          <component :is="activeComponent" v-else-if="activeComponent" :key="`${activeTab}-${followerId}-${branchId}`" />
        </section>
      </main>
    </div>

    <FollowerCustomerDrawer
      :follower-id="followerId"
      :customer-id="customerId"
      :branch-id="branchId"
      :can-open-full-profile="canOpenProfile"
      @close="openCustomer(null)"
      @full-profile="openFullProfile"
    />

    <FollowerAccountDialog
      :visible="accountMode != null"
      :mode="accountMode ?? 'create'"
      :branch-id="branchId"
      :branch-name="currentBranch?.name"
      :follower-id="accountMode === 'edit' ? followerId : null"
      :initial-name="selected?.followerName"
      @close="accountMode = null"
      @saved="onAccountSaved"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, defineAsyncComponent, provide, ref, watch, type Component } from 'vue'
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import { useQueryClient } from '@tanstack/vue-query'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import '../components/board/follower-work-board.css'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowersQuery } from '../requests/queries/useFollowersQuery'
import { useFollowerSectionCountsQuery } from '../requests/queries/useFollowerBoardQueries'
import { FOLLOWER_WORK_KEY } from '../domain/queryKeys'
import type { FollowerBoardTab } from '../domain/boardTypes'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'
import FollowerCustomerDrawer from '../components/board/FollowerCustomerDrawer.vue'
import FollowerAccountDialog from '../components/board/FollowerAccountDialog.vue'
import FollowerShopCaptureSection from '../components/board/FollowerShopCaptureSection.vue'
import { buildFollowerReturnTo } from '../composables/useFollowerReturnContext'

const BRANCH_KEY = 'follower-work-branch'
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const qc = useQueryClient()

const SECTIONS: Record<Exclude<FollowerBoardTab, 'shop-capture'>, Component> = {
  sales: defineAsyncComponent(() => import('./FollowerSalesReviewPage.vue')),
  declining: defineAsyncComponent(() => import('./FollowerDecliningPage.vue')),
  notes: defineAsyncComponent(() => import('./FollowerNotesPage.vue')),
  'old-customers': defineAsyncComponent(() => import('./FollowerOldCustomersPage.vue')),
  'delegate-notes': defineAsyncComponent(() => import('./FollowerDelegateNotesPage.vue')),
  collection: defineAsyncComponent(() => import('./FollowerCollectionPage.vue')),
  'live-map': defineAsyncComponent(() => import('./FollowerLiveMapPage.vue')),
  routes: defineAsyncComponent(() => import('./FollowerRoutesPage.vue')),
}

const TAB_DEFS: { key: FollowerBoardTab; label: string; module: PermissionModule; action: PermissionAction; countKey?: string; warn?: boolean; followerScoped: boolean }[] = [
  { key: 'sales', label: 'مصادقة البيع', module: 'FollowerSalesReview', action: 'Read', countKey: 'pendingSales', warn: true, followerScoped: true },
  { key: 'declining', label: 'المتدنية', module: 'FollowerDeclining', action: 'Read', countKey: 'declining', warn: true, followerScoped: true },
  { key: 'notes', label: 'الملاحظات', module: 'FollowerNotes', action: 'Read', countKey: 'notes', followerScoped: true },
  { key: 'old-customers', label: 'الزبائن القديمة', module: 'FollowerOldCustomers', action: 'Read', countKey: 'oldCustomers', followerScoped: true },
  { key: 'delegate-notes', label: 'ملاحظات المندوبين', module: 'FollowerDelegateNotes', action: 'Read', countKey: 'delegateNotes', followerScoped: true },
  { key: 'collection', label: 'الجباية', module: 'FollowerCollection', action: 'Read', countKey: 'collectionEnabledLists', followerScoped: true },
  { key: 'shop-capture', label: 'مواقع وصور المحلات', module: 'FollowerLocation', action: 'Read', followerScoped: false },
  { key: 'live-map', label: 'الموقع المباشر', module: 'FollowerLocation', action: 'Read', followerScoped: false },
  { key: 'routes', label: 'المسارات', module: 'FollowerRoute', action: 'Read', followerScoped: false },
]

const positive = (v: unknown) => {
  const n = Number(Array.isArray(v) ? v[0] : v)
  return Number.isInteger(n) && n > 0 ? n : null
}

const branches = computed(() => auth.branches ?? [])
const branchId = computed(() => {
  const id = positive(route.query.branchId)
  return id != null && branches.value.some(b => b.id === id) ? id : null
})
const currentBranch = computed(() => branches.value.find(b => b.id === branchId.value) ?? null)
const followerId = computed(() => positive(route.query.followerId))
const customerId = computed(() => positive(route.query.customerId))

function patch(next: Record<string, string | null | undefined>, replace = true) {
  const query: LocationQueryRaw = { ...route.query }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete query[k]
    else query[k] = v
  }
  const target = { name: 'FollowerWorkBoardRoute', query }
  void (replace ? router.replace(target) : router.push(target))
}

watch([branches, () => route.query.branchId], () => {
  if (branchId.value != null || branches.value.length === 0) return
  let stored: number | null = null
  try {
    stored = positive(localStorage.getItem(BRANCH_KEY))
  } catch {
    stored = null
  }
  const fallback = stored != null && branches.value.some(b => b.id === stored)
    ? stored
    : branches.value.length === 1 ? branches.value[0].id : null
  if (fallback != null) patch({ branchId: String(fallback) })
}, { immediate: true })

function setBranch(raw: string) {
  const id = positive(raw)
  if (id == null) return
  try {
    localStorage.setItem(BRANCH_KEY, String(id))
  } catch {
    void 0
  }
  patch({ branchId: String(id), followerId: null, customerId: null, bucket: null, page: null }, false)
}

const railSearch = ref('')
const followersQuery = useFollowersQuery({
  branchId,
  search: computed(() => ''),
  page: computed(() => 1),
  pageSize: computed(() => 100),
  sort: computed(() => 'name' as const),
  enabled: computed(() => branchId.value != null),
})
const allFollowers = computed(() => followersQuery.data.value?.items ?? [])
const railItems = computed(() => {
  const q = railSearch.value.trim()
  if (!q) return allFollowers.value
  return allFollowers.value.filter(f => f.followerName.includes(q) || String(f.followerId) === q)
})
const selected = computed(() => allFollowers.value.find(f => f.followerId === followerId.value) ?? null)

watch(allFollowers, (items) => {
  if (items.length === 0) return
  if (followerId.value == null || items.some(f => f.followerId === followerId.value) === false)
    patch({ followerId: String(items[0].followerId), customerId: null })
})

function selectFollower(id: number) {
  if (!Number.isInteger(id) || id === followerId.value) return
  patch({ followerId: String(id), customerId: null, bucket: null, page: null }, false)
}

const counts = useFollowerSectionCountsQuery(followerId, branchId)
const visibleTabs = computed(() => TAB_DEFS
  .filter(t => auth.hasPermission(t.module, t.action) === true)
  .map(t => ({ ...t, count: t.countKey ? (counts.data.value as Record<string, number | null> | undefined)?.[t.countKey] ?? null : null })))

const activeTab = computed<FollowerBoardTab | null>(() => {
  const raw = String(route.query.tab ?? route.query.section ?? '')
  const found = visibleTabs.value.find(t => t.key === raw)
  return found?.key ?? visibleTabs.value[0]?.key ?? null
})
const activeDef = computed(() => TAB_DEFS.find(t => t.key === activeTab.value) ?? null)
const activeLabel = computed(() => activeDef.value?.label ?? '')
const needsFollower = computed(() => activeDef.value?.followerScoped === true)
const activeComponent = computed(() => (activeTab.value && activeTab.value !== 'shop-capture' ? SECTIONS[activeTab.value] : null))

function setTab(tab: FollowerBoardTab) {
  if (tab === activeTab.value) return
  patch({ tab, section: null, bucket: null, page: null, customerId: null }, false)
}

function openCustomer(id: number | null) {
  patch({ customerId: id == null ? null : String(id) })
}
provide('fwOpenCustomer', openCustomer)

const canManage = computed(() => auth.hasPermission('FollowerWork', 'ManageFollowers') === true)
const canOpenProfile = computed(() => auth.hasPermission('Customer', 'Read') === true || auth.hasPermission('SalesRequest', 'ReadAuditProfile') === true)

function openFullProfile(id: number) {
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(id) },
    query: {
      returnTo: buildFollowerReturnTo({
        branchId: String(branchId.value ?? ''),
        followerId: String(followerId.value ?? ''),
        section: activeTab.value ?? 'sales',
      }),
    },
  })
}

const accountMode = ref<'create' | 'edit' | null>(null)
function openAccount(mode: 'create' | 'edit') {
  accountMode.value = mode
}
function onAccountSaved(id: number | null) {
  accountMode.value = null
  void qc.invalidateQueries({ queryKey: [FOLLOWER_WORK_KEY] })
  patch({ followerId: id == null ? null : String(id), customerId: null })
}

function refresh() {
  void qc.invalidateQueries({ queryKey: [FOLLOWER_WORK_KEY] })
}

function fixed1(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : n.toFixed(1)
}
function arrow(t: string | null | undefined) {
  return t === 'up' ? '▲' : t === 'down' ? '▼' : '■'
}
function trendText(t: string | null | undefined) {
  return t === 'up' ? 'صاعد' : t === 'down' ? 'هابط' : 'ثابت'
}
</script>
