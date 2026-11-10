<template>
  <div class="slm-page" dir="rtl">
    <header class="slm-head">
      <h1 class="slm-title">الموقع المباشر — موظفو المبيعات</h1>
      <p class="slm-sub">
        نقاط ActorKind=SalesEmployee فقط · عتبة مباشر
        {{ liveSec }}ث · التتبع بالمقدمة من تطبيق الموظف عند الدوام
      </p>
    </header>

    <div v-if="!canRead" class="slm-state slm-state--denied">غير مخول بقراءة مواقع موظفي المبيعات.</div>
    <template v-else>
      <div class="slm-filters">
        <label class="slm-field">
          <span>الفرع</span>
          <select v-model="branchModel" class="slm-select">
            <option :value="null">كل الفروع المسموحة</option>
            <option v-for="b in branches" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </label>
        <label class="slm-field slm-field--grow">
          <span>بحث / رقم موظف</span>
          <input v-model="searchModel" class="slm-input" type="search" placeholder="اسم أو رقم" />
        </label>
        <button type="button" class="slm-btn" @click="recenter">إعادة تمركز</button>
      </div>

      <div v-if="query.isLoading.value" class="slm-state">جاري التحميل…</div>
      <div v-else-if="query.isError.value" class="slm-state slm-state--error">تعذر تحميل المواقع.</div>
      <div v-else class="slm-layout">
        <aside class="slm-list">
          <p class="slm-count">{{ filtered.length }} موظف</p>
          <button
            v-for="row in filtered"
            :key="row.employeeId"
            type="button"
            class="slm-row"
            :class="{ active: selectedId === row.employeeId }"
            @click="selectedId = row.employeeId"
          >
            <strong>{{ row.employeeName }}</strong>
            <span>{{ row.branchName }}</span>
            <span>{{ statusLabel(row.locationStatus) }} · {{ shiftLabel(row.shiftStatus) }}</span>
            <span class="slm-muted">{{ fmtDt(row.capturedAtUtc) }}</span>
            <span v-if="!row.hasCoordinates" class="slm-warn">بلا نقطة على الخريطة</span>
          </button>
          <p v-if="!filtered.length" class="slm-state">لا موظفون مطابقون.</p>
        </aside>
        <div class="slm-map">
          <FollowerMapCanvas
            :pins="pins"
            :selected-key="selectedKey"
            @select="onSelectPin"
          />
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import FollowerMapCanvas, {
  type MapPin,
} from '@/modules/FollowerWork/components/tracking/FollowerMapCanvas.vue'
import { useSalesEmployeeLiveLocationsQuery } from '../requests/queries/useSalesEmployeeLiveLocationsQuery'
import type { SalesEmployeeLiveLocationRow } from '../domain/types'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('SalesLocation', 'Read') === true)
const branches = computed(() => auth.branches ?? [])

const branchModel = ref<number | null>(null)
const searchModel = ref('')
const selectedId = ref<number | null>(null)
const employeeFilter = ref<number | null>(null)
const pageVisible = ref(typeof document === 'undefined' ? true : document.visibilityState === 'visible')
const recenterToken = ref(0)

watch(
  () => route.query.branchId,
  (raw) => {
    const n = Number(raw)
    branchModel.value = Number.isInteger(n) && n > 0 ? n : null
  },
  { immediate: true }
)
watch(branchModel, (v) => {
  const q = { ...route.query } as Record<string, string>
  if (v != null && v > 0) q.branchId = String(v)
  else delete q.branchId
  void router.replace({ query: q })
})

const query = useSalesEmployeeLiveLocationsQuery(branchModel, employeeFilter, canRead)

let pollTimer: ReturnType<typeof setInterval> | null = null
function startPoll() {
  stopPoll()
  if (!canRead.value) return
  pollTimer = setInterval(() => {
    if (pageVisible.value) void query.refetch()
  }, 15000)
}
function stopPoll() {
  if (pollTimer) clearInterval(pollTimer)
  pollTimer = null
}
function onVisibility() {
  pageVisible.value = document.visibilityState === 'visible'
  if (pageVisible.value) void query.refetch()
}

onMounted(() => {
  document.addEventListener('visibilitychange', onVisibility)
  startPoll()
})
onBeforeUnmount(() => {
  document.removeEventListener('visibilitychange', onVisibility)
  stopPoll()
})

watch([branchModel, canRead], () => {
  selectedId.value = null
  startPoll()
})

const rows = computed(() => query.data.value?.items ?? [])
const liveSec = computed(() => query.data.value?.liveThresholdSeconds ?? 150)

const filtered = computed(() => {
  const term = searchModel.value.trim()
  if (!term) return rows.value
  const n = Number(term)
  return rows.value.filter(
    (r) =>
      r.employeeName.includes(term) ||
      (Number.isInteger(n) && n > 0 && r.employeeId === n)
  )
})

const selectedKey = computed(() =>
  selectedId.value != null ? String(selectedId.value) : null
)

function statusColor(status: string) {
  if (status === 'Live') return '#16a34a'
  if (status === 'Stale') return '#ea580c'
  return '#64748b'
}

const pins = computed<MapPin[]>(() =>
  filtered.value
    .filter((r) => r.hasCoordinates && r.latitude != null && r.longitude != null)
    .map((r) => ({
      key: String(r.employeeId),
      lat: r.latitude!,
      lng: r.longitude!,
      color: statusColor(r.locationStatus),
      popupHtml: `<strong>${escapeHtml(r.employeeName)}</strong><br/>${escapeHtml(statusLabel(r.locationStatus))}<br/>${escapeHtml(fmtDt(r.capturedAtUtc))}`,
    }))
)

function onSelectPin(key: string) {
  selectedId.value = Number(key) || null
}

function recenter() {
  recenterToken.value++
  selectedId.value = null
  void query.refetch()
}

function statusLabel(s: string) {
  if (s === 'Live') return 'مباشر'
  if (s === 'Stale') return 'قديم'
  if (s === 'NoLocation') return 'بلا موقع'
  if (s === 'NoShift') return 'لا دوام'
  return s
}
function shiftLabel(s: string) {
  return s === 'Active' ? 'في الدوام' : 'بدون دوام'
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
function escapeHtml(s: string) {
  return s
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
}

void recenterToken
</script>

<style scoped>
.slm-page {
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
.slm-title {
  margin: 0;
  font-size: 1.55rem;
  font-weight: 700;
}
.slm-sub {
  margin: 0.35rem 0 0;
  font-size: 14px;
  color: #006a64;
}
.slm-filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: end;
}
.slm-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 10rem;
  font-size: 14px;
}
.slm-field--grow {
  flex: 1;
}
.slm-select,
.slm-input {
  min-height: 44px;
  border: 1px solid #c5d4d2;
  border-radius: 8px;
  padding: 0 0.75rem;
  background: #fff;
  font: inherit;
}
.slm-btn {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font: inherit;
  cursor: pointer;
}
.slm-layout {
  display: grid;
  grid-template-columns: minmax(220px, 320px) 1fr;
  gap: 0.75rem;
  min-height: 480px;
}
.slm-list {
  background: #fff;
  border: 1px solid #d7e3e1;
  border-radius: 10px;
  overflow: auto;
  max-height: 70vh;
  padding: 0.5rem;
}
.slm-row {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 0.2rem;
  text-align: start;
  padding: 0.65rem;
  border: 0;
  border-bottom: 1px solid #e8efee;
  background: transparent;
  font: inherit;
  cursor: pointer;
  min-height: 44px;
}
.slm-row.active {
  background: #f0faf8;
}
.slm-map {
  min-height: 480px;
  border-radius: 10px;
  overflow: hidden;
  border: 1px solid #d7e3e1;
  background: #fff;
}
.slm-count {
  margin: 0 0 0.5rem;
  font-size: 14px;
  color: #5a6f6c;
}
.slm-muted {
  color: #5a6f6c;
  font-size: 13px;
}
.slm-warn {
  color: #a16207;
  font-size: 13px;
}
.slm-state {
  padding: 1rem;
  border-radius: 8px;
  background: #fff;
  border: 1px solid #d7e3e1;
}
.slm-state--error {
  border-color: #b91c1c;
  color: #991b1b;
}
.slm-state--denied {
  border-color: #a16207;
  color: #854d0e;
}
@media (max-width: 768px) {
  .slm-layout {
    grid-template-columns: 1fr;
  }
  .slm-map {
    min-height: 360px;
  }
}
</style>
