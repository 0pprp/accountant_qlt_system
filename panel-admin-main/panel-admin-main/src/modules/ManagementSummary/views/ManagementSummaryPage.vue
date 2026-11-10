<script setup lang="ts">
import { computed, ref } from 'vue'
import SummaryFilters from '../components/SummaryFilters.vue'
import SummaryMatrix from '../components/SummaryMatrix.vue'
import SummaryDetailsDialog from '../components/SummaryDetailsDialog.vue'
import { useSummaryFilters } from '../composables/useSummaryFilters'
import { useSummaryMatrixQuery } from '../requests/queries/useSummaryMatrixQuery'
import { useAuthStore } from '@/modules/Auth/store'
import '../components/management-summary.css'

const auth = useAuthStore()
const { filters, patch } = useSummaryFilters()

const canRead = computed(
  () => auth.hasPermission('ManagementSummary', 'Read') === true
)
const canDrilldown = computed(
  () => auth.hasPermission('ManagementSummary', 'Drilldown') === true
)

const year = computed(() => filters.value.year)
const month = computed(() => filters.value.month)
const metric = computed(() => filters.value.metric)
const branchId = computed(() => filters.value.branchId)

const matrixQuery = useSummaryMatrixQuery(
  year,
  month,
  metric,
  branchId,
  canRead
)

const drillOpen = ref(false)
const drillDay = ref<number | null>(null)
const drillBranchId = ref<number | null>(null)

function openDrill(payload: { day: number; branchId: number }) {
  if (!canDrilldown.value) return
  drillDay.value = payload.day
  drillBranchId.value = payload.branchId
  drillOpen.value = true
}

function closeDrill() {
  drillOpen.value = false
}

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat('ar-IQ', {
    maximumFractionDigits: 0,
  }).format(n)
}

const matrix = computed(() => matrixQuery.data.value)
</script>

<template>
  <div class="ms-page">
    <h1 class="ms-title">الملخص</h1>
    <p class="ms-subtitle">
      مصفوفة يومية حسب الفروع المسموحة — ليست ملخص المبيعات المحدود بسعر البيع
    </p>

    <SummaryFilters
      :year="filters.year"
      :month="filters.month"
      :metric="filters.metric"
      :branch-id="filters.branchId"
      @patch="patch"
    />

    <template v-if="canRead">
      <div
        v-if="matrixQuery.isError.value"
        class="ms-error"
        role="alert"
      >
        تعذر تحميل الملخص. لا تُعرض أصفار بدل الخطأ.
      </div>

      <div
        v-else-if="matrixQuery.isPending.value"
        class="ms-state"
      >
        جاري تحميل المصفوفة…
      </div>

      <template v-else-if="matrix">
        <div class="ms-meta">
          <span
            >المقياس: <strong>{{ matrix.metricLabel }}</strong></span
          >
          <span
            >الوحدة: <strong>{{ matrix.currency }}</strong></span
          >
          <span
            >المنطقة الزمنية: <strong>{{ matrix.timeZone }}</strong></span
          >
          <span
            >الفترة:
            <strong
              >{{ matrix.month }}/{{ matrix.year }} ({{
                matrix.daysInMonth
              }}
              يومًا)</strong
            ></span
          >
          <span
            >إجمالي الشهر: <strong>{{ fmt(matrix.monthTotal) }}</strong></span
          >
        </div>

        <div
          v-if="matrix.boxCards?.length"
          class="ms-box-row"
        >
          <div
            v-for="card in matrix.boxCards"
            :key="card.key"
            class="ms-box"
            :title="card.reason ?? undefined"
          >
            <span class="ms-box-label">{{ card.label }}</span>
            <span
              v-if="card.availability === 'Available'"
              class="ms-box-value"
              >{{ fmt(card.amount) }}</span
            >
            <span
              v-else
              class="ms-box-muted"
              >{{
                card.availability === 'Forbidden'
                  ? 'غير مخوّل'
                  : 'غير متاح'
              }}</span
            >
          </div>
        </div>

        <div
          v-if="!matrix.branches.length"
          class="ms-state"
        >
          لا توجد فروع ضمن نطاق صلاحياتك.
        </div>

        <SummaryMatrix
          v-else
          :matrix="matrix"
          :can-drilldown="canDrilldown"
          @cell-click="openDrill"
        />
      </template>
    </template>

    <SummaryDetailsDialog
      :open="drillOpen"
      :year="filters.year"
      :month="filters.month"
      :day="drillDay"
      :branch-id="drillBranchId"
      :metric="filters.metric"
      @close="closeDrill"
    />
  </div>
</template>
