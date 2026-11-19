<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { RATING_BUCKETS } from '../domain/endpoints'
import type { RatingsScope } from '../domain/types'
import { useRatingsContext } from '../composables/useRatingsContext'
import {
  useRatingsBranchesQuery,
  useRatingsCatalogQuery,
  useRatingsCustomersQuery,
  useRatingsListCardsQueries,
  useRatingsTrendQuery,
} from '../requests/queries/useRatingsQueries'
import { useAuthStore } from '@/modules/Auth/store'
import { AppDateField, AppSearchField, AppSelect } from '@/modules/Core/components/ui'
import { formatNumber } from '@/modules/Core/utils/formatDisplay'
import '../components/management-ratings.css'

const SCOPE_CHIPS: ReadonlyArray<{ label: string; value: RatingsScope }> = [
  { label: 'الفرع', value: 'branch' },
  { label: 'قائمة', value: 'list' },
  { label: 'متابع', value: 'follower' },
  { label: 'موظف مبيعات', value: 'employee' },
]

const PERIOD_CAPTION = 'الفترة = تاريخ البيع فقط، والتقييم/التسديدات تُحسب حتى اليوم.'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { ctx, routeName, patch, clearBranch } = useRatingsContext()

const isBoard = computed(() => route.meta.managementSectionId === 'delegate')

const canRead = computed(() => auth.hasPermission('ManagementRatings', 'Read') === true)
const canDetails = computed(
  () => canRead.value && auth.hasPermission('ManagementRatings', 'DetailsRead') === true
)

const branchId = computed(() => ctx.value.branchId)
const scope = computed(() => ctx.value.scope)
const resourceId = computed(() => ctx.value.resourceId)
const drill = computed(() => ctx.value.drill)
const from = computed(() => ctx.value.from)
const to = computed(() => ctx.value.to)
const level = computed(() => ctx.value.level)
const searchQ = ref('')
const debouncedQ = ref('')
let searchTimer: ReturnType<typeof setTimeout> | undefined
watch(searchQ, (v) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    debouncedQ.value = v
  }, 300)
})
const page = ref(1)

const effectiveScope = computed<RatingsScope>(() => (drill.value > 0 ? 'list' : scope.value))
const effectiveResource = computed(() => (drill.value > 0 ? drill.value : resourceId.value))
const showFollowerCards = computed(
  () => scope.value === 'follower' && resourceId.value > 0 && drill.value <= 0
)

const branchesQuery = useRatingsBranchesQuery(computed(() => canRead.value && branchId.value == null))
const catalogQuery = useRatingsCatalogQuery(branchId, canRead)
const trendQuery = useRatingsTrendQuery({
  branchId,
  scope: effectiveScope,
  resourceId: effectiveResource,
  from,
  to,
  enabled: computed(() => canRead.value && !showFollowerCards.value),
})
const customersQuery = useRatingsCustomersQuery({
  branchId,
  scope: effectiveScope,
  resourceId: effectiveResource,
  level: computed(() => (showFollowerCards.value ? null : level.value)),
  from,
  to,
  page,
  q: debouncedQ,
  enabled: canDetails,
})

watch([branchId, scope, resourceId, drill, level, debouncedQ, from, to], () => {
  page.value = 1
})

const entityOptions = computed(() => {
  const c = catalogQuery.data.value
  if (!c) return []
  if (scope.value === 'list') return c.lists
  if (scope.value === 'follower') return c.followers
  if (scope.value === 'employee') return c.employees
  return []
})

const entitySelectOptions = computed(() =>
  entityOptions.value.map((e) => ({ label: e.name, value: e.id }))
)

const pickerLabel = computed(() => (scope.value === 'employee' ? 'اختر الموظف' : 'اختر للرسم'))

watch(
  [scope, entityOptions, resourceId],
  ([s, options, current]) => {
    if (s === 'branch' || !catalogQuery.data.value) return
    if (options.length === 0) return
    if (current > 0) return
    patch({ resourceId: options[0].id, drill: 0, level: null })
  },
  { immediate: true }
)

watch(branchId, (next, prev) => {
  if (prev != null && next != null && next !== prev && (resourceId.value > 0 || level.value)) {
    patch({ scope: 'branch', resourceId: 0, drill: 0, level: null })
  }
})

const followerListIds = computed(() => {
  const c = catalogQuery.data.value
  if (!c || scope.value !== 'follower' || resourceId.value <= 0) return []
  const follower = c.followers.find((f) => f.id === resourceId.value)
  const ids = new Set(follower?.assignedListIds ?? [])
  return c.lists.filter((l) => ids.has(l.id)).map((l) => l.id)
})
const listNames = computed(() => {
  const map = new Map<number, string>()
  for (const l of catalogQuery.data.value?.lists ?? []) map.set(l.id, l.name)
  return map
})
const listCardQueries = useRatingsListCardsQueries({
  branchId,
  listIds: followerListIds,
  from,
  to,
  enabled: computed(() => canRead.value && showFollowerCards.value),
})
const followerCards = computed(() =>
  followerListIds.value
    .map((id, index) => {
      const q = listCardQueries.value[index]
      const data = q?.data
      return {
        id,
        name: listNames.value.get(id) ?? `قائمة ${id}`,
        loading: q?.isPending ?? true,
        failed: q?.isError ?? false,
        score: data?.endValue ?? null,
        label: data?.ratingLabel ?? '',
        customers: data?.customerCount ?? null,
        direction: String(data?.direction ?? 'flat').toLowerCase(),
      }
    })
    .sort((a, b) => a.name.localeCompare(b.name, 'ar'))
)
const drilledName = computed(() => listNames.value.get(drill.value) ?? '')

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return formatNumber(n)
}

function fmtScore(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return formatNumber(n, { maxFractionDigits: 2 })
}

function dirArrow(d: string) {
  if (d === 'up') return '↑'
  if (d === 'down') return '↓'
  return '→'
}

function dirClassOf(d: string) {
  if (d === 'up') return 'mr-dir-up'
  if (d === 'down') return 'mr-dir-down'
  return 'mr-dir-flat'
}

function openBranch(id: number) {
  patch({ branchId: id, scope: 'branch', resourceId: 0, drill: 0, level: null })
}

function openCustomerProfile(customerId: number) {
  if (!customerId || customerId <= 0) return
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(customerId) },
    query: {
      returnTo: routeName.value,
      branchId: branchId.value ? String(branchId.value) : undefined,
      scope: scope.value || undefined,
      resourceId: resourceId.value ? String(resourceId.value) : undefined,
      drill: drill.value ? String(drill.value) : undefined,
      level: level.value || undefined,
      from: from.value || undefined,
      to: to.value || undefined,
    },
  })
}

function setScope(s: RatingsScope) {
  if (s === scope.value) return
  const firstId =
    s === 'branch'
      ? 0
      : ((s === 'list'
          ? catalogQuery.data.value?.lists
          : s === 'follower'
            ? catalogQuery.data.value?.followers
            : catalogQuery.data.value?.employees)?.[0]?.id ?? 0)
  patch({ scope: s, resourceId: firstId, drill: 0, level: null })
}

function setResource(id: number) {
  patch({ resourceId: id, drill: 0, level: null })
}

function openFollowerList(listId: number) {
  patch({ drill: listId, level: null })
}

function clearDrill() {
  patch({ drill: 0, level: null })
}

function openBucket(key: string) {
  if (!canDetails.value) return
  patch({ level: key })
}

function closeBucket() {
  patch({ level: null })
}

const bucketLabel = computed(
  () => RATING_BUCKETS.find((b) => b.key === level.value)?.label ?? level.value ?? ''
)

const CHART_W = 400
const CHART_H = 160
const chartPoints = computed(() => trendQuery.data.value?.points ?? [])
const chartPath = computed(() => {
  const pts = chartPoints.value
  if (pts.length < 2) return ''
  const vals = pts.map((p) => p.value)
  const min = Math.min(...vals, -10)
  const max = Math.max(...vals, 10)
  const span = max - min || 1
  return pts
    .map((p, i) => {
      const x = (i / (pts.length - 1)) * (CHART_W - 16) + 8
      const y = CHART_H - 12 - ((p.value - min) / span) * (CHART_H - 24)
      return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`
    })
    .join(' ')
})

const trendDirection = computed(() => String(trendQuery.data.value?.direction || 'flat').toLowerCase())
const dirText = computed(() => {
  if (trendDirection.value === 'up') return 'تحسن'
  if (trendDirection.value === 'down') return 'انخفاض'
  return 'ثابت'
})

const customerRows = computed(() => customersQuery.data.value?.items ?? [])
const customerTotal = computed(() => customersQuery.data.value?.totalCount ?? 0)
</script>

<template>
  <div class="mr-page" :class="{ 'is-board': isBoard }">
    <template v-if="!ctx.branchId">
      <component :is="isBoard ? 'h2' : 'h1'" :class="isBoard ? 'dfb-title' : 'mr-title'">التقييمات</component>
      <p :class="isBoard ? 'dfb-subtitle' : 'mr-sub'">
        كل المحافظات المسموحة. اضغط بطاقة لفتح تفاصيل التقييم.
      </p>

      <div v-if="branchesQuery.isError.value" class="mr-error">تعذر تحميل الفروع.</div>
      <div v-else-if="branchesQuery.isPending.value" class="mr-state">جاري التحميل…</div>
      <div v-else-if="!branchesQuery.data.value?.branches.length" class="mr-state">لا فروع ضمن صلاحياتك.</div>
      <div v-else class="mr-grid">
        <button
          v-for="b in branchesQuery.data.value.branches"
          :key="b.branchId"
          type="button"
          class="mr-card"
          @click="openBranch(b.branchId)"
        >
          <h3>{{ b.name }}</h3>
          <div class="mr-card-row">
            <span>جدد هذا الشهر</span>
            <strong>{{ fmt(b.currentMonth?.newCustomersCount) }}</strong>
          </div>
          <div class="mr-card-row">
            <span>صُفّروا</span>
            <strong>{{ fmt(b.currentMonth?.zeroedCustomersCount) }}</strong>
          </div>
          <div class="mr-card-row">
            <span>صافي العدد</span>
            <strong>{{ fmt(b.currentMonth?.netCustomersCount) }}</strong>
          </div>
        </button>
      </div>
    </template>

    <template v-else>
      <button type="button" class="mr-back" @click="clearBranch">← كل الفروع</button>
      <component :is="isBoard ? 'h2' : 'h1'" :class="isBoard ? 'dfb-title' : 'mr-title'">
        {{ catalogQuery.data.value?.branchName || `فرع #${ctx.branchId}` }}
      </component>
      <p :class="isBoard ? 'dfb-subtitle' : 'mr-sub'">{{ PERIOD_CAPTION }}</p>

      <div class="mr-scope-chips" role="group" aria-label="مستوى التقييم">
        <button
          v-for="chip in SCOPE_CHIPS"
          :key="chip.value"
          type="button"
          class="mr-chip"
          :aria-pressed="ctx.scope === chip.value"
          @click="setScope(chip.value)"
        >
          {{ chip.label }}
        </button>
      </div>

      <div class="q-filters">
        <AppSelect
          v-if="ctx.scope !== 'branch'"
          :model-value="ctx.resourceId || null"
          :label="pickerLabel"
          :placeholder="pickerLabel"
          :options="entitySelectOptions"
          :empty-message="catalogQuery.isPending.value ? 'جاري التحميل…' : 'لا عناصر'"
          @update:model-value="(v) => setResource(typeof v === 'number' ? v : 0)"
        />
        <AppDateField :model-value="ctx.from" label="بيع من" @update:model-value="(v) => patch({ from: v })" />
        <AppDateField :model-value="ctx.to" label="إلى" @update:model-value="(v) => patch({ to: v })" />
      </div>

      <button v-if="ctx.drill > 0" type="button" class="mr-back" @click="clearDrill">
        → {{ drilledName ? `رجوع · ${drilledName}` : 'رجوع لقوائم المتابع' }}
      </button>

      <section v-if="showFollowerCards" aria-label="قوائم المتابع">
        <div v-if="catalogQuery.isPending.value" class="mr-state">جاري التحميل…</div>
        <div v-else-if="!followerCards.length" class="mr-state">لا قوائم مرتبطة بهذا المتابع</div>
        <div v-else class="mr-grid">
          <button
            v-for="card in followerCards"
            :key="card.id"
            type="button"
            class="mr-card mr-list-card"
            @click="openFollowerList(card.id)"
          >
            <span class="mr-list-card-text">
              <h3>{{ card.name }}</h3>
              <span v-if="card.loading" class="mr-sub-sm">جاري التحميل…</span>
              <span v-else-if="card.failed" class="mr-sub-sm">تعذر تحميل التقييم</span>
              <span v-else class="mr-sub-sm">
                <strong class="mr-num">{{ fmtScore(card.score) }}</strong>
                <template v-if="card.label"> · <span :class="`tier-${card.label}`">{{ card.label }}</span></template>
                <template v-if="card.customers != null"> · زبائن: {{ fmt(card.customers) }}</template>
              </span>
            </span>
            <span v-if="!card.loading && !card.failed" class="mr-list-dir" :class="dirClassOf(card.direction)" aria-hidden="true">
              {{ dirArrow(card.direction) }}
            </span>
          </button>
        </div>
      </section>

      <template v-else>
        <div v-if="ctx.scope !== 'branch' && effectiveResource <= 0" class="mr-state">
          {{ catalogQuery.isPending.value ? 'جاري التحميل…' : 'لا عناصر ضمن هذا المستوى.' }}
        </div>
        <div v-else-if="trendQuery.isError.value" class="mr-error">تعذر تحميل الاتجاه.</div>
        <div v-else-if="trendQuery.isPending.value" class="mr-state">جاري تحميل التقييم…</div>
        <template v-else-if="trendQuery.data.value">
          <div class="mr-metrics">
            <div class="mr-metric">
              <span class="lbl">التقييم</span>
              <span class="val">{{ fmtScore(trendQuery.data.value.endValue) }}</span>
              <span :class="`tier-${trendQuery.data.value.ratingLabel}`">{{ trendQuery.data.value.ratingLabel }}</span>
            </div>
            <div class="mr-metric">
              <span class="lbl">{{ ctx.scope === 'employee' ? 'كل الزبائن ضمن الفترة' : 'الزبائن المسددين' }}</span>
              <span class="val">{{ fmt(trendQuery.data.value.customerCount) }}</span>
            </div>
            <div class="mr-metric">
              <span class="lbl">الاتجاه</span>
              <span class="val" :class="dirClassOf(trendDirection)">{{ dirText }}</span>
              <span aria-hidden="true">{{ dirArrow(trendDirection) }}</span>
            </div>
            <div v-if="trendQuery.data.value.phantomEndValue != null" class="mr-metric">
              <span class="lbl">وهمي</span>
              <span class="val">{{ fmtScore(trendQuery.data.value.phantomEndValue) }}</span>
            </div>
            <div v-if="trendQuery.data.value.installmentSum != null" class="mr-metric">
              <span class="lbl">مجموع الأقساط</span>
              <span class="val">{{ fmt(trendQuery.data.value.installmentSum) }}</span>
            </div>
          </div>

          <div class="mr-level-chips" role="group" aria-label="فئات التقييم">
            <button
              v-for="b in RATING_BUCKETS"
              :key="b.key"
              type="button"
              class="mr-chip"
              :class="`tier-${b.label}`"
              :aria-pressed="ctx.level === b.key"
              :disabled="!canDetails"
              :title="canDetails ? 'عرض أسماء الفئة' : 'تحتاج صلاحية عرض التفاصيل'"
              @click="openBucket(b.key)"
            >
              {{ b.label }} <strong>{{ fmt(trendQuery.data.value[b.countKey] as number) }}</strong>
            </button>
          </div>

          <p class="mr-sub">{{ trendQuery.data.value.metricLabel }}</p>

          <div
            v-if="chartPoints.length"
            class="mr-chart"
            role="img"
            :aria-label="`رسم التقييم من ${trendQuery.data.value.chartFrom} إلى ${trendQuery.data.value.chartTo}`"
          >
            <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" preserveAspectRatio="none" width="100%" :height="CHART_H">
              <path :d="chartPath" fill="none" stroke="#006A64" stroke-width="2" />
            </svg>
            <div class="mr-card-row">
              <span>{{ trendQuery.data.value.chartFrom }}</span>
              <span>{{ trendQuery.data.value.chartTo }}</span>
            </div>
          </div>
        </template>

        <section v-if="ctx.level && canDetails" aria-labelledby="mr-bucket-title" class="mr-bucket">
          <div class="q-filters">
            <h3 id="mr-bucket-title" class="mr-bucket-title">
              {{ bucketLabel }}
              <template v-if="customersQuery.data.value">({{ fmt(customerTotal) }})</template>
            </h3>
            <button type="button" class="mr-back" @click="closeBucket">إغلاق</button>
            <AppSearchField v-model="searchQ" label="بحث" />
          </div>

          <div v-if="customersQuery.isError.value" class="mr-error">تعذر تحميل الزبائن.</div>
          <div v-else-if="customersQuery.isPending.value" class="mr-state">جاري التحميل…</div>
          <div v-else-if="!customerRows.length" class="mr-state">لا أسماء في هذه الحالة</div>
          <div v-else class="mr-table-wrap">
            <table class="mr-table">
              <thead>
                <tr>
                  <th>الزبون</th>
                  <th>الهاتف</th>
                  <th>النقاط</th>
                  <th>سعر البيع</th>
                  <th>القسط</th>
                  <th>الواصل</th>
                  <th>الباقي</th>
                  <th>بروفايل</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="c in customerRows"
                  :key="c.customerId"
                  class="mr-row-click"
                  @click="openCustomerProfile(c.customerId)"
                >
                  <td>{{ c.customerName }}</td>
                  <td>{{ c.phone || '—' }}</td>
                  <td class="num">{{ c.score }}</td>
                  <td class="num">{{ fmt(c.amountTotal) }}</td>
                  <td class="num">{{ fmt(c.installment) }}</td>
                  <td class="num">{{ c.hidePaidRemaining ? 'مخفي' : fmt(c.amountPaid) }}</td>
                  <td class="num">{{ c.hidePaidRemaining ? 'مخفي' : fmt(c.amountRemaining) }}</td>
                  <td>
                    <button type="button" class="mr-back mr-inline" @click.stop="openCustomerProfile(c.customerId)">فتح</button>
                  </td>
                </tr>
              </tbody>
            </table>
            <p class="mr-sub">
              المعروض {{ fmt(customerRows.length) }} من {{ fmt(customerTotal) }} — صفحة {{ customersQuery.data.value?.page }}
            </p>
            <div class="mr-filters">
              <button type="button" class="mr-back" :disabled="page <= 1" @click="page = Math.max(1, page - 1)">السابق</button>
              <button
                type="button"
                class="mr-back"
                :disabled="page * (customersQuery.data.value?.pageSize || 100) >= customerTotal"
                @click="page += 1"
              >
                التالي
              </button>
            </div>
          </div>
        </section>
      </template>
    </template>
  </div>
</template>
