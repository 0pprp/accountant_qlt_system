<template>
  <div v-if="saleInfo" class="flex flex-col h-full gap-4">
    <div
      class="flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-line bg-white px-6 py-4"
    >
      <div class="flex flex-wrap items-center gap-3">
        <span :class="approvalStatusBadgeClass">
          {{ approvalStatusLabel }}
        </span>
        <span :class="executionStatusBadgeClass">
          {{ executionStatusLabel }}
        </span>
      </div>

      <OrderApprovalActions
        v-if="canUpdateOrder && orderId"
        :id="orderId"
        :item="orderStatusItem"
      />
    </div>

    <div class="flex flex-row h-full gap-6 flex-1 min-h-0">
      <div class="w-full border border-line rounded-2xl overflow-auto">
        <SaleTabs :saleInfo="saleInfo" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import SaleTabs from '../components/SaleTabs/SaleTabs.vue'
import OrderApprovalActions from '../components/OrderApprovalActions/OrderApprovalActions.vue'
import { useSaleByIdDataQuery } from '../requests/queries'
import type { SaleInfo } from '../types/model'
import {
  getApprovalStatusFromRow,
  getApprovalStatusLabel,
  getExecutionStatusFromRow,
  getExecutionStatusLabel,
} from '@/modules/Order/utils/orderStatus'
import { OrderApprovalStatus } from '@/modules/Order/types/model'
import { usePermission } from '@/modules/Core/composable/usePermission'

const route = useRoute()
const { t } = useI18n()
const { can } = usePermission()
const canUpdateOrder = can('Order', 'Update')

const saleId = computed(() => Number(route.params.saleId))

const { data: saleInfo } = useSaleByIdDataQuery(saleId)

const orderId = computed(() => saleInfo.value?.orderInfo.id ?? 0)

function buildOrderStatusItem(info: SaleInfo): Record<string, unknown> {
  return {
    approvalStatus: info.approvalStatus ?? info.orderInfo.approvalStatus,
    executionStatus: info.executionStatus ?? info.orderInfo.executionStatus,
    step: info.step ?? info.orderInfo.step,
  }
}

const orderStatusItem = computed(() =>
  saleInfo.value ? buildOrderStatusItem(saleInfo.value) : {}
)

const approvalStatusLabel = computed(() =>
  getApprovalStatusLabel(getApprovalStatusFromRow(orderStatusItem.value), t)
)

const executionStatusLabel = computed(() =>
  getExecutionStatusLabel(getExecutionStatusFromRow(orderStatusItem.value), t)
)

const approvalStatusBadgeClass = computed(() => {
  const base = 'inline-flex px-3 py-1 rounded-xl text-sm border'

  switch (getApprovalStatusFromRow(orderStatusItem.value)) {
    case OrderApprovalStatus.Approved:
      return `${base} bg-success-50 border-success text-success`
    case OrderApprovalStatus.Rejected:
      return `${base} bg-error-50 border-error text-error`
    case OrderApprovalStatus.Pending:
      return `${base} bg-warning-50 border-warning text-warning`
    default:
      return `${base} bg-gray-50 border-line text-gray-700`
  }
})

const executionStatusBadgeClass = computed(
  () =>
    'inline-flex px-3 py-1 rounded-xl text-sm border bg-gray-50 border-line text-gray-700'
)
</script>
