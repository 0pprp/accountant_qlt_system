<template>
  <AppPage
    title="نظرة عامة للمبيعات"
    :subtitle="periodLabel || 'مؤشرات يوم العمل وفق قطع 03:00 بغداد'"
  >
    <template #filters>
      <div class="q-filters">
        <AppSelect
          v-model="branchModel"
          label="الفرع"
          placeholder="كل الفروع المسموحة"
          :options="branchOptions"
        />
      </div>
    </template>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="غير مخول بقراءة نظرة عامة المبيعات."
    />
    <DataState
      v-else-if="query.isLoading.value"
      kind="loading"
      title="جاري التحميل"
      message="لا تُعرض أصفار وهمية أثناء الانتظار."
    />
    <DataState
      v-else-if="query.isError.value"
      kind="error"
      title="تعذر التحميل"
      message="تعذر تحميل المؤشرات. ليس «لا نشاط اليوم»."
    >
      <template #actions>
        <AppButton variant="secondary" @click="query.refetch()">إعادة المحاولة</AppButton>
      </template>
    </DataState>
    <div v-else class="q-metrics">
      <button
        v-for="m in metrics"
        :key="m.key"
        type="button"
        class="q-metric"
        :class="{
          'q-metric--clickable': isClickable(m),
          'q-metric--unavailable': m.availability !== 'Available',
        }"
        :disabled="!isClickable(m)"
        @click="onMetricClick(m)"
      >
        <span class="q-metric__label">{{ m.labelAr }}</span>
        <strong class="q-metric__value">{{ displayValue(m) }}</strong>
        <span v-if="m.periodNote" class="q-metric__note">{{ m.periodNote }}</span>
        <span v-if="isClickable(m)" class="q-metric__hint">يفتح التفاصيل</span>
      </button>
    </div>
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppPage, AppSelect, DataState } from '@/modules/Core/components/ui'
import { useSalesOverviewQuery } from '../requests/queries/useSalesOverviewQuery'
import type { SalesOverviewMetric } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesOverview', 'Read') === true)
const branches = computed(() => auth.branches ?? [])
const branchOptions = computed(() =>
  branches.value.map((b) => ({ label: b.name, value: b.id })),
)

const branchModel = ref<number | null>(null)
watch(
  () => route.query.branchId,
  (raw) => {
    const n = Number(raw)
    branchModel.value = Number.isInteger(n) && n > 0 ? n : null
  },
  { immediate: true },
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
