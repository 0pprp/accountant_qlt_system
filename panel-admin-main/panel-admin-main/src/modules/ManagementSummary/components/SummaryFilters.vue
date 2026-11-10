<script setup lang="ts">
import { computed } from 'vue'
import { METRIC_OPTIONS } from '../domain/endpoints'
import type { ManagementSummaryMetricKey } from '../domain/types'
import { useAuthStore } from '@/modules/Auth/store'

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
  { value: 1, title: 'كانون الثاني' },
  { value: 2, title: 'شباط' },
  { value: 3, title: 'آذار' },
  { value: 4, title: 'نيسان' },
  { value: 5, title: 'أيار' },
  { value: 6, title: 'حزيران' },
  { value: 7, title: 'تموز' },
  { value: 8, title: 'آب' },
  { value: 9, title: 'أيلول' },
  { value: 10, title: 'تشرين الأول' },
  { value: 11, title: 'تشرين الثاني' },
  { value: 12, title: 'كانون الأول' },
]
</script>

<template>
  <div class="ms-filters">
    <div class="ms-field">
      <label for="ms-metric">المقياس</label>
      <select
        id="ms-metric"
        :value="props.metric"
        @change="
          emit('patch', {
            metric: ($event.target as HTMLSelectElement)
              .value as ManagementSummaryMetricKey,
          })
        "
      >
        <option
          v-for="m in METRIC_OPTIONS"
          :key="m.value"
          :value="m.value"
        >
          {{ m.title }}
        </option>
      </select>
    </div>

    <div class="ms-field">
      <label for="ms-month">الشهر</label>
      <select
        id="ms-month"
        :value="props.month"
        @change="
          emit('patch', {
            month: Number(($event.target as HTMLSelectElement).value),
          })
        "
      >
        <option
          v-for="m in months"
          :key="m.value"
          :value="m.value"
        >
          {{ m.title }}
        </option>
      </select>
    </div>

    <div class="ms-field">
      <label for="ms-year">السنة</label>
      <select
        id="ms-year"
        :value="props.year"
        @change="
          emit('patch', {
            year: Number(($event.target as HTMLSelectElement).value),
          })
        "
      >
        <option
          v-for="y in years"
          :key="y"
          :value="y"
        >
          {{ y }}
        </option>
      </select>
    </div>

    <div class="ms-field">
      <label for="ms-branch">المحافظة / الفرع</label>
      <select
        id="ms-branch"
        :value="props.branchId ?? 'all'"
        @change="
          emit('patch', {
            branchId:
              ($event.target as HTMLSelectElement).value === 'all'
                ? null
                : Number(($event.target as HTMLSelectElement).value),
          })
        "
      >
        <option value="all">جميع الفروع المسموحة</option>
        <option
          v-for="b in branches"
          :key="b.id"
          :value="b.id"
        >
          {{ b.name }}
        </option>
      </select>
    </div>
  </div>
</template>
