<template>
  <div
    class="flex items-center justify-around border-b border-line bg-background pt-4 rounded-xl"
  >
    <button
      v-for="tab in tabs"
      :key="tab.value"
      type="button"
      class="relative flex items-center justify-center gap-2 pb-2 text-sm font-medium transition-colors w-full text-center"
      :class="
        modelValue === tab.value
          ? 'text-gray-700 border-b border-gray-700'
          : 'text-gray-700 border-b border-gray-300 hover:text-gray-700'
      "
      @click="selectTab(tab.value)"
    >
      <span>{{ $t(tab.labelKey) }}</span>
      <span
        v-if="tab.showBadge && pendingCount > 0"
        class="inline-flex min-w-5 h-5 items-center justify-center rounded-full bg-error px-1.5 text-xs font-semibold text-white"
      >
        {{ pendingCount }}
      </span>
    </button>
  </div>
</template>

<script setup lang="ts">
import { OrderApprovalStatus } from '@/modules/Order/types/model'

withDefaults(
  defineProps<{
    pendingCount?: number
  }>(),
  {
    pendingCount: 0,
  }
)

const modelValue = defineModel<OrderApprovalStatus>('modelValue', {
  default: OrderApprovalStatus.Approved,
})

const tabs: Array<{
  value: OrderApprovalStatus
  labelKey: string
  showBadge?: boolean
}> = [
  {
    value: OrderApprovalStatus.Approved,
    labelKey: 'sale.approvalTabs.approved',
  },
  {
    value: OrderApprovalStatus.Rejected,
    labelKey: 'sale.approvalTabs.rejected',
  },
  {
    value: OrderApprovalStatus.Pending,
    labelKey: 'sale.approvalTabs.pending',
    showBadge: true,
  },
]

function selectTab(value: OrderApprovalStatus) {
  modelValue.value = value
}
</script>
