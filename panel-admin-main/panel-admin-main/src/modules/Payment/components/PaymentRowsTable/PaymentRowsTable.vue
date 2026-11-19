<template>
  <div class="pay-rows" dir="rtl">
    <div class="q-filters pay-rows__filters">
      <AppSelect
        v-model="selectedListId"
        label="القائمة"
        :placeholder="t('payment.selectList')"
        :options="listOptions"
        :filterable="listOptions.length > 6"
        :disabled="listsLoading"
        :empty-message="listsLoading ? 'جارٍ التحميل…' : 'لا توجد قوائم'"
      />
      <AppDateField
        v-model="fromDate"
        label="من"
      />
      <AppDateField
        v-model="toDate"
        label="إلى"
      />
    </div>

    <div class="pay-rows__cards" aria-live="polite">
      <div class="pay-rows__card">
        <span class="pay-rows__card-label">{{ $t('payment.table.amount') }}</span>
        <span class="pay-rows__card-value">
          {{ totalAmountFormatted }}
          <small>{{ $t('words.dinar') }}</small>
        </span>
      </div>
      <div class="pay-rows__card">
        <span class="pay-rows__card-label">{{ $t('payment.payments') }}</span>
        <span class="pay-rows__card-value">{{ totalCount }}</span>
      </div>
    </div>

    <CustomDataTable
      :data="rows"
      :columns="columns"
      :rows="pageSize"
      :pageSize="pageSize"
      :paginatorEnabled="false"
      :totalRecords="totalCount"
      tableStyle="min-width: 100%"
      class="custom-table custom-border mt-4"
      :loading="isLoading"
      @sort-change="handleSortChange"
    >
      <template #body-customerFullName="{ data }">
        <RouterLink
          v-if="data.customerId"
          class="pay-rows__customer"
          :to="{ name: 'CustomerAuditProfileRoute', params: { id: data.customerId } }"
        >
          {{ data.customerFullName || '—' }}
        </RouterLink>
        <span v-else>{{ data.customerFullName || '—' }}</span>
      </template>

      <template #body-orderListName="{ data }">
        {{ data.orderListName || '—' }}
      </template>

      <template #body-amount="{ data }">
        <span>{{ formatAmount(data.amount) }}</span>
        <span class="text-gray-700 px-1">{{ $t('words.dinar') }}</span>
      </template>

      <template #body-date="{ data }">
        {{ data.date ? formattedDate(data.date) : '—' }}
      </template>

      <template #body-sellerFullName="{ data }">
        {{ data.sellerFullName || '—' }}
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { AppDateField, AppSelect } from '@/modules/Core/components/ui'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { formattedDate } from '@/modules/Core/utils/time'
import { formatMoney } from '@/modules/Core/utils/formatDisplay'
import useAuthStore from '@/modules/Auth/store'
import { useOrderDataQuery } from '@/modules/Order/requests/queries'
import { usePaymentListDataQuery } from '@/modules/Payment/requests/queries'
import type { Payment } from '@/modules/Payment/types/model'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

/** عرض كل التسديدات دفعة واحدة بدون ترقيم صفحات */
const pageSize = ref(5000)
const pageIndex = ref(1)
const sortCriteria = ref<Array<{ property: string; direction: number }> | null>(null)

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)
const listsPageIndex = ref(1)
const listsPageSize = ref(200)
const { data: listsData, isLoading: listsLoading } = useOrderDataQuery(
  listsPageIndex,
  listsPageSize,
  branchId,
)

const ALL_LISTS_ID = 0

const listOptions = computed(() => {
  const rows = listsData.value?.items ?? []
  const options = rows.map((l) => {
    const row = l as { id: number; name?: string; orderListName?: string }
    return {
      label: row.name ?? row.orderListName ?? `#${row.id}`,
      value: row.id,
    }
  })
  return [{ label: t('payment.allLists'), value: ALL_LISTS_ID }, ...options]
})

function asDate(value: unknown): string {
  const raw = Array.isArray(value) ? value[0] : value
  if (typeof raw !== 'string') return ''
  const match = /^(\d{4}-\d{2}-\d{2})/.exec(raw)
  return match ? match[1] : ''
}

function todayIso() {
  return new Date().toISOString().slice(0, 10)
}

const selectedListId = computed({
  get(): number {
    const raw = route.query.orderListId
    if (raw == null || raw === '') return ALL_LISTS_ID
    const n = Number(Array.isArray(raw) ? raw[0] : raw)
    return Number.isFinite(n) && n >= 0 ? n : ALL_LISTS_ID
  },
  set(v: string | number | null | boolean) {
    const id = typeof v === 'number' && Number.isFinite(v) ? v : ALL_LISTS_ID
    const query = { ...route.query }
    if (id <= 0) delete query.orderListId
    else query.orderListId = String(id)
    pageIndex.value = 1
    void router.push({ query })
  },
})

const fromDate = computed({
  get: () => asDate(route.query.startDate),
  set(v: string) {
    const query = { ...route.query }
    if (!v) delete query.startDate
    else query.startDate = v
    pageIndex.value = 1
    void router.push({ query })
  },
})

const toDate = computed({
  get: () => asDate(route.query.endDate),
  set(v: string) {
    const query = { ...route.query }
    if (!v) delete query.endDate
    else query.endDate = v
    pageIndex.value = 1
    void router.push({ query })
  },
})

watch(
  [() => route.query.startDate, () => route.query.endDate],
  () => {
    const query = { ...route.query }
    let changed = false
    if (!asDate(query.startDate) || !asDate(query.endDate)) {
      const d = todayIso()
      if (!asDate(query.startDate)) {
        query.startDate = d
        changed = true
      }
      if (!asDate(query.endDate)) {
        query.endDate = d
        changed = true
      }
    }
    if (changed) void router.replace({ query })
  },
  { immediate: true },
)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

const startDateRef = computed(() => fromDate.value || null)
const endDateRef = computed(() => toDate.value || null)
const orderListIdRef = computed(() => selectedListId.value)
const showAllLists = computed(() => selectedListId.value <= 0)

watch(searchTerm, () => {
  pageIndex.value = 1
})

const { data, isLoading } = usePaymentListDataQuery(
  orderListIdRef,
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  startDateRef,
  endDateRef,
  sortCriteria,
)

const rows = computed<Payment[]>(() => {
  const items = data.value?.items
  return Array.isArray(items) ? items : []
})

const totalCount = computed(() => data.value?.totalCount ?? rows.value.length)

const totalAmountFormatted = computed(() =>
  formatMoney(
    rows.value.reduce((sum, row) => sum + (Number(row.amount) || 0), 0),
  ),
)

const columns = computed<Column<AllowedTypes>[]>(() => {
  const cols: Column<AllowedTypes>[] = [
    {
      field: 'customerFullName',
      header: t('payment.table.customerFullName'),
      sortable: true,
    },
  ]
  if (showAllLists.value) {
    cols.push({
      field: 'orderListName',
      header: t('payment.table.orderListName'),
      sortable: false,
    })
  }
  cols.push(
    {
      field: 'amount',
      header: t('payment.table.amount'),
      sortable: true,
    },
    {
      field: 'date',
      header: t('payment.table.date'),
      sortable: true,
    },
    {
      field: 'sellerFullName',
      header: t('payment.table.sellerFullName'),
      sortable: false,
    },
  )
  return cols
})

function formatAmount(value: string | number | null | undefined) {
  const n = Number(value)
  return Number.isFinite(n) ? formatMoney(n) : String(value ?? '—')
}

function handleSortChange(event: { sortField: string | null; sortOrder: 1 | -1 | 0 }) {
  if (event.sortField && event.sortOrder !== 0) {
    sortCriteria.value = [
      {
        property: event.sortField,
        direction: event.sortOrder === 1 ? 0 : 1,
      },
    ]
  } else {
    sortCriteria.value = null
  }
}
</script>

<style scoped>
.pay-rows__filters {
  margin-top: 1rem;
}
.pay-rows__cards {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
  margin-top: 1rem;
}
.pay-rows__card {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  padding: 1rem 1.1rem;
  border-radius: 1rem;
  border: 1px solid #e5e7eb;
  background: #fff;
}
.pay-rows__card-label {
  color: #4b5563;
  font-size: 0.9rem;
}
.pay-rows__card-value {
  color: #111827;
  font-size: 1.35rem;
  font-weight: 700;
  display: inline-flex;
  align-items: baseline;
  gap: 0.35rem;
}
.pay-rows__card-value small {
  color: #4b5563;
  font-size: 0.85rem;
  font-weight: 500;
}
@media (max-width: 640px) {
  .pay-rows__cards {
    grid-template-columns: 1fr;
  }
}
.pay-rows__customer {
  color: #006a64;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 2px;
}
.pay-rows__customer:hover {
  color: #004f4a;
}
</style>
