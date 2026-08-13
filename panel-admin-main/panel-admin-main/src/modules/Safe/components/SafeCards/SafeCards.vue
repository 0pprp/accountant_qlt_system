<template>
  <div v-if="isLoading && safeData === undefined">
    <TableSkeletonLoading />
  </div>

  <div v-else class="grid grid-cols-12 justify-between gap-4">
    <div
      v-for="(item, index) in cards"
      :key="index"
      class="col-span-6 md:col-span-3 rounded-2xl bg-white flex align-top p-5 gap-2"
    >
      <div
        class="rounded-lg h-fit p-1"
        :class="`bg-${item.color}/80`"
        :style="{
          backgroundColor: `color-mix(in srgb, var(--color-${item.color}) 10%, transparent)`,
        }"
      >
        <SvgIcon :name="item.iconName" :class="`text-${item.color}`" />
      </div>
      <div class="flex flex-col justify-between">
        <strong>
          {{ $t(item.title) }}
        </strong>
        <small class="text-gray-700">
          {{ $t(item.description) }}
        </small>

        <span class="text-[24px] text-center">
          {{
            item.amount
              ? formattedPrice(item.amount)
              : item.amount === 0
                ? '0'
                : '-'
          }}
          {{ $t('words.dinar') }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props } from './SafeCards.types'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'
import { formattedPrice } from '@/modules/Core/utils'
import TableSkeletonLoading from '@/modules/Core/components/shared/TableSkeletonLoading/TableSkeletonLoading.vue'

const props = defineProps<Props>()

const cards = computed(() => {
  const isCentralSafe = props.safeData?.branchId === null

  return [
    {
      title: 'safe.safeCards.remainingCashAmount.title',
      description: 'safe.safeCards.remainingCashAmount.description',
      iconName: 'payment',
      color: 'purple',
      amount: props.safeData?.remainingCashAmount,
    },
    {
      title: 'safe.safeCards.netBalance.title',
      description: 'safe.safeCards.netBalance.description',
      iconName: 'swap',
      color: 'success',
      amount: props.safeData?.netBalance,
    },
    {
      title: 'safe.safeCards.totalAmount.title',
      description: 'safe.safeCards.totalAmount.description',
      iconName: 'money',
      color: 'warning',
      amount: props.safeData?.totalAmount,
    },
    {
      title: isCentralSafe
        ? 'safe.safeCards.totalBranchesRemainingCashAmount.title'
        : 'safe.safeCards.totalUndeliveredCashAmount.title',
      description: isCentralSafe
        ? 'safe.safeCards.totalBranchesRemainingCashAmount.description'
        : 'safe.safeCards.totalUndeliveredCashAmount.description',
      iconName: 'warehouse',
      color: 'info',
      amount: isCentralSafe
        ? props.safeData?.totalBranchesRemainingCashAmount
        : props.safeData?.totalUndeliveredCashAmount,
    },
  ]
})
</script>
