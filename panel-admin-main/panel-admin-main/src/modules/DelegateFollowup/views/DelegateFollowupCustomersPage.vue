<template>
  <div class="df-page" dir="rtl">
    <header class="df-head">
      <div>
        <h1 class="df-title">{{ $t('delegateFollowup.customersPage.title') }}</h1>
        <p class="df-subtitle">
          {{ $t('delegateFollowup.customersPage.list') }}
          <span class="df-num">#{{ listId }}</span>
          · {{ $t('delegateFollowup.branch.current') }}
          <span class="df-num">#{{ branchId }}</span>
        </p>
      </div>
      <button type="button" class="df-btn ghost" @click="goBackToBoard">
        {{ $t('delegateFollowup.customersPage.backBoard') }}
      </button>
    </header>

    <div v-if="!canRead || branchId == null || listId == null" class="df-error" role="alert">
      {{ $t('delegateFollowup.customersPage.missingContext') }}
    </div>

    <template v-else>
      <div v-if="customersQuery.isLoading.value" class="df-muted" role="status">
        {{ $t('delegateFollowup.customers.loading') }}
      </div>
      <div v-else-if="customersQuery.isError.value" class="df-error" role="alert">
        {{ $t('delegateFollowup.customers.error') }}
        <button type="button" class="df-btn ghost" @click="customersQuery.refetch()">
          {{ $t('delegateFollowup.lists.retry') }}
        </button>
      </div>
      <template v-else>
        <div class="df-filters" role="search">
          <label class="df-search">
            <i class="pi pi-search" aria-hidden="true" />
            <input
              v-model="search"
              type="search"
              :placeholder="$t('delegateFollowup.customers.search')"
            />
          </label>
          <div class="df-filter-chips" role="group">
            <button
              v-for="f in filters"
              :key="f.id"
              type="button"
              class="df-chip"
              :class="{ active: statusFilter === f.id }"
              :aria-pressed="statusFilter === f.id"
              @click="statusFilter = f.id"
            >
              {{ f.label }}
            </button>
          </div>
          <button type="button" class="df-btn ghost" @click="exportCsv">
            {{ $t('delegateFollowup.customersPage.export') }}
          </button>
        </div>

        <p class="df-muted">
          {{ $t('delegateFollowup.customersPage.count') }}:
          <span class="df-num">{{ filtered.length }}</span>
        </p>

        <div class="df-table-wrap">
          <table class="df-table">
            <thead>
              <tr>
                <th>{{ $t('delegateFollowup.customers.cols.profile') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.name') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.rating') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.saleDate') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.total') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.receipts') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.remaining') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.installment') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.lastPay') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.lastAmt') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.rate') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.count') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.days') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.address') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.phone') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.shop') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.collector') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.seller') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.items') }}</th>
                <th>{{ $t('delegateFollowup.customers.cols.payments') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in filtered" :key="c.customerId">
                <td>
                  <RouterLink
                    v-if="canAudit"
                    class="df-link"
                    :to="profileLink(c)"
                  >
                    {{ $t('delegateFollowup.customers.cols.profile') }}
                  </RouterLink>
                  <span v-else>—</span>
                </td>
                <td>{{ c.customerName }}</td>
                <td>
                  <span class="df-badge" :class="tierClass(c.realRatingLabel)">
                    {{ c.realRatingLabel || '—' }}
                  </span>
                </td>
                <td class="df-num">{{ c.dateSaleDevice ?? '—' }}</td>
                <td class="df-num">{{ fmtMoney(c.amountTotalSales) }}</td>
                <td class="df-num">{{ fmtMoney(c.receiptsTotal) }}</td>
                <td class="df-num">{{ fmtMoney(c.amountRemaining) }}</td>
                <td class="df-num">{{ fmtMoney(c.amountDaySales) }}</td>
                <td class="df-num">{{ c.lastPaymentDate ?? '—' }}</td>
                <td class="df-num">{{ c.lastPaymentAmount != null ? fmtMoney(c.lastPaymentAmount) : '—' }}</td>
                <td class="df-num">{{ c.receiptRateDevice }}</td>
                <td class="df-num">{{ c.countReceiptDevice }}</td>
                <td class="df-num">{{ c.numberOfDayDevice }}</td>
                <td>{{ c.address ?? '—' }}</td>
                <td class="df-num">{{ c.phoneNumber ?? '—' }}</td>
                <td>{{ c.shopName ?? '—' }}</td>
                <td>{{ c.receiptName ?? '—' }}</td>
                <td>{{ c.saleName ?? '—' }}</td>
                <td>{{ c.itemsNames ?? '—' }}</td>
                <td>
                  <button type="button" class="df-btn ghost small" @click="openPayments(c.customerId)">
                    {{ $t('delegateFollowup.customers.cols.payments') }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="!filtered.length" class="df-empty">
          {{ $t('delegateFollowup.customers.empty') }}
        </div>
      </template>
    </template>

    <Dialog
      v-model:visible="paymentsOpen"
      modal
      :header="$t('delegateFollowup.payments.title')"
      :style="{ width: 'min(560px, 94vw)' }"
      @hide="paymentsCustomerId = null"
    >
      <div v-if="paymentsQuery.isLoading.value" class="df-muted" role="status">
        {{ $t('delegateFollowup.payments.loading') }}
      </div>
      <div v-else-if="paymentsQuery.isError.value" class="df-error" role="alert">
        {{ $t('delegateFollowup.payments.error') }}
      </div>
      <template v-else-if="paymentsQuery.data.value">
        <p class="df-muted">
          {{ $t('delegateFollowup.payments.total') }}:
          <span class="df-num">{{ fmtMoney(paymentsQuery.data.value.totalAmount) }}</span>
        </p>
        <ul class="df-pay-list">
          <li v-for="p in paymentsQuery.data.value.payments" :key="p.paymentId">
            <span class="df-num">{{ p.date }}</span>
            <strong class="df-num">{{ fmtMoney(p.amount) }}</strong>
            <span class="df-muted">{{ p.description || '—' }}</span>
          </li>
        </ul>
        <p v-if="!paymentsQuery.data.value.payments.length" class="df-empty">
          {{ $t('delegateFollowup.payments.empty') }}
        </p>
      </template>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Dialog from 'primevue/dialog'
import { useAuthStore } from '@/modules/Auth/store'
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

const filters = [
  { id: 'all', label: 'الكل' },
  { id: 'ممتاز', label: 'ممتاز' },
  { id: 'جيد', label: 'جيد' },
  { id: 'ضعيف', label: 'ضعيف' },
  { id: 'متلكئ', label: 'متلكئ' },
  { id: 'سيء', label: 'سيء' },
  { id: 'legal', label: 'القانونية' },
]

const filtered = computed(() => {
  const q = search.value.trim().toLowerCase()
  return [...rows.value]
    .filter((c) => {
      if (statusFilter.value === 'all') {
        /* keep */
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

function openPayments(customerId: number) {
  paymentsCustomerId.value = customerId
  paymentsOpen.value = true
}

function fmtMoney(n: number) {
  return new Intl.NumberFormat('ar-IQ', { maximumFractionDigits: 0 }).format(n)
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
