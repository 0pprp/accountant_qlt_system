<template>
  <div class="dfb" dir="rtl">
    <header class="dfb-head">
      <div>
        <h1 class="dfb-title">متابعة المندوبين</h1>
        <p class="dfb-subtitle">اضغط على الكونتينر لعرض تفاصيل آخر 30 يوماً — واضغط فئة غير المصفرين لإظهار/إخفاء الجدول</p>
      </div>
      <div class="dfb-actions">
        <button v-if="branchId != null && branches.length > 1" type="button" class="dfb-button" @click="changeBranch">تغيير الفرع</button>
        <button v-if="listId != null" type="button" class="dfb-button" @click="backToLists">رجوع للقوائم</button>
      </div>
    </header>

    <section v-if="branchId == null" class="dfb-section" aria-labelledby="dfb-branch-title">
      <h2 id="dfb-branch-title" class="dfb-section-title">اختر الفرع</h2>
      <p v-if="!branches.length" class="dfb-state">لا توجد فروع مصرّح بها لحسابك</p>
      <div v-else class="dfb-grid">
        <button v-for="b in branches" :key="b.id" type="button" class="dfb-tile" @click="openBranch(b.id)">
          <span class="dfb-tile-label">الفرع</span>
          <strong class="dfb-tile-value">{{ b.name }}</strong>
        </button>
      </div>
    </section>

    <template v-else-if="listId == null">
      <p v-if="currentBranch" class="dfb-hint">الفرع: {{ currentBranch.name }}</p>
      <p v-if="listsQuery.isPending.value" class="dfb-state" role="status">جاري تحميل القوائم…</p>
      <div v-else-if="listsQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل القوائم
        <button type="button" class="dfb-button is-small" @click="listsQuery.refetch()">إعادة المحاولة</button>
      </div>
      <p v-else-if="!lists.length" class="dfb-state">لا توجد قوائم نشطة في هذا الفرع</p>
      <template v-else>
        <div class="dfb-grid">
          <article
            v-for="l in lists"
            :key="l.listId"
            class="dfb-tile dfb-list-card is-click"
            :class="{ 'is-active': mapListIds.includes(l.listId) }"
            :style="mapListIds.includes(l.listId) ? { borderColor: mapColors[l.listId] } : undefined"
            role="link"
            tabindex="0"
            :aria-label="`فتح قائمة ${l.listName}`"
            @click="openList(l.listId)"
            @keydown.enter.self="openList(l.listId)"
          >
            <label class="dfb-map-toggle" @click.stop>
              <input type="checkbox" :checked="mapListIds.includes(l.listId)" @change="toggleMapList(l.listId)">
              الخريطة
            </label>
            <strong class="dfb-list-title">{{ l.listName }}</strong>
            <span class="dfb-tile-value" :class="`tier-${l.realRatingLabel}`"><span class="dfb-num">{{ fixed1(l.realRating) }}</span></span>
            <span class="dfb-tile-note">{{ l.realRatingLabel || 'التقييم الحقيقي' }}</span>
            <span class="dfb-tile-row">غير المصفرين: <strong>{{ l.nonZeroedCustomerCount }}</strong></span>
            <span class="dfb-tile-row">آخر تسديد: <span class="dfb-num">{{ l.lastPaymentDate ?? '—' }}</span></span>
          </article>
        </div>
        <div class="dfb-selection-bar">
          <span>حدّد المربعات لعرض زبائن القوائم على الخريطة — كل قائمة بلون</span>
          <button v-if="mapListIds.length < lists.length" type="button" class="dfb-button is-small" @click="selectAllMap">تحديد الكل</button>
          <button v-if="mapListIds.length" type="button" class="dfb-button is-small" @click="clearMap">إلغاء التحديد ({{ mapListIds.length }})</button>
        </div>
        <DelegateListsMap :branch-id="branchId" :lists="mapLists" :colors="mapColors" />
      </template>
    </template>

    <template v-else>
      <div class="dfb-head">
        <div>
          <h2 class="dfb-title">{{ summary?.listName ?? selectedListName }}</h2>
          <p class="dfb-subtitle">لوحة المؤشرات — اضغط الفئة لإظهار الجدول أو استخدم عرض التفاصيل</p>
        </div>
        <button type="button" class="dfb-button is-primary" :disabled="!summary" @click="openTable('all')">عرض التفاصيل</button>
      </div>

      <p v-if="summaryQuery.isPending.value" class="dfb-state" role="status">جاري تحميل تفاصيل القائمة…</p>
      <div v-else-if="summaryQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل تفاصيل القائمة
        <button type="button" class="dfb-button is-small" @click="summaryQuery.refetch()">إعادة المحاولة</button>
      </div>

      <template v-else-if="summary">
        <section class="dfb-section" aria-labelledby="dfb-ratings-title">
          <h2 id="dfb-ratings-title" class="dfb-section-title">التقييمات</h2>
          <div class="dfb-grid">
            <button type="button" class="dfb-tile" @click="openRealHistory">
              <span class="dfb-tile-label">التقييم الحقيقي</span>
              <strong class="dfb-tile-value" :class="`tier-${summary.realRatingLabel}`"><span class="dfb-num">{{ fixed1(summary.realRating) }}</span></strong>
              <span class="dfb-tile-note">{{ summary.realRatingLabel }}</span>
              <span class="dfb-tile-note" :class="`dfb-trend-${trendKey}`">قبل أسبوع: <span class="dfb-num">{{ fixed1(summary.realRatingWeekAgo) }}</span> ({{ trendText }})</span>
            </button>

            <div
              v-for="slot in boardSlots"
              :key="slot.key"
              class="dfb-tile is-click"
              role="button"
              tabindex="0"
              @click="openSlotHistory(slot.key)"
              @keydown.enter.self="openSlotHistory(slot.key)"
            >
              <span class="dfb-tile-label">{{ slot.label }}</span>
              <template v-if="slot.card.hasRating">
                <strong class="dfb-tile-value"><span class="dfb-num">{{ slot.card.score }} / 10</span></strong>
                <span v-if="slot.card.note" class="dfb-tile-note">{{ slot.card.note }}</span>
                <span v-if="slot.card.ratedByUserName" class="dfb-tile-note">بواسطة: {{ slot.card.ratedByUserName }}</span>
                <span class="dfb-tile-note"><span class="dfb-num">{{ slot.card.ratingDate }}</span></span>
              </template>
              <span v-else class="dfb-empty-slot">لا يوجد تقييم ليوم <span class="dfb-num">{{ summary.reportedYesterday }}</span></span>
              <button v-if="slot.canRate" type="button" class="dfb-button is-small is-primary" @click.stop="openRate(slot.key)">
                قيّم ليوم <span class="dfb-num">{{ summary.asOfDate }}</span>
              </button>
            </div>

            <button v-for="c in inputSlots" :key="c.key" type="button" class="dfb-tile" @click="openCommitteeHistory(c.label, c.data)">
              <span class="dfb-tile-label">{{ c.label }}</span>
              <template v-if="c.data.latest?.hasRating">
                <strong class="dfb-tile-value"><span class="dfb-num">{{ c.data.latest.score }} / 10</span></strong>
                <span v-if="c.data.latest.ratedByUserName" class="dfb-tile-note">{{ c.data.latest.ratedByUserName }}</span>
                <span v-if="c.data.latest.note" class="dfb-tile-note">{{ c.data.latest.note }}</span>
                <span class="dfb-tile-note"><span class="dfb-num">{{ c.data.latest.ratingDate }}</span></span>
              </template>
              <template v-else>
                <span class="dfb-empty-slot">لا يوجد تقييم بعد</span>
                <span class="dfb-tile-note">{{ c.emptyHint }}</span>
              </template>
            </button>
          </div>
        </section>

        <section class="dfb-section" aria-labelledby="dfb-income-title">
          <h2 id="dfb-income-title" class="dfb-section-title">الواردات</h2>
          <div class="dfb-grid">
            <div class="dfb-tile">
              <span class="dfb-tile-label">القسط المطلوب اليوم</span>
              <strong class="dfb-tile-value">{{ money(summary.requiredDueToday ?? 0) }}</strong>
              <span class="dfb-tile-note">مجموع قسط غير المصفرين عدا القانونية</span>
            </div>
            <button type="button" class="dfb-tile tone-warn" @click="unpaidOpen = true">
              <span class="dfb-tile-label">مبيعات جديدة بدون جباية (3 أيام+)</span>
              <strong class="dfb-tile-value">{{ summary.newSaleUnpaid3DaysCount }}</strong>
              <span class="dfb-tile-note">زبائن مسجّلون كمبيع وما عليهم أي تسديد</span>
            </button>
            <button type="button" class="dfb-tile" @click="openDaysHistory">
              <span class="dfb-tile-label">واردات القائمة ليوم أمس</span>
              <strong class="dfb-tile-value">{{ money(summary.collectedYesterday) }}</strong>
              <span class="dfb-tile-note"><span class="dfb-num">{{ summary.reportedYesterday }}</span></span>
            </button>
            <button type="button" class="dfb-tile" @click="openFridaysHistory">
              <span class="dfb-tile-label">واردات يوم آخر جمعة</span>
              <strong class="dfb-tile-value">{{ money(summary.collectedLastFriday) }}</strong>
              <span class="dfb-tile-note"><span class="dfb-num">{{ summary.lastFridayDate }}</span></span>
            </button>
            <button v-for="tm in summary.tierMoney" :key="tm.label" type="button" class="dfb-tile" @click="openTierHistory(tm)">
              <span class="dfb-tile-label">الواردات من {{ TIER_PLURAL[tm.label] ?? tm.label }}</span>
              <strong class="dfb-tile-value" :class="`tier-${tm.label}`">{{ money(tm.collectedYesterday) }}</strong>
              <span class="dfb-tile-note">تحصيل أمس</span>
              <span class="dfb-tile-row">سدد أمس: <strong>{{ tm.payersYesterday }} من {{ tierCustomerCount(tm) }}</strong></span>
              <span class="dfb-tile-note">القسط المطلوب: {{ money(tm.requiredDue ?? 0) }}</span>
            </button>
          </div>
        </section>

        <section class="dfb-section" aria-labelledby="dfb-unzeroed-title">
          <h2 id="dfb-unzeroed-title" class="dfb-section-title">غير المصفرين (<span class="dfb-num">{{ summary.nonZeroedCustomerCount }}</span>)</h2>
          <div class="dfb-grid">
            <button
              v-for="t in tierCards"
              :key="t.key"
              type="button"
              class="dfb-tile"
              :class="{ 'is-active': tableOpen && tableFilter === t.key }"
              :aria-pressed="tableOpen && tableFilter === t.key"
              @click="toggleTable(t.key)"
            >
              <span class="dfb-tile-label">{{ t.label }}</span>
              <strong class="dfb-tile-value" :class="t.tierClass">{{ t.count }}</strong>
              <span v-if="t.required != null" class="dfb-tile-note">القسط المطلوب: {{ money(t.required) }}</span>
              <span v-if="t.collected != null" class="dfb-tile-note">أمس وارد: {{ money(t.collected) }}</span>
            </button>
          </div>
        </section>

        <section class="dfb-section" aria-labelledby="dfb-conv-title">
          <h2 id="dfb-conv-title" class="dfb-section-title">التحويل</h2>
          <p class="dfb-hint">{{ summary.conversion.summaryText }} — اضغط الوجهة لمعرفة من أين جاؤوا (آخر 30 يوماً)</p>
          <div class="dfb-grid">
            <button v-for="tg in conversionTargets" :key="tg.targetLabel" type="button" class="dfb-tile tone-primary" @click="openConversionHistory(tg)">
              <span class="dfb-tile-label">إلى {{ tg.targetLabel }}</span>
              <strong class="dfb-tile-value" :class="`tier-${tg.targetLabel}`">{{ tg.arrivedCount }} متحول</strong>
              <span class="dfb-tile-note">{{ tg.detailText }}</span>
            </button>
          </div>
        </section>

        <DelegateSalesOrdersSection
          :branch-id="branchId"
          :list-id="listId"
          :list-name="summary.listName"
          :can-open-profile="canOpenProfile"
          :today="summary.asOfDate"
        />
      </template>
    </template>

    <DfbDialog
      :visible="tableOpen"
      size="xl"
      :title="`زبائن ${summary?.listName ?? ''}`"
      :subtitle="`غير المصفرين · ${tableFilterLabel} · محسوب حتى ${summary?.asOfDate ?? ''}`"
      @close="closeTable"
    >
      <template #summary>
        <div class="dfb-kpi"><span>الزبائن المعروضون</span><strong><span class="dfb-num">{{ filteredCustomers.length }}</span></strong></div>
        <div class="dfb-kpi"><span>مجموع الباقي</span><strong><span class="dfb-num">{{ money(tableTotals.remaining) }}</span></strong></div>
        <div class="dfb-kpi"><span>مجموع القسط اليومي</span><strong><span class="dfb-num">{{ money(tableTotals.daily) }}</span></strong></div>
        <div class="dfb-kpi"><span>سددوا خلال 30 يوماً</span><strong><span class="dfb-num">{{ tableTotals.active }}</span></strong></div>
      </template>
      <template #toolbar>
        <div class="dfb-filter-chips" role="group" aria-label="تصفية الزبائن">
          <button
            v-for="o in filterOptions"
            :key="o.value"
            type="button"
            class="dfb-chip"
            :aria-pressed="tableFilter === o.value"
            @click="tableFilter = o.value as TableFilter"
          >
            {{ o.label }} <strong><span class="dfb-num">{{ filterCount(o.value as TableFilter) }}</span></strong>
          </button>
        </div>
        <div class="dfb-actions">
          <input v-model="search" class="dfb-search" type="search" placeholder="بحث بالاسم أو الهاتف أو المحل" aria-label="بحث في الزبائن">
          <button type="button" class="dfb-button is-small" @click="openFullCustomersPage">صفحة التفاصيل الكاملة</button>
        </div>
      </template>
      <p v-if="customersQuery.isPending.value" class="dfb-state" role="status">جاري تحميل الزبائن…</p>
      <div v-else-if="customersQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل الزبائن
        <button type="button" class="dfb-button is-small" @click="customersQuery.refetch()">إعادة المحاولة</button>
      </div>
      <p v-else-if="!filteredCustomers.length" class="dfb-state">لا يوجد زبائن مطابقون للتصفية</p>
      <template v-else>
        <DelegateCustomersTable :customers="visibleCustomers" :can-open-profile="canOpenProfile" @profile="openProfile" />
        <div v-if="visibleCustomers.length < filteredCustomers.length" class="dfb-actions" style="justify-content: center; margin-top: 16px">
          <button type="button" class="dfb-button" @click="visibleLimit += PAGE_SIZE">
            عرض المزيد (<span class="dfb-num">{{ filteredCustomers.length - visibleCustomers.length }}</span> متبقٍ)
          </button>
        </div>
      </template>
    </DfbDialog>

    <DfbDialog
      :visible="unpaidOpen"
      size="lg"
      title="مبيعات جديدة بدون جباية (3 أيام+)"
      :subtitle="`${summary?.listName ?? ''} · زبائن مسجّلون كمبيع وما عليهم أي تسديد`"
      @close="unpaidOpen = false"
    >
      <p v-if="customersQuery.isPending.value" class="dfb-state" role="status">جاري تحميل الزبائن…</p>
      <p v-else-if="!unpaidCustomers.length" class="dfb-state">لا يوجد زبائن بهذا الوصف حالياً</p>
      <DelegateCustomersTable v-else :customers="unpaidCustomers" :can-open-profile="canOpenProfile" @profile="openProfile" />
    </DfbDialog>

    <BoardRatingDialog
      :visible="rateSlot !== null"
      :busy="upsertMutation.isPending.value"
      :title="rateSlot === 'accountant' ? `تقييم المحاسب الرئيسي ليوم ${summary?.asOfDate ?? ''}` : `تقييم لجنة المراقبة ليوم ${summary?.asOfDate ?? ''}`"
      :list-name="summary?.listName"
      :initial-score="rateInitial?.hasRating ? rateInitial.score : 8"
      :initial-note="rateInitial?.hasRating ? rateInitial.note : null"
      @close="rateSlot = null"
      @submit="submitRating"
    />

    <DelegateHistoryDialog
      :visible="history != null"
      :title="history?.title ?? ''"
      :subtitle="history?.subtitle"
      :hint="history?.hint"
      :items="history?.items ?? []"
      :summary="history?.summary"
      :row-clickable="history?.onRow != null"
      @row="history?.onRow?.($event)"
      @close="history = null"
    />

    <DelegateHistoryDialog
      :visible="names != null"
      :title="names?.title ?? ''"
      :subtitle="names?.subtitle"
      :items="names?.items ?? []"
      @close="names = null"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import BoardRatingDialog from '../components/BoardRatingDialog.vue'
import DelegateHistoryDialog, { type HistoryItem, type HistorySummary } from '@/modules/Core/components/neumorphic/NeuHistoryDialog.vue'
import DfbDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import DelegateCustomersTable from '../components/DelegateCustomersTable.vue'
import DelegateListsMap from '../components/DelegateListsMap.vue'
import DelegateSalesOrdersSection from '../components/DelegateSalesOrdersSection.vue'
import '../components/delegate-followup.css'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import { useListsQuery } from '../requests/queries/useListsQuery'
import { useSummaryQuery } from '../requests/queries/useSummaryQuery'
import { useCustomersQuery } from '../requests/queries/useCustomersQuery'
import { useUpsertBoardRatingMutation } from '../requests/mutations/useUpsertBoardRatingMutation'
import {
  BRANCH_STORAGE_KEY,
  type DelegateFollowupCommitteeRating,
  type DelegateFollowupConversionTarget,
  type DelegateFollowupCustomer,
  type DelegateFollowupDayRating,
  type DelegateFollowupTierMoney,
  type RatingSlot,
} from '../domain/types'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { useToastStore } from '@/modules/Core/store'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'

const PAGE_SIZE = 100
const BAND_100K = 100_000
const BAND_200K = 200_000
const TIERS = ['ممتاز', 'جيد', 'ضعيف', 'متلكئ', 'سيء'] as const
const TIER_PLURAL: Record<string, string> = {
  'ممتاز': 'الممتازين',
  'جيد': 'الجيدين',
  'ضعيف': 'الضعفاء',
  'متلكئ': 'المتلكئين',
  'سيء': 'السيئين',
}
const MAP_PALETTE = ['#006a64', '#2f5aa6', '#914d9f', '#aa581d', '#4c7330', '#ad3e55', '#436478', '#8a6d00', '#0f7d8c', '#6b4fb8', '#b5452b', '#3f7f5f']

type TableFilter = 'all' | 'lt100' | '100to200' | (typeof TIERS)[number]

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const toast = useToastStore()
const { can, canAny } = usePermission()

const branches = computed(() => auth.branches ?? [])
const positive = (v: unknown) => {
  const n = Number(Array.isArray(v) ? v[0] : v)
  return Number.isInteger(n) && n > 0 ? n : null
}
const branchId = computed(() => {
  const id = positive(route.query.branchId)
  return id != null && branches.value.some(b => b.id === id) ? id : null
})
const listId = computed(() => (branchId.value == null ? null : positive(route.query.listId)))
const currentBranch = computed(() => branches.value.find(b => b.id === branchId.value) ?? null)

function readStoredBranch(): number | null {
  try {
    return positive(localStorage.getItem(BRANCH_STORAGE_KEY))
  } catch {
    return null
  }
}

function go(query: LocationQueryRaw) {
  void router.push({ name: 'ManagementDirectorDelegateFollowupRoute', query })
}

function openBranch(id: number) {
  try {
    localStorage.setItem(BRANCH_STORAGE_KEY, String(id))
  } catch {
    void 0
  }
  go({ branchId: String(id) })
}

function changeBranch() {
  try {
    localStorage.removeItem(BRANCH_STORAGE_KEY)
  } catch {
    void 0
  }
  go({})
}

function openList(id: number) {
  go({ branchId: String(branchId.value), listId: String(id) })
}

function backToLists() {
  closeTable()
  go({ branchId: String(branchId.value) })
}

watch(
  [branches, () => route.query.branchId],
  () => {
    if (route.name !== 'ManagementDirectorDelegateFollowupRoute' || branchId.value != null) return
    if (positive(route.query.branchId) != null && branches.value.length === 0) return
    const stored = readStoredBranch()
    const fallback = stored != null && branches.value.some(b => b.id === stored)
      ? stored
      : branches.value.length === 1 ? branches.value[0].id : null
    if (fallback != null) void router.replace({ query: { ...route.query, branchId: String(fallback) } })
  },
  { immediate: true }
)

const listsQuery = useListsQuery(branchId)
const lists = computed(() => listsQuery.data.value?.lists ?? [])
const selectedListName = computed(() => lists.value.find(l => l.listId === listId.value)?.listName ?? '')
const summaryQuery = useSummaryQuery(listId, branchId)
const customersQuery = useCustomersQuery(listId, branchId)
const summary = computed(() => summaryQuery.data.value ?? null)

const mapListIds = ref<number[]>([])
const mapColors = ref<Record<number, string>>({})
const mapLists = computed(() => mapListIds.value
  .map(id => lists.value.find(l => l.listId === id))
  .filter((l): l is NonNullable<typeof l> => l != null))

function toggleMapList(id: number) {
  if (mapListIds.value.includes(id)) {
    mapListIds.value = mapListIds.value.filter(x => x !== id)
    const next = { ...mapColors.value }
    delete next[id]
    mapColors.value = next
    return
  }
  const used = new Set(Object.values(mapColors.value))
  const color = MAP_PALETTE.find(c => !used.has(c)) ?? MAP_PALETTE[mapListIds.value.length % MAP_PALETTE.length]
  mapColors.value = { ...mapColors.value, [id]: color }
  mapListIds.value = [...mapListIds.value, id]
}

function selectAllMap() {
  lists.value.filter(l => !mapListIds.value.includes(l.listId)).forEach(l => toggleMapList(l.listId))
}

function clearMap() {
  mapListIds.value = []
  mapColors.value = {}
}

watch(branchId, clearMap)

const canRateBoard = computed(() => can('DelegateFollowup', 'UpsertBoardRating'))
const canRateAccountant = computed(() => can('DelegateFollowup', 'UpsertAccountantRating'))
const accountantOnly = computed(() => canRateAccountant.value && canRateBoard.value === false)
const canOpenProfile = computed(() => canAny([
  { module: 'Customer', action: 'Read' },
  { module: 'SalesRequest', action: 'ReadAuditProfile' },
  { module: 'SalesRequest', action: 'MatchRead' },
]))

function fixed1(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : n.toFixed(1)
}

function money(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}

const trendKey = computed(() => (summary.value?.ratingTrend === 'up' || summary.value?.ratingTrend === 'down' ? summary.value.ratingTrend : 'flat'))
const trendText = computed(() => (trendKey.value === 'up' ? 'صاعد' : trendKey.value === 'down' ? 'هابط' : 'ثابت'))

const boardSlots = computed(() => {
  const s = summary.value
  if (s == null) return []
  const slots = [
    { key: 'committee' as RatingSlot, label: 'تقييم لجنة المراقبة', card: s.boardOperatorRating, canRate: canRateBoard.value, hidden: accountantOnly.value },
    { key: 'accountant' as RatingSlot, label: 'تقييم المحاسب الرئيسي', card: s.accountantRating, canRate: canRateAccountant.value, hidden: false },
  ]
  return slots.filter(x => x.hidden === false)
})

const inputSlots = computed(() => {
  const s = summary.value
  if (s == null) return []
  return [
    { key: 'follower', label: 'تقييم مسؤول اللجنة الميدانية (المتابع)', data: s.followerCommittee, emptyHint: 'يظهر بعد إنهاء الجباية وتقييم القائمة' },
    { key: 'sales', label: 'تقييم مندوب المبيعات', data: s.salesDelegate, emptyHint: 'يظهر بعد إكمال المبيع وتقييم القائمة' },
  ]
})

const rateSlot = ref<RatingSlot | null>(null)
const rateInitial = computed(() => {
  const s = summary.value
  if (s == null || rateSlot.value == null) return null
  return rateSlot.value === 'accountant' ? s.accountantRatingToday : s.boardOperatorRatingToday
})
const upsertMutation = useUpsertBoardRatingMutation()

function openRate(slot: RatingSlot) {
  rateSlot.value = slot
}

async function submitRating(payload: { score: number; note: string | null }) {
  const slot = rateSlot.value
  if (slot == null || listId.value == null || branchId.value == null || upsertMutation.isPending.value) return
  try {
    await upsertMutation.mutateAsync({ listId: listId.value, branchId: branchId.value, score: payload.score, note: payload.note, slot })
    rateSlot.value = null
    toast.setMassage({ title: 'تم حفظ التقييم', dialogState: DialogState.Success, isOpen: true })
  } catch {
    return
  }
}

function tierCustomerCount(tm: DelegateFollowupTierMoney) {
  if (tm.customerCount > 0) return tm.customerCount
  const s = summary.value
  if (s == null) return 0
  return ({ 'ممتاز': s.excellentCount, 'جيد': s.goodCount, 'ضعيف': s.weakCount, 'متلكئ': s.laggingCount, 'سيء': s.badCount } as Record<string, number>)[tm.label] ?? 0
}

const tierCards = computed(() => {
  const s = summary.value
  if (s == null) return []
  const counts: Record<string, number> = { 'ممتاز': s.excellentCount, 'جيد': s.goodCount, 'ضعيف': s.weakCount, 'متلكئ': s.laggingCount, 'سيء': s.badCount }
  const tiers = TIERS.map(label => {
    const money = s.tierMoney.find(t => t.label === label)
    return { key: label as TableFilter, label, count: counts[label], required: money?.requiredDue ?? null, collected: money?.collectedYesterday ?? null, tierClass: `tier-${label}` }
  })
  return [
    ...tiers,
    { key: 'lt100' as TableFilter, label: 'باقي أقل من 100,000', count: s.remainingLt100Count, required: null, collected: null, tierClass: '' },
    { key: '100to200' as TableFilter, label: 'باقي 100,000 – أقل من 200,000', count: s.remaining100To200Count, required: null, collected: null, tierClass: '' },
  ]
})

const conversionTargets = computed(() => (summary.value?.conversion.targets ?? []).filter(t => (TIERS as readonly string[]).includes(t.targetLabel)))

const tableOpen = ref(false)
const tableFilter = ref<TableFilter>('all')
const search = ref('')
const visibleLimit = ref(PAGE_SIZE)
const filterOptions = computed(() => [
  { value: 'all', label: 'الكل' },
  ...TIERS.map(t => ({ value: t, label: t })),
  { value: 'lt100', label: 'باقي أقل من 100,000' },
  { value: '100to200', label: 'باقي 100,000 – أقل من 200,000' },
])
const tableFilterLabel = computed(() => filterOptions.value.find(o => o.value === tableFilter.value)?.label ?? 'الكل')

function openTable(filter: TableFilter) {
  tableFilter.value = filter
  search.value = ''
  visibleLimit.value = PAGE_SIZE
  tableOpen.value = true
}

function toggleTable(filter: TableFilter) {
  if (tableOpen.value && tableFilter.value === filter) {
    closeTable()
    return
  }
  openTable(filter)
}

function closeTable() {
  tableOpen.value = false
  tableFilter.value = 'all'
}

const nonLegalCustomers = computed(() => (customersQuery.data.value?.customers ?? []).filter(c => c.isLegal === false))

function matches(c: DelegateFollowupCustomer) {
  switch (tableFilter.value) {
    case 'all': return true
    case 'lt100': return c.amountRemaining > 0 && c.amountRemaining < BAND_100K
    case '100to200': return c.amountRemaining >= BAND_100K && c.amountRemaining < BAND_200K
    default: return c.realRatingLabel === tableFilter.value
  }
}

const filteredCustomers = computed(() => {
  const q = search.value.trim().toLowerCase()
  return nonLegalCustomers.value
    .filter(matches)
    .filter(c => q === '' || [c.customerName, c.phoneNumber, c.shopName].some(v => (v ?? '').toLowerCase().includes(q)))
})
const visibleCustomers = computed(() => filteredCustomers.value.slice(0, visibleLimit.value))
watch([tableFilter, search], () => { visibleLimit.value = PAGE_SIZE })

const unpaidOpen = ref(false)
const unpaidCustomers = computed(() => nonLegalCustomers.value.filter(c => c.isNewSaleUnpaid3Days))

function openFullCustomersPage() {
  if (branchId.value == null || listId.value == null) return
  void router.push({
    name: 'ManagementDirectorDelegateFollowupCustomersRoute',
    query: { branchId: String(branchId.value), listId: String(listId.value), filter: tableFilter.value !== 'all' ? tableFilter.value : undefined },
  })
}

function openProfile(customerId: number) {
  if (canOpenProfile.value === false) return
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: customerId },
    query: { returnTo: 'ManagementDirectorDelegateFollowupRoute', branchId: branchId.value != null ? String(branchId.value) : undefined, listId: listId.value != null ? String(listId.value) : undefined },
  })
}

const tableTotals = computed(() => {
  const rows = filteredCustomers.value
  return {
    remaining: rows.reduce((s, c) => s + (c.amountRemaining || 0), 0),
    daily: rows.reduce((s, c) => s + (c.amountDaySales || 0), 0),
    active: rows.filter(c => c.isActive).length,
  }
})

function filterCount(filter: TableFilter) {
  return nonLegalCustomers.value.filter(c => {
    switch (filter) {
      case 'all': return true
      case 'lt100': return c.amountRemaining > 0 && c.amountRemaining < BAND_100K
      case '100to200': return c.amountRemaining >= BAND_100K && c.amountRemaining < BAND_200K
      default: return c.realRatingLabel === filter
    }
  }).length
}

interface HistoryState {
  title: string
  subtitle?: string
  hint?: string
  items: HistoryItem[]
  summary?: HistorySummary[]
  onRow?: (index: number) => void
}
const history = ref<HistoryState | null>(null)
const names = ref<{ title: string; subtitle?: string; items: HistoryItem[] } | null>(null)

function amountItems(days: Array<{ date: string; amount: number }>): HistoryItem[] {
  return days.map(d => ({ key: d.date, date: d.date, value: d.amount ? money(d.amount) : '—', amount: d.amount, empty: !d.amount }))
}

function amountSummary(days: Array<{ amount: number }>, unit = 'يوم'): HistorySummary[] {
  const total = days.reduce((s, d) => s + d.amount, 0)
  const withValue = days.filter(d => d.amount > 0).length
  return [
    { label: 'المجموع', value: money(total) },
    { label: `المعدل لكل ${unit}`, value: money(days.length ? Math.round(total / days.length) : 0) },
    { label: 'الأعلى', value: money(Math.max(0, ...days.map(d => d.amount))) },
    { label: `${unit === 'جمعة' ? 'جُمع' : 'أيام'} فيها وارد`, value: `${withValue} / ${days.length}` },
  ]
}

function ratingItems(days: DelegateFollowupDayRating[]): HistoryItem[] {
  return days.map(r => {
    const score = r.boardOperatorScore
    const extra = [r.boardOperatorNote, r.ratedByUserName ? `بواسطة ${r.ratedByUserName}` : null].filter(Boolean).join(' · ')
    return { key: r.date, date: r.date, value: score != null ? `${score} / 10` : 'لا تقييم', amount: score ?? 0, empty: score == null, note: extra || undefined }
  })
}

function ratingSummary(days: DelegateFollowupDayRating[]): HistorySummary[] {
  const rated = days.filter(d => d.boardOperatorScore != null).map(d => d.boardOperatorScore as number)
  return [
    { label: 'أيام مقيّمة', value: `${rated.length} / ${days.length}` },
    { label: 'المعدل', value: rated.length ? (rated.reduce((s, x) => s + x, 0) / rated.length).toFixed(1) : '—' },
    { label: 'الأدنى', value: rated.length ? String(Math.min(...rated)) : '—' },
  ]
}

function openRealHistory() {
  const s = summary.value
  if (s == null) return
  const days = s.realRatingLast30Days
  history.value = {
    title: 'التقييم الحقيقي — آخر 30 يوماً',
    subtitle: s.listName,
    summary: [
      { label: 'اليوم', value: fixed1(s.realRating) },
      { label: 'قبل أسبوع', value: fixed1(s.realRatingWeekAgo) },
      { label: 'الاتجاه', value: trendText.value },
    ],
    items: days.map(r => ({ key: r.date, date: r.date, value: `${fixed1(r.score)} · ${r.label}`, amount: r.score + 10 })),
  }
}

function openSlotHistory(slot: RatingSlot) {
  const s = summary.value
  if (s == null) return
  const days = slot === 'accountant' ? s.accountantRatingLast30Days : s.boardOperatorLast30Days
  history.value = {
    title: slot === 'accountant' ? 'تقييم المحاسب الرئيسي' : 'تقييم لجنة المراقبة',
    subtitle: `${s.listName} · اليوم ثم آخر 30 يوماً`,
    summary: ratingSummary(days),
    items: ratingItems(days),
  }
}

function openCommitteeHistory(label: string, data: DelegateFollowupCommitteeRating) {
  const days = data.last30Days.map(r => ({ ...r, boardOperatorScore: r.boardOperatorScore ?? r.score }))
  history.value = { title: label, subtitle: summary.value?.listName, summary: ratingSummary(days), items: ratingItems(days) }
}

function openDaysHistory() {
  const s = summary.value
  if (s == null) return
  history.value = { title: 'واردات القائمة — آخر 30 يوماً', subtitle: s.listName, summary: amountSummary(s.collectedLast30Days), items: amountItems(s.collectedLast30Days) }
}

function openFridaysHistory() {
  const s = summary.value
  if (s == null) return
  history.value = { title: 'واردات آخر 30 جمعة', subtitle: s.listName, summary: amountSummary(s.collectedLast30Fridays, 'جمعة'), items: amountItems(s.collectedLast30Fridays) }
}

function openTierHistory(tm: DelegateFollowupTierMoney) {
  history.value = {
    title: `الواردات من ${TIER_PLURAL[tm.label] ?? tm.label} — آخر 30 يوماً`,
    subtitle: summary.value?.listName,
    summary: amountSummary(tm.collectedLast30Days),
    items: amountItems(tm.collectedLast30Days),
  }
}

function openConversionHistory(target: DelegateFollowupConversionTarget) {
  const days = target.last30Days
  history.value = {
    title: `المتحولون إلى ${target.targetLabel}`,
    subtitle: `${summary.value?.listName ?? ''} · آخر 30 يوماً`,
    hint: `اليوم: ${target.detailText} — اضغط أي يوم لعرض أسماء المتحولين`,
    summary: [
      { label: 'اليوم', value: String(target.arrivedCount) },
      { label: 'خلال 30 يوماً', value: String(days.reduce((s, d) => s + d.arrivedCount, 0)) },
    ],
    items: days.map(d => ({
      key: d.date,
      date: d.date,
      value: d.arrivedCount ? `${d.arrivedCount} متحول` : '—',
      amount: d.arrivedCount,
      empty: !d.arrivedCount,
      note: d.fromBreakdown.map(f => `من ${f.fromLabel}: ${f.count}`).join(' · ') || undefined,
    })),
    onRow: index => {
      const day = days[index]
      if (day == null) return
      names.value = {
        title: `المتحولون إلى ${target.targetLabel}`,
        subtitle: day.date,
        items: day.customers.map(c => ({ key: String(c.customerId), title: c.customerName, subtitle: `من ${c.fromLabel}`, value: target.targetLabel })),
      }
    },
  }
}

watch(listId, () => {
  closeTable()
  unpaidOpen.value = false
  history.value = null
  names.value = null
  rateSlot.value = null
})
</script>
