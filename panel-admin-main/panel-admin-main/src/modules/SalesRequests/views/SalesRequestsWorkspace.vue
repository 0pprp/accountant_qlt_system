<template>
  <div class="sr-workspace">
    <header class="sr-header">
      <div>
        <h1 class="sr-title">{{ props.fixedTab === 'exceptions' ? 'طلبات الاستثناء' : $t('salesRequests.title') }}</h1>
        <p
          class="sr-sync"
          role="status"
          :title="realtimeTitle"
        >
          <span
            class="sr-conn"
            :class="connectionClass"
          />
          {{ connectionLabel }}
          <span v-if="displaySyncedAt" class="sr-sync-time">
            · {{ $t('salesRequests.lastSync') }}:
            {{ formatSync(displaySyncedAt) }}
          </span>
        </p>
      </div>

    </header>

    <RequestStatusTabs
      v-if="!props.fixedTab"
      :model-value="tab"
      :counts="countMap"
      :is-locked="isLocked"
      @update:model-value="onTabChange"
    />

    <div
      class="sr-filters-panel"
      :class="{ open: mobileFiltersOpen }"
    >
      <RequestFilters
        v-model="filtersForUi"
        :tab="tab"
        class="mb-3"
      />
    </div>

    <p class="sr-results">
      {{ $t('salesRequests.resultsCount', { count: totalCount }) }}
    </p>

    <div
      class="sr-layout"
      :class="{ 'has-selection': selectedId != null && isNarrow }"
    >
      <section
        
        class="sr-col-list"
      >
        <RequestList
          :items="rows"
          :selected-id="selectedId"
          :loading="listLoading"
          :error="listError"
          :empty-hint-key="
            undefined
          "
          @select="onSelect"
        >
          <template #matching="{ item }">
            <MatchingSummary v-if="tab === 'unassigned' && canMatchRead" :cards="cardsFor(item.id)" :selected-key="selectedId === item.id ? selectedCriterion : null" @select="selectedCriterion = $event as MatchCriterion; searchSource = 'priorAccounts'; onSelect(item)" />
          </template>
          <template #actions="{ item }">
            <RequestActions v-if="!item.isDeleted && tab !== 'deleted'" :request="item" :draft="getDraft(item.id)" @done="onActionDone" @draft-change="Object.assign(getDraft(item.id), $event)" />
          </template>
        </RequestList>

        <div class="sr-pager">
          <button
            type="button"
            class="sr-page-btn"
            :disabled="pageIndex <= 1"
            @click="pageIndex--"
          >
            السابق
          </button>
          <span>{{ pageIndex }}</span>
          <button
            type="button"
            class="sr-page-btn"
            :disabled="pageIndex * pageSize >= totalCount"
            @click="pageIndex++"
          >
            التالي
          </button>
        </div>
      </section>

      <Dialog :visible="selectedId != null" modal header="تفاصيل الطلب والمطابقة" :style="{width:'min(960px, 94vw)'}" :pt="{mask:{class:'bg-[#C6C6C680] backdrop-blur-[2px]'}}" class="bg-white rounded-3xl p-5" dir="rtl" @update:visible="!$event && (selectedId = null)">
        <RequestDetailsPanel
          :request="detailRequest"
          :timeline="timelineItems"
          :timeline-loading="timelineLoading"
          :timeline-error="timelineError"
          :can-timeline="canTimeline"
          :can-profile="canProfile"
          :can-order="canOrder"
          :show-back="isNarrow"
          :show-audit="tab === 'unassigned' && canMatchRead"
          :moved-message="outsideFilterMessage"
          :moved-tab="outsideFilterTargetTab"
          :active-tab="tab"
          :draft="selectedId ? getDraft(selectedId) : undefined"
          @back="selectedId = null"
          @open-tab="onOpenMovedTab"
          @action-done="onActionDone"
          @draft-change="onDraftChange"
        >
          <template
            v-if="tab === 'unassigned' && canMatchRead && detailRequest"
            #audit
          >
            <div class="space-y-3">
              <div class="flex flex-wrap gap-2">
                <button
                  type="button"
                  class="sr-source-tab"
                  :class="{ active: searchSource === 'priorAccounts' }"
                  @click="searchSource = 'priorAccounts'"
                >
                  {{ $t('salesRequests.priorAccounts') }}
                </button>
                <button
                  type="button"
                  class="sr-source-tab"
                  :class="{ active: searchSource === 'duplicates' }"
                  @click="searchSource = 'duplicates'"
                >
                  {{ $t('salesRequests.duplicateRequests') }}
                </button>
              </div>
              <MatchingSummary
                v-if="searchSource === 'priorAccounts'"
                :cards="cardsFor(detailRequest.id)"
                :selected-key="selectedCriterion"
                @select="selectedCriterion = $event as MatchCriterion"
              />
              <MatchingResults
                v-if="searchSource === 'priorAccounts'"
                :items="hitItems"
                :is-loading="hitsLoading"
                :is-error="hitsError"
                :insufficient="hitsInsufficient"
                @open-profile="openProfile"
              />
              <DuplicateRequestResults
                v-else
                :items="duplicateItems"
                :is-loading="dupLoading"
                :is-error="dupError"
              />
            </div>
          </template>
        </RequestDetailsPanel>
      </Dialog>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { usePermission } from '@/modules/Core/composable/usePermission'
import RequestStatusTabs from '../components/RequestStatusTabs.vue'
import { Dialog } from 'primevue'
import RequestActions from '../components/RequestActions.vue'
import RequestList from '../components/RequestList.vue'
import RequestFilters from '../components/RequestFilters.vue'
import RequestDetailsPanel from '../components/RequestDetailsPanel.vue'
import MatchingSummary from '../components/MatchingSummary.vue'
import MatchingResults from '../components/MatchingResults.vue'
import DuplicateRequestResults from '../components/DuplicateRequestResults.vue'
import { useSalesRequestTabAccess } from '../composables/useSalesRequestTabAccess'
import { useSalesRequestFilters } from '../composables/useSalesRequestFilters'
import { useSalesRequestSelection } from '../composables/useSalesRequestSelection'
import { useSalesRequestsQuery } from '../requests/queries/useSalesRequestsQuery'
import { useSalesRequestCountsQuery } from '../requests/queries/useSalesRequestCountsQuery'
import { useSalesRequestByIdQuery } from '../requests/queries/useSalesRequestByIdQuery'
import { useSalesRequestTimelineQuery } from '../requests/queries/useSalesRequestTimelineQuery'
import { useMatchSummaryQuery } from '../requests/queries/useMatchSummaryQuery'
import { useMatchHitsQuery } from '../requests/queries/useMatchHitsQuery'
import { useDuplicateRequestsQuery } from '../requests/queries/useDuplicateRequestsQuery'
import {
  MATCH_LABELS,
  type MatchCard,
  type MatchCriterion,
  type SalesRequestListItem,
  type SearchSource,
} from '../types'
import type { SalesRequestTabKey } from '../domain/tabs'
import {
  buildAuditReturnQuery,
} from '../composables/useAuditReturnContext'
import { useSalesRequestRealtime } from '../realtime/salesRequestRealtime'

const props = withDefaults(
  defineProps<{ defaultTab?: SalesRequestTabKey; fixedTab?: SalesRequestTabKey }>(),
  { defaultTab: undefined }
)

const router = useRouter()
const { can } = usePermission()
const tabAccess = useSalesRequestTabAccess()

const {
  canOpenWorkspace,
  canReadTab,
  isLocked,
  resolveInitialTab,
  canTimeline,
  canMatchRead,
} = tabAccess

const canProfile = can('SalesRequest', 'ReadAuditProfile') || can('Customer', 'Read')
const canOrder = can('Order', 'Read')

const {
  tab,
  pageIndex,
  pageSize,
  selectedId,
  filters,
  listFilters,
  mobileFiltersOpen,
  setTab,
} = useSalesRequestFilters((preferred) =>
  props.fixedTab ?? resolveInitialTab(props.defaultTab ?? preferred)
)

const {
  outsideFilterMessage,
  outsideFilterTargetTab,
  pinnedDetail,
  select,
  getDraft,
  clearDraft,
  onRemoteUpdate,
} = useSalesRequestSelection(selectedId, tab, canReadTab)

/** توافق RequestFilters مع employeeId */
const filtersForUi = computed({
  get: () => ({
    branchId: filters.value.branchId,
    provinceId: filters.value.provinceId,
    sourceType: filters.value.sourceType,
    fromDate: filters.value.fromDate,
    toDate: filters.value.toDate,
    search: filters.value.search,
    employeeId: filters.value.employeeId,
  }),
  set: (v) => {
    if (filters.value.branchId !== v.branchId || filters.value.search !== v.search) pageIndex.value = 1
    filters.value = {
      ...filters.value,
      branchId: v.branchId,
      provinceId: v.provinceId,
      sourceType: v.sourceType,
      fromDate: v.fromDate,
      toDate: v.toDate,
      search: v.search,
      employeeId: (v as { employeeId?: number | null }).employeeId ?? null,
    }
  },
})

const isNarrow = ref(false)
function updateNarrow() {
  isNarrow.value = window.matchMedia('(max-width: 767px)').matches
}
onMounted(() => {
  updateNarrow()
  window.addEventListener('resize', updateNarrow)
})
onUnmounted(() => window.removeEventListener('resize', updateNarrow))

const listEnabled = computed(
  () => canOpenWorkspace.value && canReadTab(tab.value)
)
const listQuery = useSalesRequestsQuery(
  pageIndex,
  pageSize,
  listFilters,
  listEnabled
)

const countFilters = computed(() => ({ ...filters.value }))
const countsEnabled = computed(() => !props.fixedTab && canOpenWorkspace.value)
const countsQuery = useSalesRequestCountsQuery(countFilters, countsEnabled)

const rows = computed(() => listQuery.data.value?.items ?? [])
const totalCount = computed(() => listQuery.data.value?.totalCount ?? 0)
const listLoading = computed(() => listQuery.isLoading.value)
const listError = computed(() => listQuery.isError.value)

const countMap = computed(() => {
  const map: Record<string, number> = {}
  for (const t of countsQuery.data.value?.tabs ?? []) {
    map[t.tab] = t.count
  }
  for (const t of countsQuery.data.value?.extraViews ?? []) {
    map[t.tab] = t.count
  }
  return map
})

const realtimeEnabled = computed(() => canOpenWorkspace.value)
const {
  connectionState: rtState,
  statusLabel: rtStatusLabel,
  lastSyncedAt: rtLastSynced,
  lastFailure: rtLastFailure,
  hubOrigin: rtHubOrigin,
} = useSalesRequestRealtime({
  enabled: realtimeEnabled,
  openRequestId: selectedId,
  onOwnershipLost: (requestId) => {
    // أعد تقييم التفاصيل دون مسح مسودة المستخدم بصمت
    if (selectedId.value === requestId) {
      void detailQuery.refetch()
      outsideFilterMessage.value =
        outsideFilterMessage.value || 'تغيّر ملكية/حالة الطلب — راجع التفاصيل'
    }
  },
})

const realtimeTitle = computed(() => {
  const parts = [
    rtHubOrigin.value ? `hub: ${rtHubOrigin.value}` : null,
    rtLastFailure.value ? `lastError: ${rtLastFailure.value}` : null,
  ].filter(Boolean)
  return parts.join(' · ')
})

const displaySyncedAt = computed(
  () =>
    rtLastSynced.value ||
    listQuery.dataUpdatedAt.value ||
    countsQuery.dataUpdatedAt.value ||
    null
)
const connectionClass = computed(() => {
  if (rtState.value === 'connected') return 'ok'
  if (rtState.value === 'reconnecting' || listQuery.isFetching.value) return 'syncing'
  if (listError.value || rtState.value === 'disconnected') return 'err'
  return 'ok'
})
const connectionLabel = computed(() => {
  if (listQuery.isFetching.value && rtState.value === 'connected') return 'جارٍ المزامنة'
  return rtStatusLabel.value
})

const detailEnabled = computed(() => selectedId.value != null && canOpenWorkspace.value)
const detailQuery = useSalesRequestByIdQuery(selectedId, detailEnabled)
const timelineEnabled = computed(
  () => selectedId.value != null && canTimeline.value
)
const timelineQuery = useSalesRequestTimelineQuery(selectedId, timelineEnabled)

const detailRequest = computed<SalesRequestListItem | null>(() => {
  return detailQuery.data.value ?? pinnedDetail.value
})

watch(
  () => detailQuery.data.value,
  (v) => {
    if (v) onRemoteUpdate(v)
  }
)

const timelineItems = computed(() => timelineQuery.data.value ?? [])
const timelineLoading = computed(() => timelineQuery.isLoading.value)
const timelineError = computed(() => timelineQuery.isError.value)

const searchSource = ref<SearchSource>('priorAccounts')
const selectedCriterion = ref<MatchCriterion>('tripleName')
const hitPage = ref(1)

const pageIds = computed(() => rows.value.map((r) => r.id))
const summaryEnabled = computed(
  () =>
    canMatchRead.value &&
    tab.value === 'unassigned' &&
    pageIds.value.length > 0 &&
    !listError.value
)
const summaryQuery = useMatchSummaryQuery(pageIds, summaryEnabled)

const hitsEnabled = computed(
  () =>
    canMatchRead.value &&
    tab.value === 'unassigned' &&
    searchSource.value === 'priorAccounts' &&
    selectedId.value != null &&
    selectedCriterion.value != null
)
const hitsQuery = useMatchHitsQuery(
  selectedId,
  selectedCriterion,
  hitPage,
  pageSize,
  hitsEnabled
)
const dupEnabled = computed(
  () =>
    canMatchRead.value &&
    tab.value === 'unassigned' &&
    searchSource.value === 'duplicates' &&
    selectedId.value != null
)
const dupQuery = useDuplicateRequestsQuery(
  selectedId,
  hitPage,
  pageSize,
  dupEnabled
)

const hitItems = computed(() => hitsQuery.data.value?.items ?? [])
const hitsLoading = computed(() => hitsQuery.isLoading.value)
const hitsError = computed(() => hitsQuery.isError.value)
const hitsInsufficient = computed(() => {
  const cards = cardsFor(selectedId.value ?? 0)
  const card = cards.find((c) => c.key === selectedCriterion.value)
  return card?.state === 'insufficientData'
})
const duplicateItems = computed(() => dupQuery.data.value?.items ?? [])
const dupLoading = computed(() => dupQuery.isLoading.value)
const dupError = computed(() => dupQuery.isError.value)

function cardsFor(requestId: number): MatchCard[] {
  const item = summaryQuery.data.value?.items?.find(
    (x) => x.requestId === requestId
  )
  if (item?.cards?.length) return item.cards as MatchCard[]
  const state = summaryQuery.isLoading.value
    ? 'loading'
    : 'failed'
  return (Object.keys(MATCH_LABELS) as MatchCriterion[]).map((key) => ({
    key,
    label: MATCH_LABELS[key],
    state: state as MatchCard['state'],
    count: null,
  }))
}

function onTabChange(next: SalesRequestTabKey) {
  if (props.fixedTab || !canReadTab(next)) return
  setTab(next)
}

async function onSelect(item: SalesRequestListItem) {
  select(item)
  await nextTick()
  if (isNarrow.value) document.querySelector('.sr-details')?.scrollIntoView({ block: 'start' })
}

function onOpenMovedTab(key: string) {
  if (props.fixedTab || !canReadTab(key)) return
  setTab(key as SalesRequestTabKey)
}

function onActionDone() {
  if (selectedId.value) clearDraft(selectedId.value)
  void listQuery.refetch()
  if (!props.fixedTab) void countsQuery.refetch()
  void detailQuery.refetch()
}

function onDraftChange(d: {
  reject?: string
  exception?: string
  assignReason?: string
}) {
  if (!selectedId.value) return
  const draft = getDraft(selectedId.value)
  Object.assign(draft, d)
}

function openProfile(customerId: number) {
  const query = buildAuditReturnQuery({
    branchId: filters.value.branchId
      ? String(filters.value.branchId)
      : null,
    provinceId: filters.value.provinceId
      ? String(filters.value.provinceId)
      : null,
    sourceType: filters.value.sourceType,
    fromDate: filters.value.fromDate,
    toDate: filters.value.toDate,
    search: filters.value.search,
    page: String(pageIndex.value),
    requestId: selectedId.value ? String(selectedId.value) : null,
    criterion: selectedCriterion.value,
    searchSource: searchSource.value,
    scrollY: String(window.scrollY),
  })
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: customerId },
    query: {
      ...query,
      returnTo: 'SalesRequestsWorkspaceRoute',
      tab: tab.value,
    },
  })
}

function formatSync(ts: number) {
  return new Intl.DateTimeFormat('ar-IQ', {
    timeStyle: 'short',
  }).format(new Date(ts))
}

watch(tab, () => {
  // عند تغيير التبويب لا تمسح المسودة إن بقي الطلب مفتوحًا
})
</script>

<style scoped>
.sr-workspace {
  font-family: 'ElMessiri', 'El Messiri', sans-serif;
  color: #00201e;
  background: linear-gradient(180deg, #f3f7f6 0%, #f8f8f8 40%);
  min-height: 100%;
  padding: 1rem 1rem 5rem;
  padding-bottom: max(5rem, env(safe-area-inset-bottom));
  max-width: 100%;
  overflow-x: hidden;
}
.sr-header {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.85rem;
}
.sr-title {
  margin: 0;
  font-size: clamp(1.4rem, 2.5vw, 1.85rem);
  font-weight: 600;
}
.sr-sync {
  margin: 0.35rem 0 0;
  font-size: 0.8rem;
  opacity: 0.8;
  display: flex;
  align-items: center;
  gap: 0.35rem;
}
.sr-conn {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 50%;
  background: #145a32;
}
.sr-conn.syncing {
  background: #9a6700;
}
.sr-conn.err {
  background: #9b1c1c;
}
.sr-filter-toggle {
  min-height: 44px;
  border-radius: 0.85rem;
  border: 1px solid #d7e2e0;
  background: #fff;
  padding: 0.4rem 0.85rem;
  font-family: inherit;
}
.sr-filters-panel {
  margin: 0.75rem 0;
}
@media (max-width: 767px) {
  .sr-filters-panel:not(.open) {
    display: block;
  }
  .md-hidden {
    display: inline-flex;
  }
}
@media (min-width: 768px) {
  .md-hidden {
    display: none;
  }
}
.sr-results {
  font-size: 0.85rem;
  opacity: 0.75;
  margin: 0.5rem 0 0.75rem;
}
.sr-layout {
  display: grid;
  gap: 1rem;
}
@media (min-width: 1024px) {
  .sr-layout {
    grid-template-columns: minmax(0, 1.1fr) minmax(0, 0.9fr);
    align-items: start;
  }
}
.sr-pager {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 0.75rem;
  margin-top: 1rem;
}
.sr-page-btn {
  min-height: 44px;
  min-width: 44px;
  border-radius: 0.75rem;
  border: 1px solid #d7e2e0;
  background: #fff;
  padding: 0.35rem 0.75rem;
  font-family: inherit;
}
.sr-source-tab {
  border-radius: 999px;
  border: 1px solid #d7e2e0;
  padding: 0.4rem 0.9rem;
  min-height: 44px;
  background: #fff;
  color: #00201e;
  font-family: inherit;
}
.sr-source-tab.active {
  background: #006a64;
  border-color: #006a64;
  color: #fff;
}
.sr-workspace { direction:rtl; color:#00201e; }
.sr-header { padding-bottom:16px; border-bottom:1px solid #dce7e5; margin-bottom:20px; }
.sr-title { font-size:30px; line-height:1.6; }
.sr-sync { font-size:14px; line-height:1.7; }
.sr-filters-panel { background:#fff; border:1px solid #dce7e5; border-radius:14px; padding:16px; margin-top:18px; }
.sr-results { font-size:15px; padding-block:12px; }
.sr-layout { align-items:start; gap:24px; }
@media(min-width:1024px) { .sr-layout { grid-template-columns:minmax(360px,1fr) minmax(0,1.2fr); } }
</style>

<style scoped>
.sr-layout {display:block;} .sr-filters-panel {border:0;background:transparent;padding:0;} :deep(.sr-list) {display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:20px;} @media(max-width:1100px){:deep(.sr-list){grid-template-columns:1fr;}}
</style>

