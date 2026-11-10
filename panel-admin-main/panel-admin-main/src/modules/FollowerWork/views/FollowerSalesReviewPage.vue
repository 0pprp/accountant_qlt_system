<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerSalesReviewQuery } from '../requests/queries/useFollowerSalesReviewQuery'
import { useFollowerRemainingCustomersQuery } from '../requests/queries/useFollowerRemainingCustomersQuery'
import { buildFollowerReturnTo, sanitizeFollowerReturnTo } from '../composables/useFollowerReturnContext'
import type { RemainingBucket, SalesReviewSaleRow } from '../domain/salesReviewTypes'
import { REMAINING_BUCKETS } from '../domain/salesReviewTypes'
import '../components/follower-work.css'
import '../components/sales-review/sales-review.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(
  () => auth.hasPermission('FollowerSalesReview', 'Read') === true
)

const followerId = computed(() => {
  const n = Number(route.params.followerId ?? route.query.followerId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const followerName = computed(() => String(route.query.followerName ?? '').trim())
const branchName = computed(() => {
  const b = (auth.branches ?? []).find((x) => x.id === branchId.value)
  return b?.name ?? ''
})

const bucket = computed<RemainingBucket | null>(() => {
  const raw = String(route.query.bucket ?? '').trim()
  return (REMAINING_BUCKETS as readonly string[]).includes(raw)
    ? (raw as RemainingBucket)
    : null
})

const profileOrderId = ref<number | null>(null)

const salesQuery = useFollowerSalesReviewQuery({
  followerId,
  branchId,
  enabled: canRead,
})
const remainingQuery = useFollowerRemainingCustomersQuery({
  followerId,
  branchId,
  bucket,
  enabled: canRead,
})

watch([followerId, branchId], () => {
  profileOrderId.value = null
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: route.name ?? 'ManagementFollowerSalesRoute', query: q, params: route.params })
}

function toggleBucket(key: string) {
  patchQuery({ bucket: bucket.value === key ? null : key })
}

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n) || n === 0) return '—'
  return new Intl.NumberFormat('ar-IQ', { maximumFractionDigits: 0 }).format(n)
}

function fmtRating(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return Math.abs(n - Math.round(n)) < 1e-9 ? String(Math.round(n)) : n.toFixed(1)
}

function trendArrow(t: string | null | undefined) {
  if (t === 'up') return '↑'
  if (t === 'down') return '↓'
  return '→'
}

function authBadgeClass(row: SalesReviewSaleRow) {
  if (!row.authenticated) return 'fsr-badge fsr-badge--warn'
  if (row.isLowAuthRisk) return 'fsr-badge fsr-badge--danger'
  return 'fsr-badge fsr-badge--ok'
}

function authBadgeText(row: SalesReviewSaleRow) {
  if (!row.authenticated) return 'بانتظار'
  if (row.isLowAuthRisk) return 'مصادق — خطر منخفض التقييم'
  return 'مصادق'
}

const profileRow = computed(() => {
  const id = profileOrderId.value
  if (id == null) return null
  return salesQuery.data.value?.salesRows.find((r) => r.orderId === id) ?? null
})

function openProfile(row: SalesReviewSaleRow) {
  profileOrderId.value = row.orderId
}

function openFullCustomerProfile(customerId: number) {
  const returnTo = buildFollowerReturnTo({
    branchId: String(branchId.value ?? ''),
    followerId: String(followerId.value ?? ''),
    section: 'sales',
    ...(bucket.value ? { bucket: bucket.value } : {}),
  })
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(customerId) },
    query: { returnTo: sanitizeFollowerReturnTo(returnTo) ?? undefined },
  })
}

const workspace = computed(() => salesQuery.data.value)
</script>

<template>
  <div class="fsr-page fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">مصادقة البيع</h1>
      <p class="fw-page__sub">
        {{ followerName || workspace?.follower?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
      <p class="fw-page__sub">
        عرض نتائج مصادقة المتابع الميدانية — لا يوجد اعتماد من اللوحة.
      </p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة مصادقة البيع.
    </div>
    <div v-else-if="followerId == null || branchId == null" class="fw-state" role="alert">
      يلزم اختيار متابع ومحافظة من دليل عمل المتابعين.
    </div>
    <div v-else-if="salesQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
      جاري تحميل مصادقة البيع…
    </div>
    <div v-else-if="salesQuery.isError.value" class="fw-state fw-state--error" role="alert">
      تعذر تحميل بيانات القسم.
      <button type="button" class="fw-retry" @click="salesQuery.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="workspace">
      <section class="fsr-section" aria-labelledby="fsr-lists">
        <h2 id="fsr-lists" class="fsr-section__title">كروت القوائم</h2>
        <div class="fsr-grid">
          <article v-for="lc in workspace.listCards" :key="lc.listId" class="fsr-card">
            <h3 class="fsr-card__title">{{ lc.listName }}</h3>
            <p class="fsr-card__meta">
              تقييم وهمي:
              <b>{{ fmtRating(lc.phantomRating) }} {{ trendArrow(lc.scoreTrend) }}</b>
              · زبائن: <b>{{ fmt(lc.ratedCustomerCount) }}</b>
            </p>
          </article>
          <p v-if="!workspace.listCards.length" class="fw-state">لا قوائم مسندة</p>
        </div>
      </section>

      <section class="fsr-section" aria-labelledby="fsr-ratings">
        <h2 id="fsr-ratings" class="fsr-section__title">كروت التقييمات</h2>
        <div class="fsr-bucket-grid">
          <button
            v-for="rb in workspace.ratingBuckets"
            :key="rb.levelKey"
            type="button"
            class="fsr-bucket"
            :class="{ 'fsr-bucket--active': bucket === rb.levelKey }"
            @click="toggleBucket(rb.levelKey)"
          >
            <span>{{ rb.label }}</span>
            <b>{{ fmt(rb.customerCount) }} {{ trendArrow(rb.countTrend) }}</b>
          </button>
        </div>
      </section>

      <section class="fsr-section" aria-labelledby="fsr-remaining">
        <h2 id="fsr-remaining" class="fsr-section__title">كروت المتبقي</h2>
        <div class="fsr-bucket-grid">
          <button
            type="button"
            class="fsr-bucket"
            :class="{ 'fsr-bucket--active': bucket === 'le100' }"
            @click="toggleBucket('le100')"
          >
            <span>باقي أقل من 100,000</span>
            <b>{{ fmt(workspace.remainingLe100Count) }} {{ trendArrow(workspace.remainingLe100Trend) }}</b>
          </button>
          <button
            type="button"
            class="fsr-bucket"
            :class="{ 'fsr-bucket--active': bucket === 'lt200' }"
            @click="toggleBucket('lt200')"
          >
            <span>باقي من 100,000 إلى أقل من 200,000</span>
            <b>{{ fmt(workspace.remainingLt200Count) }} {{ trendArrow(workspace.remainingLt200Trend) }}</b>
          </button>
        </div>
      </section>

      <section v-if="bucket" class="fsr-section" aria-label="زبائن المجموعة">
        <h2 class="fsr-section__title">أسماء الزبائن — {{ bucket }}</h2>
        <div v-if="remainingQuery.isPending.value" class="fw-state fw-state--loading">جاري التحميل…</div>
        <div v-else-if="remainingQuery.isError.value" class="fw-state fw-state--error">تعذر تحميل الأسماء</div>
        <div v-else class="fsr-name-grid">
          <button
            v-for="r in remainingQuery.data.value?.items ?? []"
            :key="`${r.listId}-${r.customerId}`"
            type="button"
            class="fsr-name"
            @click="
              openFullCustomerProfile(r.customerId)
            "
          >
            <span class="fsr-name__title">{{ r.customerName }}</span>
            <span class="fsr-name__sub">
              {{ r.listName || '—' }} · {{ r.ratingLabel || '—' }} · متبقي {{ fmt(r.amountRemaining) }}
            </span>
          </button>
          <p v-if="!(remainingQuery.data.value?.items.length)" class="fw-state">لا زبائن في هذه القائمة</p>
        </div>
      </section>

      <section v-if="profileRow" class="fsr-profile" aria-label="بروفايل مختصر">
        <div class="fsr-profile__head">
          <h2>
            {{ profileRow.customerName }}
            <span :class="authBadgeClass(profileRow)">{{ authBadgeText(profileRow) }}</span>
          </h2>
          <button type="button" class="fw-pager__btn" @click="profileOrderId = null">إغلاق</button>
        </div>
        <div class="fsr-profile__grid">
          <div><span>الهاتف</span><b>{{ profileRow.phone || '—' }}</b></div>
          <div><span>القائمة</span><b>{{ profileRow.listName || '—' }}</b></div>
          <div><span>سعر البيع</span><b>{{ fmt(profileRow.salePrice) }}</b></div>
          <div><span>تاريخ البيع</span><b>{{ profileRow.saleDate || '—' }}</b></div>
          <div><span>القسط</span><b>{{ fmt(profileRow.dailyInstallment) }}</b></div>
          <div><span>البائع</span><b>{{ profileRow.sellerName || '—' }}</b></div>
          <div><span>واصل</span><b>{{ fmt(profileRow.paidAmount) }}</b></div>
          <div><span>متبقي</span><b>{{ fmt(profileRow.remainingAmount) }}</b></div>
          <div><span>تقييم المصادقة</span><b>{{ profileRow.authRating ?? '—' }}</b></div>
          <div class="fsr-profile__wide"><span>ملاحظة</span><b>{{ profileRow.authNote || '—' }}</b></div>
          <div><span>بضاعة المحل</span><b>{{ fmt(profileRow.shopStockEstimatedValue) }}</b></div>
          <div><span>وارد يومي</span><b>{{ fmt(profileRow.estimatedDailyRevenue) }}</b></div>
          <div><span>وقت المصادقة</span><b>{{ profileRow.authenticatedAtUtc || '—' }}</b></div>
        </div>
        <button
          type="button"
          class="fw-card__btn"
          @click="openFullCustomerProfile(profileRow.customerId)"
        >
          عرض بروفايل الزبون الكامل
        </button>
      </section>

      <section class="fsr-section" aria-labelledby="fsr-sales">
        <h2 id="fsr-sales" class="fsr-section__title">
          أسماء مصادقة المبيعات
          <span class="fsr-hint">
            بانتظار {{ workspace.pendingSalesCount }} · مصادق {{ workspace.authenticatedSalesCount }}
          </span>
        </h2>
        <p class="fw-page__sub">
          قيد الانتظار = مبيع على قائمة المتابع بلا سجل مصادقة ميدانية بعد.
        </p>
        <div class="fsr-name-grid">
          <button
            v-for="r in workspace.salesRows"
            :key="r.orderId"
            type="button"
            class="fsr-name"
            :class="{ 'fsr-name--active': profileOrderId === r.orderId }"
            @click="openProfile(r)"
          >
            <span class="fsr-name__title">
              {{ r.customerName }}
              <span :class="authBadgeClass(r)">{{ authBadgeText(r) }}</span>
            </span>
            <span class="fsr-name__sub">
              {{ r.listName || '—' }} · {{ r.sellerName || '—' }} ·
              {{ r.currentRatingLabel || '—' }} · متبقي {{ fmt(r.remainingAmount) }}
            </span>
          </button>
          <p v-if="!workspace.salesRows.length" class="fw-state">
            لا مبيعات بانتظار مصادقة ولا مصادقات على قوائم هذا المتابع
          </p>
        </div>
      </section>
    </template>
  </div>
</template>
