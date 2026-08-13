<template>
  <div class="custom-datatable-wrapper">
    <DataTable
      :value="data"
      v-model:selection="selectedItems"
      v-model:expandedRows="expandedRows"
      :paginator="paginatorEnabled"
      :showHeaders="!hideHeaders"
      :rows="pageSize"
      :totalRecords="totalRecords"
      :unstyled="false"
      :tableStyle="tableStyle"
      :class="tableClasses"
      :lazy="true"
      :loading="loading"
      :responsiveLayout="responsiveLayout"
      :sortMode="'single'"
      scrollable
      dataKey="id"
      @page="handlePageChange($event)"
      @row-expand="handleRowExpand"
      @row-collapse="handleRowCollapse"
      @sort="handleSortChange"
    >
      <template #paginatorend>
        <slot name="paginator">
          <div class="rounded-xl bg-white border-line border py-2 px-4">
            <span class="text-gray-700">{{ paginatorLabel }}: </span>
            <span>{{ totalRecords }}</span>
          </div>
        </slot>
      </template>

      <template #loading>
        <TableSkeletonLoading />
      </template>

      <Column
        v-if="selectable"
        selectionMode="multiple"
        headerStyle="width: 3rem"
      />

      <Column v-if="showRowNumbers">
        <template #body="{ index }">
          {{ getRowNumber(index) }}
        </template>
      </Column>

      <Column v-if="showFlag" style="width: 50px">
        <template #header>
          <SvgIcon name="flag" />
        </template>

        <template #body>
          <SvgIcon name="flag" class="text-gray-700" />
        </template>
      </Column>

      <Column v-if="expandable" expander style="width: 3rem" />

      <!-- Dynamic Columns -->
      <Column
        v-for="column in columns"
        :key="column.field"
        :field="column.field"
        :header="column.header"
        :sortable="column.sortable || false"
        :style="column.style"
        :headerStyle="column.headerStyle"
      >
        <template #header v-if="hasHeaderSlot(column.field)">
          <slot
            :name="`header-${column.field}`"
            :column="column"
            :header="column.header"
          />
        </template>
        <template #body="slotProps">
          <slot
            :name="`body-${column.field}`"
            :data="slotProps.data"
            :field="column.field"
            :value="getFieldValue(slotProps.data, column.field)"
            :index="slotProps.index"
            :column="column"
          >
            <!-- Default rendering -->
            <span>{{
              getFieldValue(slotProps.data, column.field) || '-'
            }}</span>
          </slot>
        </template>
      </Column>

      <!-- Expansion Template -->
      <template #expansion="slotProps" v-if="expandable">
        <slot
          name="expansion"
          :data="slotProps.data"
          :index="slotProps.index"
          :expanded="isExpanded(slotProps.data)"
        >
          <div class="expansion-loading p-4" v-if="expansionLoading">
            <TableSkeletonLoading />
          </div>
        </slot>
      </template>

      <template #empty>
        <slot name="empty">
          <NoData class="mx-auto" />
        </slot>
      </template>
    </DataTable>
  </div>
</template>

<script
  setup
  lang="ts"
  generic="
    T extends
      import('./CustomDataTable.types').AllowedTypes = import('./CustomDataTable.types').AllowedTypes
  "
>
import { DataTable, Column } from 'primevue'
import type { DataTableSortEvent } from 'primevue/datatable'
import { ref, toValue, useSlots, watch } from 'vue'
import TableSkeletonLoading from '../TableSkeletonLoading/TableSkeletonLoading.vue'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import NoData from '../NoData/NoData.vue'
import type { AllowedTypes, Emits, Props } from './CustomDataTable.types'

const props = defineProps<Props<T>>()
const emit = defineEmits<Emits>()
const slots = useSlots()

const selectedItems = ref<T[]>([])
const expandedRows = ref<T[]>([])
const currentPage = ref(0)

function getRowNumber(index: number): number {
  return currentPage.value * props.pageSize + index + 1
}

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  currentPage.value = event.page
  emit('pageChange', event)
}

function handleRowExpand(event: { data: unknown }): void {
  emit('rowExpand', { data: event.data as T })
}

function handleRowCollapse(event: { data: unknown }): void {
  emit('rowCollapse', { data: event.data as T })
}

function handleSortChange(event: DataTableSortEvent): void {
  emit('sortChange', {
    sortField: typeof event.sortField === 'string' ? event.sortField : null,
    sortOrder: (event.sortOrder as 1 | -1 | 0) || 0,
  })
}

function hasHeaderSlot(field: string): boolean {
  return !!slots[`header-${field}`]
}

function isExpanded(data: T): boolean {
  const rowId = (data as AllowedTypes & { id: string | number }).id
  return expandedRows.value.some(
    (row) => (row as AllowedTypes & { id: string | number }).id === rowId
  )
}
function getFieldValue(data: T, field: string): unknown {
  return field.split('.').reduce((obj: unknown, key: string) => {
    return obj && typeof obj === 'object' && key in obj
      ? (obj as Record<string, unknown>)[key]
      : undefined
  }, data)
}

watch(
  selectedItems,
  (newSelection) => {
    emit('selectionChange', toValue(newSelection))
  },
  { deep: true }
)
</script>
