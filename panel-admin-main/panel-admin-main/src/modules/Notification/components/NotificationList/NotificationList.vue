<template>
  <CustomDataTable
    :data="tableData"
    :columns="columns"
    :rows="pageSize"
    :pageSize="pageSize"
    paginatorEnabled
    :totalRecords="totalRecords"
    tableStyle="min-width: 100%"
    class="custom-table custom-border mt-4"
    :loading="loading"
    :paginatorLabel="$t('notification.paginatorLabel')"
    @page-change="handlePageChange"
    @sort-change="handleSortChange"
  >
    <template #body-title="{ data }">
      <div class="flex items-center gap-2">
        <span
          v-if="!data.hasRead"
          class="inline-block w-2 h-2 rounded-full bg-success shrink-0"
        />
        <span>{{ data.title }}</span>
      </div>
    </template>

    <template #body-createdAt="{ data }">
      {{ formattedDateTime(data.createdAt) || '-' }}
    </template>

    <template #body-description="{ data }">
      {{ data.description || '-' }}
    </template>

    <template #body-actorUserFullName="{ data }">
      {{ data.actorUserFullName || '-' }}
    </template>
  </CustomDataTable>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { useNotificationListQuery } from '../../requests/queries'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type { Column } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import useAuthStore from '@/modules/Auth/store'
import { formattedDateTime } from '@/modules/Core/utils/time'
import type { Notification } from '@/modules/Notification/types/model'

const route = useRoute()
const authStore = useAuthStore()
const { t } = useI18n()

const pageIndex = ref(1)
const pageSize = ref(10)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null

  const terms = Array.isArray(s) ? s.map(String) : [String(s)]
  return terms.join(', ')
})

const startDate = computed(() =>
  typeof route.query.startDate === 'string' ? route.query.startDate : null
)
const endDate = computed(() =>
  typeof route.query.endDate === 'string' ? route.query.endDate : null
)

const branchId = computed(() => authStore.selectedBranch?.id ?? 0)

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

watch([searchTerm, startDate, endDate, branchId], () => {
  pageIndex.value = 1
})

const { data: notificationListData, isLoading } = useNotificationListQuery(
  pageIndex,
  pageSize,
  branchId,
  searchTerm,
  startDate,
  endDate,
  sortCriteria
)

const tableData = computed<Notification[]>(
  () => notificationListData.value?.items || []
)
const totalRecords = computed(() => notificationListData.value?.totalCount || 0)
const loading = computed(() => isLoading.value)

const columns = computed<Column<Notification>[]>(() => [
  {
    field: 'title',
    header: t('notification.tableColumns.title'),
    sortable: true,
    style: 'min-width: 220px',
  },
  {
    field: 'actorUserFullName',
    header: t('notification.tableColumns.actorUserFullName'),
    sortable: true,
    style: 'min-width: 160px',
  },
  {
    field: 'createdAt',
    header: t('notification.tableColumns.createdAt'),
    sortable: true,
    style: 'min-width: 180px',
  },
  {
    field: 'description',
    header: t('notification.tableColumns.description'),
    sortable: true,
    style: 'min-width: 280px',
  },
])

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
    const direction = event.sortOrder === 1 ? 0 : 1
    sortCriteria.value = [
      {
        property: event.sortField,
        direction,
      },
    ]
  } else {
    sortCriteria.value = null
  }
}
</script>
