<script setup lang="ts">
import { computed } from 'vue'
import { METRIC_OPTIONS } from '../domain/endpoints'
import type { ManagementSummaryMetricKey } from '../domain/types'
import { useAuthStore } from '@/modules/Auth/store'
import { AppSelect } from '@/modules/Core/components/ui'

const props = defineProps<{
  year: number
  month: number
  metric: ManagementSummaryMetricKey
  branchId: number | null
}>()

const emit = defineEmits<{
  patch: [
    partial: Partial<{
      year: number
      month: number
      metric: ManagementSummaryMetricKey
      branchId: number | null
    }>,
  ]
}>()

const auth = useAuthStore()
const branches = computed(() => auth.branches ?? [])

const years = computed(() => {
  const y = new Date().getFullYear()
  return Array.from({ length: 8 }, (_, i) => y - 5 + i)
})

const months = [
  { value: 1, label: 'كانون الثاني' },
  { value: 2, label: 'شباط' },
  { value: 3, label: 'آذار' },
  { value: 4, label: 'نيسان' },
  { value: 5, label: 'أيار' },
  { value: 6, label: 'حزيران' },
  { value: 7, label: 'تموز' },
  { value: 8, label: 'آب' },
  { value: 9, label: 'أيلول' },
  { value: 10, label: 'تشرين الأول' },
  { value: 11, label: 'تشرين الثاني' },
  { value: 12, label: 'كانون الأول' },
]

const metricOptions = computed(() =>
  METRIC_OPTIONS.map((m) => ({ label: m.title, value: m.value })),
)
const yearOptions = computed(() => years.value.map((y) => ({ label: String(y), value: y })))
const branchOptions = computed(() => [
  { label: 'جميع الفروع المسموحة', value: null as number | null },
  ...branches.value.map((b) => ({ label: b.name, value: b.id })),
])

const metricModel = computed({
  get: () => props.metric,
  set: (v: string | number | null | boolean) =>
    emit('patch', { metric: v as ManagementSummaryMetricKey }),
})
const monthModel = computed({
  get: () => props.month,
  set: (v: string | number | null | boolean) => emit('patch', { month: Number(v) }),
})
const yearModel = computed({
  get: () => props.year,
  set: (v: string | number | null | boolean) => emit('patch', { year: Number(v) }),
})
const branchModel = computed({
  get: () => props.branchId,
  set: (v: string | number | null | boolean) =>
    emit('patch', { branchId: v == null || v === '' ? null : Number(v) }),
})
</script>

<template>
  <div class="q-filters">
    <AppSelect v-model="metricModel" label="المقياس" :options="metricOptions" />
    <AppSelect v-model="monthModel" label="الشهر" :options="months" />
    <AppSelect v-model="yearModel" label="السنة" :options="yearOptions" />
    <AppSelect
      v-model="branchModel"
      label="المحافظة / الفرع"
      placeholder="جميع الفروع المسموحة"
      :options="branchOptions"
    />
  </div>
</template>
