<template>
  <div>
    <DataTable
      :rows="rows"
      :totalRecords="totalRecords"
      :unstyled="false"
      :paginator="true"
      class="custom-paginator rounded-xl my-2 px-4"
      :lazy="true"
      dataKey="id"
      @page="handlePageChange($event)"
    >
      <template #paginatorend>
        <slot name="paginator">
          <div class="rounded-xl bg-background !border-line border py-2 px-4">
            <span class="text-gray-700">{{ label }}: </span>
            <span>{{ totalRecords }}</span>
          </div>
        </slot>
      </template>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
import { DataTable } from 'primevue'
import { ref } from 'vue'
import type { Emits, Props } from './CustomPaginator.types'

defineProps<Props>()
const emit = defineEmits<Emits>()
const currentPage = ref(0)

function handlePageChange(event: {
  first: number
  page: number
  rows: number
}) {
  currentPage.value = event.page
  emit('pageChange', event)
}
</script>
