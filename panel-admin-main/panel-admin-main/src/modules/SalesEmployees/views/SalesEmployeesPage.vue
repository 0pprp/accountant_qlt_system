<template>
  <div class="se-page" dir="rtl">
    <header class="se-head">
      <h1 class="se-title">موظفو المبيعات</h1>
      <p class="se-sub">دور المندوب ضمن الفروع المسموحة — الدوام من SalesShift وليس من Login/GPS.</p>
    </header>

    <div class="se-filters">
      <label class="se-field">
        <span>الفرع</span>
        <select v-model="branchModel" class="se-select">
          <option :value="null">كل الفروع المسموحة</option>
          <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
        </select>
      </label>
      <label class="se-field se-field--grow">
        <span>بحث</span>
        <input v-model="searchModel" class="se-input" type="search" placeholder="اسم الموظف" />
      </label>
      <label class="se-field">
        <span>الدوام</span>
        <select v-model="shiftModel" class="se-select">
          <option value="">الكل</option>
          <option value="Active">في الدوام</option>
          <option value="NoShift">بدون دوام</option>
        </select>
      </label>
      <label class="se-field">
        <span>ترتيب</span>
        <select v-model="sortModel" class="se-select">
          <option value="name">الاسم</option>
          <option value="branch">الفرع</option>
          <option value="shift">الدوام</option>
          <option value="rating">التقييم</option>
          <option value="lastLocation">آخر موقع</option>
        </select>
      </label>
    </div>

    <div v-if="!canRead" class="se-state se-state--denied">غير مخول بقراءة موظفي المبيعات.</div>
    <div v-else-if="listQuery.isLoading.value" class="se-state">جاري التحميل…</div>
    <div v-else-if="listQuery.isError.value" class="se-state se-state--error">تعذر تحميل القائمة.</div>
    <template v-else>
      <p class="se-count">{{ totalCount }} موظف</p>
      <div class="se-table-wrap">
        <table class="se-table">
          <thead>
            <tr>
              <th>الموظف</th>
              <th>الفرع</th>
              <th>الدوام</th>
              <th>الموقع</th>
              <th>آخر تحديث</th>
              <th>تقييم</th>
              <th>اليوم / معلّق</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.employeeId">
              <td>
                <strong>{{ row.employeeName }}</strong>
              </td>
              <td>{{ row.branchName }}</td>
              <td>{{ shiftLabel(row.shiftStatus) }}</td>
              <td>{{ locationLabel(row.locationStatus) }}</td>
              <td>{{ fmtDt(row.lastLocationAtUtc) }}</td>
              <td>
                <button type="button" class="se-linkish" @click="openRating(row)">
                  {{ ratingText(row.latestRating) }}
                  <span class="se-muted">/ {{ ratingText(row.phantomLatestRating) }}</span>
                </button>
              </td>
              <td>{{ row.salesTodayCount }} / {{ row.pendingSalesCount }}</td>
              <td>
                <button
                  v-for="link in allowedLinks(row)"
                  :key="link.key"
                  type="button"
                  class="se-btn se-btn--sm"
                  @click="goLink(link, row)"
                >
                  {{ link.labelAr }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <p v-if="!items.length" class="se-state">لا موظفون مطابقون للفلاتر (صفر فعلي).</p>
      </div>

      <div class="se-pager">
        <button type="button" class="se-btn" :disabled="page <= 1" @click="page = page - 1">السابق</button>
        <span>صفحة {{ page }}</span>
        <button
          type="button"
          class="se-btn"
          :disabled="page * pageSize >= totalCount"
          @click="page = page + 1"
        >
          التالي
        </button>
      </div>
    </template>

    <SalesEmployeeRatingDialog
      :open="dialogOpen"
      :busy="ratingBusy"
      :error="ratingError"
      :employee-name="selected?.employeeName || ''"
      :summary="ratingSummary"
      :bucket-customers="bucketCustomers"
      :bucket-label="bucketLabel"
      @close="closeDialog"
      @open-bucket="loadBucket"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import SalesEmployeeRatingDialog from '../components/SalesEmployeeRatingDialog.vue'
import {
  useSalesEmployeesQuery,
  type SalesEmployeesFilters,
} from '../requests/queries/useSalesEmployeesQuery'
import { useSalesEmployeeRatingSummaryQuery } from '../requests/queries/useSalesEmployeeRatingSummaryQuery'
import type { SalesEmployeeCard, SalesEmployeeLink } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesEmployee', 'Read') === true)
const branches = computed(() => auth.branches ?? [])

const branchModel = ref<number | null>(null)
const searchModel = ref('')
const shiftModel = ref('')
const sortModel = ref('name')
const page = ref(1)
const pageSize = 20

function syncFromRoute() {
  const q = route.query
  const b = Number(q.branchId)
  branchModel.value = Number.isInteger(b) && b > 0 ? b : null
  searchModel.value = typeof q.search === 'string' ? q.search : ''
  shiftModel.value = typeof q.shiftState === 'string' ? q.shiftState : ''
  sortModel.value = typeof q.sort === 'string' && q.sort ? q.sort : 'name'
  const p = Number(q.page)
  page.value = Number.isInteger(p) && p > 0 ? p : 1
}
syncFromRoute()

watch(
  () => route.query,
  () => syncFromRoute(),
  { deep: true }
)

watch([branchModel, searchModel, shiftModel, sortModel, page], () => {
  const q: Record<string, string> = {}
  if (branchModel.value != null && branchModel.value > 0) q.branchId = String(branchModel.value)
  if (searchModel.value.trim()) q.search = searchModel.value.trim()
  if (shiftModel.value) q.shiftState = shiftModel.value
  if (sortModel.value && sortModel.value !== 'name') q.sort = sortModel.value
  if (page.value > 1) q.page = String(page.value)
  void router.replace({ query: q })
})

watch([branchModel, searchModel, shiftModel, sortModel], () => {
  page.value = 1
})

const filters = computed<SalesEmployeesFilters>(() => ({
  branchId: branchModel.value,
  search: searchModel.value,
  shiftState: shiftModel.value,
  page: page.value,
  pageSize,
  sort: sortModel.value,
}))

const listQuery = useSalesEmployeesQuery(filters, canRead)
const items = computed(() => listQuery.data.value?.items ?? [])
const totalCount = computed(() => listQuery.data.value?.totalCount ?? 0)

const dialogOpen = ref(false)
const selected = ref<SalesEmployeeCard | null>(null)
const bucketKey = ref<string | null>(null)
const ratingError = ref('')

const selectedId = computed(() => selected.value?.employeeId ?? null)
const selectedBranch = computed(() => selected.value?.primaryBranchId ?? branchModel.value)
const ratingEnabled = computed(() => dialogOpen.value && canRead.value)

const ratingQuery = useSalesEmployeeRatingSummaryQuery(
  selectedId,
  selectedBranch,
  bucketKey,
  ratingEnabled
)

const ratingBusy = computed(() => ratingQuery.isFetching.value)
const ratingSummary = computed(() =>
  bucketKey.value ? ratingQuery.data.value : ratingQuery.data.value
)
const bucketCustomers = computed(() =>
  bucketKey.value ? ratingQuery.data.value?.customers ?? [] : null
)
const bucketLabel = computed(() => {
  const map: Record<string, string> = {
    excellent: 'ممتاز',
    good: 'جيد',
    weak: 'ضعيف',
    rejected: 'متلكئ',
    bad: 'سيء',
  }
  return bucketKey.value ? map[bucketKey.value] || bucketKey.value : ''
})

watch(
  () => ratingQuery.isError.value,
  (err) => {
    ratingError.value = err ? 'تعذر تحميل تقييم الموظف' : ''
  }
)

function openRating(row: SalesEmployeeCard) {
  selected.value = row
  bucketKey.value = null
  ratingError.value = ''
  dialogOpen.value = true
}

function closeDialog() {
  dialogOpen.value = false
  selected.value = null
  bucketKey.value = null
}

function loadBucket(key: string) {
  bucketKey.value = key
}

function allowedLinks(row: SalesEmployeeCard) {
  return (row.links ?? []).filter((l) => l.allowed)
}

function goLink(link: SalesEmployeeLink, row: SalesEmployeeCard) {
  void router.push({
    path: link.path,
    query: {
      branchId: String(row.primaryBranchId || ''),
      employeeId: String(row.employeeId),
    },
  })
}

function shiftLabel(s: string) {
  if (s === 'Active') return 'في الدوام'
  if (s === 'NoShift') return 'بدون دوام'
  return s || '—'
}

function locationLabel(s: string) {
  if (s === 'Live') return 'مباشر'
  if (s === 'Stale') return 'قديم'
  if (s === 'NoLocation') return 'بلا موقع'
  if (s === 'NoShift') return 'لا دوام'
  return s || '—'
}

function ratingText(v: number | null | undefined) {
  if (v == null || Number.isNaN(Number(v))) return '—'
  return Number(v).toFixed(2)
}

function fmtDt(v: string | null | undefined) {
  if (!v) return '—'
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'Asia/Baghdad',
    }).format(new Date(v))
  } catch {
    return v
  }
}
</script>

<style scoped>
.se-page {
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
.se-title {
  margin: 0;
  font-size: 1.55rem;
  font-weight: 700;
}
.se-sub {
  margin: 0.35rem 0 0;
  font-size: 14px;
  color: #006a64;
}
.se-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}
.se-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 10rem;
  font-size: 14px;
}
.se-field--grow {
  flex: 1;
  min-width: 12rem;
}
.se-select,
.se-input {
  min-height: 44px;
  border: 1px solid #c5d4d2;
  border-radius: 8px;
  padding: 0 0.75rem;
  background: #fff;
  font: inherit;
  color: inherit;
}
.se-table-wrap {
  overflow: auto;
  background: #fff;
  border: 1px solid #d7e3e1;
  border-radius: 10px;
}
.se-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 720px;
}
.se-table th,
.se-table td {
  padding: 0.75rem;
  border-bottom: 1px solid #e8efee;
  text-align: start;
  vertical-align: top;
  font-size: 14px;
}
.se-table th {
  background: #f0f7f6;
  color: #006a64;
  font-weight: 600;
}
.se-linkish {
  border: 0;
  background: transparent;
  color: #006a64;
  font: inherit;
  cursor: pointer;
  text-decoration: underline;
  min-height: 44px;
}
.se-muted {
  color: #5a6f6c;
  text-decoration: none;
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
.se-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.se-btn--sm {
  min-height: 36px;
  padding: 0 0.65rem;
  font-size: 13px;
  margin-inline-start: 0.25rem;
}
.se-pager {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
.se-count {
  margin: 0;
  font-size: 14px;
  color: #5a6f6c;
}
.se-state {
  padding: 1rem;
  border-radius: 8px;
  background: #fff;
  border: 1px solid #d7e3e1;
}
.se-state--error {
  border-color: #b91c1c;
  color: #991b1b;
}
.se-state--denied {
  border-color: #a16207;
  color: #854d0e;
}
@media (max-width: 768px) {
  .se-table {
    min-width: 560px;
  }
}
</style>
