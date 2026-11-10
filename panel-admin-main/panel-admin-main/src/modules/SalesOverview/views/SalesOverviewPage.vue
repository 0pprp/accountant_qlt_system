<template>
  <div class="so-page" dir="rtl">
    <header class="so-head">
      <h1 class="so-title">نظرة عامة للمبيعات</h1>
      <p v-if="periodLabel" class="so-period">{{ periodLabel }}</p>
    </header>

    <div class="so-filters">
      <label class="so-field">
        <span>الفرع</span>
        <select v-model="branchModel" class="so-select">
          <option :value="null">كل الفروع المسموحة</option>
          <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
        </select>
      </label>
    </div>

    <div v-if="!canRead" class="so-state so-state--denied">غير مخول بقراءة نظرة عامة المبيعات.</div>
    <div v-else-if="query.isLoading.value" class="so-state">جاري التحميل…</div>
    <div v-else-if="query.isError.value" class="so-state so-state--error">
      تعذر تحميل المؤشرات. ليس «لا نشاط اليوم».
    </div>
    <div v-else class="so-grid">
      <button
        v-for="m in metrics"
        :key="m.key"
        type="button"
        class="so-metric"
        :class="{
          'so-metric--clickable': isClickable(m),
          'so-metric--unavailable': m.availability !== 'Available',
        }"
        :disabled="!isClickable(m)"
        @click="onMetricClick(m)"
      >
        <span class="so-metric__label">{{ m.labelAr }}</span>
        <strong class="so-metric__value">{{ displayValue(m) }}</strong>
        <span v-if="m.periodNote" class="so-metric__note">{{ m.periodNote }}</span>
        <span v-if="isClickable(m)" class="so-metric__hint">يفتح التفاصيل</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useSalesOverviewQuery } from '../requests/queries/useSalesOverviewQuery'
import type { SalesOverviewMetric } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesOverview', 'Read') === true)
const branches = computed(() => auth.branches ?? [])

const branchModel = ref<number | null>(null)
watch(
  () => route.query.branchId,
  (raw) => {
    const n = Number(raw)
    branchModel.value = Number.isInteger(n) && n > 0 ? n : null
  },
  { immediate: true }
)
watch(branchModel, (v) => {
  const q = { ...route.query } as Record<string, string>
  if (v != null && v > 0) q.branchId = String(v)
  else delete q.branchId
  void router.replace({ query: q })
})

const branchId = computed(() => branchModel.value)
const query = useSalesOverviewQuery(branchId, canRead)

const metrics = computed(() => query.data.value?.metrics ?? [])
const periodLabel = computed(() => {
  const p = query.data.value?.period
  if (!p) return ''
  return `يوم العمل ${p.businessDateIraq} · ${p.timeZoneId}`
})

function displayValue(m: SalesOverviewMetric) {
  if (m.availability === 'Forbidden') return 'غير مخول'
  if (m.availability === 'Unavailable') return m.unavailableReason || 'مصدر غير متاح'
  if (m.value == null) return '—'
  return String(m.value)
}

function isClickable(m: SalesOverviewMetric) {
  return m.availability === 'Available' && m.link?.allowed === true
}

function onMetricClick(m: SalesOverviewMetric) {
  if (!isClickable(m) || !m.link) return
  void router.push({ path: m.link.path, query: m.link.query ?? undefined })
}
</script>

<style scoped>
.so-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 0.25rem 0 1.5rem;
  background: #f8f8f8;
  min-height: 100%;
  font-family: ElMessiri, 'El Messiri', sans-serif;
  font-size: 16px;
  color: #00201e;
}
.so-title {
  margin: 0;
  font-size: 1.55rem;
  font-weight: 700;
  color: #00201e;
}
.so-period {
  margin: 0.25rem 0 0;
  font-size: 14px;
  color: #006a64;
}
.so-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}
.so-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 12rem;
  font-size: 14px;
}
.so-select {
  min-height: 44px;
  border: 1px solid #c5d4d2;
  border-radius: 8px;
  padding: 0 0.75rem;
  background: #fff;
  font: inherit;
  color: inherit;
}
.so-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
  gap: 0.75rem;
}
.so-metric {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.35rem;
  text-align: start;
  min-height: 7rem;
  padding: 1rem;
  border: 1px solid #d7e3e1;
  border-radius: 10px;
  background: #fff;
  color: inherit;
  font: inherit;
}
.so-metric--clickable {
  cursor: pointer;
  border-color: #006a64;
}
.so-metric--clickable:hover {
  background: #f0faf8;
}
.so-metric--unavailable {
  opacity: 0.85;
}
.so-metric__label {
  font-size: 14px;
  color: #006a64;
}
.so-metric__value {
  font-size: 1.75rem;
  font-weight: 700;
  line-height: 1.2;
}
.so-metric__note,
.so-metric__hint {
  font-size: 12px;
  color: #5a6f6c;
}
.so-state {
  padding: 1rem;
  border-radius: 8px;
  background: #fff;
  border: 1px solid #d7e3e1;
}
.so-state--error {
  border-color: #b91c1c;
  color: #991b1b;
}
.so-state--denied {
  border-color: #a16207;
  color: #854d0e;
}
@media (max-width: 768px) {
  .so-grid {
    grid-template-columns: 1fr;
  }
}
</style>
