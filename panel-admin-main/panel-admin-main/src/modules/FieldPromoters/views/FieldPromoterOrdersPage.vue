<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">الطلبات</h2>
        <p class="dfb-subtitle">طلبات البيع التي رفعها المروّجون ومسار تسديدها</p>
      </div>
    </header>

    <section class="fpp-filters" aria-label="تصفية الطلبات">
      <div class="fpp-stage-chips" role="group" aria-label="مرحلة الطلب">
        <button v-for="s in STAGES" :key="s" type="button" class="fpp-stage" :aria-pressed="stage === s" @click="stage = s">
          {{ STAGE_LABELS[s] }} <strong>{{ countOf(s) }}</strong>
        </button>
      </div>
      <label class="fpp-field">
        <span>المروّج</span>
        <select v-model="userId" class="dfb-search">
          <option :value="null">كل المروّجين</option>
          <option v-for="p in promoters" :key="p.id" :value="p.id">{{ p.fullName }}</option>
        </select>
      </label>
      <label class="fpp-field">
        <span>من</span>
        <input v-model="from" class="dfb-search" type="date" :max="to || today">
      </label>
      <label class="fpp-field">
        <span>إلى</span>
        <input v-model="to" class="dfb-search" type="date" :min="from" :max="today">
      </label>
    </section>

    <p v-if="orders.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="orders.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(orders.error.value, 'تعذر تحميل الطلبات.') }}
      <button type="button" class="dfb-button is-small" @click="orders.refetch()">إعادة المحاولة</button>
    </div>
    <section v-else class="fpp-card">
      <p v-if="!rows.length" class="dfb-state">لا توجد طلبات {{ STAGE_LABELS[stage] }}.</p>
      <div v-else class="fpp-table-wrap">
        <table class="fpp-table">
          <thead>
            <tr>
              <th>الزبون</th>
              <th>الهاتف</th>
              <th>المنتج</th>
              <th>المروّج</th>
              <th>الفرع</th>
              <th>الحالة</th>
              <th>التسديد</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in rows" :key="r.order.id" class="is-click" tabindex="0" @click="selected = r" @keydown.enter="selected = r">
              <td>
                <strong>{{ r.order.customerName }}</strong>
                <small class="dfb-cell-sub">{{ dateTime(r.order.createdAt) }}</small>
              </td>
              <td><span dir="ltr">{{ r.order.customerPhone }}</span></td>
              <td>{{ r.order.product }}</td>
              <td>{{ r.promoterName }}</td>
              <td>{{ r.order.branchName }}</td>
              <td><span class="fpp-chip" :class="`tone-${STAGE_TONES[r.order.stage]}`">{{ r.order.statusLabel }}</span></td>
              <td>
                <span v-if="r.order.sale" class="fpp-progress">
                  <span class="fpp-bar" :class="{ 'is-done': r.order.sale.percent >= 100 }"><span :style="{ inlineSize: `${Math.min(100, r.order.sale.percent)}%` }" /></span>
                  {{ num(r.order.sale.percent) }}%
                </span>
                <span v-else class="fpp-muted">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <nav v-if="page > 1 || hasMore" class="fpp-pager" aria-label="الصفحات">
        <button type="button" class="dfb-button is-small" :disabled="page <= 1" @click="page--">السابق</button>
        <span>صفحة {{ num(page) }}</span>
        <button type="button" class="dfb-button is-small" :disabled="!hasMore" @click="page++">التالي</button>
      </nav>
    </section>

    <NeuDialog
      :visible="selected != null"
      :title="selected ? `طلب ${selected.order.customerName}` : ''"
      :subtitle="selected ? `#${selected.order.id} · ${selected.promoterName} · ${dateTime(selected.order.createdAt)}` : undefined"
      size="lg"
      @close="selected = null"
    >
      <div v-if="selected" class="fpp">
        <dl class="fpp-kv">
          <div><dt>الحالة</dt><dd>{{ selected.order.statusLabel }}</dd></div>
          <div><dt>الهاتف</dt><dd dir="ltr">{{ selected.order.customerPhone }}</dd></div>
          <div><dt>المنتج</dt><dd>{{ selected.order.product }}</dd></div>
          <div><dt>الفرع</dt><dd>{{ selected.order.branchName }}</dd></div>
          <div><dt>العنوان</dt><dd>{{ selected.order.address || '—' }}</dd></div>
          <div><dt>ملاحظة</dt><dd>{{ selected.order.note || '—' }}</dd></div>
          <div v-if="selected.order.reason"><dt>السبب</dt><dd>{{ selected.order.reason }}</dd></div>
        </dl>

        <section v-if="selected.order.sale" class="fpp-tiles" aria-label="أرقام البيع">
          <div class="fpp-tile"><span>مبلغ البيع</span><strong>{{ money(selected.order.sale.total) }}</strong></div>
          <div class="fpp-tile"><span>الواصل</span><strong>{{ money(selected.order.sale.paid) }}</strong></div>
          <div class="fpp-tile"><span>المتبقي</span><strong>{{ money(selected.order.sale.remaining) }}</strong></div>
          <div class="fpp-tile"><span>عدد التسديدات</span><strong>{{ num(selected.order.sale.paymentsCount) }}</strong></div>
          <div class="fpp-tile"><span>آخر تسديد</span><strong>{{ dateLabel(selected.order.sale.lastPaymentDate) }}</strong></div>
          <div class="fpp-tile"><span>نسبة التسديد</span><strong>{{ num(selected.order.sale.percent) }}%</strong></div>
        </section>

        <div class="fpp-split">
          <section class="fpp-card">
            <h3 class="fpp-card-title">صورة المحل</h3>
            <p v-if="!selected.hasShopPhoto" class="dfb-state">لم تُرفع صورة للمحل.</p>
            <p v-else-if="photoLoading" class="dfb-state" role="status">جاري تحميل الصورة…</p>
            <p v-else-if="photoError" class="dfb-error" role="alert">تعذر تحميل الصورة.</p>
            <img v-else-if="photoUrl" :src="photoUrl" class="fpp-photo" alt="صورة المحل">
          </section>
          <section class="fpp-card">
            <h3 class="fpp-card-title">موقع المحل</h3>
            <p v-if="!shopMarker.length" class="dfb-state">لم يُسجَّل موقع المحل.</p>
            <div v-else class="fpp-map is-small">
              <FpMapCanvas :markers="shopMarker" label="موقع المحل" />
            </div>
          </section>
        </div>
      </div>
      <template #footer>
        <button type="button" class="dfb-button" @click="selected = null">إغلاق</button>
      </template>
    </NeuDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import FpMapCanvas, { type FpMarker } from '../components/FpMapCanvas.vue'
import {
  apiErrorMessage,
  fetchShopPhoto,
  useOrdersQuery,
  usePromotersLiveQuery,
  type AdminPromoterOrder,
  type OrderStage,
} from '../requests/fieldPromoters'
import { STAGE_LABELS, dateLabel, dateTime, escapeHtml, iraqToday, money, num } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'

const STAGES: OrderStage[] = ['inProgress', 'sold', 'finished', 'rejected']
const STAGE_TONES: Record<OrderStage, string> = { inProgress: 'warn', sold: 'info', finished: 'good', rejected: 'danger' }

const { branchId } = useFpNav()
const today = iraqToday()
const stage = ref<OrderStage>('inProgress')
const userId = ref<number | null>(null)
const from = ref('')
const to = ref('')
const page = ref(1)
const selected = ref<AdminPromoterOrder | null>(null)

watch([branchId, stage, userId, from, to], () => {
  page.value = 1
})

const live = usePromotersLiveQuery(branchId)
const promoters = computed(() => live.data.value?.promoters ?? [])
const orders = useOrdersQuery(
  computed(() => ({ branchId: branchId.value, userId: userId.value, stage: stage.value, from: from.value, to: to.value, page: page.value }))
)
const rows = computed(() => orders.data.value?.items ?? [])
const hasMore = computed(() => orders.data.value?.hasMore ?? false)

function countOf(s: OrderStage) {
  const c = orders.data.value?.counts
  return c ? num(c[s]) : '…'
}

const shopMarker = computed<FpMarker[]>(() => {
  const s = selected.value
  if (!s || s.shopLat == null || s.shopLng == null) return []
  return [{ key: 'shop', lat: s.shopLat, lng: s.shopLng, color: '#006a64', popupHtml: `<div dir="rtl">${escapeHtml(s.order.customerName)}</div>` }]
})

const photoUrl = ref<string | null>(null)
const photoLoading = ref(false)
const photoError = ref(false)
let photoToken = 0

function releasePhoto() {
  if (photoUrl.value) URL.revokeObjectURL(photoUrl.value)
  photoUrl.value = null
}

watch(selected, async (s) => {
  const token = ++photoToken
  releasePhoto()
  photoError.value = false
  photoLoading.value = false
  if (!s?.hasShopPhoto) return
  photoLoading.value = true
  try {
    const url = await fetchShopPhoto(s.order.id)
    if (token !== photoToken) {
      URL.revokeObjectURL(url)
      return
    }
    photoUrl.value = url
  } catch {
    if (token === photoToken) photoError.value = true
  } finally {
    if (token === photoToken) photoLoading.value = false
  }
})

onBeforeUnmount(() => {
  photoToken++
  releasePhoto()
})
</script>
