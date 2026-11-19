<template>
  <div class="smb-rating">
    <p v-if="employeeId == null" class="dfb-state">اختر موظفاً من العمود لعرض تقييمه</p>
    <p v-else-if="summaryQuery.isPending.value" class="dfb-state" role="status">جاري تحميل التقييم…</p>
    <div v-else-if="summaryQuery.isError.value" class="dfb-error" role="alert">
      تعذر تحميل التقييم
      <button type="button" class="dfb-button is-small" @click="summaryQuery.refetch()">إعادة المحاولة</button>
    </div>
    <template v-else-if="summary">
      <div class="smb-kpis">
        <div class="dfb-kpi">
          <span>التقييم الحقيقي</span>
          <strong :class="`tier-${summary.ratingLabel ?? ''}`"><span class="dfb-num">{{ fixed1(summary.latestRating) }}</span> {{ summary.ratingLabel ?? '' }}</strong>
        </div>
        <div class="dfb-kpi">
          <span>التقييم الوهمي</span>
          <strong><span class="dfb-num">{{ fixed1(summary.phantomLatestRating) }}</span></strong>
        </div>
        <div class="dfb-kpi">
          <span>زبائن التقييم</span>
          <strong><span class="dfb-num">{{ totalCustomers }}</span></strong>
        </div>
      </div>

      <div class="smb-buckets" role="group" aria-label="فئات التقييم">
        <button
          v-for="b in buckets"
          :key="b.key"
          type="button"
          class="smb-bucket"
          :class="[`tier-${b.label}`, { 'is-active': bucket === b.key }]"
          :aria-pressed="bucket === b.key"
          @click="toggle(b.key)"
        >
          <span>{{ b.label }}</span>
          <strong class="dfb-num">{{ b.count }}</strong>
        </button>
      </div>

      <section v-if="bucket" class="smb-customers" :aria-label="`زبائن ${bucketLabel}`">
        <h3 class="dfb-section-title">زبائن «{{ bucketLabel }}»</h3>
        <p v-if="bucketQuery.isPending.value" class="dfb-state" role="status">جاري تحميل الزبائن…</p>
        <p v-else-if="!bucketCustomers.length" class="dfb-state">لا يوجد زبائن في هذه الفئة</p>
        <div v-else class="dfb-table-wrap">
          <table class="dfb-table">
            <thead>
              <tr>
                <th scope="col">الزبون</th>
                <th scope="col">تاريخ البيع</th>
                <th scope="col">آخر تسديد</th>
                <th scope="col">الواصل</th>
                <th scope="col">الباقي</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in bucketCustomers" :key="c.customerId">
                <td>
                  <span class="dfb-cell-main">{{ c.customerName }}</span>
                  <span v-if="c.phone" class="dfb-cell-sub dfb-num">{{ c.phone }}</span>
                </td>
                <td class="dfb-num">{{ c.dateSale?.slice(0, 10) ?? '—' }}</td>
                <td class="dfb-num">{{ c.lastPaymentDate?.slice(0, 10) ?? '—' }}</td>
                <td class="dfb-num">{{ c.amountsRedacted ? '—' : money(c.amountPaid) }}</td>
                <td class="dfb-num">{{ c.amountsRedacted ? '—' : money(c.amountRemaining) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, toRef, watch } from 'vue'
import { useSalesEmployeeRatingSummaryQuery } from '@/modules/SalesEmployees/requests/queries/useSalesEmployeeRatingSummaryQuery'

const props = defineProps<{ employeeId: number | null; branchId: number | null }>()

const bucket = ref<string | null>(null)
watch(() => props.employeeId, () => { bucket.value = null })

const enabled = computed(() => props.employeeId != null)
const summaryQuery = useSalesEmployeeRatingSummaryQuery(toRef(props, 'employeeId'), toRef(props, 'branchId'), ref(null), enabled)
const bucketQuery = useSalesEmployeeRatingSummaryQuery(
  toRef(props, 'employeeId'),
  toRef(props, 'branchId'),
  bucket,
  computed(() => enabled.value && bucket.value != null),
)

const summary = computed(() => summaryQuery.data.value ?? null)
const bucketCustomers = computed(() => bucketQuery.data.value?.customers ?? [])
const buckets = computed(() => {
  const s = summary.value
  return [
    { key: 'excellent', label: 'ممتاز', count: s?.excellentCount ?? 0 },
    { key: 'good', label: 'جيد', count: s?.goodCount ?? 0 },
    { key: 'weak', label: 'ضعيف', count: s?.weakCount ?? 0 },
    { key: 'rejected', label: 'متلكئ', count: s?.rejectedCount ?? 0 },
    { key: 'bad', label: 'سيء', count: s?.badCount ?? 0 },
  ]
})
const totalCustomers = computed(() => buckets.value.reduce((a, b) => a + b.count, 0))
const bucketLabel = computed(() => buckets.value.find((b) => b.key === bucket.value)?.label ?? '')

function toggle(key: string) {
  bucket.value = bucket.value === key ? null : key
}

function fixed1(n: number | null | undefined) {
  return n == null || Number.isFinite(n) === false ? '—' : n.toFixed(1)
}

function money(n: number | null | undefined) {
  return n == null ? '—' : new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}
</script>
