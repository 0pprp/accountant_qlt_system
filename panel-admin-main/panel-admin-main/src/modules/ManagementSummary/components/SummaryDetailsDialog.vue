<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useSummaryDetailsQuery } from '../requests/queries/useSummaryDetailsQuery'
import type { ManagementSummaryMetricKey } from '../domain/types'

const props = defineProps<{
  open: boolean
  year: number
  month: number
  day: number | null
  branchId: number | null
  metric: ManagementSummaryMetricKey
}>()

const emit = defineEmits<{ close: [] }>()

const page = ref(1)
const pageSize = ref(25)

const yearRef = computed(() => props.year)
const monthRef = computed(() => props.month)
const dayRef = computed(() => props.day)
const branchRef = computed(() => props.branchId)
const metricRef = computed(() => props.metric)
const openRef = computed(() => props.open)

watch(
  () => [props.day, props.branchId, props.metric, props.year, props.month],
  () => {
    page.value = 1
  }
)

const detailsQuery = useSummaryDetailsQuery({
  year: yearRef,
  month: monthRef,
  day: dayRef,
  metric: metricRef,
  branchId: branchRef,
  page,
  pageSize,
  enabled: openRef,
})

function fmt(n: number | null | undefined) {
  if (n == null || Number.isNaN(n)) return '—'
  return new Intl.NumberFormat('ar-IQ', {
    maximumFractionDigits: 0,
  }).format(n)
}

function fmtDate(iso: string) {
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      timeZone: 'Asia/Baghdad',
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}

const totalPages = computed(() => {
  const total = detailsQuery.data.value?.totalCount ?? 0
  return Math.max(1, Math.ceil(total / pageSize.value))
})

function onBackdrop(e: MouseEvent) {
  if (e.target === e.currentTarget) emit('close')
}
</script>

<template>
  <div
    v-if="open"
    class="ms-dialog-backdrop"
    role="presentation"
    @click="onBackdrop"
  >
    <div
      class="ms-dialog"
      role="dialog"
      aria-modal="true"
      aria-labelledby="ms-details-title"
    >
      <div class="ms-dialog-head">
        <h2 id="ms-details-title">تفاصيل الخلية</h2>
        <button
          type="button"
          class="ms-dialog-close"
          aria-label="إغلاق"
          @click="emit('close')"
        >
          ×
        </button>
      </div>

      <div class="ms-dialog-body">
        <div
          v-if="detailsQuery.isPending.value"
          class="ms-state"
        >
          جاري التحميل…
        </div>

        <div
          v-else-if="detailsQuery.isError.value"
          class="ms-error"
        >
          تعذر تحميل التفاصيل. قد تكون البيانات تغيّرت — حدّث المصفوفة ثم أعد
          المحاولة.
        </div>

        <template v-else-if="detailsQuery.data.value">
          <div class="ms-dialog-meta">
            <span
              >الفرع:
              <strong>{{ detailsQuery.data.value.branchName }}</strong></span
            >
            <span
              >اليوم:
              <strong
                >{{ detailsQuery.data.value.day }}/{{
                  detailsQuery.data.value.month
                }}/{{ detailsQuery.data.value.year }}</strong
              ></span
            >
            <span
              >المقياس:
              <strong>{{ detailsQuery.data.value.metricLabel }}</strong></span
            >
            <span
              >المجموع:
              <strong>{{ fmt(detailsQuery.data.value.cellTotal) }}</strong>
              {{ detailsQuery.data.value.currency }}</span
            >
          </div>

          <div
            v-if="!detailsQuery.data.value.items.length"
            class="ms-state"
          >
            لا توجد عمليات لهذه الخلية.
          </div>

          <table v-else>
            <thead>
              <tr>
                <th>البيان</th>
                <th>التفاصيل</th>
                <th>التاريخ</th>
                <th>المبلغ</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="row in detailsQuery.data.value.items"
                :key="row.id"
              >
                <td>{{ row.title }}</td>
                <td>{{ row.subtitle || row.reference || '—' }}</td>
                <td>{{ fmtDate(row.occurredAt) }}</td>
                <td class="amt">{{ fmt(row.amount) }}</td>
              </tr>
            </tbody>
          </table>

          <div
            v-if="(detailsQuery.data.value.totalCount ?? 0) > pageSize"
            class="ms-pager"
          >
            <button
              type="button"
              :disabled="page <= 1"
              @click="page = Math.max(1, page - 1)"
            >
              السابق
            </button>
            <span>{{ page }} / {{ totalPages }}</span>
            <button
              type="button"
              :disabled="page >= totalPages"
              @click="page = Math.min(totalPages, page + 1)"
            >
              التالي
            </button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
