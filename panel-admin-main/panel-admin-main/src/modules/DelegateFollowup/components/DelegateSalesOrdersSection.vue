<template>
  <section class="dfb-section" aria-labelledby="dfb-orders-title">
    <div class="dfb-section-head">
      <h2 id="dfb-orders-title" class="dfb-section-title">طلبات بيع المندوب</h2>
      <button v-if="data && data.total" type="button" class="dfb-button is-small" @click="open('all')">
        عرض كل الطلبات
      </button>
    </div>

    <p v-if="query.isPending.value" class="dfb-state" role="status">جاري تحميل طلبات البيع…</p>
    <div v-else-if="query.isError.value" class="dfb-error" role="alert">
      تعذر تحميل طلبات البيع
      <button type="button" class="dfb-button is-small" @click="query.refetch()">إعادة المحاولة</button>
    </div>
    <p v-else-if="data && !data.total" class="dfb-state">لم يرسل مندوب هذه القائمة أي طلب بيع.</p>

    <template v-else-if="data">
      <p class="dfb-hint">
        نسبة التحويل <span class="dfb-num">{{ data.conversionPercent }}%</span> (من الطلبات التي حُسمت) — اضغط أي كونتينر لعرض الطلبات المحسوبة فيه
      </p>

      <div class="dfb-grid">
        <button type="button" class="dfb-tile" @click="open('all')">
          <span class="dfb-tile-label">كل الطلبات</span>
          <strong class="dfb-tile-value">{{ data.total }}</strong>
          <span class="dfb-tile-note">هذا الشهر: {{ data.ordersThisMonth }}</span>
        </button>
        <button type="button" class="dfb-tile tone-good" @click="open('sold')">
          <span class="dfb-tile-label">صارت مبيعات</span>
          <strong class="dfb-tile-value">{{ data.sold }}</strong>
          <span class="dfb-tile-note">قيمة المبيعات: {{ fmt(data.totalSoldValue) }}</span>
          <span v-if="data.averageDaysToSale != null" class="dfb-tile-note">معدل أيام البيع: {{ data.averageDaysToSale }}</span>
        </button>
        <button type="button" class="dfb-tile tone-danger" @click="open('rejected')">
          <span class="dfb-tile-label">مرفوضة</span>
          <strong class="dfb-tile-value">{{ data.rejected }}</strong>
        </button>
        <button type="button" class="dfb-tile" @click="open('open')">
          <span class="dfb-tile-label">قيد العمل</span>
          <strong class="dfb-tile-value">{{ data.open }}</strong>
        </button>
        <button type="button" class="dfb-tile tone-primary" @click="open('newAll')">
          <span class="dfb-tile-label">مبيعات جديدة (كل الفترة)</span>
          <strong class="dfb-tile-value">{{ data.newCustomerSales }}</strong>
          <span class="dfb-tile-note">زبائن غير موجودين سابقاً بالاسم الثلاثي أو الهاتف</span>
        </button>
        <button type="button" class="dfb-tile tone-primary" @click="open('newMonth')">
          <span class="dfb-tile-label">مبيعات جديدة هذا الشهر</span>
          <strong class="dfb-tile-value">{{ data.newCustomerSalesThisMonth }}</strong>
          <span class="dfb-tile-note">كل مبيعات الشهر: {{ data.soldThisMonth }}</span>
        </button>
        <button type="button" class="dfb-tile" @click="open('old')">
          <span class="dfb-tile-label">مبيعات لزبائن قدامى</span>
          <strong class="dfb-tile-value">{{ data.oldCustomerSales }}</strong>
        </button>
        <button type="button" class="dfb-tile" :class="{ 'tone-danger': data.wrongSales > 0 }" @click="open('wrong')">
          <span class="dfb-tile-label">مبيعات خاطئة</span>
          <strong class="dfb-tile-value">{{ data.wrongSales }}</strong>
          <span v-if="data.averageShopRating != null" class="dfb-tile-note">معدل تقييم المحل: {{ data.averageShopRating }} / 10</span>
        </button>
      </div>

      <h3 class="dfb-subtitle-sm">تقييم موظف المبيعات للطلبات</h3>
      <div class="dfb-chips">
        <button v-for="e in data.evaluations" :key="`ev-${e.key}`" type="button" class="dfb-chip" @click="open(`eval:${e.key}`)">
          {{ e.label }}: <strong>{{ e.count }}</strong>
        </button>
      </div>

      <h3 class="dfb-subtitle-sm">تقييم التسديد الحالي للمبيعات</h3>
      <div class="dfb-chips">
        <button v-for="p in data.paymentRatings" :key="`pay-${p.key}`" type="button" class="dfb-chip" :class="`tier-${p.key}`" @click="open(`pay:${p.key}`)">
          {{ p.label }}: <strong>{{ p.count }}</strong>
        </button>
      </div>

      <h3 class="dfb-subtitle-sm">آخر الطلبات</h3>
      <div class="dfb-table-wrap">
        <table class="dfb-table">
          <thead>
            <tr>
              <th scope="col">التاريخ</th>
              <th scope="col">الزبون</th>
              <th scope="col">الحالة</th>
              <th scope="col">تقييم الموظف</th>
              <th scope="col">نوع الزبون</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="o in preview" :key="`pv-${o.requestId}`">
              <td class="nowrap"><span class="dfb-num">{{ o.createdAtIraq ?? '—' }}</span></td>
              <td>{{ o.customerName }}</td>
              <td><span class="dfb-status" :class="`is-${o.outcome}`">{{ o.statusLabel }}</span></td>
              <td>{{ o.evaluationLabel }}</td>
              <td>{{ customerKind(o) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <DfbDialog
      :visible="dialogKey !== null"
      size="xl"
      :title="dialogTitle"
      :subtitle="`${listName} · ${dialogRows.length} طلب`"
      @close="dialogKey = null"
    >
      <template #toolbar>
        <input v-model="search" class="dfb-search" type="search" placeholder="بحث بالاسم أو الهاتف أو رقم الطلب" aria-label="بحث في الطلبات">
      </template>
      <p v-if="!dialogRows.length" class="dfb-state">لا توجد طلبات</p>
      <div v-else class="dfb-panel has-cards">
        <table class="dfb-ctable">
          <thead>
            <tr>
              <th scope="col">الطلب</th>
              <th scope="col">الزبون</th>
              <th scope="col">الحالة</th>
              <th scope="col">التقييمات</th>
              <th scope="col">البيع</th>
              <th scope="col">نوع الزبون</th>
              <th scope="col">الموظف</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="o in dialogRows" :key="`dl-${o.requestId}`">
              <td class="is-num">
                <span class="dfb-cell-label">الطلب</span>
                <span class="dfb-cell-main"><span class="dfb-num">#{{ o.requestId }}</span></span>
                <span class="dfb-cell-sub"><span class="dfb-num">{{ o.createdAtIraq ?? '—' }}</span></span>
              </td>
              <td class="is-wide">
                <a v-if="o.customerId && canOpenProfile" :href="profileHref(o.customerId)" target="_blank" rel="noopener noreferrer" class="dfb-name-link">{{ o.customerName }}</a>
                <span v-else class="dfb-cell-main">{{ o.customerName }}</span>
                <span class="dfb-cell-sub"><span class="dfb-num">{{ o.customerPhone || '—' }}</span><template v-if="o.isDeclaredOld"> · مبيع قديم</template></span>
              </td>
              <td>
                <span class="dfb-cell-label">الحالة</span>
                <span class="dfb-status" :class="`is-${o.outcome}`">{{ o.statusLabel }}</span>
                <span v-if="o.rejectionReason" class="dfb-cell-sub dfb-tier-bad">{{ o.rejectionReason }}</span>
              </td>
              <td>
                <span class="dfb-cell-label">التقييمات</span>
                <span class="dfb-cell-sub">الموظف: {{ o.evaluationLabel }}</span>
                <span class="dfb-cell-sub">
                  المحل: <span class="dfb-num">{{ o.shopRating != null ? `${o.shopRating}/10` : '—' }}</span>
                  <span v-if="o.isWrongSale" class="dfb-status is-rejected">خاطئ</span>
                </span>
              </td>
              <td class="is-num">
                <span class="dfb-cell-label">البيع</span>
                <span class="dfb-cell-main"><span class="dfb-num">{{ o.soldAtIraq ?? '—' }}</span></span>
                <span class="dfb-cell-sub">{{ o.finalSalePrice != null ? fmt(o.finalSalePrice) : '—' }}</span>
              </td>
              <td>
                <span class="dfb-cell-label">نوع الزبون</span>
                <span class="dfb-cell-sub">{{ customerKind(o) }}</span>
                <span v-if="o.paymentRatingLabel" class="dfb-badge" :class="`tier-${o.paymentRatingLabel}`">{{ o.paymentRatingLabel }}</span>
              </td>
              <td>
                <span class="dfb-cell-label">الموظف</span>
                <span class="dfb-cell-sub">{{ o.targetEmployeeName || '—' }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </DfbDialog>
  </section>
</template>

<script setup lang="ts">
import { computed, ref, toRef } from 'vue'
import DfbDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { useRouter } from 'vue-router'
import { useSalesOrdersQuery } from '../requests/queries/useSalesOrdersQuery'
import type { DelegateSalesOrder } from '../domain/types'

const props = defineProps<{
  branchId: number
  listId: number
  listName: string
  canOpenProfile: boolean
  today: string
}>()

const router = useRouter()
const query = useSalesOrdersQuery(toRef(props, 'listId'), toRef(props, 'branchId'))
const data = computed(() => query.data.value ?? null)
const PREVIEW_COUNT = 5
const preview = computed(() => (data.value?.orders ?? []).slice(0, PREVIEW_COUNT))
const dialogKey = ref<string | null>(null)
const search = ref('')

const monthPrefix = computed(() => props.today.slice(0, 7))
const inMonth = (d: string | null) => d != null && d.slice(0, 7) === monthPrefix.value

const FILTERS: Record<string, { title: string; fn: (o: DelegateSalesOrder) => boolean }> = {
  all: { title: 'كل طلبات البيع', fn: () => true },
  sold: { title: 'صارت مبيعات', fn: o => o.outcome === 'sold' },
  rejected: { title: 'مرفوضة', fn: o => o.outcome === 'rejected' },
  open: { title: 'قيد العمل', fn: o => o.outcome === 'open' },
  newAll: { title: 'مبيعات جديدة (كل الفترة)', fn: o => o.outcome === 'sold' && o.isNewCustomer === true },
  newMonth: { title: 'مبيعات جديدة هذا الشهر', fn: o => o.outcome === 'sold' && o.isNewCustomer === true && inMonth(o.soldAtIraq) },
  old: { title: 'مبيعات لزبائن قدامى', fn: o => o.outcome === 'sold' && o.isNewCustomer === false },
  wrong: { title: 'مبيعات خاطئة (تقييم محل منخفض)', fn: o => o.isWrongSale },
}

function filterFor(key: string) {
  if (FILTERS[key]) return FILTERS[key]
  if (key.startsWith('eval:')) {
    const level = Number(key.slice(5))
    const label = data.value?.evaluations.find(e => e.key === String(level))?.label ?? ''
    return { title: `تقييم موظف المبيعات: ${label}`, fn: (o: DelegateSalesOrder) => o.evaluationLevel === level }
  }
  if (key.startsWith('pay:')) {
    const label = key.slice(4)
    return { title: `تقييم التسديد الحالي: ${label}`, fn: (o: DelegateSalesOrder) => o.outcome === 'sold' && o.paymentRatingLabel === label }
  }
  return FILTERS.all
}

const dialogTitle = computed(() => filterFor(dialogKey.value ?? 'all').title)
const dialogRows = computed(() => {
  if (!data.value || dialogKey.value === null) return []
  const fn = filterFor(dialogKey.value).fn
  const q = search.value.trim()
  return data.value.orders.filter(fn).filter(o => !q
    || o.customerName.includes(q)
    || (o.customerPhone ?? '').includes(q)
    || String(o.requestId) === q)
})

function open(key: string) {
  search.value = ''
  dialogKey.value = key
}

function fmt(n: number) {
  return Number.isFinite(n) ? new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n) : '—'
}

function customerKind(o: DelegateSalesOrder) {
  if (o.outcome !== 'sold') return '—'
  if (o.isNewCustomer === true) return 'زبون جديد'
  if (o.isNewCustomer === false) return `قديم — ${o.oldCustomerReason ?? ''}${o.matchedCustomerName ? ` (${o.matchedCustomerName})` : ''}`
  return '—'
}

function profileHref(customerId: number) {
  return router.resolve({
    name: 'CustomerAuditProfileRoute',
    params: { id: customerId },
    query: { returnTo: 'ManagementDirectorDelegateFollowupRoute', branchId: String(props.branchId), listId: String(props.listId) },
  }).href
}
</script>
