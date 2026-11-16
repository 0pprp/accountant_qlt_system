<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppDateField, AppInput, AppSelect, DataState } from '@/modules/Core/components/ui'
import { formatDateTime, formatNumber } from '@/modules/Core/utils/formatDisplay'
import {
  useSalesEmployeesQuery,
  type SalesEmployeesFilters,
} from '@/modules/SalesEmployees/requests/queries/useSalesEmployeesQuery'
import { useSalesEmployeeRouteQuery } from '../requests/queries/useSalesEmployeeRouteQuery'
import FollowerMapCanvas, {
  type MapPin,
} from '@/modules/FollowerWork/components/tracking/FollowerMapCanvas.vue'
import '@/modules/FollowerWork/components/follower-work.css'
import '@/modules/FollowerWork/components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesRoute', 'Read') === true)
const canListEmployees = computed(() => auth.hasPermission('SalesEmployee', 'Read') === true)

const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const employeeId = computed(() => {
  const n = Number(route.query.employeeId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const dateIraq = computed(() => {
  const v = String(route.query.date ?? '').trim()
  if (v) return v
  try {
    return new Intl.DateTimeFormat('en-CA', {
      timeZone: 'Asia/Baghdad',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    }).format(new Date())
  } catch {
    return new Date().toISOString().slice(0, 10)
  }
})
const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const pageSize = ref(500)
const selectedIndex = ref(-1)

const employeesFilters = computed<SalesEmployeesFilters>(() => ({
  branchId: branchId.value,
  search: '',
  shiftState: '',
  page: 1,
  pageSize: 100,
  sort: 'name',
}))
const employeesQuery = useSalesEmployeesQuery(employeesFilters, canListEmployees)

const routeQuery = useSalesEmployeeRouteQuery({
  employeeId,
  branchId,
  date: dateIraq,
  page,
  pageSize,
  enabled: canRead,
})

watch([employeeId, dateIraq, branchId], () => {
  selectedIndex.value = -1
})

const routeData = computed(() => routeQuery.data.value)
const points = computed(() => routeData.value?.points ?? [])
const gapsFromApi = computed(() => routeData.value?.gaps ?? [])

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'SaleRoutesRoute',
    query: q,
    params: route.params,
  })
}

const branchOptions = computed(() => [
  { label: 'كل الفروع المسموحة', value: null as number | null },
  ...(auth.branches ?? []).map((b) => ({ label: b.name, value: b.id })),
])
const employeeOptions = computed(() => [
  { label: 'اختر', value: null as number | null },
  ...(employeesQuery.data.value?.items ?? []).map((e) => ({
    label: e.employeeName,
    value: e.employeeId,
  })),
])

function onFilterId(key: 'branchId' | 'employeeId', v: string | number | null | boolean) {
  patchQuery({
    [key]: v == null || v === '' || typeof v === 'boolean' ? null : String(v),
    page: '1',
  })
}

function fmtDt(v: string | null | undefined) {
  return formatDateTime(v)
}

function fmtAcc(v: number | null | undefined) {
  if (v == null || !Number.isFinite(v)) return '—'
  return `${formatNumber(Math.round(v))} م`
}

const pins = computed<MapPin[]>(() =>
  points.value.map((p, i) => ({
    key: String(i),
    lat: p.latitude,
    lng: p.longitude,
    color: i === 0 || i === points.value.length - 1 ? '#006a64' : '#16a34a',
    popupHtml: `<div dir="rtl">${fmtDt(p.capturedAtUtc)}<br>دقة ${fmtAcc(p.accuracyMeters)}</div>`,
  }))
)

const gapsNote = computed(() => {
  const n = gapsFromApi.value.length
  if (n > 0) return `توجد ${formatNumber(n)} فجوة زمنية > 30 دقيقة — markers فقط بلا خط يوحي بمسار متصل.`
  if (points.value.length < 2) return null
  let largeGaps = 0
  for (let i = 1; i < points.value.length; i++) {
    const a = new Date(points.value[i - 1].capturedAtUtc).getTime()
    const b = new Date(points.value[i].capturedAtUtc).getTime()
    if (Number.isFinite(a) && Number.isFinite(b) && b - a > 30 * 60 * 1000) largeGaps++
  }
  return largeGaps > 0
    ? `توجد ${formatNumber(largeGaps)} فجوة زمنية > 30 دقيقة — لا يُرسم خط بينها كمسار متصل.`
    : null
})
</script>

<template>
  <div class="fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">مسارات موظفي المبيعات</h1>
      <p class="fw-page__sub">
        يوم تقويمي بغداد (00:00→+1) — ليس قطع بيع 03:00. نقاط ActorKind=SalesEmployee فقط.
        Markers بلا توليد نقاط وهمية.
      </p>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة المسارات (SalesRoute.Read). صلاحية الموقع المباشر لا تكفي."
    />

    <template v-else>
      <div class="q-filters">
        <AppSelect
          :model-value="branchId"
          label="الفرع"
          placeholder="كل الفروع المسموحة"
          :options="branchOptions"
          @update:model-value="onFilterId('branchId', $event)"
        />
        <AppSelect
          v-if="canListEmployees"
          :model-value="employeeId"
          label="الموظف"
          placeholder="اختر"
          :options="employeeOptions"
          @update:model-value="onFilterId('employeeId', $event)"
        />
        <AppInput
          v-else
          :model-value="employeeId ?? ''"
          label="الموظف"
          type="number"
          placeholder="رقم الموظف"
          @change="
            patchQuery({
              employeeId: ($event.target as HTMLInputElement).value || null,
              page: '1',
            })
          "
        />
        <AppDateField
          :model-value="dateIraq"
          label="التاريخ"
          @update:model-value="(v) => patchQuery({ date: v || null, page: '1' })"
        />
      </div>

      <DataState
        v-if="employeeId == null"
        kind="empty"
        title="اختر موظفًا"
        message="اختر موظفًا لعرض المسار."
      />
      <DataState
        v-else-if="routeQuery.isPending.value"
        kind="loading"
        title="جاري التحميل"
        message="جاري تحميل المسار…"
      />
      <DataState
        v-else-if="routeQuery.isError.value"
        kind="error"
        title="تعذر التحميل"
        message="تعذر تحميل المسار."
      >
        <template #actions>
          <AppButton variant="secondary" @click="routeQuery.refetch()">إعادة المحاولة</AppButton>
        </template>
      </DataState>

      <template v-else-if="routeData">
        <p class="fw-page__sub" role="status">
          الفترة: {{ fmtDt(routeData.fromUtc) }} → {{ fmtDt(routeData.toUtc) }}
          · {{ routeData.dayBoundNote }}
        </p>
        <p v-if="gapsNote" class="fw-page__sub" role="status">{{ gapsNote }}</p>
        <p v-if="routeData.truncated" class="fw-page__sub" role="status">
          النتائج مبتورة — صفحة {{ formatNumber(routeData.page) }} /
          {{ formatNumber(routeData.totalPages) }} ({{ formatNumber(routeData.totalCount) }}
          نقطة). استخدم التصفح للاستكمال.
        </p>

        <FollowerMapCanvas
          :pins="pins"
          :selected-key="selectedIndex >= 0 ? String(selectedIndex) : null"
          @select="(k) => (selectedIndex = Number(k))"
        />

        <div class="fdc-meta" style="margin-top: 0.75rem">
          <span>{{ routeData.employeeName || `موظف ${routeData.employeeId}` }}</span>
          <span>النقاط: <b>{{ formatNumber(points.length) }}</b> / {{ formatNumber(routeData.totalCount) }}</span>
        </div>

        <DataState
          v-if="!points.length"
          kind="empty"
          title="لا نقاط"
          message="لا نقاط لهذا اليوم ضمن النطاق."
        />
        <div v-else class="fdc-list" role="list">
          <button
            v-for="(p, i) in points"
            :key="p.pointId"
            type="button"
            class="fdc-row"
            :class="{ 'fdc-row--active': selectedIndex === i }"
            @click="selectedIndex = i"
          >
            <span class="fdc-row__title">نقطة {{ formatNumber(i + 1) }}</span>
            <span class="fdc-row__sub">
              {{ fmtDt(p.capturedAtUtc) }} · دقة {{ fmtAcc(p.accuracyMeters) }} ·
              {{ p.latitude.toFixed(5) }}, {{ p.longitude.toFixed(5) }}
              <template v-if="p.branchId"> · فرع #{{ formatNumber(p.branchId) }}</template>
            </span>
          </button>
        </div>

        <div v-if="routeData.totalPages > 1" class="fdc-pager">
          <AppButton
            variant="secondary"
            :disabled="page <= 1"
            @click="patchQuery({ page: String(page - 1) })"
          >
            السابق
          </AppButton>
          <span>صفحة {{ formatNumber(routeData.page) }} / {{ formatNumber(routeData.totalPages) }}</span>
          <AppButton
            variant="secondary"
            :disabled="page >= routeData.totalPages"
            @click="patchQuery({ page: String(page + 1) })"
          >
            التالي
          </AppButton>
        </div>
      </template>
    </template>
  </div>
</template>
