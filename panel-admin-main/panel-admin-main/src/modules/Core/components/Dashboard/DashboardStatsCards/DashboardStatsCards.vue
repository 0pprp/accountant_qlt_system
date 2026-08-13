<template>
  <div class="grid grid-cols-12 gap-4">
    <div
      v-for="card in cards"
      :key="card.key"
      class="col-span-12 md:col-span-4 rounded-2xl bg-white border-2 border-line p-4 shadow-sm"
    >
      <div class="flex flex-col gap-4 items-start">
        <div class="flex items-center gap-2.5">
          <div
            class="rounded-lg h-fit p-1"
            :style="{
              backgroundColor: `color-mix(in srgb, var(--color-${card.color}) 10%, transparent)`,
            }"
          >
            <SvgIcon :name="card.iconName" :class="`text-${card.color}`" />
          </div>
          <div class="flex flex-col items-start">
            <strong class="text-base text-gray-900">
              {{ $t(card.title) }}
            </strong>
            <small class="text-gray-700 text-xs">
              {{ $t(card.description) }}
            </small>
          </div>
        </div>

        <div class="flex items-center gap-2 justify-start w-full">
          <span
            class="text-sm flex items-center gap-0.5"
            :class="trendClass(card.trend)"
          >
            <span v-if="card.trend === 'up'">↑</span>
            <span v-else-if="card.trend === 'down'">↓</span>
            {{ formatChange(card.change) }}
          </span>
          <span class="text-[24px] text-gray-900">
            {{ card.count }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type {
  DashboardStats,
  DashboardTrend,
} from '@/modules/Core/types/model/dashboard'
import SvgIcon from '@/modules/Core/components/SvgIcon/SvgIcon.vue'

const props = defineProps<{
  stats: DashboardStats
}>()

const cards = computed(() => [
  {
    key: 'customers',
    title: 'dashboard.stats.customers.title',
    description: 'dashboard.stats.customers.description',
    iconName: 'customer',
    color: 'purple',
    count: props.stats.customers.count,
    change: props.stats.customers.change,
    trend: props.stats.customers.trend,
  },
  {
    key: 'representatives',
    title: 'dashboard.stats.representatives.title',
    description: 'dashboard.stats.representatives.description',
    iconName: 'boldUserPlus',
    color: 'info',
    count: props.stats.representatives.count,
    change: props.stats.representatives.change,
    trend: props.stats.representatives.trend,
  },
  {
    key: 'users',
    title: 'dashboard.stats.users.title',
    description: 'dashboard.stats.users.description',
    iconName: 'boldUser',
    color: 'success',
    count: props.stats.users.count,
    change: props.stats.users.change,
    trend: props.stats.users.trend,
  },
])

function formatChange(change: number) {
  return `${change.toFixed(2)}%`
}

function trendClass(trend: DashboardTrend) {
  if (trend === 'up') return 'text-success'
  if (trend === 'down') return 'text-error'
  return 'text-gray-300'
}
</script>
