<template>
  <div class="p-4">
    <ActivityLogFilters
      v-model:activityType="selectedActivityType"
      v-model:targetEntityType="selectedTargetEntityType"
      v-model:userId="selectedUserId"
    />

    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :totalRecords="totalRecords"
      :pageSize="pageSize"
      :loading="loading"
      :selectable="false"
      :showFlag="false"
      :paginatorEnabled="true"
      class="custom-table custom-border mt-4"
      :paginatorLabel="$t('activityLog.paginatorLabel')"
      @page-change="handlePageChange"
      @sort-change="handleSortChange"
    >
      <template #body-activityType="{ data }">
        {{ getActivityTypeLabel(data.activityType, t) }}
      </template>

      <template #body-description="{ data }">
        {{
          data.description || getActivityTypeLabel(data.activityType, t) || '-'
        }}
      </template>

      <template #body-userName="{ data }">
        {{ data.userName || '-' }}
      </template>

      <template #body-userRoles="{ data }">
        {{ data.userRoles || '-' }}
      </template>

      <template #body-targetEntityType="{ data }">
        {{ getTargetEntityTypeLabel(data.targetEntityType, t) }}
      </template>

      <template #body-createdAt="{ data }">
        {{ formattedDate(data.createdAt || '') || '-' }}
      </template>

      <template #body-details="{ data: rowData }">
        <SvgIcon
          name="boldEye"
          class="w-7.5 h-7.5 text-gray-300 hover:text-primary cursor-pointer transition-colors"
          :title="$t('words.eyeView')"
          @click="navigateToDetail(rowData.id)"
        />
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import ActivityLogFilters from '../ActivityLogFilters/ActivityLogFilters.vue'
import { useActivityLogListQuery } from '../../requests/queries'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type { Column } from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import useAuthStore from '@/modules/Auth/store'
import { formattedDate } from '@/modules/Core/utils/time.ts'
import {
  getActivityTypeLabel,
  getTargetEntityTypeLabel,
} from '@/modules/ActivityLog/utils/labels'
import type { ActivityLogFilterParams } from '@/modules/ActivityLog/types/api'
import type { ActivityLogListItem } from '@/modules/ActivityLog/types/model'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { t } = useI18n()

const pageIndex = ref(1)
const pageSize = ref(10)

function parseQueryNumber(key: string): number | null {
  const queryValue = route.query[key]
  if (queryValue === undefined) {
    return null
  }

  const parsed = Number(Array.isArray(queryValue) ? queryValue[0] : queryValue)
  return Number.isNaN(parsed) ? null : parsed
}

const selectedActivityType = ref<number | null>(
  parseQueryNumber('activityType')
)
const selectedTargetEntityType = ref<number | null>(
  parseQueryNumber('targetEntityType')
)
const selectedUserId = ref<number | null>(parseQueryNumber('userId'))

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

const filter = computed<ActivityLogFilterParams>(() => ({
  branchId: branchId.value,
  searchTerm: searchTerm.value,
  startDate: startDate.value,
  endDate: endDate.value,
  activityType: selectedActivityType.value,
  targetEntityType: selectedTargetEntityType.value,
  userId: selectedUserId.value,
}))

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

watch(
  () => route.query,
  () => {
    selectedActivityType.value = parseQueryNumber('activityType')
    selectedTargetEntityType.value = parseQueryNumber('targetEntityType')
    selectedUserId.value = parseQueryNumber('userId')
  },
  { immediate: true }
)

watch(selectedActivityType, (value) => {
  pageIndex.value = 1
  const newQuery = { ...route.query }

  if (value !== null) {
    newQuery.activityType = String(value)
  } else {
    delete newQuery.activityType
  }

  router.replace({ query: newQuery })
})

watch(selectedTargetEntityType, (value) => {
  pageIndex.value = 1
  const newQuery = { ...route.query }

  if (value !== null) {
    newQuery.targetEntityType = String(value)
  } else {
    delete newQuery.targetEntityType
  }

  router.replace({ query: newQuery })
})

watch(selectedUserId, (value) => {
  pageIndex.value = 1
  const newQuery = { ...route.query }

  if (value !== null) {
    newQuery.userId = String(value)
  } else {
    delete newQuery.userId
  }

  router.replace({ query: newQuery })
})

watch([searchTerm, startDate, endDate, branchId], () => {
  pageIndex.value = 1
})

const { data: activityLogListData, isLoading } = useActivityLogListQuery(
  pageIndex,
  pageSize,
  filter,
  sortCriteria
)

const tableData = computed<ActivityLogListItem[]>(
  () => activityLogListData.value?.items || []
)
const totalRecords = computed(() => activityLogListData.value?.totalCount || 0)
const loading = computed(() => isLoading.value)

const columns = computed<Column<ActivityLogListItem>[]>(() => [
  {
    field: 'activityType',
    header: t('activityLog.tableColumns.activityType'),
    sortable: true,
    style: 'min-width: 180px',
  },
  {
    field: 'description',
    header: t('activityLog.tableColumns.description'),
    sortable: true,
    style: 'min-width: 220px',
  },
  {
    field: 'userName',
    header: t('activityLog.tableColumns.userName'),
    sortable: true,
    style: 'min-width: 160px',
  },
  {
    field: 'userRoles',
    header: t('activityLog.tableColumns.userRoles'),
    sortable: false,
    style: 'min-width: 160px',
  },
  {
    field: 'targetEntityType',
    header: t('activityLog.tableColumns.targetEntityType'),
    sortable: true,
    style: 'min-width: 140px',
  },
  {
    field: 'createdAt',
    header: t('activityLog.tableColumns.createdAt'),
    sortable: true,
    style: 'min-width: 140px',
  },
  {
    field: 'details',
    header: t('activityLog.tableColumns.details'),
    sortable: false,
    style: 'min-width: 100px',
  },
])

function navigateToDetail(id: number) {
  router.push({ name: 'ActivityLogDetailRoute', params: { id: String(id) } })
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
