<template>
  <Card class="rounded-2xl bg-white border-2 border-line p-4 shadow-sm h-full">
    <template #content>
      <div class="flex flex-col gap-4 h-full">
        <div class="flex items-center gap-2 justify-between w-full">
          <p class="text-base font-medium text-gray-700 w-full text-right">
            {{ $t('dashboard.monthlyChart.title') }}
          </p>

          <SvgIcon name="chartLine" class="text-gray-700 w-6 h-6" />
        </div>

        <div class="flex flex-col lg:flex-row gap-6 items-stretch flex-1">
          <div class="flex flex-col gap-6 justify-center shrink-0">
            <div
              v-for="legend in legends"
              :key="legend.key"
              class="flex flex-col items-start gap-1"
            >
              <div class="flex items-center gap-2">
                <span
                  class="w-2.5 h-2.5 rounded-full shrink-0"
                  :style="{ backgroundColor: legend.color }"
                />
                <span class="text-base text-gray-700">{{ legend.label }}</span>
              </div>
              <div class="flex items-center gap-2">
                <span class="text-[24px] text-gray-900">
                  {{ formattedPrice(legend.total) }}
                </span>
                <span class="text-xs text-gray-700 tracking-wide">
                  {{ $t('dashboard.monthlyChart.currency') }}
                </span>
              </div>
            </div>
          </div>

          <div
            class="flex-1 relative overflow-hidden mx-2"
            style="height: 300px"
          >
            <Chart
              type="line"
              :data="chartData"
              :options="chartOptions"
              class="h-full w-full"
            />
            <!-- <div
              ref="tooltipRef"
              class="chart-tooltip pointer-events-none absolute z-10 opacity-0"
            /> -->
          </div>
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
import type { DashboardMonthlyChartData } from '@/modules/Core/types/model/dashboard'
import { formattedPrice } from '@/modules/Core/utils'

const props = defineProps<{
  data: DashboardMonthlyChartData
}>()

const { t } = useI18n()

const legends = computed(() => [
  {
    key: 'sales',
    label: t('dashboard.monthlyChart.sales'),
    color: '#34c759',
    total: props.data.totals.sales,
  },
  {
    key: 'purchases',
    label: t('dashboard.monthlyChart.purchases'),
    color: '#e3427a',
    total: props.data.totals.purchases,
  },
  {
    key: 'payments',
    label: t('dashboard.monthlyChart.payments'),
    color: '#007aff',
    total: props.data.totals.payments,
  },
])

const chartData = computed(() => ({
  labels: props.data.labels,
  datasets: [
    {
      label: t('dashboard.monthlyChart.sales'),
      data: props.data.sales,
      borderColor: '#34c759',
      backgroundColor: 'transparent',
      tension: 0.4,
      pointRadius: 0,
      pointHoverRadius: 5,
      pointHoverBackgroundColor: '#34c759',
      pointHoverBorderColor: '#34c759',
      borderWidth: 2,
    },
    {
      label: t('dashboard.monthlyChart.purchases'),
      data: props.data.purchases,
      borderColor: '#e3427a',
      backgroundColor: 'transparent',
      tension: 0.4,
      pointRadius: 0,
      pointHoverRadius: 5,
      pointHoverBackgroundColor: '#e3427a',
      pointHoverBorderColor: '#e3427a',
      borderWidth: 2,
    },
    {
      label: t('dashboard.monthlyChart.payments'),
      data: props.data.payments,
      borderColor: '#007aff',
      backgroundColor: 'transparent',
      tension: 0.4,
      pointRadius: 0,
      pointHoverRadius: 5,
      pointHoverBackgroundColor: '#007aff',
      pointHoverBorderColor: '#007aff',
      borderWidth: 2,
    },
  ],
}))

function createVerticalGridGradient(chart: {
  ctx: CanvasRenderingContext2D
  chartArea?: { top: number; bottom: number }
}) {
  const { ctx, chartArea } = chart
  if (!chartArea) return 'transparent'

  const gradient = ctx.createLinearGradient(
    0,
    chartArea.bottom,
    0,
    chartArea.top
  )
  gradient.addColorStop(0, 'rgba(217, 217, 217, 0.9)')
  gradient.addColorStop(0.35, 'rgba(217, 217, 217, 0.25)')
  gradient.addColorStop(1, 'rgba(217, 217, 217, 0)')
  return gradient
}

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index',
    intersect: false,
    axis: 'x',
  },
  plugins: {
    legend: { display: false },
    tooltip: {
      mode: 'index',
      intersect: false,
      displayColors: true,
      backgroundColor: '#ffffff',
      titleColor: '#00201e',
      bodyColor: '#00201e',
      borderColor: '#f0effe',
      borderWidth: 1,
      padding: 12,
      cornerRadius: 8,
      boxPadding: 6,
      callbacks: {
        label: (context: {
          dataset?: { label?: string; borderColor?: string }
          parsed?: { y?: number }
        }) => {
          const label = context.dataset?.label ?? ''
          const value = formattedPrice(context.parsed?.y ?? 0)
          return `${label}: ${value} ${t('words.dinar')}`
        },
        title: (items: Array<{ label?: string }>) => items[0]?.label ?? '',
        labelColor: (context: { dataset?: { borderColor?: string } }) => ({
          borderColor: context.dataset?.borderColor ?? '#000000',
          backgroundColor: context.dataset?.borderColor ?? '#000000',
          borderWidth: 0,
          borderRadius: 4,
        }),
      },
    },
  },
  scales: {
    x: {
      reverse: true,
      grid: {
        display: true,
        drawOnChartArea: true,
        drawTicks: false,
        lineWidth: 1,
        color: (context: {
          chart: {
            ctx: CanvasRenderingContext2D
            chartArea?: { top: number; bottom: number }
          }
        }) => createVerticalGridGradient(context.chart),
      },
      border: { display: false },
      ticks: {
        color: '#cdcdcd',
        padding: 8,
      },
    },
    y: {
      display: false,
      grid: { display: false },
    },
  },
}))
</script>
