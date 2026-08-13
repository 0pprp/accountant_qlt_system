<template>
  <div class="p-4 custom-table">
    <TableSkeletonLoading v-if="listLoading" />

    <div v-else-if="tableData.length > 0">
      <div class="grid grid-cols-12 justify-between gap-4">
        <div
          v-for="item in tableData"
          :key="item.id"
          class="col-span-12 md:col-span-4"
        >
          <ExpensesCard :expense="item" :listLoading />
        </div>
      </div>

      <CustomPaginator
        :rows="pageSize"
        :totalRecords="totalRecords"
        :label="$t('expenses.expensesCount')"
        @page-change="handlePageChange($event)"
      />
    </div>

    <div v-else class="flex justify-center">
      <NoData />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
// import type{ Props } from './ExpensesList.types'
import { useExpensesDataQuery } from '../../requests/queries'
import ExpensesCard from './ExpensesCard/ExpensesCard.vue'
import useAuthStore from '@/modules/Auth/store'
import CustomPaginator from '@/modules/Core/components/shared/CustomPaginator/CustomPaginator.vue'
import NoData from '@/modules/Core/components/shared/NoData/NoData.vue'
import TableSkeletonLoading from '@/modules/Core/components/shared/TableSkeletonLoading/TableSkeletonLoading.vue'

const route = useRoute()
const authStore = useAuthStore()
const pageIndex = ref(1)
const pageSize = ref(6)

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

const { data: expensesListData, isFetching: listLoading } =
  useExpensesDataQuery(
    pageIndex,
    pageSize,
    branchId,
    searchTerm,
    startDate,
    endDate
  )

const tableData = computed(() => expensesListData.value?.items || [])
const totalRecords = computed(() => expensesListData.value?.totalCount || 0)

function handlePageChange(event: { first: number }) {
  pageIndex.value = Math.floor(event.first / pageSize.value) + 1
}
</script>
