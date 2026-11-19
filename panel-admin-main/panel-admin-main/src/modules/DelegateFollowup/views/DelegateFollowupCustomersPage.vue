<template>
  <div class="dfb" dir="rtl">
    <header class="dfb-head">
      <div>
        <h1 class="dfb-title">تفاصيل زبائن القائمة</h1>
        <p class="dfb-subtitle">غير المصفرين — اختر الفئة أو ابحث، واضغط اسم الزبون لفتح بروفايله</p>
      </div>
      <div class="dfb-actions">
        <button type="button" class="dfb-button" :disabled="!filtered.length" @click="exportCsv">تصدير CSV</button>
        <button type="button" class="dfb-button is-primary" @click="goBackToBoard">رجوع للوحة</button>
      </div>
    </header>

    <div v-if="!canRead || branchId == null || listId == null" class="dfb-error" role="alert">
      معرّف الفرع أو القائمة ناقص — افتح الصفحة من لوحة المتابعة
    </div>

    <template v-else>
      <p v-if="customersQuery.isLoading.value" class="dfb-state" role="status">جاري تحميل الزبائن…</p>
      <div v-else-if="customersQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل الزبائن
        <button type="button" class="dfb-button is-small" @click="customersQuery.refetch()">إعادة المحاولة</button>
      </div>
      <template v-else>
        <div class="dfb-kpis">
          <div class="dfb-kpi"><span>الزبائن المعروضون</span><strong><span class="dfb-num">{{ filtered.length }}</span></strong></div>
          <div class="dfb-kpi"><span>مجموع الباقي</span><strong><span class="dfb-num">{{ fmtMoney(totals.remaining) }}</span></strong></div>
          <div class="dfb-kpi"><span>مجموع القسط اليومي</span><strong><span class="dfb-num">{{ fmtMoney(totals.daily) }}</span></strong></div>
          <div class="dfb-kpi"><span>مجموع الواصل</span><strong><span class="dfb-num">{{ fmtMoney(totals.received) }}</span></strong></div>
        </div>

        <section class="dfb-section" aria-label="تصفية الزبائن">
          <div class="dfb-dialog-toolbar">
            <div class="dfb-filter-chips" role="group" aria-label="الفئة">
              <button
                v-for="o in statusOptions"
                :key="o.value"
                type="button"
                class="dfb-chip"
                :aria-pressed="statusFilter === o.value"
                @click="statusFilter = o.value"
              >
                {{ o.label }} <strong><span class="dfb-num">{{ countFor(o.value) }}</span></strong>
              </button>
            </div>
            <input v-model="search" class="dfb-search" type="search" placeholder="بحث بالاسم أو الهاتف أو المحل" aria-label="بحث في الزبائن">
          </div>
          <p v-if="!filtered.length" class="dfb-state">لا يوجد زبائن مطابقون للتصفية</p>
          <DelegateCustomersTable
            v-else
            :customers="filtered"
            :can-open-profile="canAudit"
            show-payments
            @profile="openProfileById"
            @payments="openPayments"
          />
        </section>
      </template>
    </template>

    <DfbDialog
      :visible="paymentsOpen"
      size="md"
      title="تسديدات الزبون"
      :subtitle="paymentsCustomerName"
      @close="closePayments"
    >
      <template v-if="paymentsQuery.data.value" #summary>
        <div class="dfb-kpi"><span>المجموع</span><strong><span class="dfb-num">{{ fmtMoney(paymentsQuery.data.value.totalAmount) }}</span></strong></div>
        <div class="dfb-kpi"><span>عدد التسديدات</span><strong><span class="dfb-num">{{ paymentsQuery.data.value.payments.length }}</span></strong></div>
      </template>
      <p v-if="paymentsQuery.isLoading.value" class="dfb-state" role="status">جاري تحميل التسديدات…</p>
      <p v-else-if="paymentsQuery.isError.value" class="dfb-error" role="alert">تعذر تحميل التسديدات</p>
      <template v-else-if="paymentsQuery.data.value">
        <p v-if="!paymentsQuery.data.value.payments.length" class="dfb-state">لا تسديدات</p>
        <div v-else class="dfb-panel">
          <div class="dfb-history">
            <div v-for="p in paymentsQuery.data.value.payments" :key="p.paymentId" class="dfb-history-row">
              <span class="dfb-history-day"><strong><span class="dfb-num">{{ p.date }}</span></strong></span>
              <span class="dfb-cell-sub">{{ p.description || '—' }}</span>
              <span class="dfb-history-value"><span class="dfb-num">{{ fmtMoney(p.amount) }}</span></span>
            </div>
          </div>
        </div>
      </template>
    </DfbDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import DfbDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import DelegateCustomersTable from '../components/DelegateCustomersTable.vue'
import '@/modules/Core/components/neumorphic/neumorphic.css'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useCustomersQuery } from '../requests/queries/useCustomersQuery'
import { useCustomerPaymentsQuery } from '../requests/queries/useCustomerPaymentsQuery'
import type { DelegateFollowupCustomer } from '../domain/types'
import {
  buildSafeReturnQuery,
  DELEGATE_FOLLOWUP_BOARD_ROUTE,
  DELEGATE_FOLLOWUP_CUSTOMERS_ROUTE,
  parseReturnContext,
} from '../domain/returnContext'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const { can } = usePermission()

const canRead = computed(() => can('DelegateFollowup', 'Read'))
const canAudit = computed(() => can('SalesRequest', 'ReadAuditProfile'))

const ctx = computed(() => parseReturnContext(route.query as Record<string, unknown>))
const branchId = computed(() => ctx.value?.branchId ?? null)
const listId = computed(() => ctx.value?.listId ?? null)

const statusFilter = ref<string>((route.query.filter as string) || 'all')
const search = ref(String(route.query.search ?? ''))

const customersQuery = useCustomersQuery(listId, branchId)
const rows = computed(() => customersQuery.data.value?.customers ?? [])

const statusOptions = [
  { value: 'all', label: 'الكل' },
  { value: 'ممتاز', label: 'ممتاز' },
  { value: 'جيد', label: 'جيد' },
  { value: 'ضعيف', label: 'ضعيف' },
  { value: 'متلكئ', label: 'متلكئ' },
  { value: 'سيء', label: 'سيء' },
  { value: 'lt100', label: 'باقي أقل من 100,000' },
  { value: '100to200', label: 'باقي 100,000 – أقل من 200,000' },
  { value: 'legal', label: 'القانونية' },
]

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  return [...rows.value]
    .filter((c) => {
      if (statusFilter.value === 'all') {
        if (c.isLegal) return false
      } else if (statusFilter.value === 'lt100') {
        if (c.isLegal || !(c.amountRemaining > 0 && c.amountRemaining < 100_000)) return false
      } else if (statusFilter.value === '100to200') {
        if (c.isLegal || !(c.amountRemaining >= 100_000 && c.amountRemaining < 200_000)) return false
      } else if (statusFilter.value === 'legal') {
        if (!c.isLegal) return false
      } else if (c.isLegal || c.realRatingLabel !== statusFilter.value) {
        return false
      }
      if (!q) return true
      return [c.customerName, c.phoneNumber, c.shopName]
        .filter((v): v is string => typeof v === 'string')
        .some((v) => v.toLowerCase().includes(q))
    })
    .sort((a, b) => {
      const ad = a.lastPaymentDate ?? ''
      const bd = b.lastPaymentDate ?? ''
      if (ad === bd) return a.customerName.localeCompare(b.customerName, 'ar')
      if (!ad) return 1
      if (!bd) return -1
      return bd.localeCompare(ad)
    })
})

watch([statusFilter, search], () => {
  void router.replace({
    query: {
      ...route.query,
      filter: statusFilter.value !== 'all' ? statusFilter.value : undefined,
      search: search.value.trim() || undefined,
    },
  })
})

onMounted(() => {
  const y = Number(route.query.scrollY)
  if (Number.isFinite(y) && y > 0) {
    requestAnimationFrame(() => window.scrollTo(0, y))
  }
})

function goBackToBoard() {
  if (branchId.value == null || listId.value == null) {
    void router.push({ name: DELEGATE_FOLLOWUP_BOARD_ROUTE })
    return
  }
  void router.push({
    name: DELEGATE_FOLLOWUP_BOARD_ROUTE,
    query: { branchId: String(branchId.value), listId: String(listId.value) },
  })
}

function profileLink(c: DelegateFollowupCustomer) {
  return {
    name: 'CustomerAuditProfileRoute',
    params: { id: String(c.customerId) },
    query: buildSafeReturnQuery({
      returnTo: DELEGATE_FOLLOWUP_CUSTOMERS_ROUTE,
      branchId: branchId.value!,
      listId: listId.value!,
      filter: statusFilter.value,
      search: search.value.trim() || undefined,
      scrollY: String(Math.round(window.scrollY)),
    }),
  }
}

const paymentsOpen = ref(false)
const paymentsCustomerId = ref<number | null>(null)
const paymentsEnabled = computed(() => paymentsOpen.value)
const paymentsQuery = useCustomerPaymentsQuery(
  listId,
  branchId,
  paymentsCustomerId,
  paymentsEnabled
)

const paymentsCustomerName = computed(() => rows.value.find(c => c.customerId === paymentsCustomerId.value)?.customerName ?? '')

function closePayments() {
  paymentsOpen.value = false
  paymentsCustomerId.value = null
}

function openProfileById(customerId: number) {
  const customer = rows.value.find(c => c.customerId === customerId)
  if (customer && canAudit.value) void router.push(profileLink(customer))
}

const totals = computed(() => ({
  remaining: filtered.value.reduce((s, c) => s + (c.amountRemaining || 0), 0),
  daily: filtered.value.reduce((s, c) => s + (c.amountDaySales || 0), 0),
  received: filtered.value.reduce((s, c) => s + (c.receiptsTotal || 0), 0),
}))

function matchesStatus(c: DelegateFollowupCustomer, value: string) {
  if (value === 'all') return c.isLegal === false
  if (value === 'lt100') return c.isLegal === false && c.amountRemaining > 0 && c.amountRemaining < 100_000
  if (value === '100to200') return c.isLegal === false && c.amountRemaining >= 100_000 && c.amountRemaining < 200_000
  if (value === 'legal') return c.isLegal === true
  return c.isLegal === false && c.realRatingLabel === value
}

function countFor(value: string) {
  return rows.value.filter(c => matchesStatus(c, value)).length
}

function openPayments(customerId: number) {
  paymentsCustomerId.value = customerId
  paymentsOpen.value = true
}

function fmtMoney(n: number) {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}

function tierClass(label: string) {
  if (label === 'قانونية') return 'tier-legal'
  if (label === 'ممتاز') return 'tier-excellent'
  if (label === 'جيد') return 'tier-good'
  if (label === 'ضعيف') return 'tier-weak'
  if (label === 'متلكئ') return 'tier-lagging'
  if (label === 'سيء') return 'tier-bad'
  return ''
}

function exportCsv() {
  const cols = [
    'customerId',
    'customerName',
    'realRatingLabel',
    'dateSaleDevice',
    'amountTotalSales',
    'receiptsTotal',
    'amountRemaining',
    'amountDaySales',
    'lastPaymentDate',
    'lastPaymentAmount',
    'phoneNumber',
    'shopName',
  ] as const
  const lines = [
    cols.join(','),
    ...filtered.value.map((c) =>
      cols
        .map((k) => {
          const v = c[k]
          const s = v == null ? '' : String(v)
          return `"${s.replaceAll('"', '""')}"`
        })
        .join(',')
    ),
  ]
  const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `delegate-followup-${listId.value}.csv`
  a.click()
  URL.revokeObjectURL(url)
}

void auth
</script>

<style scoped>
.df-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  color: #00201e;
  font-size: 16px;
}
.df-head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.df-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
}
.df-subtitle {
  margin: 0.35rem 0 0;
  color: #526b67;
  font-size: 14px;
}
.df-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: center;
}
.df-search {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  min-height: 44px;
  padding: 0 0.75rem;
  background: #f8f8f8;
  border: 1px solid #d7e2e0;
  border-radius: 10px;
  flex: 1 1 220px;
}
.df-search input {
  border: 0;
  background: transparent;
  width: 100%;
  font: inherit;
  outline: none;
}
.df-filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}
.df-chip,
.df-btn {
  min-height: 44px;
  padding: 0.4rem 0.9rem;
  border-radius: 12px;
  border: 1px solid #d7e2e0;
  background: #f8f8f8;
  font: inherit;
  cursor: pointer;
  color: #00201e;
}
.df-chip.active {
  background: #edf5f3;
  border-color: #006a64;
  color: #006a64;
}
.df-btn.ghost {
  background: #fff;
}
.df-btn.small {
  min-height: 40px;
  font-size: 14px;
}
.df-table-wrap {
  overflow-x: auto;
  border: 1px solid #d7e2e0;
  border-radius: 12px;
  max-width: 100%;
}
.df-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}
.df-table th,
.df-table td {
  padding: 0.55rem 0.65rem;
  border-bottom: 1px solid #e3ebe8;
  text-align: start;
  white-space: nowrap;
}
.df-table thead th {
  position: sticky;
  top: 0;
  background: #f3f7f6;
  z-index: 1;
}
.df-num {
  font-variant-numeric: tabular-nums;
}
.df-badge {
  display: inline-block;
  padding: 0.15rem 0.45rem;
  border-radius: 8px;
  background: #edf5f3;
}
.tier-legal {
  background: #fcebea;
  color: #963d35;
}
.df-link {
  color: #006a64;
}
.df-muted {
  color: #526b67;
  font-size: 14px;
}
.df-error {
  padding: 0.85rem;
  background: #fcebea;
  color: #963d35;
  border-radius: 12px;
}
.df-empty {
  padding: 1rem;
  color: #526b67;
}
.df-pay-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.df-pay-list li {
  display: grid;
  grid-template-columns: 7rem 1fr 1fr;
  gap: 0.5rem;
  min-height: 44px;
  align-items: center;
  border-bottom: 1px solid #e3ebe8;
}
</style>
