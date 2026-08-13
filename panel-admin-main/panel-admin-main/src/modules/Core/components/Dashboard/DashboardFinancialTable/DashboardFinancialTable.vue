<template>
  <Card class="rounded-2xl bg-white border-2 border-line p-4 shadow-sm h-full">
    <template #content>
      <CustomDataTable
        :data="rows"
        :columns="columns"
        :pageSize="rows.length"
        :paginatorEnabled="false"
        tableStyle="min-width: 100%"
        class="custom-table custom-border"
      >
        <template #body-period="{ data }">
          <span class="text-gray-900 font-medium">{{ data.period }}</span>
        </template>

        <template #body-cashSafes="{ data }">
          <span class="text-gray-900">
            {{ formattedPrice(data.cashSafes) }}
            <small class="text-gray-700 mr-1">{{ $t('words.dinar') }}</small>
          </span>
        </template>

        <template #body-withdrawals="{ data }">
          <span class="text-gray-900">
            {{ formattedPrice(data.withdrawals) }}
            <small class="text-gray-700 mr-1">{{ $t('words.dinar') }}</small>
          </span>
        </template>

        <template #body-transfer="{ data }">
          <span class="text-gray-900">
            {{ formattedPrice(data.transfer) }}
            <small class="text-gray-700 mr-1">{{ $t('words.dinar') }}</small>
          </span>
        </template>

        <template #body-additions="{ data }">
          <span class="text-gray-900">
            {{ formattedPrice(data.additions) }}
            <small class="text-gray-700 mr-1">{{ $t('words.dinar') }}</small>
          </span>
        </template>
      </CustomDataTable>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Card } from 'primevue'
import { useI18n } from 'vue-i18n'
import type { DashboardFinancialRow } from '@/modules/Core/types/model/dashboard'
import CustomDataTable from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.vue'
import type {
  AllowedTypes,
  Column,
} from '@/modules/Core/components/shared/CustomDataTable/CustomDataTable.types'
import { formattedPrice } from '@/modules/Core/utils'

defineProps<{
  rows: DashboardFinancialRow[]
}>()

const { t } = useI18n()

const columns = computed<Column<AllowedTypes>[]>(() => [
  {
    field: 'period',
    header: t('dashboard.financialTable.date'),
    sortable: false,
  },
  {
    field: 'cashSafes',
    header: t('dashboard.financialTable.cashSafes'),
    sortable: false,
  },
  {
    field: 'withdrawals',
    header: t('dashboard.financialTable.withdrawals'),
    sortable: false,
  },
  {
    field: 'transfer',
    header: t('dashboard.financialTable.transfer'),
    sortable: false,
  },
  {
    field: 'additions',
    header: t('dashboard.financialTable.additions'),
    sortable: false,
  },
])
</script>
