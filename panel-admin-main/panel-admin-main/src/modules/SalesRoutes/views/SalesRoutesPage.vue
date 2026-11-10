<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
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

function fmtAcc(v: number | null | undefined) {
  if (v == null || !Number.isFinite(v)) return '—'
  return `${Math.round(v)} م`
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
  if (n > 0) return `توجد ${n} فجوة زمنية > 30 دقيقة — markers فقط بلا خط يوحي بمسار متصل.`
  if (points.value.length < 2) return null
  let largeGaps = 0
  for (let i = 1; i < points.value.length; i++) {
    const a = new Date(points.value[i - 1].capturedAtUtc).getTime()
    const b = new Date(points.value[i].capturedAtUtc).getTime()
    if (Number.isFinite(a) && Number.isFinite(b) && b - a > 30 * 60 * 1000) largeGaps++
  }
  return largeGaps > 0
    ? `توجد ${largeGaps} فجوة زمنية > 30 دقيقة — لا يُرسم خط بينها كمسار متصل.`
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

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة المسارات (SalesRoute.Read). صلاحية الموقع المباشر لا تكفي.
    </div>

    <template v-else>
      <div class="fw-toolbar">
        <label class="fw-sort">
          <span class="fw-sort__label">الفرع</span>
          <select
            class="fw-sort__select"
            :value="branchId ?? ''"
            @change="
              patchQuery({
                branchId: ($event.target as HTMLSelectElement).value || null,
                page: '1',
              })
            "
          >
            <option value="">كل الفروع المسموحة</option>
            <option v-for="b in auth.branches ?? []" :key="b.id" :value="b.id">
              {{ b.name }}
            </option>
          </select>
        </label>
        <label class="fw-sort">
          <span class="fw-sort__label">الموظف</span>
          <select
            v-if="canListEmployees"
            class="fw-sort__select"
            :value="employeeId ?? ''"
            @change="
              patchQuery({
                employeeId: ($event.target as HTMLSelectElement).value || null,
                page: '1',
              })
            "
          >
            <option value="">اختر</option>
            <option
              v-for="e in employeesQuery.data.value?.items ?? []"
              :key="e.employeeId"
              :value="e.employeeId"
            >
              {{ e.employeeName }}
            </option>
          </select>
          <input
            v-else
            class="fw-sort__select"
            type="number"
            min="1"
            :value="employeeId ?? ''"
            placeholder="رقم الموظف"
            @change="
              patchQuery({
                employeeId: ($event.target as HTMLInputElement).value || null,
                page: '1',
              })
            "
          />
        </label>
        <label class="fw-sort">
          <span class="fw-sort__label">التاريخ</span>
          <input
            class="fw-sort__select"
            type="date"
            :value="dateIraq"
            @change="
              patchQuery({
                date: ($event.target as HTMLInputElement).value || null,
                page: '1',
              })
            "
          />
        </label>
      </div>

      <div v-if="employeeId == null" class="fw-state">اختر موظفًا لعرض المسار.</div>
      <div v-else-if="routeQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
        جاري تحميل المسار…
      </div>
      <div v-else-if="routeQuery.isError.value" class="fw-state fw-state--error" role="alert">
        تعذر تحميل المسار.
        <button type="button" class="fw-retry" @click="routeQuery.refetch()">إعادة المحاولة</button>
      </div>

      <template v-else-if="routeData">
        <p class="fw-page__sub" role="status">
          الفترة: {{ fmtDt(routeData.fromUtc) }} → {{ fmtDt(routeData.toUtc) }}
          · {{ routeData.dayBoundNote }}
        </p>
        <p v-if="gapsNote" class="fw-page__sub" role="status">{{ gapsNote }}</p>
        <p v-if="routeData.truncated" class="fw-page__sub" role="status">
          النتائج مبتورة — صفحة {{ routeData.page }} / {{ routeData.totalPages }} ({{
            routeData.totalCount
          }}
          نقطة). استخدم التصفح للاستكمال.
        </p>

        <FollowerMapCanvas
          :pins="pins"
          :selected-key="selectedIndex >= 0 ? String(selectedIndex) : null"
          @select="(k) => (selectedIndex = Number(k))"
        />

        <div class="fdc-meta" style="margin-top: 0.75rem">
          <span>{{ routeData.employeeName || `موظف ${routeData.employeeId}` }}</span>
          <span>النقاط: <b>{{ points.length }}</b> / {{ routeData.totalCount }}</span>
        </div>

        <div v-if="!points.length" class="fw-state" role="status">لا نقاط لهذا اليوم ضمن النطاق.</div>
        <div v-else class="fdc-list" role="list">
          <button
            v-for="(p, i) in points"
            :key="p.pointId"
            type="button"
            class="fdc-row"
            :class="{ 'fdc-row--active': selectedIndex === i }"
            @click="selectedIndex = i"
          >
            <span class="fdc-row__title">نقطة {{ i + 1 }}</span>
            <span class="fdc-row__sub">
              {{ fmtDt(p.capturedAtUtc) }} · دقة {{ fmtAcc(p.accuracyMeters) }} ·
              {{ p.latitude.toFixed(5) }}, {{ p.longitude.toFixed(5) }}
              <template v-if="p.branchId"> · فرع #{{ p.branchId }}</template>
            </span>
          </button>
        </div>

        <div v-if="routeData.totalPages > 1" class="fdc-pager">
          <button
            type="button"
            class="fw-pager__btn"
            :disabled="page <= 1"
            @click="patchQuery({ page: String(page - 1) })"
          >
            السابق
          </button>
          <span>صفحة {{ routeData.page }} / {{ routeData.totalPages }}</span>
          <button
            type="button"
            class="fw-pager__btn"
            :disabled="page >= routeData.totalPages"
            @click="patchQuery({ page: String(page + 1) })"
          >
            التالي
          </button>
        </div>
      </template>
    </template>
  </div>
</template>
