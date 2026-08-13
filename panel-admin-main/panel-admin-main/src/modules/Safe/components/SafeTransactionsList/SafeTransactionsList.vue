<template>
  <div class="p-4">
    <SafeCards v-if="canReadSafe" :safeData :isLoading="safeDataLoading" />

    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :totalRecords="totalRecords"
      :pageSize="pageSize"
      :loading="loading || isFetching"
      showRowNumbers
      :showFlag="false"
      :paginatorEnabled="true"
      class="custom-table custom-border mt-4"
      :paginatorLabel="$t('safe.paginatorLabel')"
      @selection-change="handleSelectionChange"
      @page-change="handlePageChange"
    >
      <template #body-source="{ data }">
        {{ data.source || '-' }}
      </template>

      <template #body-destination="{ data }">
        {{ data.destination || '-' }}
      </template>

      <template #body-amount="{ data }">
        {{ formattedPrice(data.amount) }}
        <small class="text-gray-700">
          {{ $t('words.dinar') }}
        </small>
      </template>

      <template #body-createdAt="{ data }">
        {{ formattedDate(data.createdAt) || '-' }}
      </template>

      <template #body-status="{ data }" v-if="canUpdateTransaction">
        <SvgIcon
          :name="getStatusIcon(data.status)"
          :class="`text-${getStatusColor(data.status)}`"
        />
      </template>

      <template #body-type="{ data }">
        {{ getTransactionTypeLabel(data.type) }}
      </template>

      <template #body-statusDescription="{ data }">
        {{ data.statusDescription || '-' }}
      </template>

      <template #body-actions="{ data: rowData }">
        <ChangeStatus :id="rowData.id" :status="rowData.status" />
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, watchEffect } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import ChangeStatus from '../ChangeStatus/ChangeStatus.vue'
import {
  useSafeCurrentDataQuery,
  useSafeTransactionsDataQuery,
} from '../../requests/queries'
import useSafeStore from '../../store/index'
import SafeCards from '../SafeCards/SafeCards.vue'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import {
  type SafeTransactions,
  TransactionStatus,
  TransactionType,
} from '@/modules/Safe/types/model'
import type { AllowedTypes } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import useAuthStore from '@/modules/Auth/store'
import { formattedDate } from '@/modules/Core/utils/time.ts'
import { formattedPrice } from '@/modules/Core/utils'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const route = useRoute()
const authStore = useAuthStore()
const safeStore = useSafeStore()
const pageIndex = ref(1)
const pageSize = ref(10)

const searchTerm = ref<string | null>(
  route.query.s ? String(route.query.s) : null
)

const branchId = computed(() => {
  // اگر route name SafeMainMainRoute باشد یا query parameter main=true باشد، القاصة الرئيسية را نمایش بده
  if (route.name === 'SafeMainMainRoute' || route.query.main === 'true') {
    return null
  }
  // در غیر این صورت، القاصه شاخه انتخاب شده را نمایش بده
  return authStore.selectedBranch?.id || 0
})

watch(
  () => route.query,
  (newQuery) => {
    searchTerm.value = newQuery.s ? String(newQuery.s) : null
  },
  { immediate: true }
)

const selectedItems = ref<SafeTransactions[]>([])

const {
  data: safeData,
  isLoading: safeDataLoading,
  isFetched,
} = useSafeCurrentDataQuery(branchId)

const safeId = ref()

watchEffect(() => {
  if (isFetched.value && safeData.value?.id) {
    safeStore.setLoading(safeDataLoading.value)
    safeStore.setSafeData(safeData.value)
    safeId.value = safeStore.safeId
  }
})

const {
  data: safeTransactionListData,
  isLoading,
  isFetching,
} = useSafeTransactionsDataQuery(pageIndex, pageSize, safeId, searchTerm, {
  enabled: computed(() => safeId.value > 0),
})

const tableData = computed(() => safeTransactionListData.value?.items || [])
const totalRecords = computed(
  () => safeTransactionListData.value?.totalCount || 0
)
const loading = computed(() => isLoading.value)

const { t } = useI18n()

const { can } = usePermission()
const canUpdateTransaction = can('Transaction', 'Update')
const canReadSafe = can('Safe', 'Read')
const columns = computed(() => {
  const baseColumns = [
    {
      field: 'source',
      header: t('safe.tableTransactionColumns.source'),
      sortable: false,
    },
    {
      field: 'destination',
      header: t('safe.tableTransactionColumns.destination'),
      sortable: false,
    },
    {
      field: 'amount',
      header: t('safe.tableTransactionColumns.amount'),
      sortable: false,
    },
    {
      field: 'createdAt',
      header: t('safe.tableTransactionColumns.createdAt'),
      sortable: false,
    },
    {
      field: 'type',
      header: t('safe.tableTransactionColumns.type'),
      sortable: false,
    },
    {
      field: 'status',
      header: t('safe.tableTransactionColumns.status'),
      sortable: false,
    },
    {
      field: 'statusDescription',
      header: t('safe.tableTransactionColumns.statusDescription'),
      sortable: false,
      style: 'min-width: 200px; max-width: 200px',
    },
  ]

  if (canUpdateTransaction) {
    baseColumns.push({
      field: 'actions',
      header: t('safe.tableTransactionColumns.actions'),
      sortable: false,
    })
  }

  return baseColumns
})
function getStatusIcon(status: TransactionStatus): string {
  const statusIcon = {
    [TransactionStatus.Pending]: 'boldMinusCircle',
    [TransactionStatus.Approved]: 'boldCheckCircle',
    [TransactionStatus.Rejected]: 'boldCloseCircle',
  }
  return statusIcon[status] || '-'
}

function getStatusColor(status: TransactionStatus): string {
  const statusColor = {
    [TransactionStatus.Pending]: 'info',
    [TransactionStatus.Approved]: 'success',
    [TransactionStatus.Rejected]: 'error',
  }
  return statusColor[status] || ''
}

function getTransactionTypeLabel(type: TransactionType): string {
  const typeLabels: Partial<Record<TransactionType, string>> = {
    [TransactionType.SellerPayment]: t('safe.transactionType.sellerPayment'),
    [TransactionType.Purchase]: t('safe.transactionType.purchase'),
    [TransactionType.Expense]: t('safe.transactionType.expense'),
    [TransactionType.SafeTransfer]: t('safe.transactionType.safeTransfer'),
  }
  return typeLabels[type] || '-'
}

function handleSelectionChange(selection: AllowedTypes[]) {
  selectedItems.value = selection as SafeTransactions[]
}

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  pageIndex.value = event.page + 1
}
</script>
