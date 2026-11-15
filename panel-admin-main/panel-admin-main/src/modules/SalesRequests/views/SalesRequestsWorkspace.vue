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

    <div class="sr-layout">
      <section
        
        class="sr-col-list"
      >
        <RequestList
          :items="rows"
          :selected-id="selectedId"
          :loading="listLoading"
          :error="listError"
          :empty-hint-key="undefined"
        >
          <template #matching="{ item }">
            <div
              v-if="showMatching && !item.isDeleted"
              class="sr-match-block"
              @click.stop
            >
              <MatchingSummary
                :cards="cardsFor(item.id)"
                :selected-key="
                  expandedMatch?.requestId === item.id
                    ? expandedMatch.criterion
                    : null
                "
                :customer-name="item.customerName"
                :customer-phone="item.customerPhone"
                @select="onExpandMatch(item.id, $event as MatchCriterion)"
              />
              <div
                v-if="expandedMatch?.requestId === item.id"
                class="sr-match-dropdown"
                role="region"
                :aria-label="MATCH_LABELS[expandedMatch.criterion]"
              >
                <MatchingResults
                  :items="listHitItems"
                  :is-loading="listHitsLoading"
                  :is-error="listHitsError"
                  :insufficient="listHitsInsufficient"
                  @open-profile="openProfile"
                />
                <div
                  v-if="listHitTotal > matchHitsPageSize"
                  class="sr-match-dropdown__pager"
                >
                  <button
                    type="button"
                    class="sr-page-btn"
                    :disabled="listHitPage <= 1"
                    @click="listHitPage--"
                  >
                    السابق
                  </button>
                  <span>{{ listHitPage }}</span>
                  <button
                    type="button"
                    class="sr-page-btn"
                    :disabled="listHitPage * matchHitsPageSize >= listHitTotal"
                    @click="listHitPage++"
                  >
                    التالي
                  </button>
                </div>
              </div>
            </div>
          </template>
          <template #actions="{ item }">
            <div class="sr-card-toolbar">
              <RequestActions
                v-if="!item.isDeleted && tab !== 'deleted'"
                :request="item"
                :draft="getDraft(item.id)"
                @done="onActionDone"
                @draft-change="Object.assign(getDraft(item.id), $event)"
              />
              <RequestLifecycleActions
                :request="item"
                :active-tab="tab"
                @done="onActionDone"
              />
            </div>
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import RequestStatusTabs from '../components/RequestStatusTabs.vue'
import RequestActions from '../components/RequestActions.vue'
import RequestLifecycleActions from '../components/RequestLifecycleActions.vue'
import RequestList from '../components/RequestList.vue'
import RequestFilters from '../components/RequestFilters.vue'
import MatchingSummary from '../components/MatchingSummary.vue'
import MatchingResults from '../components/MatchingResults.vue'
import { useSalesRequestTabAccess } from '../composables/useSalesRequestTabAccess'
import { useSalesRequestFilters } from '../composables/useSalesRequestFilters'
import { useSalesRequestSelection } from '../composables/useSalesRequestSelection'
import { useSalesRequestsQuery } from '../requests/queries/useSalesRequestsQuery'
import { useSalesRequestCountsQuery } from '../requests/queries/useSalesRequestCountsQuery'
import { useMatchSummaryQuery } from '../requests/queries/useMatchSummaryQuery'
import { useMatchHitsQuery } from '../requests/queries/useMatchHitsQuery'
import {
  MATCH_LABELS,
  type MatchCard,
  type MatchCriterion,
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
const tabAccess = useSalesRequestTabAccess()

const {
  canOpenWorkspace,
  canReadTab,
  isLocked,
  resolveInitialTab,
  canMatchRead,
} = tabAccess

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
  getDraft,
  clearDraft,
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
    exceptionStatus: filters.value.exceptionStatus,
  }),
  set: (v) => {
    if (
      filters.value.branchId !== v.branchId ||
      filters.value.search !== v.search ||
      filters.value.exceptionStatus !== v.exceptionStatus
    ) {
      pageIndex.value = 1
    }
    filters.value = {
      ...filters.value,
      branchId: v.branchId,
      provinceId: v.provinceId,
      sourceType: v.sourceType,
      fromDate: v.fromDate,
      toDate: v.toDate,
      search: v.search,
      employeeId: (v as { employeeId?: number | null }).employeeId ?? null,
      exceptionStatus: v.exceptionStatus ?? null,
    }
  },
})

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
  onOwnershipLost: () => {
    void listQuery.refetch()
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

const searchSource = ref<SearchSource>('priorAccounts')
const selectedCriterion = ref<MatchCriterion>('tripleName')
/** توسيع نتائج المطابقة من القائمة */
const expandedMatch = ref<{
  requestId: number
  criterion: MatchCriterion
} | null>(null)
const listHitPage = ref(1)
const matchHitsPageSize = ref(20)

/** مثل SalesHaider: كروت المطابقة لكل الحالات عدا المحذوف */
const showMatching = computed(
  () => canMatchRead.value && tab.value !== 'deleted'
)

const matchCriteria = Object.keys(MATCH_LABELS) as MatchCriterion[]

const pageIds = computed(() =>
  rows.value.filter((r) => !r.isDeleted).map((r) => r.id)
)
const summaryEnabled = computed(
  () =>
    showMatching.value &&
    pageIds.value.length > 0 &&
    !listError.value
)
const summaryQuery = useMatchSummaryQuery(pageIds, summaryEnabled)

const listHitRequestId = computed(
  () => expandedMatch.value?.requestId ?? null
)
const listHitCriterion = computed(
  () => expandedMatch.value?.criterion ?? null
)
const listHitsEnabled = computed(
  () => showMatching.value && expandedMatch.value != null
)
const listHitsQuery = useMatchHitsQuery(
  listHitRequestId,
  listHitCriterion,
  listHitPage,
  matchHitsPageSize,
  listHitsEnabled
)
const listHitItems = computed(() => listHitsQuery.data.value?.items ?? [])
const listHitTotal = computed(
  () => listHitsQuery.data.value?.totalCount ?? 0
)
const listHitsLoading = computed(() => listHitsQuery.isLoading.value)
const listHitsError = computed(() => listHitsQuery.isError.value)
const listHitsInsufficient = computed(() => {
  if (!expandedMatch.value) return false
  const card = cardsFor(expandedMatch.value.requestId).find(
    (c) => c.key === expandedMatch.value!.criterion
  )
  return card?.state === 'insufficientData'
})

function onExpandMatch(requestId: number, criterion: MatchCriterion) {
  searchSource.value = 'priorAccounts'
  selectedCriterion.value = criterion
  if (
    expandedMatch.value?.requestId === requestId &&
    expandedMatch.value.criterion === criterion
  ) {
    expandedMatch.value = null
    return
  }
  expandedMatch.value = { requestId, criterion }
  listHitPage.value = 1
}

function cardsFor(requestId: number): MatchCard[] {
  const item = summaryQuery.data.value?.items?.find(
    (x) => x.requestId === requestId
  )
  const byKey = new Map(
    (item?.cards ?? []).map((c) => [c.key as MatchCriterion, c as MatchCard])
  )
  const fallbackState: MatchCard['state'] = summaryQuery.isLoading.value
    ? 'loading'
    : summaryQuery.isError.value || (summaryEnabled.value && !item)
      ? 'failed'
      : 'loading'

  return matchCriteria.map((key) => {
    const existing = byKey.get(key)
    if (existing) {
      return {
        key,
        label: MATCH_LABELS[key],
        state: existing.state,
        count: existing.count ?? null,
      }
    }
    return {
      key,
      label: MATCH_LABELS[key],
      state: fallbackState,
      count: null,
    }
  })
}

function onTabChange(next: SalesRequestTabKey) {
  if (props.fixedTab || !canReadTab(next)) return
  setTab(next)
}

function onActionDone() {
  if (selectedId.value) clearDraft(selectedId.value)
  void listQuery.refetch()
  if (!props.fixedTab) void countsQuery.refetch()
}

function openProfile(customerId: number) {
  const requestId = expandedMatch.value?.requestId ?? null
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
    requestId: requestId ? String(requestId) : null,
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

watch(tab, () => {
  expandedMatch.value = null
})

watch(pageIndex, () => {
  expandedMatch.value = null
})

function formatSync(ts: number) {
  return new Intl.DateTimeFormat('en-US', {
    timeStyle: 'short',
  }).format(new Date(ts))
}
</script>

<style scoped>
.sr-workspace {
  font-family: inherit;
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
  border: 0;
  padding: 0.4rem 0.9rem;
  min-height: 44px;
  background: #edf5f3;
  color: #00201e;
  font-family: inherit;
  font-weight: 700;
  box-shadow:
    4px 4px 10px rgba(0, 32, 30, 0.12),
    -4px -4px 10px rgba(255, 255, 255, 0.9);
}
.sr-source-tab.active {
  background: #006a64;
  color: #fff;
  box-shadow:
    inset 3px 3px 7px rgba(0, 0, 0, 0.18),
    inset -2px -2px 6px rgba(255, 255, 255, 0.15);
}
:deep(.sr-detail-dialog.p-dialog) {
  border: 0;
  border-radius: 20px;
  overflow: hidden;
  background: #e8f0ef;
  box-shadow:
    12px 12px 28px rgba(0, 32, 30, 0.18),
    -8px -8px 20px rgba(255, 255, 255, 0.5);
}
.sr-workspace { direction:rtl; color:#00201e; }
.sr-header { padding-bottom:16px; border-bottom:1px solid #dce7e5; margin-bottom:20px; }
.sr-title { font-size:30px; line-height:1.6; }
.sr-sync { font-size:14px; line-height:1.7; }
.sr-filters-panel { background:#fff; border:1px solid #dce7e5; border-radius:14px; padding:16px; margin-top:18px; }
.sr-results { font-size:15px; padding-block:12px; }
.sr-layout { align-items:start; gap:24px; }
@media(min-width:1024px) { .sr-layout { grid-template-columns:minmax(360px,1fr) minmax(0,1.2fr); } }

.sr-card-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.45rem;
  width: 100%;
}
.sr-match-block {
  display: grid;
  gap: 0.75rem;
  width: 100%;
}

.sr-match-dropdown {
  margin-top: 0.15rem;
  padding: 0.85rem;
  border-radius: 16px;
  background: #edf5f3;
  box-shadow:
    inset 4px 4px 10px rgba(0, 32, 30, 0.08),
    inset -4px -4px 10px rgba(255, 255, 255, 0.85);
  max-height: min(70vh, 640px);
  overflow: auto;
}

.sr-match-dropdown__pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  margin-top: 0.75rem;
}
</style>

<style scoped>
.sr-layout {display:block;} .sr-filters-panel {border:0;background:transparent;padding:0;} :deep(.sr-list) {display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:20px;} @media(max-width:1100px){:deep(.sr-list){grid-template-columns:1fr;}}
</style>

