<template>
  <DfbDialog :visible="visible" :title="title" :subtitle="subtitle" size="md" @close="emit('close')">
    <template v-if="summary?.length" #summary>
      <div v-for="s in summary" :key="s.label" class="dfb-kpi">
        <span>{{ s.label }}</span>
        <strong><span class="dfb-num">{{ s.value }}</span></strong>
      </div>
    </template>
    <p v-if="hint" class="dfb-hint" style="margin-bottom: 12px">{{ hint }}</p>
    <p v-if="!items.length" class="dfb-state">لا توجد بيانات لهذه الفترة</p>
    <div v-else class="dfb-panel">
      <div class="dfb-history">
        <component
          :is="rowClickable ? 'button' : 'div'"
          v-for="(item, i) in items"
          :key="item.key"
          :type="rowClickable ? 'button' : undefined"
          class="dfb-history-row"
          @click="rowClickable && emit('row', i)"
        >
          <span class="dfb-history-day">
            <strong>{{ item.title ?? weekday(item.date) }}</strong>
            <small><span class="dfb-num">{{ item.date ? dayMonth(item.date) : item.subtitle }}</span></small>
          </span>
          <span v-if="item.amount != null" class="dfb-track" aria-hidden="true">
            <span :style="{ inlineSize: `${barPercent(item.amount)}%` }" />
          </span>
          <span v-else />
          <span class="dfb-history-value" :class="{ 'is-empty': item.empty }"><span class="dfb-num">{{ item.value }}</span></span>
          <span v-if="item.note" class="dfb-history-note">{{ item.note }}</span>
        </component>
      </div>
    </div>
  </DfbDialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import DfbDialog from './NeuDialog.vue'

export interface HistoryItem {
  key: string
  date?: string
  title?: string
  subtitle?: string
  value: string
  amount?: number
  empty?: boolean
  note?: string
}

export interface HistorySummary {
  label: string
  value: string
}

const props = defineProps<{
  visible: boolean
  title: string
  subtitle?: string
  hint?: string
  items: HistoryItem[]
  summary?: HistorySummary[]
  rowClickable?: boolean
}>()

const emit = defineEmits<{ close: []; row: [index: number] }>()

const WEEKDAYS = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
const maxAmount = computed(() => Math.max(1, ...props.items.map(i => Math.abs(i.amount ?? 0))))

function parts(date: string) {
  const [y, m, d] = date.split('-').map(Number)
  return { y, m, d }
}

function weekday(date?: string) {
  if (!date) return ''
  const { y, m, d } = parts(date)
  return WEEKDAYS[new Date(Date.UTC(y, m - 1, d)).getUTCDay()]
}

function dayMonth(date: string) {
  const { y, m, d } = parts(date)
  return `${String(d).padStart(2, '0')}/${String(m).padStart(2, '0')}/${y}`
}

function barPercent(amount: number) {
  return Math.round((Math.abs(amount) / maxAmount.value) * 100)
}
</script>
