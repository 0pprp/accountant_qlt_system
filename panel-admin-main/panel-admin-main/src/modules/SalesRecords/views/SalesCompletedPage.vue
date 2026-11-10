<template>
  <div class="sr-page" dir="rtl">
    <header class="sr-head">
      <h1 class="sr-title">المبيعات المنجزة</h1>
      <p class="sr-sub">
        Orders معتمدة فقط · تاريخ البيع = SaleDate · الواصل من دفعات البيع المحدد · د.ع
      </p>
    </header>

    <div v-if="!canRead" class="sr-state sr-state--denied" role="alert">
      غير مخول بقراءة المبيعات (Order.Read).
    </div>

    <template v-else>
      <div class="sr-filters">
        <label class="sr-field sr-field--grow">
          <span>بحث</span>
          <div class="sr-search">
            <span class="sr-search__icon" aria-hidden="true">⌕</span>
            <input
              v-model="searchModel"
              class="sr-input"
              type="search"
              placeholder="زبون / هاتف / بائع / رقم بيع"
            />
          </div>
        </label>
        <label class="sr-field">
          <span>المحافظة / الفرع</span>
          <select v-model="branchModel" class="sr-select">
            <option :value="null">كل الفروع المسموحة</option>
            <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <label class="sr-field">
          <span>البائع (رقم)</span>
          <input v-model="sellerModel" class="sr-input" type="number" min="1" placeholder="اختياري" />
        </label>
        <label class="sr-field">
          <span>من تاريخ بيع</span>
          <input v-model="fromModel" class="sr-input" type="date" />
        </label>
        <label class="sr-field">
          <span>إلى تاريخ بيع</span>
          <input v-model="toModel" class="sr-input" type="date" />
        </label>
        <label class="sr-field">
          <span>ترتيب</span>
          <select v-model="sortModel" class="sr-select">
            <option value="saleDate">تاريخ البيع</option>
            <option value="sellAmount">سعر البيع</option>
            <option value="customer">الزبون</option>
            <option value="seller">البائع</option>
            <option value="id">رقم البيع</option>
          </select>
        </label>
      </div>

      <div v-if="query.isPending.value" class="sr-state" aria-busy="true">جاري التحميل…</div>
      <div v-else-if="query.isError.value" class="sr-state sr-state--error" role="alert">
        تعذر تحميل المبيعات.
        <button type="button" class="sr-btn" @click="query.refetch()">إعادة المحاولة</button>
      </div>

      <template v-else-if="data">
        <div class="sr-totals" role="status">
          <span>عدد النتائج: <b>{{ data.totalCount }}</b></span>
          <span>إجمالي البيع: <b>{{ money(data.totals.totalSellAmount) }}</b></span>
          <span>الواصل: <b>{{ money(data.totals.totalReceipts) }}</b></span>
          <span>الباقي: <b>{{ money(data.totals.totalRemaining) }}</b></span>
          <span class="sr-muted">{{ data.totals.scopeNote }}</span>
        </div>

        <div class="sr-table-wrap">
          <table class="sr-table">
            <thead>
              <tr>
                <th>البيع</th>
                <th>التاريخ</th>
                <th>الزبون</th>
                <th>البائع</th>
                <th>الفرع</th>
                <th>سعر البيع</th>
                <th>عربون</th>
                <th>قسط يومي</th>
                <th>الواصل</th>
                <th>الباقي</th>
                <th>الحالة</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in data.items" :key="row.orderId">
                <td>
                  <strong>#{{ row.orderId }}</strong>
                  <span v-if="row.linkedSalesRequestId" class="sr-muted">
                    · طلب #{{ row.linkedSalesRequestId }}
                  </span>
                </td>
                <td>{{ row.saleDate || '—' }}</td>
                <td>
                  <button
                    v-if="canProfile"
                    type="button"
                    class="sr-linkish"
                    @click="openCustomer(row)"
                  >
                    {{ row.customerName }}
                  </button>
                  <strong v-else>{{ row.customerName }}</strong>
                  <div class="sr-muted">{{ row.customerPhone || '' }}</div>
                </td>
                <td>{{ row.sellerName || (row.sellerId ? `#${row.sellerId}` : '—') }}</td>
                <td>{{ row.branchName }}</td>
                <td>{{ money(row.sellAmount) }}</td>
                <td>{{ money(row.prepaymentAmount) }}</td>
                <td>{{ money(row.dailyInstallmentAmount) }}</td>
                <td>{{ money(row.receiptsTotal) }}</td>
                <td>{{ money(row.remainingAmount) }}</td>
                <td>
                  <span class="sr-badge sr-badge--ok">{{ statusLabel(row.approvalStatus) }}</span>
                </td>
                <td>
                  <button type="button" class="sr-btn sr-btn--sm" @click="openSale(row)">تفاصيل</button>
                </td>
              </tr>
            </tbody>
          </table>
          <p v-if="!data.items.length" class="sr-state">لا مبيعات مطابقة (صفر فعلي).</p>
        </div>

        <div class="sr-mobile" aria-label="تفاصيل الجوال">
          <article v-for="row in data.items" :key="'m-' + row.orderId" class="sr-card">
            <header>
              <strong>#{{ row.orderId }}</strong>
              <span class="sr-badge sr-badge--ok">{{ statusLabel(row.approvalStatus) }}</span>
            </header>
            <dl>
              <div><dt>التاريخ</dt><dd>{{ row.saleDate || '—' }}</dd></div>
              <div>
                <dt>الزبون</dt>
                <dd>
                  <button type="button" class="sr-linkish" @click="openCustomer(row)">
                    {{ row.customerName }}
                  </button>
                </dd>
              </div>
              <div><dt>البائع</dt><dd>{{ row.sellerName || '—' }}</dd></div>
              <div><dt>الفرع</dt><dd>{{ row.branchName }}</dd></div>
              <div><dt>سعر البيع</dt><dd>{{ money(row.sellAmount) }}</dd></div>
              <div><dt>الواصل</dt><dd>{{ money(row.receiptsTotal) }}</dd></div>
              <div><dt>الباقي</dt><dd>{{ money(row.remainingAmount) }}</dd></div>
            </dl>
            <button type="button" class="sr-btn" @click="openSale(row)">تفاصيل البيع</button>
          </article>
        </div>

        <div class="sr-pager">
          <button type="button" class="sr-btn" :disabled="page <= 1" @click="page = page - 1">
            السابق
          </button>
          <span>صفحة {{ data.page }} · {{ data.pageSize }} / صفحة</span>
          <button
            type="button"
            class="sr-btn"
            :disabled="page * pageSize >= data.totalCount"
            @click="page = page + 1"
          >
            التالي
          </button>
        </div>
      </template>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import {
  useSalesRecordsQuery,
  type SalesRecordsFilters,
} from '../requests/queries/useSalesRecordsQuery'
import type { SalesRecordRow } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('Order', 'Read') === true)
const canProfile = computed(
  () =>
    auth.hasPermission('SalesRequest', 'ReadAuditProfile') === true ||
    auth.hasPermission('SalesRequest', 'MatchRead') === true ||
    auth.hasPermission('Customer', 'Read') === true
)
const branches = computed(() => auth.branches ?? [])

const branchModel = ref<number | null>(null)
const sellerModel = ref<string>('')
const searchModel = ref('')
const fromModel = ref('')
const toModel = ref('')
const sortModel = ref('saleDate')
const page = ref(1)
const pageSize = 20

function syncFromRoute() {
  const q = route.query
  const b = Number(q.branchId)
  branchModel.value = Number.isInteger(b) && b > 0 ? b : null
  const s = Number(q.sellerId)
  sellerModel.value = Number.isInteger(s) && s > 0 ? String(s) : ''
  searchModel.value = String(q.search ?? '')
  fromModel.value = String(q.saleDateFrom ?? '')
  toModel.value = String(q.saleDateTo ?? '')
  sortModel.value = String(q.sort ?? 'saleDate') || 'saleDate'
  const p = Number(q.page)
  page.value = Number.isInteger(p) && p > 0 ? p : 1
}
syncFromRoute()
watch(() => route.query, syncFromRoute)

watch(
  [branchModel, sellerModel, searchModel, fromModel, toModel, sortModel, page],
  () => {
    const q: Record<string, string> = {}
    if (branchModel.value) q.branchId = String(branchModel.value)
    if (sellerModel.value) q.sellerId = String(sellerModel.value)
    if (searchModel.value.trim()) q.search = searchModel.value.trim()
    if (fromModel.value) q.saleDateFrom = fromModel.value
    if (toModel.value) q.saleDateTo = toModel.value
    if (sortModel.value && sortModel.value !== 'saleDate') q.sort = sortModel.value
    if (page.value > 1) q.page = String(page.value)
    void router.replace({ name: route.name ?? 'SaleCompletedRoute', query: q })
  }
)

watch([branchModel, sellerModel, searchModel, fromModel, toModel, sortModel], () => {
  page.value = 1
})

const filters = computed<SalesRecordsFilters>(() => {
  const sid = Number(sellerModel.value)
  return {
    branchId: branchModel.value,
    sellerId: Number.isInteger(sid) && sid > 0 ? sid : null,
    search: searchModel.value,
    saleDateFrom: fromModel.value,
    saleDateTo: toModel.value,
    page: page.value,
    pageSize,
    sort: sortModel.value || 'saleDate',
  }
})

const query = useSalesRecordsQuery(filters, canRead)
const data = computed(() => query.data.value)

function money(v: number | null | undefined) {
  if (v == null || Number.isNaN(Number(v))) return '—'
  return `${Number(v).toLocaleString('ar-IQ')} د.ع`
}

function statusLabel(s: string) {
  if (s === 'Approved') return 'معتمد'
  return s || '—'
}

function openSale(row: SalesRecordRow) {
  void router.push({
    name: 'SaleViewRoute',
    params: { saleId: String(row.orderId) },
  })
}

function openCustomer(row: SalesRecordRow) {
  if (!canProfile.value) return
  void router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: String(row.customerId) },
    query: {
      returnTo: 'SaleCompletedRoute',
      branchId: branchModel.value ? String(branchModel.value) : undefined,
      sellerId: sellerModel.value || undefined,
      search: searchModel.value.trim() || undefined,
      saleDateFrom: fromModel.value || undefined,
      saleDateTo: toModel.value || undefined,
      sort: sortModel.value !== 'saleDate' ? sortModel.value : undefined,
      page: page.value > 1 ? String(page.value) : undefined,
    },
  })
}
</script>

<style scoped>
.sr-page {
  font-family: 'El Messiri', 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
  font-size: 16px;
  padding: 1rem 1.25rem 2rem;
  background: linear-gradient(180deg, #f8f8f8 0%, #eef5f4 100%);
  min-height: 100%;
}
.sr-title {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 700;
  color: #006a64;
}
.sr-sub {
  margin: 0.35rem 0 0;
  font-size: 14px;
  color: #3d524f;
}
.sr-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1.25rem 0 1rem;
  align-items: flex-end;
}
.sr-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 9rem;
}
.sr-field--grow {
  flex: 1 1 16rem;
  min-width: 14rem;
}
.sr-field span {
  font-size: 13px;
  color: #3d524f;
}
.sr-search {
  display: flex;
  align-items: stretch;
  background: #fff;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
  min-height: 44px;
}
.sr-search__icon {
  display: grid;
  place-items: center;
  width: 2.5rem;
  color: #006a64;
  background: #f8f8f8;
}
.sr-input,
.sr-select {
  min-height: 44px;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  padding: 0 0.75rem;
  background: #fff;
  font: inherit;
  color: inherit;
}
.sr-search .sr-input {
  border: 0;
  border-radius: 0;
  flex: 1;
}
.sr-totals {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem 1.25rem;
  margin-bottom: 0.75rem;
  padding: 0.75rem 1rem;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #d7e3e1;
}
.sr-muted {
  color: #5a6f6c;
  font-size: 13px;
}
.sr-table-wrap {
  overflow-x: auto;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #d7e3e1;
}
.sr-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 64rem;
}
.sr-table th,
.sr-table td {
  padding: 0.65rem 0.75rem;
  border-bottom: 1px solid #e6eeee;
  text-align: right;
  vertical-align: top;
}
.sr-table th {
  background: #f0f7f6;
  font-weight: 600;
  color: #006a64;
}
.sr-badge {
  display: inline-block;
  padding: 0.15rem 0.5rem;
  border-radius: 4px;
  font-size: 13px;
}
.sr-badge--ok {
  background: #d8f3e8;
  color: #0b6b4a;
}
.sr-linkish {
  background: none;
  border: 0;
  color: #006a64;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
  text-decoration: underline;
  padding: 0;
  min-height: 44px;
}
.sr-btn {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #006a64;
  color: #fff;
  font: inherit;
  cursor: pointer;
}
.sr-btn--sm {
  min-height: 36px;
  padding: 0 0.75rem;
  font-size: 14px;
}
.sr-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}
.sr-pager {
  display: flex;
  gap: 1rem;
  align-items: center;
  margin-top: 1rem;
}
.sr-state {
  padding: 1.25rem;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #d7e3e1;
}
.sr-state--denied,
.sr-state--error {
  border-color: #c45c5c;
  color: #7a1f1f;
}
.sr-mobile {
  display: none;
}
.sr-card {
  background: #fff;
  border: 1px solid #d7e3e1;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 0.75rem;
}
.sr-card header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}
.sr-card dl {
  margin: 0 0 0.75rem;
}
.sr-card dl > div {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.35rem 0;
  border-bottom: 1px solid #eef3f2;
}
.sr-card dt {
  color: #5a6f6c;
}
.sr-card dd {
  margin: 0;
  font-weight: 600;
}
@media (max-width: 768px) {
  .sr-table-wrap {
    display: none;
  }
  .sr-mobile {
    display: block;
  }
}
@media (min-width: 769px) {
  .sr-mobile {
    display: none;
  }
}
</style>
