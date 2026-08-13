<template>
  <CustomDataTable
    :data="data?.items.items || []"
    v-model:selection="selectedOrderLists"
    :columns="columns"
    :rows="pageSize"
    selectable
    :pageSize="pageSize"
    paginatorEnabled
    :totalRecords="data?.items.totalCount"
    tableStyle="min-width: 100%"
    class="custom-table custom-border mt-4"
    :loading="isLoading"
    @page-change="onPageChange"
    @sort-change="handleSortChange"
  >
    <template #paginator>
      <div class="inline rounded-xl bg-white border-line border py-2 px-4">
        <span class="text-gray-700"> {{ $t('payment.payments') }}: </span>
        <span>{{ data?.totalInstallmentPaymentCount }}</span>
      </div>

      <div class="inline rounded-xl bg-white border-line border py-2 px-4 mx-2">
        <span class="text-gray-700"> {{ $t('payment.table.amount') }}: </span>
        <span>{{ data?.totalCollectedAmount }}</span>
        <span class="text-gray-700 text-sm pr-1">
          {{ $t('words.dinar') }}
        </span>
      </div>
    </template>

    <template #body-installmentPaymentsCount="{ data }">
      <span>
        {{ data.installmentPaymentsCount }}
      </span>
      <span class="text-gray-700 px-1">
        {{ $t('payment.payment') }}
      </span>
    </template>

    <template #body-totalPaidInstallmentsAmount="{ data }">
      <span>
        {{ data.totalPaidInstallmentsAmount }}
      </span>
      <span class="text-gray-700 px-1">
        {{ $t('words.dinar') }}
      </span>
    </template>

    <template #body-customersCount="{ data }">
      {{ data.customersCount || '-' }}
    </template>

    <template #body-createdAt="{ data }">
      {{ data.createdAt ? formattedDate(data.createdAt) : '-' }}
    </template>

    <template #body-mandob.fullName="{ data }">
      {{ data.mandob?.fullName || '-' }}
    </template>

    <template #body-view="{ data }" v-if="canReadInstallmentPayment">
      <Button class="text-gray-300" v-slot="slotProps">
        <RouterLink
          :to="{
            name: 'InstallmentPaymentListRoute',
            params: { orderListId: data.id },
          }"
          :class="slotProps.a11yAttrs"
        >
          <SvgIcon
            name="note"
            class="w-7.5 h-7.5 text-gray-300 hover:text-primary cursor-pointer transition-colors"
          />
        </RouterLink>
      </Button>
    </template>
  </CustomDataTable>
</template>

<script setup lang="ts">
import { Button } from 'primevue'
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { useOrderListReportDataQuery } from '../../requests/queries'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import useAuthStore from '@/modules/Auth/store'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { formattedDate } from '@/modules/Core/utils/time'

const { t } = useI18n()
const route = useRoute()

const selectedOrderLists = ref()

const { can } = usePermission()
const canReadInstallmentPayment = can('InstallmentPayment', 'Read')

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = [
    {
      field: 'name',
      header: t('payment.table.orderListName'),
      sortable: true,
    },
    {
      field: 'installmentPaymentsCount',
      header: t('payment.table.installmentPaymentsCount'),
      sortable: true,
    },
    {
      field: 'totalPaidInstallmentsAmount',
      header: t('payment.table.totalPaidAmount'),
      sortable: true,
    },
    {
      field: 'mandob.fullName',
      header: t('payment.table.sellerFullName'),
      sortable: false,
    },
    {
      field: 'customersCount',
      header: t('payment.table.customersCount'),
      sortable: true,
    },
    {
      field: 'createdAt',
      header: t('payment.table.date'),
      sortable: true,
    },
  ]

  if (canReadInstallmentPayment) {
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

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

const startDate = computed(() => {
  return typeof route.query.startDate === 'string'
    ? route.query.startDate
    : null
})

const endDate = computed(() => {
  return typeof route.query.endDate === 'string' ? route.query.endDate : null
})

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

watch(
  searchTerm,
  () => {
    pageIndex.value = 1
  },
  { deep: true }
)

watch([startDate, endDate], () => {
  pageIndex.value = 1
})

const { data, isLoading } = useOrderListReportDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  startDate,
  endDate,
  sortCriteria
)

function onPageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
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

defineExpose({
  branchId,
  searchTerm,
})
</script>
