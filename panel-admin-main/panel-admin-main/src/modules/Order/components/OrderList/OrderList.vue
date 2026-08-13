<template>
  <CustomDataTable
    :data="data?.items || []"
    :columns="columns"
    :rows="pageSize"
    :pageSize="pageSize"
    showRowNumbers
    paginatorEnabled
    :totalRecords="data?.totalCount"
    tableStyle="min-width: 100%"
    class="custom-table custom-border mt-4"
    :loading="isLoading"
    @page-change="onPageChange"
    @sort-change="handleSortChange"
  >
    <template #paginator>
      <div class="rounded-xl bg-white border-line border py-2 px-4">
        <span class="text-gray-700"> {{ $t('order.orders') }}: </span>

        <span>{{ data?.totalCount }}</span>
      </div>
    </template>

    <template #body-createdAt="{ data }">
      {{ formattedDate(data.createdAt) }}
    </template>

    <template #body-edit="{ data }" v-if="canUpdateOrder">
      <SvgIcon
        name="boldPencil"
        class="w-7.5 h-7.5 text-gray-300 hover:text-primary cursor-pointer transition-colors"
        :title="$t('words.edit')"
        @click="handleEdit(data)"
      />
    </template>
  </CustomDataTable>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useOrderDataQuery } from '../../requests/queries'
import type { Order } from '../../types/model'
import type { Emits } from './OrderList.Types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import useAuthStore from '@/modules/Auth/store'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import { usePermission } from '@/modules/Core/composable/usePermission'

const emit = defineEmits<Emits>()

const { t } = useI18n()
const route = useRoute()
const { can } = usePermission()
const canUpdateOrder = can('Order', 'Update')

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = [
    {
      field: 'branch.name',
      header: t('order.table.branchName'),
      sortable: true,
    },
    {
      field: 'name',
      header: t('order.table.name'),
      sortable: true,
    },
    {
      field: 'mandob.fullName',
      header: t('order.table.mandobName'),
      sortable: true,
    },
    {
      field: 'motaba.fullName',
      header: t('order.table.motabaName'),
      sortable: true,
    },
    {
      field: 'customersCount',
      header: t('order.table.customersCount'),
      sortable: true,
    },
    {
      field: 'createdAt',
      header: t('order.table.createdAt'),
      sortable: true,
    },
  ]

  if (canUpdateOrder) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
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

const { data, isLoading } = useOrderDataQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  sortCriteria
)

function handleEdit(item: Order) {
  emit('editItem', item)
}

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
</script>
