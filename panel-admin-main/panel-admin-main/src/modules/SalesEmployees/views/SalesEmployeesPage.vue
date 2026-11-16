<template>
  <AppPage
    title="موظفو المبيعات"
    subtitle="دور المندوب ضمن الفروع المسموحة — الدوام من SalesShift وليس من Login/GPS."
  >
    <template #filters>
      <div class="q-filters">
        <AppSelect
          v-model="branchModel"
          label="الفرع"
          placeholder="كل الفروع المسموحة"
          :options="branchOptions"
        />
        <AppInput
          v-model="searchModel"
          label="بحث"
          type="search"
          placeholder="اسم الموظف"
          grow
        />
        <AppSelect
          v-model="shiftModel"
          label="الدوام"
          placeholder="الكل"
          :options="shiftOptions"
        />
        <AppSelect v-model="sortModel" label="ترتيب" :options="sortOptions" />
      </div>
    </template>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="غير مخول بقراءة موظفي المبيعات."
    />
    <DataState
      v-else-if="listQuery.isLoading.value"
      kind="loading"
      title="جاري التحميل"
      message="جاري جلب قائمة الموظفين."
    />
    <DataState
      v-else-if="listQuery.isError.value"
      kind="error"
      title="تعذر التحميل"
      message="تعذر تحميل القائمة."
    >
      <template #actions>
        <AppButton variant="secondary" @click="listQuery.refetch()">إعادة المحاولة</AppButton>
      </template>
    </DataState>
    <template v-else>
      <p class="q-count">{{ totalCount }} موظف</p>
      <div class="q-table-wrap">
        <table class="q-table">
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
                <AppButton variant="ghost" size="sm" @click="openRating(row)">
                  {{ ratingText(row.latestRating) }}
                  <span>/ {{ ratingText(row.phantomLatestRating) }}</span>
                </AppButton>
              </td>
              <td>{{ row.salesTodayCount }} / {{ row.pendingSalesCount }}</td>
              <td>
                <div class="se-actions">
                  <AppButton
                    v-for="link in allowedLinks(row)"
                    :key="link.key"
                    variant="secondary"
                    size="sm"
                    @click="goLink(link, row)"
                  >
                    {{ link.labelAr }}
                  </AppButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <DataState
          v-if="!items.length"
          kind="empty"
          title="لا نتائج"
          message="لا موظفون مطابقون للفلاتر (صفر فعلي)."
        />
      </div>

      <div class="q-pager">
        <AppButton variant="secondary" :disabled="page <= 1" @click="page = page - 1">
          السابق
        </AppButton>
        <span>صفحة {{ page }}</span>
        <AppButton
          variant="secondary"
          :disabled="page * pageSize >= totalCount"
          @click="page = page + 1"
        >
          التالي
        </AppButton>
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
  </AppPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppInput, AppPage, AppSelect, DataState } from '@/modules/Core/components/ui'
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
const branchOptions = computed(() =>
  branches.value.map((b) => ({ label: b.name, value: b.id })),
)
const shiftOptions = [
  { label: 'في الدوام', value: 'Active' },
  { label: 'بدون دوام', value: 'NoShift' },
]
const sortOptions = [
  { label: 'الاسم', value: 'name' },
  { label: 'الفرع', value: 'branch' },
  { label: 'الدوام', value: 'shift' },
  { label: 'التقييم', value: 'rating' },
  { label: 'آخر موقع', value: 'lastLocation' },
]

const branchModel = ref<number | null>(null)
const searchModel = ref('')
const shiftModel = ref<string | null>('')
const sortModel = ref<string>('name')
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
  { deep: true },
)

watch([branchModel, searchModel, shiftModel, sortModel, page], () => {
  const q: Record<string, string> = {}
  if (branchModel.value != null && branchModel.value > 0) q.branchId = String(branchModel.value)
  if (searchModel.value.trim()) q.search = searchModel.value.trim()
  if (shiftModel.value) q.shiftState = String(shiftModel.value)
  if (sortModel.value && sortModel.value !== 'name') q.sort = String(sortModel.value)
  if (page.value > 1) q.page = String(page.value)
  void router.replace({ query: q })
})

watch([branchModel, searchModel, shiftModel, sortModel], () => {
  page.value = 1
})

const filters = computed<SalesEmployeesFilters>(() => ({
  branchId: branchModel.value,
  search: searchModel.value,
  shiftState: typeof shiftModel.value === 'string' ? shiftModel.value : '',
  page: page.value,
  pageSize,
  sort: typeof sortModel.value === 'string' ? sortModel.value : 'name',
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
  ratingEnabled,
)

const ratingBusy = computed(() => ratingQuery.isFetching.value)
const ratingSummary = computed(() => ratingQuery.data.value ?? null)
const bucketCustomers = computed(() =>
  bucketKey.value ? ratingQuery.data.value?.customers ?? [] : null,
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
  },
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
    return new Intl.DateTimeFormat('en-US', {
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
.se-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
</style>
