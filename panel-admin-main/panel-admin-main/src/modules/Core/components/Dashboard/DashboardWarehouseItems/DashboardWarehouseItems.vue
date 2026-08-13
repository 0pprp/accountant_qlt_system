<template>
  <Card class="rounded-2xl bg-white border-2 border-line p-4 shadow-sm h-full">
    <template #content>
      <div class="flex flex-col gap-4 h-full">
        <div class="flex items-center gap-2 justify-between w-full">
          <p class="text-base font-medium text-gray-700 w-full text-right">
            {{ $t('dashboard.warehouse.title') }}
          </p>

          <SvgIcon name="shop" class="text-gray-700 w-6 h-6" />
        </div>

        <div
          class="flex justify-center items-center relative"
          style="height: 220px"
        >
          <Chart
            type="doughnut"
            :data="chartData"
            :options="chartOptions"
            class="w-full h-full"
          />
        </div>

        <div v-if="selectedBranch" class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-2.5 h-2.5 rounded-full bg-primary shrink-0" />
            <span class="text-sm text-gray-900">{{ selectedBranch.name }}</span>
          </div>
          <span class="text-sm text-gray-700">
            {{ productsCount }} {{ $t('dashboard.warehouse.items') }}
          </span>
        </div>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Card } from 'primevue'
import Chart from 'primevue/chart'
import { useI18n } from 'vue-i18n'
import SvgIcon from '../../SvgIcon/SvgIcon.vue'
import { useAuthStore } from '@/modules/Auth/store'

const props = defineProps<{
  productsCount: number
}>()

const { t } = useI18n()
const authStore = useAuthStore()

const selectedBranch = computed(() => authStore.selectedBranch)

const chartData = computed(() => ({
  labels: [t('dashboard.warehouse.items')],
  datasets: [
    {
      data: [
        props.productsCount,
        Math.max(1, Math.round(props.productsCount * 0.3)),
      ],
      backgroundColor: ['#006a64', '#F4FBF9'],
      borderWidth: 0,
      borderRadius: 8,
    },
  ],
}))

const chartOptions = computed(() => ({
  plugins: {
    legend: { display: false },
    tooltip: { enabled: false },
  },
  cutout: '75%',
  maintainAspectRatio: false,
}))
</script>
