<template>
  <div>
    <SaleApprovalStatusTabs
      v-model="activeApprovalStatus"
      :pendingCount="pendingCount"
    />

    <div v-if="selectableColumns.length > 0" class="mb-4 mt-4">
      <ColumnSelectorDropdown
        v-model="optionalSelectedColumns"
        :options="selectableColumns"
        labelKey="sale.columnSelector.title"
        placeholderKey="sale.columnSelector.placeholder"
      />
    </div>

    <CustomDataTable
      :data="data?.paginatedOrders.items || []"
      v-model:selection="selectedSales"
      :columns="columns"
      :rows="pageSize"
      selectable
      :pageSize="pageSize"
      paginatorEnabled
      :totalRecords="data?.paginatedOrders.totalCount"
      tableStyle="min-width: 100%"
      class="custom-table custom-border mt-4"
      :loading="isLoading"
      @page-change="onPageChange"
      @sort-change="handleSortChange"
    >
      <template #paginator>
        <div class="flex items-center gap-4">
          <div class="rounded-xl border border-line bg-white px-4 py-2">
            <span class="text-gray-700">
              {{ $t('sale.table.totalBuyAmount') }}:
            </span>
            <span>{{ formattedPrice(data?.totalBuyAmount ?? 0) }}</span>
            <small class="text-gray-700 mr-1">
              {{ $t('words.dinar') }}
            </small>
          </div>

          <div class="rounded-xl border border-line bg-white px-4 py-2">
            <span class="text-gray-700">
              {{ $t('sale.table.totalSellAmount') }}:
            </span>
            <span>{{ formattedPrice(data?.totalSellAmount ?? 0) }}</span>
            <small class="text-gray-700 mr-1">
              {{ $t('words.dinar') }}
            </small>
          </div>
        </div>
      </template>

      <template #body-Id="{ data: rowData }">
        {{ getFieldValue(rowData, 'Id') }}
      </template>

      <template #body-CustomerPhoneNumber="{ data: rowData }">
        {{ getFieldValue(rowData, 'CustomerPhoneNumber') }}
      </template>

      <template #body-RemainingAmount="{ data: rowData }">
        <span>{{
          formatColumnValue(getFieldValue(rowData, 'RemainingAmount'), 'number')
        }}</span>
        <small class="text-gray-700 mr-1">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-LastInstallmentDate="{ data: rowData }">
        {{
          formatColumnValue(
            getFieldValue(rowData, 'LastInstallmentDate'),
            'date'
          )
        }}
      </template>

      <template #body-InstallmentsCount="{ data: rowData }">
        {{
          formatColumnValue(
            getFieldValue(rowData, 'InstallmentsCount'),
            'number'
          )
        }}
      </template>

      <template #body-TotalInstallmentsAmount="{ data: rowData }">
        <span>{{
          formatColumnValue(
            getFieldValue(rowData, 'TotalInstallmentsAmount'),
            'number'
          )
        }}</span>
        <small class="text-gray-700 mr-1">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-ApprovalStatus="{ data: rowData }">
        <span :class="getApprovalStatusBadgeClass(rowData)">
          {{ formatApprovalStatusDisplay(rowData) }}
        </span>
      </template>

      <template #body-ExecutionStatus="{ data: rowData }">
        <span :class="getExecutionStatusBadgeClass(rowData)">
          {{ formatExecutionStatusDisplay(rowData) }}
        </span>
      </template>

      <template #body-approvalActions="{ data: rowData }" v-if="canUpdateOrder">
        <OrderApprovalActions
          v-if="getSaleId(rowData)"
          :id="getSaleId(rowData)!"
          :item="rowData as Record<string, unknown>"
          :alwaysEnabled="activeApprovalStatus === OrderApprovalStatus.Pending"
        />
        <span v-else class="text-gray-400">-</span>
      </template>

      <!-- Dynamic column slots (excluding default columns which have explicit slots) -->
      <template
        v-for="column in nonDefaultDynamicColumns"
        :key="column.key"
        #[`body-${column.key}`]="{ data: rowData }"
      >
        <template v-if="column.key === 'ApprovalStatus'">
          <span :class="getApprovalStatusBadgeClass(rowData)">
            {{ formatApprovalStatusDisplay(rowData) }}
          </span>
        </template>
        <template v-else-if="column.key === 'ExecutionStatus'">
          <span :class="getExecutionStatusBadgeClass(rowData)">
            {{ formatExecutionStatusDisplay(rowData) }}
          </span>
        </template>
        <template v-else-if="column.dataType === 'currency'">
          <span>{{
            formatColumnValue(
              getFieldValue(rowData, column.key),
              column.dataType
            )
          }}</span>
          <small class="text-gray-700 mr-1">
            {{ $t('words.dinar') }}
          </small>
        </template>
        <template v-else-if="column.dataType === 'number'">
          {{
            formatColumnValue(
              getFieldValue(rowData, column.key),
              column.dataType
            )
          }}
        </template>
        <template
          v-else-if="
            column.dataType === 'date' || column.dataType === 'datetime'
          "
        >
          {{
            formatColumnValue(
              getFieldValue(rowData, column.key),
              column.dataType
            )
          }}
        </template>
        <template v-else>
          {{ getFieldValue(rowData, column.key) }}
        </template>
      </template>

      <template #body-edit="{ data: rowData }" v-if="canUpdateOrder">
        <Button class="text-gray-300" v-slot="slotProps">
          <RouterLink
            v-if="getSaleId(rowData)"
            :to="{
              name: 'SaleUpdateRoute',
              params: { saleId: String(getSaleId(rowData)) },
              query: {
                customerId: String(
                  getFieldValue(rowData, 'CustomerId') ||
                    getFieldValue(rowData, 'customerId') ||
                    ''
                ),
              },
            }"
            :class="slotProps.a11yAttrs"
          >
            <SvgIcon
              name="boldPencil"
              class="text-gray-300 hover:text-primary transition-colors"
            />
          </RouterLink>
          <span v-else class="text-gray-400">-</span>
        </Button>
      </template>

      <template #body-view="{ data: rowData }" v-if="canReadOrder">
        <Button class="text-gray-300" v-slot="slotProps">
          <RouterLink
            v-if="getSaleId(rowData)"
            :to="{
              name: 'SaleViewRoute',
              params: { saleId: String(getSaleId(rowData)) },
              query: {
                customerId: String(
                  getFieldValue(rowData, 'CustomerId') ||
                    getFieldValue(rowData, 'customerId') ||
                    ''
                ),
              },
            }"
            :class="slotProps.a11yAttrs"
          >
            <!-- TODO : CHANGE ICON -->
            <i
              class="pi pi-pen-to-square hover:text-info transition-colors"
            ></i>
          </RouterLink>
          <span v-else class="text-gray-400">-</span>
        </Button>
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import { computed, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import {
  useSaleDataQuery,
  useAvailableColumnsQuery,
} from '../../requests/queries'
import type { Sale } from '../../types/model'
import type { AvailableColumn } from '../../types/api'
import SaleApprovalStatusTabs from '../SaleApprovalStatusTabs/SaleApprovalStatusTabs.vue'
import OrderApprovalActions from '../OrderApprovalActions/OrderApprovalActions.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import { formattedPrice } from '@/modules/Core/utils/price'
import useAuthStore from '@/modules/Auth/store'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import ColumnSelectorDropdown from '@/modules/Core/components/shared/ColumnSelectorDropdown/ColumnSelectorDropdown.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import {
  formatApprovalStatusCell,
  formatExecutionStatusCell,
  getApprovalStatusFromRow,
  getExecutionStatusFromRow,
  migrateColumnKeys,
} from '@/modules/Order/utils/orderStatus'
import {
  OrderApprovalStatus,
  OrderExecutionStatus,
  OrderStep,
} from '@/modules/Order/types/model'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()

const STORAGE_KEY = 'sale-list-selected-columns'

const selectedSales = ref()

const { can } = usePermission()
const canReadOrder = can('Order', 'Read')
const canUpdateOrder = can('Order', 'Update')
const { data: availableColumnsData } = useAvailableColumnsQuery()
const DEFAULT_COLUMNS = [
  'Id',
  'CustomerPhoneNumber',
  'RemainingAmount',
  'LastInstallmentDate',
  'InstallmentsCount',
  'TotalInstallmentsAmount',
]

const INTERNAL_COLUMNS = ['ApprovalStatus', 'Step']

function parseApprovalStatusFromQuery(): OrderApprovalStatus {
  const queryValue = route.query.approvalStatus
  if (queryValue === undefined) {
    return OrderApprovalStatus.Pending
  }

  const parsed = Number(Array.isArray(queryValue) ? queryValue[0] : queryValue)
  if (
    parsed === OrderApprovalStatus.Pending ||
    parsed === OrderApprovalStatus.Approved ||
    parsed === OrderApprovalStatus.Rejected
  ) {
    return parsed
  }

  return OrderApprovalStatus.Pending
}

const activeApprovalStatus = ref<OrderApprovalStatus>(
  parseApprovalStatusFromQuery()
)

const stepFilter = computed(() =>
  activeApprovalStatus.value === OrderApprovalStatus.Pending
    ? OrderStep.Completed
    : null
)

const pendingCountStepFilter = computed(() => OrderStep.Completed)

const selectableColumns = computed<Array<AvailableColumn>>(() => {
  if (!availableColumnsData.value) return []
  return availableColumnsData.value.filter(
    (col) => !DEFAULT_COLUMNS.includes(col.key)
  )
})

function loadSelectedColumns(): Array<string> | null {
  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored) {
      return migrateColumnKeys(JSON.parse(stored) as Array<string>)
    }
  } catch {
    // Silently fail if localStorage is not available
  }
  return null
}

function saveSelectedColumns(columns: Array<string>): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(columns))
  } catch {
    // Silently fail if localStorage is not available
  }
}

const selectedColumns = ref<Array<string>>([...DEFAULT_COLUMNS])

onMounted(() => {
  const stored = loadSelectedColumns()
  if (stored && Array.isArray(stored) && stored.length > 0) {
    const mergedColumns = migrateColumnKeys([...DEFAULT_COLUMNS])
    stored.forEach((col) => {
      const migratedCol = migrateColumnKeys([col])[0]
      if (
        !DEFAULT_COLUMNS.includes(migratedCol) &&
        !mergedColumns.includes(migratedCol)
      ) {
        mergedColumns.push(migratedCol)
      }
    })
    selectedColumns.value = mergedColumns
  } else {
    selectedColumns.value = [...DEFAULT_COLUMNS]
  }
  saveSelectedColumns(selectedColumns.value)

  if (route.query.approvalStatus === undefined) {
    router.replace({
      query: {
        ...route.query,
        approvalStatus: String(OrderApprovalStatus.Pending),
      },
    })
  }
})

watch(
  selectedColumns,
  (newValue) => {
    const mergedColumns = migrateColumnKeys([...DEFAULT_COLUMNS])
    newValue.forEach((col) => {
      const migratedCol = migrateColumnKeys([col])[0]
      if (
        !DEFAULT_COLUMNS.includes(migratedCol) &&
        !mergedColumns.includes(migratedCol)
      ) {
        mergedColumns.push(migratedCol)
      }
    })
    const arraysEqual =
      mergedColumns.length === newValue.length &&
      mergedColumns.every((col) => newValue.includes(col)) &&
      newValue.every((col) => mergedColumns.includes(col))

    if (!arraysEqual) {
      selectedColumns.value = mergedColumns
    }
    saveSelectedColumns(selectedColumns.value)
  },
  { deep: true }
)

const optionalSelectedColumns = computed<Array<string>>({
  get: () =>
    selectedColumns.value.filter((key) => !DEFAULT_COLUMNS.includes(key)),
  set: (optionalKeys) => {
    selectedColumns.value = [
      ...DEFAULT_COLUMNS,
      ...optionalKeys.filter((key) => !DEFAULT_COLUMNS.includes(key)),
    ]
  },
})

const dynamicColumns = computed<Array<AvailableColumn>>(() => {
  if (!availableColumnsData.value) return []
  return availableColumnsData.value.filter((col) =>
    selectedColumns.value.includes(col.key)
  )
})

const nonDefaultDynamicColumns = computed<Array<AvailableColumn>>(() => {
  return dynamicColumns.value.filter(
    (col) => !DEFAULT_COLUMNS.includes(col.key)
  )
})

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = []

  dynamicColumns.value.forEach((col) => {
    baseColumns.push({
      field: col.key,
      header: col.displayName,
      sortable: true,
    })
  })

  if (
    canUpdateOrder &&
    activeApprovalStatus.value === OrderApprovalStatus.Pending
  ) {
    baseColumns.push({
      field: 'approvalActions',
      header: t('sale.approvalActions.title'),
      sortable: false,
    })
  }

  if (canUpdateOrder) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
      sortable: false,
    })
  }

  if (canReadOrder) {
    baseColumns.push({
      field: 'view',
      header: t('words.view'),
      sortable: false,
    })
  }

  return baseColumns
})

const pageSize = ref(10)
const pageIndex = ref(1)

const authStore = useAuthStore()

const branchId = computed(() => authStore.selectedBranch!.id)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

const startDate = computed(() =>
  typeof route.query.startDate === 'string' ? route.query.startDate : null
)
const endDate = computed(() =>
  typeof route.query.endDate === 'string' ? route.query.endDate : null
)

watch(
  searchTerm,
  () => {
    pageIndex.value = 1
  },
  { deep: true, flush: 'sync' }
)

watch(
  () => [route.query.startDate, route.query.endDate] as const,
  (newDates, oldDates) => {
    if (newDates[0] !== oldDates?.[0] || newDates[1] !== oldDates?.[1]) {
      pageIndex.value = 1
    }
  },
  { flush: 'sync' }
)

watch(activeApprovalStatus, (status) => {
  pageIndex.value = 1
  router.replace({
    query: {
      ...route.query,
      approvalStatus: String(status),
    },
  })
})

watch(
  () => route.query.approvalStatus,
  () => {
    activeApprovalStatus.value = parseApprovalStatusFromQuery()
  }
)

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

const columnsForApi = computed<Array<string> | null>(() => {
  const cols = [...selectedColumns.value]

  INTERNAL_COLUMNS.forEach((columnKey) => {
    if (!cols.includes(columnKey)) {
      cols.push(columnKey)
    }
  })

  return cols.length > 0 ? cols : null
})

const customerId = computed<number | undefined>(() => {
  const customerIdQuery = route.query.customerId
  if (!customerIdQuery) return undefined
  const parsed = Number(customerIdQuery)
  return isNaN(parsed) ? undefined : parsed
})

const { data, isLoading } = useSaleDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  columnsForApi,
  sortCriteria,
  activeApprovalStatus,
  startDate,
  endDate,
  stepFilter
)

const pendingCountPageIndex = ref(1)
const pendingCountPageSize = ref(1)
const pendingStatusFilter = ref<OrderApprovalStatus>(
  OrderApprovalStatus.Pending
)
const pendingCountColumns = ref<Array<string> | null>(null)

const { data: pendingCountData } = useSaleDataQuery(
  pendingCountPageIndex,
  pendingCountPageSize,
  branchId,
  searchTerm,
  pendingCountColumns,
  ref(null),
  pendingStatusFilter,
  startDate,
  endDate,
  pendingCountStepFilter,
  {
    enabled: computed(
      () => activeApprovalStatus.value !== OrderApprovalStatus.Pending
    ),
  }
)

const pendingCount = computed(() => {
  if (activeApprovalStatus.value === OrderApprovalStatus.Pending) {
    return data.value?.paginatedOrders.totalCount ?? 0
  }
  return pendingCountData.value?.paginatedOrders.totalCount ?? 0
})

function formatApprovalStatusDisplay(rowData: Sale): string {
  const raw = getFieldValue(rowData, 'ApprovalStatus')
  return formatApprovalStatusCell(raw, t)
}

function formatExecutionStatusDisplay(rowData: Sale): string {
  const raw = getFieldValue(rowData, 'ExecutionStatus')
  return formatExecutionStatusCell(raw, t)
}

function getApprovalStatusBadgeClass(rowData: Sale): string {
  const base = 'inline-flex px-2 py-1 rounded-xl text-xs border'

  switch (getApprovalStatusFromRow(rowData as Record<string, unknown>)) {
    case OrderApprovalStatus.Approved:
      return `${base} bg-success-50 border-success text-success`
    case OrderApprovalStatus.Rejected:
      return `${base} bg-error-50 border-error text-error`
    case OrderApprovalStatus.Pending:
      return `${base} bg-warning-50 border-warning text-warning`
    default:
      return `${base} bg-gray-50 border-line text-gray-700`
  }
}

function getExecutionStatusBadgeClass(rowData: Sale): string {
  const base =
    'inline-flex px-2 py-1 rounded-xl text-xs border bg-gray-50 border-line text-gray-700'

  switch (getExecutionStatusFromRow(rowData as Record<string, unknown>)) {
    case OrderExecutionStatus.InProgress:
      return `${base} bg-primary-50 border-primary text-primary`
    case OrderExecutionStatus.Completed:
      return `${base} bg-success-50 border-success text-success`
    default:
      return base
  }
}

function getFieldValue(data: Sale, field: string): unknown {
  if (!data || typeof data !== 'object') return '-'

  if (field in data && data[field] !== null && data[field] !== undefined) {
    return data[field]
  }

  const lowerField = field.toLowerCase()
  for (const key in data) {
    if (
      key.toLowerCase() === lowerField &&
      data[key] !== null &&
      data[key] !== undefined
    ) {
      return data[key]
    }
  }

  return '-'
}

function getSaleId(data: Sale): number | null {
  if (!data || typeof data !== 'object') return null

  const idValue = getFieldValue(data, 'Id')
  if (idValue !== '-' && idValue !== null && idValue !== undefined) {
    const numId = Number(idValue)
    if (!isNaN(numId)) {
      return numId
    }
  }

  return null
}

function formatColumnValue(value: unknown, dataType: string): string {
  if (value === null || value === undefined || value === '-') return '-'

  switch (dataType) {
    case 'date':
      return formattedDate(String(value)) || '-'
    case 'number':
      return typeof value === 'number' ? value.toLocaleString() : String(value)
    case 'currency':
      return typeof value === 'number' ? value.toLocaleString() : String(value)
    default:
      return String(value)
  }
}

function onPageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
}

function handleSortChange(event: {
  sortField: string | null
  sortOrder: 1 | -1 | 0
}) {
  if (event.sortField && event.sortOrder !== 0) {
    const direction = event.sortOrder === 1 ? 0 : 1
    sortCriteria.value = [
      {
        property: event.sortField,
        direction: direction,
      },
    ]
  } else {
    sortCriteria.value = null
  }
}

const exposedApprovalStatus = computed(() => activeApprovalStatus.value)

defineExpose({
  branchId,
  searchTerm,
  columnsForApi,
  customerId,
  approvalStatus: exposedApprovalStatus,
  stepFilter,
  startDate,
  endDate,
})
</script>
