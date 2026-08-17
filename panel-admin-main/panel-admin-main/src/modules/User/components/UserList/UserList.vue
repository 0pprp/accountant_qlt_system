<template>
  <div>
    <div
      class="flex flex-wrap gap-4 mb-4 p-4 rounded-xl border-line border bg-white mt-4"
    >
      <div class="min-w-[200px]">
        <SelectField
          v-model="selectedRoleId"
          :options="roleFilterOptions"
          label="user.filters.role.label"
          clearable
          placeholder="user.filters.role.placeholder"
          :loading="isRolesLoading"
        />
      </div>

      <ColumnSelectorDropdown
        v-if="selectableColumns.length > 0"
        v-model="optionalSelectedColumns"
        :options="selectableColumns"
        labelKey="user.columnSelector.title"
        placeholderKey="user.columnSelector.placeholder"
      />
    </div>

    <CustomDataTable
      :data="tableData"
      :columns="columns"
      :rows="pageSize"
      :pageSize="pageSize"
      paginatorEnabled
      :totalRecords="data?.paginatedUsers.totalCount"
      tableStyle="min-width: 100%"
      class="custom-table custom-border mt-4"
      :loading="isLoading"
      @page-change="onPageChange"
      @sort-change="handleSortChange"
    >
      <template #paginator>
        <div class="rounded-xl bg-white border-line border py-2 px-4">
          <span class="text-gray-700"> {{ $t('user.users') }}: </span>

          <span>{{ data?.paginatedUsers.totalCount }}</span>
        </div>
      </template>

      <template #body-FullName="{ data: rowData }">
        {{ getFieldValue(rowData, 'FullName') }}
      </template>

      <template #body-BranchNames="{ data: rowData }">
        {{ getFieldValue(rowData, 'BranchNames') }}
      </template>

      <template #body-CreatedAt="{ data: rowData }">
        {{ formatColumnValue(getFieldValue(rowData, 'CreatedAt'), 'datetime') }}
      </template>

      <!-- Dynamic column slots (excluding default columns which have explicit slots) -->
      <template
        v-for="column in nonDefaultDynamicColumns"
        :key="column.key"
        #[`body-${column.key}`]="{ data: rowData }"
      >
        <template v-if="column.dataType === 'currency'">
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

      <template #body-edit="{ data: rowData }" v-if="canUpdateUser">
        <Button class="text-gray-300" v-slot="slotProps">
          <RouterLink
            v-if="getUserId(rowData)"
            :to="{
              name: 'UserUpdateRoute',
              params: { userId: String(getUserId(rowData)) },
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

      <template #body-managePermissions="{ data: rowData }" v-if="canUpdateUser">
        <Button
          v-if="canManageUserPermissions(rowData)"
          class="text-primary inline-flex items-center gap-1"
          @click="handleManagePermissions(rowData)"
        >
          <SvgIcon name="securityShieldUser" class="text-primary w-5 h-5" />
          <span class="whitespace-nowrap text-sm">
            {{ $t('user.managePermissions') }}
          </span>
        </Button>
        <span v-else class="text-gray-400">-</span>
      </template>

      <template #body-view="{ data: rowData }" v-if="canReadUser">
        <Button class="text-gray-300" v-slot="slotProps">
          <RouterLink
            v-if="getUserId(rowData)"
            :to="{
              name: 'UserViewRoute',
              params: { userId: String(getUserId(rowData)) },
            }"
            :class="slotProps.a11yAttrs"
          >
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
  useUserDataQuery,
  useAvailableColumnsQuery,
} from '../../requests/queries'
import type { User } from '../../types/model'
import type { AvailableColumn } from '../../types/api'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import useAuthStore from '@/modules/Auth/store'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import ColumnSelectorDropdown from '@/modules/Core/components/shared/ColumnSelectorDropdown/ColumnSelectorDropdown.vue'
import SelectField from '@/modules/Core/components/base/Fields/SelectField/SelectField.vue'
import { useRoleDataQuery } from '@/modules/Role/requests/queries'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { can } = usePermission()
const canReadUser = can('User', 'Read')
const canUpdateUser = can('User', 'Update')

const STORAGE_KEY = 'user-list-selected-columns'

const { data: availableColumnsData } = useAvailableColumnsQuery()
const DEFAULT_COLUMNS = ['FullName', 'BranchNames', 'CreatedAt']
const INTERNAL_COLUMNS = ['Id', 'RoleNames']

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
      return JSON.parse(stored) as Array<string>
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
    const mergedColumns = [...DEFAULT_COLUMNS]
    stored.forEach((col) => {
      if (!DEFAULT_COLUMNS.includes(col) && !mergedColumns.includes(col)) {
        mergedColumns.push(col)
      }
    })
    selectedColumns.value = mergedColumns
  } else {
    selectedColumns.value = [...DEFAULT_COLUMNS]
  }
  saveSelectedColumns(selectedColumns.value)
})

watch(
  selectedColumns,
  (newValue) => {
    const mergedColumns = [...DEFAULT_COLUMNS]
    newValue.forEach((col) => {
      if (!DEFAULT_COLUMNS.includes(col) && !mergedColumns.includes(col)) {
        mergedColumns.push(col)
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

  if (canUpdateUser) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
      sortable: false,
    })
    baseColumns.push({
      field: 'managePermissions',
      header: t('user.managePermissions'),
      sortable: false,
      style: 'min-width: 160px',
    })
  }

  if (canReadUser) {
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

const { data: roleData, isFetching: isRolesLoading } = useRoleDataQuery()

function parseRoleIdFromQuery(): number | null {
  const queryValue = route.query.roleId
  if (queryValue === undefined) {
    return null
  }

  const parsed = Number(Array.isArray(queryValue) ? queryValue[0] : queryValue)
  return isNaN(parsed) ? null : parsed
}

const selectedRoleId = ref<number | null>(parseRoleIdFromQuery())

const roleFilterOptions = computed(() => {
  const options: Array<{ value: number | null; label: string }> = [
    {
      value: null,
      label: t('user.filters.role.all'),
    },
  ]

  roleData.value?.forEach((role) => {
    options.push({
      value: role.id,
      label: role.displayName,
    })
  })

  return options
})

const roleIds = computed<Array<number> | null>(() => {
  if (selectedRoleId.value === null) {
    return null
  }

  return [selectedRoleId.value]
})

const startDate = computed(() =>
  typeof route.query.startDate === 'string' ? route.query.startDate : null
)
const endDate = computed(() =>
  typeof route.query.endDate === 'string' ? route.query.endDate : null
)

const searchTerm = computed(() => {
  const s = route.query.s
  if (!s) return null
  return Array.isArray(s) ? s.map(String) : [String(s)]
})

watch(
  searchTerm,
  () => {
    pageIndex.value = 1
  },
  { deep: true }
)

watch(
  () => route.query.roleId,
  () => {
    selectedRoleId.value = parseRoleIdFromQuery()
  }
)

watch(selectedRoleId, (roleId) => {
  const currentRoleId = parseRoleIdFromQuery()
  if (currentRoleId === roleId) {
    return
  }

  pageIndex.value = 1

  const newQuery = { ...route.query }
  if (roleId !== null) {
    newQuery.roleId = String(roleId)
  } else {
    delete newQuery.roleId
  }

  router.replace({ query: newQuery })
})

watch(
  () => [route.query.startDate, route.query.endDate] as const,
  (newDates, oldDates) => {
    if (newDates[0] !== oldDates?.[0] || newDates[1] !== oldDates?.[1]) {
      pageIndex.value = 1
    }
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

const { data, isLoading } = useUserDataQuery(
  pageIndex,
  pageSize,
  roleIds,
  branchId,
  searchTerm,
  columnsForApi,
  sortCriteria,
  startDate,
  endDate
)

const tableData = computed<User[]>(() => data.value?.paginatedUsers.items ?? [])

function getFieldValue(data: User, field: string): unknown {
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

function getUserId(data: User): number | null {
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

function isAdminUser(rowData: User): boolean {
  const roleNames = getFieldValue(rowData, 'RoleNames')
  if (roleNames === '-' || roleNames === null || roleNames === undefined) {
    return false
  }

  return String(roleNames)
    .split(',')
    .some((name) => name.trim() === 'Admin')
}

function canManageUserPermissions(rowData: User): boolean {
  return getUserId(rowData) !== null && isAdminUser(rowData) === false
}

function formatColumnValue(value: unknown, dataType: string): string {
  if (value === null || value === undefined || value === '-') return '-'

  switch (dataType) {
    case 'date':
    case 'datetime':
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

function handleManagePermissions(rowData: User) {
  if (canManageUserPermissions(rowData) === false) {
    return
  }

  const userId = getUserId(rowData)
  if (userId === null) {
    return
  }

  router.push({
    name: 'UserUpdateRoute',
    params: { userId: String(userId) },
    query: { tab: 'permissions' },
  })
}
</script>
