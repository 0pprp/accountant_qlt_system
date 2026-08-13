<template>
  <div>
    <TheHeader>
      <template #create>
        <RouterLink
          v-if="canCreateOrder"
          :to="{ name: 'SaleCreateRoute' }"
          class="bg-primary rounded-2xl text-white inline-flex items-center gap-2 px-4 py-2"
        >
          {{ $t('sale.createSale') }}
          <SvgIcon name="addCircle" />
        </RouterLink>
      </template>

      <template #export>
        <ExcelReport
          v-if="canReadOrder"
          endpoint="admin/Orders/excel-report"
          :filename="`sales-report-${new Date().toISOString().split('T')[0]}.xlsx`"
          :params="excelParams"
        />
      </template>

      <template #calendar>
        <HeaderDatePicker />
      </template>
    </TheHeader>

    <SaleList v-if="canReadOrder" ref="saleListRef" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import SaleList from '../components/SaleList/SaleList.vue'
import TheHeader from '@/modules/Core/components/layout/TheHeader/TheHeader.vue'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import ExcelReport from '@/modules/Core/components/base/ExcelReport/ExcelReport.vue'
import HeaderDatePicker from '@/modules/Core/components/base/HeaderDatePicker/HeaderDatePicker.vue'
import type { ExcelReportParamValue } from '@/modules/Core/components/base/ExcelReport/ExcelReport.types'
import { usePermission } from '@/modules/Core/composable/usePermission'

const { can } = usePermission()
const canReadOrder = can('Order', 'Read')
const canCreateOrder = can('Order', 'Create')

const saleListRef = ref<InstanceType<typeof SaleList> | null>(null)

const excelParams = computed(() => {
  const searchTermValue = saleListRef.value?.searchTerm
  const searchTermString = Array.isArray(searchTermValue)
    ? (searchTermValue[0] ?? '')
    : searchTermValue
      ? String(searchTermValue)
      : ''

  const params: Record<string, ExcelReportParamValue> = {
    branchId: saleListRef.value?.branchId,
    searchTerm: searchTermString,
  }

  if (saleListRef.value?.customerId !== undefined) {
    params.customerId = saleListRef.value.customerId
  }

  if (saleListRef.value?.columnsForApi) {
    params.columns = saleListRef.value.columnsForApi
  }

  if (saleListRef.value?.approvalStatus !== undefined) {
    params.ApprovalStatus = saleListRef.value.approvalStatus
  }

  if (
    saleListRef.value?.stepFilter !== null &&
    saleListRef.value?.stepFilter !== undefined
  ) {
    params.Step = saleListRef.value.stepFilter
  }

  if (saleListRef.value?.startDate) {
    params.startDate = saleListRef.value.startDate
  }

  if (saleListRef.value?.endDate) {
    params.endDate = saleListRef.value.endDate
  }

  return params
})
</script>
