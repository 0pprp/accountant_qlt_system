<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppSelect, DataState } from '@/modules/Core/components/ui'
import { formatDateTime, formatNumber } from '@/modules/Core/utils/formatDisplay'
import { useFollowerLiveLocationsQuery } from '../requests/queries/useFollowerLiveLocationsQuery'
import FollowerMapCanvas, {
  type MapPin,
} from '../components/tracking/FollowerMapCanvas.vue'
import { locationStatusLabel, type FollowerLiveLocationRow } from '../domain/trackingTypes'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerLocation', 'Read') === true)
const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const followerFilter = computed(() => {
  const n = Number(route.query.followerId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const branchName = computed(() => {
  const b = (auth.branches ?? []).find((x) => x.id === branchId.value)
  return b?.name ?? ''
})
const pageVisible = ref(typeof document === 'undefined' ? true : document.visibilityState === 'visible')
const selectedId = ref<number | null>(null)

const liveQuery = useFollowerLiveLocationsQuery({
  branchId,
  followerId: followerFilter,
  enabled: canRead,
})

let pollTimer: ReturnType<typeof setInterval> | null = null
function startPoll() {
  stopPoll()
  if (!canRead.value) return
  pollTimer = setInterval(() => {
    if (pageVisible.value) void liveQuery.refetch()
  }, 15000)
}
function stopPoll() {
  if (pollTimer) clearInterval(pollTimer)
  pollTimer = null
}

function onVisibility() {
  pageVisible.value = document.visibilityState === 'visible'
  if (pageVisible.value) void liveQuery.refetch()
}

onMounted(() => {
  document.addEventListener('visibilitychange', onVisibility)
  startPoll()
})
onBeforeUnmount(() => {
  document.removeEventListener('visibilitychange', onVisibility)
  stopPoll()
})

watch([branchId, canRead], () => {
  selectedId.value = null
  startPoll()
})

const data = computed(() => liveQuery.data.value)
const rows = computed(() => data.value?.items ?? [])
const withCoords = computed(() =>
  rows.value.filter(
    (r) =>
      r.latitude != null &&
      r.longitude != null &&
      Number.isFinite(r.latitude) &&
      Number.isFinite(r.longitude)
  )
)

function statusColor(status: string) {
  if (status === 'Live') return '#16a34a'
  if (status === 'Stale') return '#ea580c'
  return '#64748b'
}

const branchOptions = computed(() => [
  { label: 'اختر', value: null as number | null },
  ...(auth.branches ?? []).map((b) => ({ label: b.name, value: b.id })),
])

function fmtDt(v: string | null | undefined) {
  return formatDateTime(v)
}

function fmtAcc(v: number | null | undefined) {
  if (v == null || !Number.isFinite(v)) return '—'
  return `${formatNumber(Math.round(v))} م`
}

const pins = computed<MapPin[]>(() =>
  withCoords.value.map((r) => ({
    key: String(r.followerId),
    lat: r.latitude!,
    lng: r.longitude!,
    color: statusColor(r.locationStatus),
    popupHtml: `<div dir="rtl"><b>${r.followerName}</b><br>${r.branchName || ''}<br>${fmtDt(r.capturedAtUtc)}<br>${locationStatusLabel[r.locationStatus] || r.locationStatus}</div>`,
  }))
)

const selected = computed<FollowerLiveLocationRow | null>(() => {
  const id = selectedId.value
  if (id == null) return null
  return rows.value.find((r) => r.followerId === id) ?? null
})

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerLiveMapRoute',
    query: q,
    params: route.params,
  })
}

function onSelectPin(key: string) {
  selectedId.value = Number(key)
}

function setBranch(id: number | null) {
  patchQuery({
    branchId: id != null ? String(id) : null,
    followerId: null,
  })
}

function onBranchPick(v: string | number | null | boolean) {
  const n = typeof v === 'number' ? v : Number(v)
  setBranch(Number.isInteger(n) && n > 0 ? n : null)
}
</script>

<template>
  <div class="fw-page" dir="rtl">
    <header class="fw-page__header">
      <h1 class="fw-page__title">الموقع المباشر</h1>
      <p class="fw-page__sub">
        متابعون مكلّفون فقط — مركز المحافظة للخريطة ليس موقع شخص.
        تحديث كل 15 ثانية أثناء بقاء الصفحة مرئية.
      </p>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة الموقع المباشر."
    />

    <template v-else>
      <div class="q-filters">
        <AppSelect
          :model-value="branchId"
          label="المحافظة"
          placeholder="اختر"
          :options="branchOptions"
          @update:model-value="onBranchPick"
        />
        <span v-if="branchName" class="fw-page__sub">{{ branchName }}</span>
      </div>

      <DataState
        v-if="branchId == null"
        kind="empty"
        title="اختر محافظة"
        message="اختر محافظة لعرض المواقع."
      />
      <DataState
        v-else-if="liveQuery.isPending.value"
        kind="loading"
        title="جاري التحميل"
        message="جاري تحميل المواقع…"
      />
      <DataState
        v-else-if="liveQuery.isError.value"
        kind="error"
        title="تعذر التحميل"
        message="تعذر تحميل المواقع."
      >
        <template #actions>
          <AppButton variant="secondary" @click="liveQuery.refetch()">إعادة المحاولة</AppButton>
        </template>
      </DataState>

      <template v-else-if="data">
        <FollowerMapCanvas
          :pins="pins"
          :selected-key="selectedId != null ? String(selectedId) : null"
          @select="onSelectPin"
        />

        <div class="fdc-meta" style="margin-top: 0.75rem">
          <span>بعلامات: <b>{{ formatNumber(withCoords.length) }}</b></span>
          <span>إجمالي الصفوف: <b>{{ formatNumber(rows.length) }}</b></span>
        </div>

        <DataState
          v-if="!rows.length"
          kind="empty"
          title="لا متابعون"
          message="لا متابعون في هذا النطاق."
        />

        <div v-else class="fdc-list" role="list" style="margin-top: 0.75rem">
          <button
            v-for="r in rows"
            :key="r.followerId"
            type="button"
            class="fdc-row"
            :class="{ 'fdc-row--active': selectedId === r.followerId }"
            @click="selectedId = r.followerId"
          >
            <span class="fdc-row__title">{{ r.followerName }}</span>
            <span class="fdc-row__sub">
              {{ r.branchName || '—' }} ·
              {{ locationStatusLabel[r.locationStatus] || r.locationStatus }} ·
              {{ fmtDt(r.capturedAtUtc) }} · دقة {{ fmtAcc(r.accuracyMeters) }}
            </span>
            <span v-if="r.latitude == null" class="fdc-row__sub">لا موقع للعرض على الخريطة</span>
          </button>
        </div>

        <section v-if="selected" class="fdc-details" aria-label="تفاصيل المتابع">
          <div class="fdc-details__head">
            <h2>{{ selected.followerName }}</h2>
            <button type="button" class="fw-pager__btn" @click="selectedId = null">إغلاق</button>
          </div>
          <div class="fdc-details__grid">
            <div><span>الحالة</span><b>{{ locationStatusLabel[selected.locationStatus] || selected.locationStatus }}</b></div>
            <div><span>دوام نشط</span><b>{{ selected.hasActiveShift ? 'نعم' : 'لا' }}</b></div>
            <div><span>آخر التقاط</span><b>{{ fmtDt(selected.capturedAtUtc) }}</b></div>
            <div><span>الاستلام</span><b>{{ fmtDt(selected.receivedAtUtc) }}</b></div>
            <div><span>الدقة</span><b>{{ fmtAcc(selected.accuracyMeters) }}</b></div>
            <div>
              <span>الإحداثيات</span>
              <b>
                <template v-if="selected.latitude != null">
                  {{ selected.latitude.toFixed(5) }}, {{ selected.longitude!.toFixed(5) }}
                </template>
                <template v-else>—</template>
              </b>
            </div>
          </div>
        </section>
      </template>
    </template>
  </div>
</template>
