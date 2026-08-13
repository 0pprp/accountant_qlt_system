<template>
  <div>
    <div class="rounded-xl bg-white border-line border py-2 px-4">
      <span class="text-gray-700">
        {{ $t('user.dailyReport.undeliveredCashAmount') }}:
      </span>

      <span
        >{{ formattedPrice(data?.undeliveredCashAmount || 0) }}
        {{ $t('words.dinar') }}</span
      >
    </div>
    <CustomDataTable
      :data="data?.items || []"
      :columns="columns"
      :pageSize="10000000000"
      :totalRecords="data?.undeliveredCashAmount"
      tableStyle="min-width: 100%"
      class="custom-table custom-border mt-4"
      :loading="isLoading"
      @sort-change="handleSortChange"
    >
      <template #body-date="{ data }">
        {{ formattedDate(data.date) }}
      </template>

      <template #body-totalInstallmentAmount="{ data }">
        {{ formattedPrice(data.totalInstallmentAmount) }}
      </template>

      <template #body-totalDeliveredCashAmount="{ data }">
        {{ formattedPrice(data.totalDeliveredCashAmount) }}
      </template>

      <template #body-cumulativeUndeliveredCashAmount="{ data }">
        {{ formattedPrice(data.cumulativeUndeliveredCashAmount) }}
      </template>
    </CustomDataTable>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { useUserDailyReportDataQuery } from '../../requests/queries'
import { formattedDate } from '@/modules/Core/utils/time'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import { formattedPrice } from '@/modules/Core/utils'

const { t } = useI18n()
const route = useRoute()

const columns = computed<Column<AllowedTypes>[]>(() => {
  const baseColumns: Column<AllowedTypes>[] = [
    {
      field: 'date',
      header: t('user.dailyReport.date'),
      sortable: true,
    },
    {
      field: 'totalInstallmentAmount',
      header: t('user.dailyReport.totalInstallmentAmount'),
      sortable: true,
    },
    {
      field: 'totalDeliveredCashAmount',
      header: t('user.dailyReport.totalDeliveredCashAmount'),
      sortable: true,
    },

    {
      field: 'cumulativeUndeliveredCashAmount',
      header: t('user.dailyReport.cumulativeUndeliveredCashAmount'),
      sortable: true,
    },
  ]
  return baseColumns
})

const id = computed(() => Number(route.params.userId))
const startDate = computed(() => route.query.startDate as string)
const endDate = computed(() => route.query.endDate as string)

const sortCriteria = ref<Array<{
  property: string
  direction: number
}> | null>(null)

const { data, isLoading } = useUserDailyReportDataQuery(id, startDate, endDate)

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
</script>
