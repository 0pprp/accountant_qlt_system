<script setup lang="ts">
import { computed } from 'vue'
import type { ManagementSummaryMatrixResponse } from '../domain/types'

const props = defineProps<{
  matrix: ManagementSummaryMatrixResponse
  canDrilldown: boolean
}>()

const emit = defineEmits<{
  cellClick: [payload: { day: number; branchId: number; amount: number }]
}>()

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat('ar-IQ', {
    maximumFractionDigits: 0,
  }).format(n)
}

const cellMap = computed(() => {
  const map = new Map<string, number>()
  for (const c of props.matrix.cells) {
    map.set(`${c.day}:${c.branchId}`, c.amount)
  }
  return map
})

const dayTotalMap = computed(() => {
  const map = new Map<number, number>()
  for (const d of props.matrix.dayTotals) map.set(d.day, d.amount)
  return map
})

const branchTotalMap = computed(() => {
  const map = new Map<number, number>()
  for (const b of props.matrix.branchTotals) map.set(b.branchId, b.amount)
  return map
})

function cellAmount(day: number, branchId: number) {
  return cellMap.value.get(`${day}:${branchId}`) ?? 0
}

function onCell(day: number, branchId: number) {
  const amount = cellAmount(day, branchId)
  if (!props.canDrilldown || amount <= 0) return
  emit('cellClick', { day, branchId, amount })
}
</script>

<template>
  <div class="ms-table-scroll">
    <table class="ms-table">
      <thead>
        <tr>
          <th class="ms-th-day">اليوم</th>
          <th
            v-for="b in matrix.branches"
            :key="b.id"
          >
            {{ b.name }}
          </th>
          <th class="ms-th-sum">المجموع</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="day in matrix.daysInMonth"
          :key="day"
        >
          <td class="ms-td-day">{{ day }}</td>
          <td
            v-for="b in matrix.branches"
            :key="`${day}-${b.id}`"
          >
            <button
              type="button"
              class="ms-cell-btn"
              :class="{
                'is-clickable':
                  canDrilldown && cellAmount(day, b.id) > 0,
              }"
              :disabled="!(canDrilldown && cellAmount(day, b.id) > 0)"
              @click="onCell(day, b.id)"
            >
              {{ fmt(cellAmount(day, b.id)) }}
            </button>
          </td>
          <td class="ms-td-sum">{{ fmt(dayTotalMap.get(day) ?? 0) }}</td>
        </tr>
        <tr class="ms-foot-row">
          <td class="ms-td-day">المجموع</td>
          <td
            v-for="b in matrix.branches"
            :key="`t-${b.id}`"
            class="ms-td-sum"
          >
            {{ fmt(branchTotalMap.get(b.id) ?? 0) }}
          </td>
          <td class="ms-td-sum">{{ fmt(matrix.monthTotal) }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
