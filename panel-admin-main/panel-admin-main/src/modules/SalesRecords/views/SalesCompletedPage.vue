<template>
  <AppPage
    title="المبيعات المنجزة"
    subtitle="Orders معتمدة فقط · تاريخ البيع = SaleDate · الواصل من دفعات البيع المحدد · المبالغ بـ د.ع وأرقام إنجليزية"
  >
    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="غير مخول بقراءة المبيعات (Order.Read)."
    />

    <template v-else>
      <div class="q-filters">
        <AppSearchField
          v-model="searchModel"
          label="بحث"
          placeholder="زبون / هاتف / بائع / رقم بيع"
          grow
        />
        <AppSelect
          v-model="branchModel"
          label="المحافظة / الفرع"
          placeholder="كل الفروع المسموحة"
          :options="branchOptions"
        />
        <AppInput
          v-model="sellerModel"
          label="البائع (رقم)"
          type="number"
          placeholder="اختياري"
        />
        <AppDateField v-model="fromModel" label="من تاريخ بيع" />
        <AppDateField v-model="toModel" label="إلى تاريخ بيع" />
        <AppSelect v-model="sortModel" label="ترتيب" :options="sortOptions" />
      </div>

      <DataState
        v-if="query.isPending.value"
        kind="loading"
        title="جاري التحميل"
        message="جاري جلب المبيعات المنجزة."
      />
      <DataState
        v-else-if="query.isError.value"
        kind="error"
        title="تعذر التحميل"
        message="تعذر تحميل المبيعات."
      >
        <template #actions>
          <AppButton variant="secondary" @click="query.refetch()">إعادة المحاولة</AppButton>
        </template>
      </DataState>

      <template v-else-if="data">
        <div class="q-summary" role="status">
          <div class="q-summary__item">
            <span class="q-summary__label">عدد النتائج</span>
            <strong class="q-summary__value">{{ formatNumber(data.totalCount) }}</strong>
          </div>
          <div class="q-summary__item">
            <span class="q-summary__label">إجمالي البيع</span>
            <strong class="q-summary__value">{{ formatMoney(data.totals.totalSellAmount) }}</strong>
          </div>
          <div class="q-summary__item">
            <span class="q-summary__label">الواصل</span>
            <strong class="q-summary__value">{{ formatMoney(data.totals.totalReceipts) }}</strong>
          </div>
          <div class="q-summary__item">
            <span class="q-summary__label">الباقي</span>
            <strong class="q-summary__value">{{ formatMoney(data.totals.totalRemaining) }}</strong>
          </div>
          <p v-if="data.totals.scopeNote" class="q-summary__note">{{ data.totals.scopeNote }}</p>
        </div>

        <div class="q-table-wrap sr-desktop">
          <table class="q-table">
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
                  <strong>#{{ formatNumber(row.orderId) }}</strong>
                  <span v-if="row.linkedSalesRequestId" class="q-muted">
                    · طلب #{{ formatNumber(row.linkedSalesRequestId) }}
                  </span>
                </td>
                <td class="q-num">{{ row.saleDate || '—' }}</td>
                <td>
                  <AppButton
                    v-if="canProfile"
                    variant="ghost"
                    size="sm"
                    class="sr-link"
                    @click="openCustomer(row)"
                  >
                    {{ row.customerName }}
                  </AppButton>
                  <strong v-else>{{ row.customerName }}</strong>
                  <div v-if="row.customerPhone" class="q-muted q-num" dir="ltr">
                    {{ row.customerPhone }}
                  </div>
                </td>
                <td>
                  {{
                    row.sellerName ||
                    (row.sellerId ? `#${formatNumber(row.sellerId)}` : '—')
                  }}
                </td>
                <td>{{ row.branchName }}</td>
                <td class="q-num">{{ formatMoney(row.sellAmount) }}</td>
                <td class="q-num">{{ formatMoney(row.prepaymentAmount) }}</td>
                <td class="q-num">{{ formatMoney(row.dailyInstallmentAmount) }}</td>
                <td class="q-num">{{ formatMoney(row.receiptsTotal) }}</td>
                <td class="q-num">{{ formatMoney(row.remainingAmount) }}</td>
                <td>
                  <StatusBadge :label="statusLabel(row.approvalStatus)" tone="success" />
                </td>
                <td>
                  <AppButton variant="secondary" size="sm" @click="openSale(row)">
                    تفاصيل
                  </AppButton>
                </td>
              </tr>
            </tbody>
          </table>
          <DataState
            v-if="!data.items.length"
            kind="empty"
            title="لا نتائج"
            message="لا مبيعات مطابقة (صفر فعلي)."
          />
        </div>

        <div class="sr-mobile" aria-label="تفاصيل الجوال">
          <article v-for="row in data.items" :key="'m-' + row.orderId" class="q-panel sr-card">
            <header class="sr-card__head">
              <strong>#{{ formatNumber(row.orderId) }}</strong>
              <StatusBadge :label="statusLabel(row.approvalStatus)" tone="success" />
            </header>
            <dl class="sr-card__dl">
              <div>
                <dt>التاريخ</dt>
                <dd class="q-num">{{ row.saleDate || '—' }}</dd>
              </div>
              <div>
                <dt>الزبون</dt>
                <dd>
                  <AppButton variant="ghost" size="sm" @click="openCustomer(row)">
                    {{ row.customerName }}
                  </AppButton>
                </dd>
              </div>
              <div>
                <dt>البائع</dt>
                <dd>{{ row.sellerName || '—' }}</dd>
              </div>
              <div>
                <dt>الفرع</dt>
                <dd>{{ row.branchName }}</dd>
              </div>
              <div>
                <dt>سعر البيع</dt>
                <dd class="q-num">{{ formatMoney(row.sellAmount) }}</dd>
              </div>
              <div>
                <dt>الواصل</dt>
                <dd class="q-num">{{ formatMoney(row.receiptsTotal) }}</dd>
              </div>
              <div>
                <dt>الباقي</dt>
                <dd class="q-num">{{ formatMoney(row.remainingAmount) }}</dd>
              </div>
            </dl>
            <AppButton variant="primary" block @click="openSale(row)">تفاصيل البيع</AppButton>
          </article>
        </div>

        <div class="q-pager">
          <AppButton variant="secondary" :disabled="page <= 1" @click="page = page - 1">
            السابق
          </AppButton>
          <span class="q-num">
            صفحة {{ formatNumber(data.page) }} · {{ formatNumber(data.pageSize) }} / صفحة
          </span>
          <AppButton
            variant="secondary"
            :disabled="page * pageSize >= data.totalCount"
            @click="page = page + 1"
          >
            التالي
          </AppButton>
        </div>
      </template>
    </template>
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import {
  AppButton,
  AppDateField,
  AppInput,
  AppPage,
  AppSearchField,
  AppSelect,
  DataState,
  StatusBadge,
} from '@/modules/Core/components/ui'
import { formatMoney, formatNumber } from '@/modules/Core/utils/formatDisplay'
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
    auth.hasPermission('Customer', 'Read') === true,
)
const branches = computed(() => auth.branches ?? [])
const branchOptions = computed(() =>
  branches.value.map((b) => ({ label: b.name, value: b.id })),
)
const sortOptions = [
  { label: 'تاريخ البيع', value: 'saleDate' },
  { label: 'سعر البيع', value: 'sellAmount' },
  { label: 'الزبون', value: 'customer' },
  { label: 'البائع', value: 'seller' },
  { label: 'رقم البيع', value: 'id' },
]

const branchModel = ref<number | null>(null)
const sellerModel = ref('')
const searchModel = ref('')
const fromModel = ref('')
const toModel = ref('')
const sortModel = ref<string>('saleDate')
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
    if (sortModel.value && sortModel.value !== 'saleDate') q.sort = String(sortModel.value)
    if (page.value > 1) q.page = String(page.value)
    void router.replace({ name: route.name ?? 'SaleCompletedRoute', params: route.params, query: q })
  },
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
    sort: typeof sortModel.value === 'string' ? sortModel.value : 'saleDate',
  }
})

const query = useSalesRecordsQuery(filters, canRead)
const data = computed(() => query.data.value)

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
      sort: sortModel.value !== 'saleDate' ? String(sortModel.value) : undefined,
      page: page.value > 1 ? String(page.value) : undefined,
    },
  })
}
</script>

<style scoped>
.q-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
  margin-bottom: 16px;
  padding: 16px;
  background: #fff;
  border: 1px solid #d5e2e0;
  border-radius: 16px;
}
.q-summary__label {
  display: block;
  font-size: 13px;
  color: #5a6b6a;
  margin-bottom: 4px;
}
.q-summary__value {
  font-size: 18px;
  color: #006a64;
  font-variant-numeric: lining-nums tabular-nums;
}
.q-summary__note {
  grid-column: 1 / -1;
  margin: 0;
  font-size: 13px;
  color: #5a6b6a;
}
.q-muted {
  color: #5a6b6a;
  font-size: 13px;
}
.q-num {
  font-variant-numeric: lining-nums tabular-nums;
}
.sr-link {
  padding-inline: 0 !important;
  min-height: auto !important;
  text-decoration: underline;
}
.sr-card__head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.sr-card__dl {
  margin: 0 0 14px;
}
.sr-card__dl > div {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 0;
  border-bottom: 1px solid #eef3f2;
}
.sr-card__dl dt {
  color: #5a6b6a;
}
.sr-card__dl dd {
  margin: 0;
  font-weight: 600;
}
.sr-mobile {
  display: none;
}
@media (max-width: 768px) {
  .sr-desktop {
    display: none;
  }
  .sr-mobile {
    display: grid;
    gap: 12px;
  }
}
</style>
