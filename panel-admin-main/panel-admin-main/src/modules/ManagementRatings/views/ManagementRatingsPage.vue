<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { RATING_BUCKETS } from '../domain/endpoints'
import type { RatingsScope } from '../domain/types'
import { useRatingsContext } from '../composables/useRatingsContext'
import {
  useRatingsBranchesQuery,
  useRatingsCatalogQuery,
  useRatingsCustomersQuery,
  useRatingsTrendQuery,
} from '../requests/queries/useRatingsQueries'
import { useAuthStore } from '@/modules/Auth/store'
import '../components/management-ratings.css'

const router = useRouter()

const auth = useAuthStore()
const { ctx, patch, clearBranch } = useRatingsContext()

const canRead = computed(
  () => auth.hasPermission('ManagementRatings', 'Read') === true
)
const canDetails = computed(
  () =>
    canRead.value &&
    auth.hasPermission('ManagementRatings', 'DetailsRead') === true
)

const branchId = computed(() => ctx.value.branchId)
const scope = computed(() => ctx.value.scope)
const resourceId = computed(() => ctx.value.resourceId)
const from = computed(() => ctx.value.from)
const to = computed(() => ctx.value.to)
const level = computed(() => ctx.value.level)
const searchQ = ref('')
const page = ref(1)

const branchesQuery = useRatingsBranchesQuery(canRead)
const catalogQuery = useRatingsCatalogQuery(branchId, canRead)
const trendQuery = useRatingsTrendQuery({
  branchId,
  scope,
  resourceId,
  from,
  to,
  enabled: canRead,
})
const customersQuery = useRatingsCustomersQuery({
  branchId,
  scope,
  resourceId,
  level,
  from,
  to,
  page,
  q: searchQ,
  enabled: canDetails,
})

watch([branchId, scope, resourceId, level], () => {
  page.value = 1
})

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat('ar-IQ', { maximumFractionDigits: 0 }).format(n)
}

function fmtScore(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return n.toFixed(2)
}

function openBranch(id: number) {
  patch({
    branchId: id,
    scope: 'branch',
    resourceId: 0,
    level: null,
  })
}

function openCustomerProfile(customerId: number) {
  if (!customerId || customerId <= 0) return
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(customerId) },
    query: {
      returnTo: 'ManagementRatingsRoute',
      branchId: branchId.value ? String(branchId.value) : undefined,
      scope: scope.value || undefined,
      resourceId: resourceId.value ? String(resourceId.value) : undefined,
      level: level.value || undefined,
      from: from.value || undefined,
      to: to.value || undefined,
    },
  })
}

function setScope(s: RatingsScope) {
  patch({ scope: s, resourceId: 0, level: null })
}

function setResource(id: number) {
  patch({ resourceId: id, level: null })
}

function openBucket(key: string) {
  if (!canDetails.value) return
  patch({ level: key })
}

function closeBucket() {
  patch({ level: null })
}

const entityOptions = computed(() => {
  const c = catalogQuery.data.value
  if (!c) return []
  if (scope.value === 'follower') return c.followers
  if (scope.value === 'employee') return c.employees
  return []
})

const chartPoints = computed(() => trendQuery.data.value?.points ?? [])
const chartPath = computed(() => {
  const pts = chartPoints.value
  if (pts.length < 2) return ''
  const vals = pts.map((p) => p.value)
  const min = Math.min(...vals, -10)
  const max = Math.max(...vals, 10)
  const w = Math.max(320, pts.length * 8)
  const h = 160
  const span = max - min || 1
  return pts
    .map((p, i) => {
      const x = (i / (pts.length - 1)) * (w - 16) + 8
      const y = h - 12 - ((p.value - min) / span) * (h - 24)
      return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`
    })
    .join(' ')
})

const dirClass = computed(() => {
  const d = String(trendQuery.data.value?.direction || 'flat').toLowerCase()
  if (d === 'up') return 'mr-dir-up'
  if (d === 'down') return 'mr-dir-down'
  return 'mr-dir-flat'
})

const dirText = computed(() => {
  const d = String(trendQuery.data.value?.direction || 'flat').toLowerCase()
  if (d === 'up') return 'تحسن'
  if (d === 'down') return 'انخفاض'
  return 'ثابت'
})
</script>

<template>
  <div class="mr-page">
    <template v-if="!ctx.branchId">
      <h1 class="mr-title">التقييمات</h1>
      <p class="mr-sub">
        كل الفروع المسموحة. اضغط بطاقة لفتح تفاصيل التقييم — ليست تقييم موظفي
        المبيعات ولا عامل اللوحة.
      </p>

      <div
        v-if="branchesQuery.isError.value"
        class="mr-error"
      >
        تعذر تحميل الفروع.
      </div>
      <div
        v-else-if="branchesQuery.isPending.value"
        class="mr-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!branchesQuery.data.value?.branches.length"
        class="mr-state"
      >
        لا فروع ضمن صلاحياتك.
      </div>
      <div
        v-else
        class="mr-grid"
      >
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
      <button
        type="button"
        class="mr-back"
        @click="clearBranch"
      >
        ← كل الفروع
      </button>
      <h1 class="mr-title">
        {{ catalogQuery.data.value?.branchName || `فرع #${ctx.branchId}` }}
      </h1>
      <p class="mr-sub">
        الفترة = تاريخ البيع فقط — التقييم حتى اليوم (بغداد). اضغط الفئة لأسماء
        الزبائن.
      </p>

      <div class="mr-filters">
        <div class="mr-field">
          <label for="mr-scope">المستوى</label>
          <select
            id="mr-scope"
            :value="ctx.scope"
            @change="
              setScope(
                ($event.target as HTMLSelectElement).value as RatingsScope
              )
            "
          >
            <option value="branch">تقييم الفرع</option>
            <option value="follower">المتابعين</option>
            <option value="employee">موظفو المبيعات</option>
          </select>
        </div>
        <div
          v-if="ctx.scope !== 'branch'"
          class="mr-field"
        >
          <label for="mr-entity">المورد</label>
          <select
            id="mr-entity"
            :value="ctx.resourceId || ''"
            @change="
              setResource(Number(($event.target as HTMLSelectElement).value))
            "
          >
            <option value="">اختر…</option>
            <option
              v-for="e in entityOptions"
              :key="e.id"
              :value="e.id"
            >
              {{ e.name }}
            </option>
          </select>
        </div>
        <div class="mr-field">
          <label for="mr-from">من (بيع)</label>
          <input
            id="mr-from"
            type="date"
            :value="ctx.from"
            @change="
              patch({ from: ($event.target as HTMLInputElement).value })
            "
          />
        </div>
        <div class="mr-field">
          <label for="mr-to">إلى (بيع)</label>
          <input
            id="mr-to"
            type="date"
            :value="ctx.to"
            @change="patch({ to: ($event.target as HTMLInputElement).value })"
          />
        </div>
      </div>

      <div
        v-if="trendQuery.isError.value"
        class="mr-error"
      >
        تعذر تحميل الاتجاه.
      </div>
      <div
        v-else-if="trendQuery.isPending.value"
        class="mr-state"
      >
        جاري تحميل التقييم…
      </div>
      <template v-else-if="trendQuery.data.value">
        <div class="mr-metrics">
          <div class="mr-metric">
            <span class="lbl">التقييم</span>
            <span class="val">{{ fmtScore(trendQuery.data.value.endValue) }}</span>
            <span>{{ trendQuery.data.value.ratingLabel }}</span>
          </div>
          <div class="mr-metric">
            <span class="lbl">العدد</span>
            <span class="val">{{ fmt(trendQuery.data.value.customerCount) }}</span>
          </div>
          <div class="mr-metric">
            <span class="lbl">الاتجاه</span>
            <span
              class="val"
              :class="dirClass"
              >{{ dirText }}</span
            >
            <span aria-hidden="true">{{
              trendQuery.data.value.direction === 'up'
                ? '↑'
                : trendQuery.data.value.direction === 'down'
                  ? '↓'
                  : '→'
            }}</span>
          </div>
          <div
            v-if="trendQuery.data.value.phantomEndValue != null"
            class="mr-metric"
          >
            <span class="lbl">وهمي</span>
            <span class="val">{{
              fmtScore(trendQuery.data.value.phantomEndValue)
            }}</span>
          </div>
          <div
            v-for="b in RATING_BUCKETS"
            :key="b.key"
            class="mr-metric"
          >
            <button
              type="button"
              :disabled="!canDetails"
              :title="
                canDetails ? 'فتح أسماء الفئة' : 'تحتاج صلاحية عرض التفاصيل'
              "
              @click="openBucket(b.key)"
            >
              <span class="lbl">{{ b.label }}</span>
              <span class="val">{{
                fmt(trendQuery.data.value[b.countKey] as number)
              }}</span>
            </button>
          </div>
        </div>

        <p class="mr-sub">{{ trendQuery.data.value.metricLabel }}</p>

        <div
          v-if="chartPoints.length"
          class="mr-chart"
          role="img"
          :aria-label="`رسم التقييم من ${trendQuery.data.value.chartFrom} إلى ${trendQuery.data.value.chartTo}`"
        >
          <svg
            viewBox="0 0 400 160"
            preserveAspectRatio="none"
            width="100%"
            height="160"
          >
            <path
              :d="chartPath"
              fill="none"
              stroke="#006A64"
              stroke-width="2"
            />
          </svg>
          <div class="mr-card-row">
            <span>{{ trendQuery.data.value.chartFrom }}</span>
            <span>{{ trendQuery.data.value.chartTo }}</span>
          </div>
        </div>
      </template>

      <section
        v-if="ctx.level && canDetails"
        aria-labelledby="mr-bucket-title"
      >
        <div class="mr-filters">
          <h2
            id="mr-bucket-title"
            class="mr-title"
            style="font-size: 1.2rem"
          >
            زبائن: {{ ctx.level }}
          </h2>
          <button
            type="button"
            class="mr-back"
            @click="closeBucket"
          >
            إغلاق
          </button>
          <div class="mr-field">
            <label for="mr-q">بحث</label>
            <input
              id="mr-q"
              v-model="searchQ"
              type="search"
            />
          </div>
        </div>

        <div
          v-if="customersQuery.isError.value"
          class="mr-error"
        >
          تعذر تحميل الزبائن.
        </div>
        <div
          v-else-if="customersQuery.isPending.value"
          class="mr-state"
        >
          جاري التحميل…
        </div>
        <div
          v-else-if="!customersQuery.data.value?.items.length"
          class="mr-state"
        >
          لا زبائن في هذه الفئة ضمن النطاق.
        </div>
        <div
          v-else
          class="mr-table-wrap"
        >
          <table class="mr-table">
            <thead>
              <tr>
                <th>الزبون</th>
                <th>الهاتف</th>
                <th>النقاط</th>
                <th>الواصل</th>
                <th>الباقي</th>
                <th>بروفايل</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="c in customersQuery.data.value.items"
                :key="c.customerId"
              >
                <td>{{ c.customerName }}</td>
                <td>{{ c.phone || '—' }}</td>
                <td class="num">{{ c.score }}</td>
                <td class="num">
                  {{
                    c.hidePaidRemaining ? 'مخفي' : fmt(c.amountPaid)
                  }}
                </td>
                <td class="num">
                  {{
                    c.hidePaidRemaining ? 'مخفي' : fmt(c.amountRemaining)
                  }}
                </td>
                <td>
                  <button
                    type="button"
                    class="mr-back"
                    @click="openCustomerProfile(c.customerId)"
                  >
                    فتح
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <p class="mr-sub">
            الإجمالي: {{ customersQuery.data.value.totalCount }} — صفحة
            {{ customersQuery.data.value.page }}
          </p>
          <div class="mr-filters">
            <button
              type="button"
              class="mr-back"
              :disabled="page <= 1"
              @click="page = Math.max(1, page - 1)"
            >
              السابق
            </button>
            <button
              type="button"
              class="mr-back"
              :disabled="
                page * 100 >= (customersQuery.data.value.totalCount || 0)
              "
              @click="page += 1"
            >
              التالي
            </button>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>
