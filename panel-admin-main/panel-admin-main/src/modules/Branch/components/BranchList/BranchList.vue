<template>
  <CustomDataTable
    :data="data?.data.items || []"
    :columns="columns"
    :rows="pageSize"
    :pageSize="pageSize"
    showRowNumbers
    paginatorEnabled
    :totalRecords="data?.data.totalCount"
    tableStyle="min-width: 100%"
    class="custom-table custom-border mt-4"
    :loading="isLoading"
    @page-change="onPageChange"
  >
    <template #paginator>
      <div class="rounded-xl bg-white border-line border py-2 px-4">
        <span class="text-gray-700"> {{ $t('branch.branches') }}: </span>
        <span>{{ data?.data.totalCount }}</span>
      </div>
    </template>

    <template #body-provinceName="{ data }">
      {{ data.province?.name || '-' }}
    </template>

    <template #body-createdAt="{ data }">
      {{ formattedDate(data.createdAt) }}
    </template>

    <template #body-edit="{ data }" v-if="canUpdateBranch">
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
import { useBranchDataQuery } from '../../requests/queries'
import type { Branch } from '../../types/model'
import type { Emits } from './BranchList.Types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedDate } from '@/modules/Core/utils/time'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

const emit = defineEmits<Emits>()

const { t } = useI18n()
const route = useRoute()
const { can } = usePermission()
const canUpdateBranch = can('Branch', 'Update')
const pageSize = ref(10)
const pageIndex = ref(1)

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

const { data, isLoading } = useBranchDataQuery(pageIndex, pageSize, searchTerm)

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = [
    {
      field: 'name',
      header: t('branch.table.name'),
    },
    {
      field: 'provinceName',
      header: t('branch.table.provinceName'),
    },
    {
      field: 'createdAt',
      header: t('branch.table.createdAt'),
    },
  ]

  if (canUpdateBranch) {
    baseColumns.push({
      field: 'edit',
      header: t('words.edit'),
    })
  }

  return baseColumns
})

function handleEdit(item: Branch) {
  emit('editItem', item)
}

function onPageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
}
</script>
