<template>
  <div class="p-4">
    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :totalRecords="totalRecords"
      :pageSize="pageSize"
      :loading="loading"
      :selectable="true"
      :showFlag="false"
      :paginatorEnabled="true"
      class="custom-table custom-border mt-4"
      :paginatorLabel="$t('customer.paginatorLabel')"
      @selection-change="handleSelectionChange"
      @page-change="handlePageChange"
      @sort-change="handleSortChange"
    >
      <template #body-fullName="{ data }">
        {{ data.fullName || '-' }}
      </template>

      <template #body-businessName="{ data }">
        {{ data.businessName || '-' }}
      </template>

      <template #body-ordersCount="{ data }">
        {{ data.ordersCount || '-' }}
      </template>

      <template #body-lastInstallmentPaymentDate="{ data }">
        {{ formattedDate(data.lastInstallmentPaymentDate || '') || '-' }}
      </template>

      <template #body-details="{ data: rowData }" v-if="canUpdateCustomer">
        <SvgIcon
          name="note"
          class="w-7.5 h-7.5 text-gray-300 hover:text-primary cursor-pointer transition-colors"
          :title="$t('words.edit')"
          @click="navigateToViewPage(rowData.id)"
        />
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useCustomerDataQuery } from '../../requests/queries'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type { Customer } from '@/modules/Customer/types/model'
import type { AllowedTypes } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import useAuthStore from '@/modules/Auth/store'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time.ts'
import { usePermission } from '@/modules/Core/composable/usePermission'

const route = useRoute()
const authStore = useAuthStore()

const pageIndex = ref(1)
const pageSize = ref(10)

const searchTerm = ref<string | null>(
  route.query.s ? String(route.query.s) : null
)
const branchId = ref(authStore.selectedBranch?.id || 0)
const startDate = ref<string | null>(
  route.query.startDate ? String(route.query.startDate) : null
)
const endDate = ref<string | null>(
  route.query.endDate ? String(route.query.endDate) : null
)

watch(
  () => route.query,
  (newQuery) => {
    searchTerm.value = newQuery.s ? String(newQuery.s) : null
    startDate.value = newQuery.startDate ? String(newQuery.startDate) : null
    endDate.value = newQuery.endDate ? String(newQuery.endDate) : null
  },
  { immediate: true }
)

watch(
  () => authStore.selectedBranch?.id,
  (newBranchId) => {
    branchId.value = newBranchId || 0
  },
  { immediate: true }
)

const selectedItems = ref<Customer[]>([])

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

const { data: customerListData, isLoading } = useCustomerDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  startDate,
  endDate,
  sortCriteria
)

const tableData = computed(() => customerListData.value?.items || [])
const totalRecords = computed(() => customerListData.value?.totalCount || 0)
const loading = computed(() => isLoading.value)

const { t } = useI18n()
const { can } = usePermission()
const canUpdateCustomer = can('Customer', 'Update')

const columns = computed(() => {
  const baseColumns = [
    {
      field: 'fullName',
      header: t('customer.tableColumns.fullName'),
      sortable: true,
      style: 'min-width: 200px',
    },
    {
      field: 'businessName',
      header: t('customer.tableColumns.businessName'),
      sortable: true,
      style: 'min-width: 180px',
    },
    {
      field: 'orderListName',
      header: t('customer.tableColumns.orderListName'),
      sortable: true,
      style: 'min-width: 150px',
    },
    {
      field: 'ordersCount',
      header: t('customer.tableColumns.start'),
      sortable: true,
      style: 'min-width: 120px',
    },
    {
      field: 'lastInstallmentPaymentDate',
      header: t('customer.tableColumns.end'),
      sortable: true,
      style: 'min-width: 140px',
    },
    {
      field: 'businessAddress',
      header: t('customer.tableColumns.businessAddress'),
      sortable: true,
      style: 'min-width: 200px',
    },
  ]

  if (canUpdateCustomer) {
    baseColumns.push({
      field: 'details',
      header: t('customer.tableColumns.details'),
      sortable: false,
      style: 'min-width: 160px',
    })
  }

  return baseColumns
})
const router = useRouter()

function navigateToViewPage(id: number) {
  router.push({ name: 'EditCustomerView', params: { id: id } })
}

function handleSelectionChange(selection: AllowedTypes[]) {
  selectedItems.value = selection as Customer[]
}

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  pageIndex.value = event.page + 1
}

function handleSortChange(event: {
  sortField: string | null
  sortOrder: 1 | -1 | 0
}) {
  if (event.sortField && event.sortOrder !== 0) {
    // Map PrimeVue sortOrder to API direction
    // sortOrder: 1 (ascending) → direction: 0 (Ascending - SortDirection.Ascending)
    // sortOrder: -1 (descending) → direction: 1 (Descending - SortDirection.Descending)
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
</script>
