<template>
  <div class="pay-rows" dir="rtl">
    <div class="q-filters pay-rows__filters">
      <AppSelect
        v-model="selectedListId"
        label="القائمة"
        placeholder="اختر القائمة"
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

    <DataState
      v-if="selectedListId == null || selectedListId <= 0"
      kind="empty"
      title="اختر قائمة"
      message="اختر القائمة ثم حدد الفترة لعرض تسديدات الزبائن."
    />
    <template v-else>
      <CustomDataTable
        :data="rows"
        :columns="columns"
        :rows="pageSize"
        :pageSize="pageSize"
        paginatorEnabled
        :totalRecords="totalCount"
        tableStyle="min-width: 100%"
        class="custom-table custom-border mt-4"
        :loading="isLoading"
        @page-change="onPageChange"
        @sort-change="handleSortChange"
      >
        <template #paginator>
          <div class="inline rounded-xl bg-white border-line border py-2 px-4">
            <span class="text-gray-700">{{ $t('payment.payments') }}: </span>
            <span>{{ totalCount }}</span>
          </div>
          <div class="inline rounded-xl bg-white border-line border py-2 px-4 mx-2">
            <span class="text-gray-700">{{ $t('payment.table.amount') }}: </span>
            <span>{{ pageTotalFormatted }}</span>
            <span class="text-gray-700 text-sm pr-1">{{ $t('words.dinar') }}</span>
          </div>
        </template>

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
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { AppDateField, AppSelect, DataState } from '@/modules/Core/components/ui'
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

const pageSize = ref(10)
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

const listOptions = computed(() => {
  const rows = listsData.value?.items ?? []
  return rows.map((l) => {
    const row = l as { id: number; name?: string; orderListName?: string }
    return {
      label: row.name ?? row.orderListName ?? `#${row.id}`,
      value: row.id,
    }
  })
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
  get(): number | null {
    const raw = route.query.orderListId
    const n = Number(Array.isArray(raw) ? raw[0] : raw)
    return Number.isFinite(n) && n > 0 ? n : null
  },
  set(v: string | number | null | boolean) {
    const id = typeof v === 'number' && Number.isFinite(v) ? v : null
    const query = { ...route.query }
    if (id == null || id <= 0) delete query.orderListId
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
  [listOptions, () => route.query.orderListId, () => route.query.startDate, () => route.query.endDate],
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
    const currentList = Number(Array.isArray(query.orderListId) ? query.orderListId[0] : query.orderListId)
    if ((!Number.isFinite(currentList) || currentList <= 0) && listOptions.value[0]) {
      query.orderListId = String(listOptions.value[0].value)
      changed = true
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
const orderListIdRef = computed(() => selectedListId.value ?? 0)

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

const pageTotalFormatted = computed(() =>
  formatMoney(
    rows.value.reduce((sum, row) => sum + (Number(row.amount) || 0), 0),
  ),
)

const columns = computed<Column<AllowedTypes>[]>(() => [
  {
    field: 'customerFullName',
    header: t('payment.table.customerFullName'),
    sortable: true,
  },
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
])

function formatAmount(value: string | number | null | undefined) {
  const n = Number(value)
  return Number.isFinite(n) ? formatMoney(n) : String(value ?? '—')
}

function onPageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
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
