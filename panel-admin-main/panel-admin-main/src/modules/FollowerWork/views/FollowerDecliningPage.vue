<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useFollowerDecliningQuery } from '../requests/queries/useFollowerDecliningQuery'
import { buildFollowerReturnTo, sanitizeFollowerReturnTo } from '../composables/useFollowerReturnContext'
import {
  declineDirectionLabel,
  scoreDelta,
  type FollowerDecliningRow,
} from '../domain/decliningTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(
  () => auth.hasPermission('FollowerDeclining', 'Read') === true
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

const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const pageSize = ref(50)

const selectedCustomerId = ref<number | null>(null)

const decliningQuery = useFollowerDecliningQuery({
  followerId,
  branchId,
  page,
  pageSize,
  enabled: canRead,
})

watch([followerId, branchId], () => {
  selectedCustomerId.value = null
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerDecliningRoute',
    query: q,
    params: route.params,
  })
}

const data = computed(() => decliningQuery.data.value)

const selected = computed<FollowerDecliningRow | null>(() => {
  const id = selectedCustomerId.value
  if (id == null) return null
  return data.value?.items.find((r) => r.customerId === id) ?? null
})

function directionClass(row: FollowerDecliningRow) {
  const d = scoreDelta(row)
  if (d < 0) return 'fdc-dir fdc-dir--down'
  if (d > 0) return 'fdc-dir fdc-dir--up'
  return 'fdc-dir fdc-dir--flat'
}

function directionArrow(row: FollowerDecliningRow) {
  const d = scoreDelta(row)
  if (d < 0) return '↓'
  if (d > 0) return '↑'
  return '→'
}

function noteText(row: FollowerDecliningRow) {
  if (row.hasNote == null) return 'غير متاح'
  return row.hasNote ? 'نعم' : 'لا'
}

function openFullCustomerProfile(customerId: number) {
  const returnTo = buildFollowerReturnTo({
    branchId: String(branchId.value ?? ''),
    followerId: String(followerId.value ?? ''),
    section: 'declining',
    page: String(page.value),
  })
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(customerId) },
    query: { returnTo: sanitizeFollowerReturnTo(returnTo) ?? undefined },
  })
}
</script>

<template>
  <div class="fdc-page fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">المتدنية</h1>
      <p class="fw-page__sub">
        {{ followerName || data?.followerName || `متابع ${followerId ?? '—'}` }}
        ·
        {{ branchName || `فرع ${branchId ?? '—'}` }}
      </p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة قسم المتدنية.
    </div>
    <div v-else-if="followerId == null || branchId == null" class="fw-state" role="alert">
      يلزم اختيار متابع ومحافظة من دليل عمل المتابعين.
    </div>
    <div v-else-if="decliningQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
      جاري تحميل المتدنية…
    </div>
    <div v-else-if="decliningQuery.isError.value" class="fw-state fw-state--error" role="alert">
      تعذر تحميل بيانات القسم.
      <button type="button" class="fw-retry" @click="decliningQuery.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="data">
      <p class="fdc-hint">
        {{ data.lookbackRule }}
        — مقارنة لقطات تقييم مخزّنة خلال آخر {{ data.lookbackDays }} أيام ({{ data.asOfLocalDate }}،
        {{ data.timeZoneId }}). ليست كل زبائن «ضعيف» وليست كل من عليه باقي.
      </p>

      <div class="fdc-meta" aria-label="ملخص المقارنة">
        <span>عدد المتدنية: <b>{{ data.totalCount }}</b></span>
        <span>وضع المقارنة: <b>{{ data.comparisonMode === 'stored_snapshots' ? 'لقطات مخزّنة' : data.comparisonMode }}</b></span>
        <span>الانتقالات: <b>{{ data.targetTransitions.join(' · ') }}</b></span>
      </div>

      <div v-if="data.totalCount === 0" class="fw-state" role="status">
        لا زبائن متدنية حالياً ضمن تعريف القسم (قد يعني عدم وجود لقطة سابقة خلال 7 أيام للمقارنة).
      </div>

      <div v-else class="fdc-list" role="list">
        <button
          v-for="r in data.items"
          :key="r.customerId"
          type="button"
          class="fdc-row"
          :class="{ 'fdc-row--active': selectedCustomerId === r.customerId }"
          role="listitem"
          @click="selectedCustomerId = r.customerId"
        >
          <span class="fdc-row__title">{{ r.customerName }}</span>
          <span class="fdc-row__sub">
            {{ r.fromLabel || '—' }} → {{ r.toLabel || r.ratingLabel || '—' }}
            ·
            <span :class="directionClass(r)">
              {{ directionArrow(r) }} {{ declineDirectionLabel(r) }}
              ({{ scoreDelta(r) }})
            </span>
            · {{ r.daysBetween }} يوم
            · {{ r.listName || '—' }}
          </span>
        </button>
      </div>

      <div v-if="data.totalPages > 1" class="fdc-pager">
        <button
          type="button"
          class="fw-pager__btn"
          :disabled="page <= 1"
          @click="patchQuery({ page: String(page - 1) })"
        >
          السابق
        </button>
        <span>صفحة {{ data.page }} / {{ data.totalPages }}</span>
        <button
          type="button"
          class="fw-pager__btn"
          :disabled="page >= data.totalPages"
          @click="patchQuery({ page: String(page + 1) })"
        >
          التالي
        </button>
      </div>

      <section v-if="selected" class="fdc-details" aria-label="تفاصيل المتدني">
        <div class="fdc-details__head">
          <h2>{{ selected.customerName }}</h2>
          <button type="button" class="fw-pager__btn" @click="selectedCustomerId = null">إغلاق</button>
        </div>
        <div class="fdc-details__grid">
          <div><span>الهاتف</span><b>{{ selected.phone || '—' }}</b></div>
          <div><span>القائمة</span><b>{{ selected.listName || '—' }}</b></div>
          <div><span>من</span><b>{{ selected.fromLabel }} ({{ selected.fromScore }})</b></div>
          <div><span>إلى</span><b>{{ selected.toLabel }} ({{ selected.toScore }})</b></div>
          <div>
            <span>الاتجاه</span>
            <b :class="directionClass(selected)">
              {{ directionArrow(selected) }} {{ declineDirectionLabel(selected) }}
              · فرق {{ scoreDelta(selected) }}
            </b>
          </div>
          <div><span>الأيام بين اللقطتين</span><b>{{ selected.daysBetween }}</b></div>
          <div><span>تاريخ اللقطة السابقة</span><b>{{ selected.priorCaptureDate }}</b></div>
          <div><span>تاريخ اللقطة الحالية</span><b>{{ selected.currentCaptureDate }}</b></div>
          <div><span>ملاحظة ميدانية</span><b>{{ noteText(selected) }}</b></div>
          <div><span>مصادقة بيع على القائمة</span><b>{{ selected.hasSaleAuth ? 'نعم' : 'لا' }}</b></div>
        </div>
        <button
          type="button"
          class="fw-card__btn"
          style="margin-top: 1rem; min-height: 44px"
          @click="openFullCustomerProfile(selected.customerId)"
        >
          عرض بروفايل الزبون
        </button>
      </section>
    </template>
  </div>
</template>
