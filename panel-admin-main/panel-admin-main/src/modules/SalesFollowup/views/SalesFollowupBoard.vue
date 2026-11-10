<template>
  <div class="sf-board" dir="rtl">
    <section class="sr-filters">
      <div class="grid gap-3 md:grid-cols-[240px_1fr]">
        <label class="flex flex-col gap-1 text-sm">
          <span>{{ $t('salesFollowup.selectBranch') }}</span>
          <Select
            v-model="branchId"
            :options="branchOptions"
            option-label="name"
            option-value="id"
            :unstyled="false"
            :placeholder="$t('salesFollowup.allBranches')"
            filter
            filter-placeholder="ابحث عن الفرع…"
            empty-filter-message="لا يوجد فرع بهذا الاسم"
            empty-message="لا توجد فروع متاحة"
            aria-label="الفرع"
            class="w-full"
            :reset-filter-on-hide="true"
          />
        </label>
      </div>
    </section>

    <p v-if="employeesError" class="sf-error">تعذر تحميل موظفي المبيعات</p>
    <div v-else-if="employeesLoading" class="sf-muted">جاري التحميل…</div>
    <div v-else-if="!cards.length" class="sf-muted">
      {{ $t('salesFollowup.noEmployees') }}
    </div>

    <div class="sf-cards">
      <article
        v-for="c in cards"
        :key="c.employeeId"
        class="sr-card"
        :class="{ selected: selectedId === c.employeeId }"
      >
        <header class="sr-card-top">
          <div class="sr-identity">
            <span class="sr-id">موظف #{{ c.employeeId }}</span>
            <h3 class="sr-name">{{ c.employeeName }}</h3>
            <span class="sr-missing">{{ c.branchName || '—' }}</span>
          </div>
          <span class="sr-status">{{ c.ratingLabel }} · {{ c.averageScore }}</span>
        </header>

        <dl class="sr-grid">
          <div>
            <dt>{{ $t('salesFollowup.customers') }}</dt>
            <dd>{{ c.customerCount }}</dd>
          </div>
          <div>
            <dt>{{ $t('salesFollowup.avgScore') }}</dt>
            <dd>{{ c.averageScore }}</dd>
          </div>
        </dl>

        <button
          type="button"
          class="sr-open"
          :aria-pressed="selectedId === c.employeeId"
          @click="selectEmployee(c)"
        >
          <span>
            {{
              selectedId === c.employeeId
                ? $t('salesFollowup.detailOpen')
                : $t('salesFollowup.openDetail')
            }}
          </span>
          <span aria-hidden="true">←</span>
        </button>
      </article>
    </div>

    <Dialog
      :visible="selectedId != null"
      modal
      closable
      dismissable-mask
      close-on-escape
      :header="detailDialogHeader"
      :style="{ width: 'min(1100px, 96vw)' }"
      :pt="dialogMaskPt"
      class="sf-detail-dialog bg-white rounded-3xl p-5"
      dir="rtl"
      @update:visible="!$event && closeDetail()"
    >
      <div v-if="detailLoading" class="sf-muted">جاري تحميل التفاصيل…</div>
      <div v-else-if="detailError" class="sf-error">تعذر تحميل تفاصيل الموظف</div>
      <div v-else-if="detail" class="sf-detail">
        <header class="sf-detail-head">
          <div>
            <p class="sf-meta">
              {{ detail.employee.ratingLabel }} · متوسط {{ detail.employee.averageScore }}
              · {{ detail.employee.customerCount }} زبون
              <span v-if="detail.employee.branchName"> · {{ detail.employee.branchName }}</span>
            </p>
          </div>
          <button
            v-if="canManageBlock"
            type="button"
            class="sf-btn"
            :class="detail.isAppBlocked ? 'danger' : 'ok'"
            :disabled="blockMutation.isPending.value"
            @click="toggleBlock"
          >
            {{ detail.isAppBlocked ? $t('salesFollowup.unblockApp') : $t('salesFollowup.blockApp') }}
          </button>
        </header>

        <div class="sf-kpi-row">
          <button type="button" class="sf-kpi" @click="openLowDialog">
            <span>{{ $t('salesFollowup.lowAuth') }}</span>
            <strong>{{ detail.followerSaleAuthRatings.low.length }}</strong>
          </button>
          <button type="button" class="sf-kpi" @click="openSystemDialog">
            <span>{{ $t('salesFollowup.systemAvg') }}</span>
            <strong>{{ detail.systemRatingHistory[0]?.averageScore ?? '—' }}</strong>
          </button>
          <button
            v-for="cc in detail.classCounts"
            :key="cc.label"
            type="button"
            class="sf-kpi"
            @click="applyFilter(cc.label)"
          >
            <span>{{ cc.label }}</span>
            <strong>{{ cc.count }}</strong>
          </button>
        </div>

        <div class="sf-section">
          <h3>{{ $t('salesFollowup.byList') }}</h3>
          <div class="sf-list-grid">
            <div
              v-for="row in detail.salesByList"
              :key="`${row.listId}-${row.listName}`"
              class="sf-list-card"
            >
              <strong>{{ row.listName }}</strong>
              <span>{{ row.customerCount }} زبون</span>
              <span>مبيعات {{ formatMoney(row.totalSales) }}</span>
              <span>باقي {{ formatMoney(row.totalRemaining) }}</span>
            </div>
          </div>
        </div>

        <div class="sf-section">
          <h3>{{ $t('salesFollowup.conversion') }}</h3>
          <p>{{ detail.conversion.summaryText }}</p>
          <div class="sf-conv">
            <button
              v-for="t in detail.conversion.targets"
              :key="t.targetLabel"
              type="button"
              class="sf-chip-btn"
              @click="openConversion(t)"
            >
              {{ t.targetLabel }}: {{ t.arrivedCount }}
            </button>
          </div>
        </div>

        <div class="sf-section">
          <div class="sf-table-tools">
            <h3>{{ $t('salesFollowup.focusTable') }}</h3>
            <input
              v-model="searchText"
              class="sf-input"
              :placeholder="$t('salesFollowup.search')"
            >
            <Select
              v-model="tableFilter"
              :options="tableFilterOptions"
              option-label="label"
              option-value="value"
              class="sf-table-filter"
              :unstyled="false"
            />
            <button type="button" class="sf-btn" @click="exportCsv">
              {{ $t('salesFollowup.exportExcel') }}
            </button>
          </div>

          <div class="sf-table-wrap">
            <table class="sf-table">
              <thead>
                <tr>
                  <th>{{ $t('salesFollowup.profile') }}</th>
                  <th>العميل</th>
                  <th>التقييم</th>
                  <th>تاريخ البيع</th>
                  <th>سعر البيع</th>
                  <th>الواصل</th>
                  <th>الباقي</th>
                  <th>الهاتف</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="row in filteredCustomers"
                  :key="row.customerId"
                >
                  <td>
                    <button
                      type="button"
                      class="sf-link"
                      @click="openProfile(row.customerId)"
                    >
                      فتح
                    </button>
                  </td>
                  <td>{{ row.customerName }}</td>
                  <td>{{ row.ratingLabel }}</td>
                  <td>{{ row.dateSaleDevice || '—' }}</td>
                  <td>{{ formatMoney(row.amountTotalSales) }}</td>
                  <td>{{ formatMoney(row.receiptsTotal) }}</td>
                  <td>{{ formatMoney(row.amountRemaining) }}</td>
                  <td>{{ row.phoneNumber || '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </Dialog>

    <Dialog
      :visible="histDialog"
      modal
      closable
      dismissable-mask
      close-on-escape
      :header="histTitle"
      :style="{ width: 'min(720px, 94vw)' }"
      :pt="dialogMaskPt"
      class="bg-white rounded-3xl p-5"
      dir="rtl"
      @update:visible="histDialog = $event"
    >
      <ul v-if="histMode === 'low'" class="sf-hist">
        <li
          v-for="(r, i) in histLow"
          :key="i"
        >
          {{ r.ratingDate?.slice?.(0, 10) || r.ratingDate }} — درجة {{ r.score }}
          <span v-if="r.note"> · {{ r.note }}</span>
        </li>
      </ul>
      <ul v-else class="sf-hist">
        <li
          v-for="(r, i) in histSystem"
          :key="i"
        >
          {{ String(r.ratingDate).slice(0, 10) }} — {{ r.ratingLabel }} ({{ r.averageScore }}) · {{ r.customerCount }} زبون
        </li>
      </ul>
    </Dialog>

    <Dialog
      :visible="convDialog"
      modal
      closable
      dismissable-mask
      close-on-escape
      :header="convTitle"
      :style="{ width: 'min(720px, 94vw)' }"
      :pt="dialogMaskPt"
      class="bg-white rounded-3xl p-5"
      dir="rtl"
      @update:visible="convDialog = $event"
    >
      <p v-if="convExtra">{{ convExtra }}</p>
      <ul class="sf-hist">
        <li
          v-for="(d, i) in convRows"
          :key="i"
        >
          {{ d.date }} — وصل {{ d.arrivedCount }}
          <span v-if="d.fromBreakdown?.length">
            ({{ d.fromBreakdown.map((f) => `${f.fromLabel}:${f.count}`).join(' · ') }})
          </span>
        </li>
      </ul>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { Dialog } from 'primevue'
import Select from 'primevue/select'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { useAuthStore } from '@/modules/Auth/store'
import { useSalesFollowupEmployeesQuery } from '../requests/queries/useSalesFollowupEmployeesQuery'
import { useSalesFollowupDetailQuery } from '../requests/queries/useSalesFollowupDetailQuery'
import { useSetAppBlockMutation } from '../requests/mutations/useSetAppBlockMutation'
import type { SalesFollowupDetail, SalesFollowupEmployeeCard } from '../domain/types'
import { useI18n } from 'vue-i18n'

const router = useRouter()
const { can } = usePermission()
const auth = useAuthStore()
const { t } = useI18n()

const branches = computed(() =>
  (auth.branches ?? []).map((b: { id: number; name: string }) => ({
    id: b.id,
    name: b.name,
  }))
)

const branchOptions = computed(() => [
  { id: null as number | null, name: t('salesFollowup.allBranches') },
  ...branches.value,
])

/** null = كل الفروع */
const branchId = ref<number | null>(null)
const selectedId = ref<number | null>(null)
/** فرع التفاصيل = فرع الموظف الأساسي عند اختيار «كل الفروع» */
const detailBranchId = ref<number | null>(null)

watch(branchId, () => {
  selectedId.value = null
  detailBranchId.value = null
})

const employeesQuery = useSalesFollowupEmployeesQuery(branchId)
const cards = computed(() => employeesQuery.data.value?.items ?? [])
const employeesLoading = computed(() => employeesQuery.isLoading.value)
const employeesError = computed(() => employeesQuery.isError.value)

const detailQuery = useSalesFollowupDetailQuery(selectedId, detailBranchId)
const detail = computed(() => detailQuery.data.value ?? null)
const detailLoading = computed(() => detailQuery.isLoading.value || detailQuery.isFetching.value)
const detailError = computed(() => detailQuery.isError.value)
const detailDialogHeader = computed(
  () => detail.value?.employee.employeeName ?? 'تفاصيل موظف المبيعات'
)

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const canManageBlock = computed(() => can('SalesFollowup', 'ManageAppBlock'))
const blockMutation = useSetAppBlockMutation()

function selectEmployee(c: SalesFollowupEmployeeCard) {
  selectedId.value = c.employeeId
  detailBranchId.value =
    branchId.value != null && branchId.value > 0
      ? branchId.value
      : c.branchId
  searchText.value = ''
  tableFilter.value = 'all'
}

function closeDetail() {
  selectedId.value = null
  detailBranchId.value = null
  histDialog.value = false
  convDialog.value = false
}

const searchText = ref('')
const tableFilter = ref('all')
const ratingBuckets = ['ممتاز', 'جيد', 'ضعيف', 'متلكئ', 'سيء']
const tableFilterOptions = computed(() => [
  { value: 'all', label: 'الكل' },
  ...ratingBuckets.map((b) => ({ value: b, label: b })),
  { value: 'legal', label: 'قانونية' },
  { value: 'zeroed', label: 'المصفرين' },
])

const filteredCustomers = computed(() => {
  const rows = detail.value?.customers ?? []
  const q = searchText.value.trim()
  return rows.filter((r) => {
    if (tableFilter.value === 'legal') {
      if (!r.isLegal) return false
    } else if (tableFilter.value === 'zeroed') {
      if (!(r.amountTotalSales > 0 && r.receiptsTotal >= r.amountTotalSales && r.amountRemaining <= 0 && !r.isLegal))
        return false
    } else if (tableFilter.value !== 'all' && r.ratingLabel !== tableFilter.value) {
      return false
    }
    if (!q) return true
    return (
      r.customerName.includes(q) ||
      (r.phoneNumber ?? '').includes(q) ||
      (r.shopName ?? '').includes(q)
    )
  })
})

function applyFilter(label: string) {
  if (label === 'قانونية') tableFilter.value = 'legal'
  else if (label === 'المصفرين') tableFilter.value = 'zeroed'
  else tableFilter.value = label
}

async function toggleBlock() {
  if (!detail.value || detailBranchId.value == null || selectedId.value == null) return
  await blockMutation.mutateAsync({
    employeeId: selectedId.value,
    branchId: detailBranchId.value,
    isBlocked: !detail.value.isAppBlocked,
  })
}

const histDialog = ref(false)
const histTitle = ref('')
const histMode = ref<'low' | 'system'>('low')
const histLow = ref<SalesFollowupDetail['followerSaleAuthRatings']['low']>([])
const histSystem = ref<SalesFollowupDetail['systemRatingHistory']>([])

function openLowDialog() {
  histMode.value = 'low'
  histTitle.value = 'تقييمات منخفضة (بدون هوية زبون)'
  histLow.value = detail.value?.followerSaleAuthRatings.low ?? []
  histDialog.value = true
}

function openSystemDialog() {
  histMode.value = 'system'
  histTitle.value = 'متوسط النظام — 30 يوم'
  histSystem.value = detail.value?.systemRatingHistory ?? []
  histDialog.value = true
}

const convDialog = ref(false)
const convTitle = ref('')
const convExtra = ref('')
const convRows = ref<
  SalesFollowupDetail['conversion']['targets'][number]['last30Days']
>([])

function openConversion(t: SalesFollowupDetail['conversion']['targets'][number]) {
  convTitle.value = `تحويلات → ${t.targetLabel}`
  convExtra.value = t.detailText
  convRows.value = t.last30Days
  convDialog.value = true
}

function openProfile(customerId: number) {
  router.push({
    name: 'CustomerAuditProfileRoute',
    params: { id: customerId },
    query: {
      returnTo: 'SalesFollowupRoute',
      branchId: detailBranchId.value != null ? String(detailBranchId.value) : undefined,
    },
  })
}

function formatMoney(n: number) {
  return new Intl.NumberFormat('en-IQ').format(Math.round(n))
}

function exportCsv() {
  const rows = filteredCustomers.value
  const header = [
    'العميل',
    'التقييم',
    'تاريخ البيع',
    'سعر البيع',
    'الواصل',
    'الباقي',
    'الهاتف',
  ]
  const lines = [
    header.join(','),
    ...rows.map((r) =>
      [
        r.customerName,
        r.ratingLabel,
        r.dateSaleDevice ?? '',
        r.amountTotalSales,
        r.receiptsTotal,
        r.amountRemaining,
        r.phoneNumber ?? '',
      ]
        .map((v) => `"${String(v).replace(/"/g, '""')}"`)
        .join(',')
    ),
  ]
  const blob = new Blob(['\ufeff' + lines.join('\n')], {
    type: 'text/csv;charset=utf-8',
  })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `sales-followup-${selectedId.value ?? 'export'}.csv`
  a.click()
  URL.revokeObjectURL(url)
}
</script>

<style scoped>
.sf-board {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.sr-filters label > span {
  font-size: 14px;
  color: #526b67;
  margin-bottom: 4px;
}
.sf-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1rem;
}
.sr-card {
  background: #fff;
  border: 1px solid #dce7e5;
  border-radius: 18px;
  padding: 24px;
  color: #00201e;
  font-family: inherit;
  border-inline-start: 4px solid #dce7e5;
  transition: border-color 180ms ease;
  overflow-wrap: anywhere;
}
.sr-card.selected {
  border-color: #006a64;
  border-inline-start-color: #006a64;
}
.sr-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}
.sr-identity {
  min-width: 0;
}
.sr-id {
  color: #526b67;
  font-size: 14px;
}
.sr-name {
  margin: 6px 0;
  font-size: 22px;
  line-height: 1.65;
  font-weight: 700;
}
.sr-missing {
  color: #61756f;
  font-size: 15px;
}
.sr-status {
  flex-shrink: 0;
  padding: 7px 13px;
  border-radius: 9px;
  background: #e9f3f0;
  color: #145c4c;
  font-size: 14px;
  line-height: 1.5;
}
dt {
  color: #61756f;
  font-size: 14px;
  margin-bottom: 5px;
}
dd {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  line-height: 1.75;
}
.sr-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px 24px;
  margin: 18px 0;
}
.sr-open {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  background: #edf5f3;
  border: 1px solid #cfdfda;
  border-radius: 10px;
  color: #006a64;
  padding: 13px 16px;
  min-height: 48px;
  font-family: inherit;
  font-size: 16px;
  font-weight: 600;
  line-height: 1.6;
  cursor: pointer;
}
.sr-open:hover {
  background: #dfeee9;
}
.sr-open:focus-visible {
  outline: 3px solid #006a64;
  outline-offset: 3px;
}
.sf-detail-dialog :deep(.p-dialog-content),
.sf-detail-dialog :deep([data-pc-section='content']) {
  max-height: min(78vh, 820px);
  overflow: auto;
  padding-top: 0.25rem;
}
.sf-detail {
  display: flex;
  flex-direction: column;
  gap: 1.15rem;
  color: #00201e;
}
.sf-section h3 {
  margin: 0 0 0.55rem;
  font-size: 1.05rem;
  color: #00201e;
}
.sf-detail-head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
}
.sf-meta {
  color: #5f6f6c;
  font-size: 0.85rem;
}
.sf-btn {
  border: 0;
  border-radius: 0.7rem;
  padding: 0.45rem 0.9rem;
  background: #0e6655;
  color: #fff;
  cursor: pointer;
}
.sf-btn.danger {
  background: #922b21;
}
.sf-btn.ok {
  background: #145a32;
}
.sf-kpi-row {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(7.5rem, 1fr));
  gap: 0.55rem;
}
.sf-kpi {
  border: 1px solid #d5e0dd;
  border-radius: 0.85rem;
  padding: 0.65rem;
  background: #f8fbfa;
  text-align: right;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  cursor: pointer;
}
.sf-list-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(12rem, 1fr));
  gap: 0.55rem;
}
.sf-list-card {
  border: 1px solid #e1eae8;
  border-radius: 0.8rem;
  padding: 0.65rem;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  font-size: 0.9rem;
}
.sf-conv {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}
.sf-chip-btn {
  border: 1px solid #c5d0ce;
  border-radius: 999px;
  padding: 0.25rem 0.7rem;
  background: #fff;
  cursor: pointer;
}
.sf-table-tools {
  display: flex;
  flex-wrap: wrap;
  gap: 0.55rem;
  align-items: center;
  margin-bottom: 0.55rem;
}
.sf-input {
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
  padding: 0.55rem 0.75rem;
  background: #f8f8f8;
  color: #00201e;
  font-family: inherit;
  min-height: 46px;
  font-size: 16px;
}
.sf-table-filter {
  min-width: 10rem;
}
.sf-table-wrap {
  overflow: auto;
  border: 1px solid #e1eae8;
  border-radius: 0.8rem;
}
.sf-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 48rem;
}
.sf-table th,
.sf-table td {
  padding: 0.55rem 0.65rem;
  border-bottom: 1px solid #edf2f1;
  text-align: right;
  font-size: 0.9rem;
}
.sf-table th {
  background: #f3f8f7;
  position: sticky;
  top: 0;
}
.sf-link {
  border: 0;
  background: none;
  color: #0e6655;
  text-decoration: underline;
  cursor: pointer;
}
.sf-muted,
.sf-error {
  padding: 0.5rem 0;
}
.sf-error {
  color: #922b21;
}
.sf-hist {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
@media (max-width: 480px) {
  .sr-card {
    padding: 18px 15px;
  }
  .sr-name {
    font-size: 21px;
  }
  .sr-card-top {
    flex-wrap: wrap;
  }
}
@media (prefers-reduced-motion: reduce) {
  .sr-card {
    transition: none;
  }
}
</style>
