<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { useSalesSummaryMatrixQuery } from '../requests/queries/useSalesSummaryMatrixQuery'
import { useSalesSummaryDetailsQuery } from '../requests/queries/useSalesSummaryDetailsQuery'
import '@/modules/ManagementSummary/components/management-summary.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesSummary', 'Read') === true)
const canDrill = computed(() => auth.hasPermission('SalesSummary', 'Drilldown') === true)

function baghdadNow() {
  try {
    const parts = new Intl.DateTimeFormat('en-CA', {
      timeZone: 'Asia/Baghdad',
      year: 'numeric',
      month: '2-digit',
    }).formatToParts(new Date())
    const y = Number(parts.find((p) => p.type === 'year')?.value)
    const m = Number(parts.find((p) => p.type === 'month')?.value)
    return { y, m }
  } catch {
    const d = new Date()
    return { y: d.getFullYear(), m: d.getMonth() + 1 }
  }
}

const bag = baghdadNow()
const year = computed(() => {
  const n = Number(route.query.year)
  return Number.isInteger(n) && n >= 2000 ? n : bag.y
})
const month = computed(() => {
  const n = Number(route.query.month)
  return Number.isInteger(n) && n >= 1 && n <= 12 ? n : bag.m
})
const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: 'SaleSummaryRoute', query: q })
}

const matrixQuery = useSalesSummaryMatrixQuery(year, month, branchId, canRead)
const matrix = computed(() => matrixQuery.data.value)

const drillOpen = ref(false)
const drillDay = ref<number | null>(null)
const drillBranchId = ref<number | null>(null)
const drillPage = ref(1)
const drillEnabled = computed(() => canDrill.value && drillOpen.value)

const detailsQuery = useSalesSummaryDetailsQuery({
  year,
  month,
  day: drillDay,
  branchId: drillBranchId,
  page: drillPage,
  enabled: drillEnabled,
})

watch([year, month, branchId], () => {
  drillOpen.value = false
})

function fmt(n: number | null | undefined) {
  if (n == null || !Number.isFinite(n) || n === 0) return '—'
  return new Intl.NumberFormat('ar-IQ', { maximumFractionDigits: 0 }).format(Math.round(n))
}

const cellMap = computed(() => {
  const map = new Map<string, number>()
  for (const c of matrix.value?.cells ?? []) map.set(`${c.day}:${c.branchId}`, c.amount)
  return map
})
function cellAmount(day: number, branchId: number) {
  return cellMap.value.get(`${day}:${branchId}`) ?? 0
}

function openDrill(day: number, bId: number) {
  if (!canDrill.value || cellAmount(day, bId) <= 0) return
  drillDay.value = day
  drillBranchId.value = bId
  drillPage.value = 1
  drillOpen.value = true
}

const years = [bag.y - 1, bag.y, bag.y + 1]
</script>

<template>
  <div class="ms-page" dir="rtl">
    <h1 class="ms-title">ملخص المبيعات</h1>
    <p class="ms-subtitle">
      سعر البيع حسب الفروع والأيام — مقياس ثابت. ليس الملخص الإداري متعدد المقاييس.
    </p>

    <div v-if="!canRead" class="ms-error" role="alert">غير مخول بقراءة ملخص المبيعات.</div>

    <template v-else>
      <div class="ms-filters">
        <label>
          الشهر
          <select
            class="ms-select"
            :value="month"
            @change="patchQuery({ month: ($event.target as HTMLSelectElement).value })"
          >
            <option v-for="m in 12" :key="m" :value="m">{{ m }}</option>
          </select>
        </label>
        <label>
          السنة
          <select
            class="ms-select"
            :value="year"
            @change="patchQuery({ year: ($event.target as HTMLSelectElement).value })"
          >
            <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
          </select>
        </label>
        <label>
          الفرع
          <select
            class="ms-select"
            :value="branchId ?? ''"
            @change="
              patchQuery({
                branchId: ($event.target as HTMLSelectElement).value || null,
              })
            "
          >
            <option value="">كل الفروع المسموحة</option>
            <option v-for="b in auth.branches ?? []" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <button type="button" class="ms-refresh" @click="matrixQuery.refetch()">تحديث</button>
      </div>

      <p class="ms-subtitle">
        يُحتسب اليوم بتاريخ البيع في التقويم، وإن غاب فيُعتمد وقت الإنشاء بحدود منتصف ليل بغداد — وليس يوم العمل بقطع الثالثة فجرًا.
      </p>

      <div v-if="matrixQuery.isPending.value" class="ms-loading" aria-busy="true">جاري التحميل…</div>
      <div v-else-if="matrixQuery.isError.value" class="ms-error" role="alert">
        تعذر تحميل الملخص. لا تُعرض أصفار بدل الخطأ.
      </div>
      <template v-else-if="matrix">
        <p class="ms-subtitle">
          إجمالي الشهر: <strong>{{ fmt(matrix.monthTotal) }}</strong> {{ matrix.currency }}
        </p>
        <div class="ms-table-scroll">
          <table class="ms-table">
            <thead>
              <tr>
                <th class="ms-th-day">اليوم</th>
                <th v-for="b in matrix.branches" :key="b.id">{{ b.name }}</th>
                <th class="ms-th-sum">المجموع</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="day in matrix.daysInMonth" :key="day">
                <td class="ms-td-day">{{ day }}</td>
                <td v-for="b in matrix.branches" :key="`${day}-${b.id}`">
                  <button
                    type="button"
                    class="ms-cell-btn"
                    :class="{ 'is-clickable': canDrill && cellAmount(day, b.id) > 0 }"
                    :disabled="!(canDrill && cellAmount(day, b.id) > 0)"
                    @click="openDrill(day, b.id)"
                  >
                    {{ fmt(cellAmount(day, b.id)) }}
                  </button>
                </td>
                <td class="ms-td-sum">
                  {{
                    fmt(matrix.dayTotals.find((d) => d.day === day)?.amount ?? 0)
                  }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </template>

    <div v-if="drillOpen" class="ms-dialog-backdrop" @click.self="drillOpen = false">
      <div class="ms-dialog" role="dialog" aria-modal="true">
        <header>
          <h2>سعر البيع — يوم {{ drillDay }}</h2>
          <button type="button" @click="drillOpen = false">إغلاق</button>
        </header>
        <div v-if="!canDrill" class="ms-error">غير مخول بالتفاصيل.</div>
        <div v-else-if="detailsQuery.isPending.value">جاري التحميل…</div>
        <div v-else-if="detailsQuery.isError.value" class="ms-error">تعذر تحميل التفاصيل.</div>
        <template v-else-if="detailsQuery.data.value">
          <p>
            {{ detailsQuery.data.value.branchName }} · المجموع
            {{ fmt(detailsQuery.data.value.cellTotal) }}
          </p>
          <ul class="ms-detail-list">
            <li v-for="row in detailsQuery.data.value.items" :key="row.orderId">
              <strong>#{{ row.orderId }}</strong> {{ row.title }} —
              {{ fmt(row.amount) }}
            </li>
          </ul>
          <p v-if="!detailsQuery.data.value.items.length">لا صفوف.</p>
          <div class="ms-pager">
            <button
              type="button"
              :disabled="drillPage <= 1"
              @click="drillPage--"
            >
              السابق
            </button>
            <span>صفحة {{ detailsQuery.data.value.page }}</span>
            <button
              type="button"
              :disabled="
                drillPage * detailsQuery.data.value.pageSize >=
                detailsQuery.data.value.totalCount
              "
              @click="drillPage++"
            >
              التالي
            </button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ms-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1rem 0;
  align-items: flex-end;
}
.ms-filters label {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 14px;
}
.ms-select,
.ms-refresh {
  min-height: 44px;
  font: inherit;
  padding: 0 0.75rem;
  border-radius: 8px;
  border: 1px solid #c5c5c5;
  background: #f8f8f8;
}
.ms-refresh {
  background: #006a64;
  color: #fff;
  border-color: #006a64;
  cursor: pointer;
}
.ms-dialog-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.ms-dialog {
  background: #fff;
  border-radius: 12px;
  max-width: 36rem;
  width: 100%;
  max-height: 80vh;
  overflow: auto;
  padding: 1rem 1.25rem;
}
.ms-dialog header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}
.ms-detail-list {
  list-style: none;
  padding: 0;
  margin: 0.75rem 0;
}
.ms-detail-list li {
  padding: 0.5rem 0;
  border-bottom: 1px solid #e6eeee;
}
.ms-pager {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}
</style>
