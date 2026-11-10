<template>
  <div v-if="open" class="se-dialog-backdrop" @click.self="emit('close')">
    <div class="se-dialog" role="dialog" aria-modal="true">
      <header class="se-dialog__head">
        <h2>{{ employeeName || 'تقييم الموظف' }}</h2>
        <button type="button" class="se-btn" @click="emit('close')">إغلاق</button>
      </header>

      <div v-if="busy" class="se-state">جاري التحميل…</div>
      <div v-else-if="error" class="se-state se-state--error">{{ error }}</div>
      <template v-else-if="summary">
        <div class="se-scores">
          <div>
            <span>الحقيقي</span>
            <strong>{{ fmt(summary.latestRating) }}</strong>
            <em>{{ summary.ratingLabel || '—' }}</em>
          </div>
          <div>
            <span>الوهمي</span>
            <strong>{{ fmt(summary.phantomLatestRating) }}</strong>
          </div>
        </div>

        <div class="se-buckets">
          <button
            v-for="b in buckets"
            :key="b.key"
            type="button"
            class="se-bucket"
            @click="emit('open-bucket', b.key)"
          >
            <span>{{ b.label }}</span>
            <strong>{{ countOf(b.key) }}</strong>
          </button>
        </div>

        <div v-if="points.length" class="se-chart-wrap">
          <svg :viewBox="`0 0 ${chart.w} ${chart.h}`" class="se-chart" role="img">
            <path :d="chart.path" fill="none" stroke="#006a64" stroke-width="2" />
            <circle
              v-for="c in chart.coords"
              :key="c.date"
              :cx="c.x"
              :cy="c.y"
              r="3"
              fill="#00201e"
            />
          </svg>
          <div class="se-chart-meta">{{ chart.first }} → {{ chart.last }} ({{ chart.count }} يوم)</div>
        </div>

        <div v-if="bucketCustomers" class="se-customers">
          <h3>زبائن: {{ bucketLabel }}</h3>
          <p v-if="!bucketCustomers.length" class="se-state">لا زبائن في هذه الفئة.</p>
          <ul v-else>
            <li v-for="c in bucketCustomers" :key="c.customerId">
              <strong>{{ c.customerName }}</strong>
              <span>{{ c.ratingLabel }}</span>
              <span v-if="!c.amountsRedacted">
                متبقي {{ money(c.amountRemaining) }} / إجمالي {{ money(c.amountTotal) }}
              </span>
              <span v-else>المبالغ محجوبة</span>
            </li>
          </ul>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type {
  SalesEmployeeRatingCustomer,
  SalesEmployeeRatingSummary,
} from '../domain/types'

const props = defineProps<{
  open: boolean
  busy: boolean
  error: string
  employeeName: string
  summary: SalesEmployeeRatingSummary | null
  bucketCustomers: SalesEmployeeRatingCustomer[] | null
  bucketLabel: string
}>()

const emit = defineEmits<{
  close: []
  'open-bucket': [key: string]
}>()

const buckets = [
  { key: 'excellent', label: 'ممتاز' },
  { key: 'good', label: 'جيد' },
  { key: 'weak', label: 'ضعيف' },
  { key: 'rejected', label: 'متلكئ' },
  { key: 'bad', label: 'سيء' },
]

function countOf(key: string) {
  const s = props.summary
  if (!s) return 0
  if (key === 'excellent') return s.excellentCount
  if (key === 'good') return s.goodCount
  if (key === 'weak') return s.weakCount
  if (key === 'rejected') return s.rejectedCount
  return s.badCount
}

function fmt(v: number | null | undefined) {
  if (v == null || Number.isNaN(Number(v))) return '—'
  return Number(v).toFixed(2)
}

function money(v: number | null | undefined) {
  if (v == null || !Number.isFinite(Number(v))) return '—'
  return `${Number(v).toLocaleString('en-US')} د.ع`
}

const points = computed(() => props.summary?.points ?? [])

const chart = computed(() => {
  const pts = points.value
  const w = 720
  const h = 180
  if (pts.length < 1) {
    return { path: '', coords: [] as { x: number; y: number; date: string }[], w, h, first: '', last: '', count: 0 }
  }
  const vals = pts.map((p) => Number(p.value) || 0)
  const min = Math.min(...vals)
  const max = Math.max(...vals)
  const span = max - min || 1
  const padX = 10
  const padY = 14
  const n = vals.length
  const coords = vals.map((v, i) => {
    const x = n === 1 ? w / 2 : padX + (i / (n - 1)) * (w - padX * 2)
    const y = h - padY - ((v - min) / span) * (h - padY * 2)
    return { x, y, date: pts[i]?.date ?? '' }
  })
  const path = coords.map((c, i) => `${i === 0 ? 'M' : 'L'}${c.x.toFixed(1)},${c.y.toFixed(1)}`).join(' ')
  return {
    path,
    coords,
    w,
    h,
    first: pts[0]?.date ?? '',
    last: pts[pts.length - 1]?.date ?? '',
    count: n,
  }
})
</script>

<style scoped>
.se-dialog-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 40;
  padding: max(1rem, env(safe-area-inset-top)) 1rem max(1rem, env(safe-area-inset-bottom));
}
.se-dialog {
  width: min(720px, 100%);
  max-height: 90vh;
  overflow: auto;
  background: #fff;
  border-radius: 12px;
  padding: 1rem;
  color: #00201e;
  font-family: ElMessiri, 'El Messiri', sans-serif;
}
.se-dialog__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}
.se-dialog__head h2 {
  margin: 0;
  font-size: 1.2rem;
}
.se-btn {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font: inherit;
  cursor: pointer;
}
.se-scores {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}
.se-scores div {
  background: #f8f8f8;
  border-radius: 8px;
  padding: 0.75rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}
.se-buckets {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(6.5rem, 1fr));
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.se-bucket {
  min-height: 44px;
  border: 1px solid #c5d4d2;
  border-radius: 8px;
  background: #fff;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  font: inherit;
  cursor: pointer;
}
.se-chart-wrap {
  margin: 0.75rem 0;
}
.se-chart {
  width: 100%;
  height: auto;
  background: #f8f8f8;
  border-radius: 8px;
}
.se-chart-meta {
  font-size: 14px;
  color: #5a6f6c;
}
.se-customers ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.se-customers li {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
  padding: 0.5rem;
  background: #f8f8f8;
  border-radius: 8px;
  font-size: 14px;
}
.se-state {
  padding: 0.75rem;
  border-radius: 8px;
  background: #f8f8f8;
}
.se-state--error {
  color: #991b1b;
  background: #fef2f2;
}
</style>
