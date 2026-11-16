<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppDateField, AppSelect, DataState } from '@/modules/Core/components/ui'
import { formatDateTime, formatNumber } from '@/modules/Core/utils/formatDisplay'
import { useFollowersQuery } from '../requests/queries/useFollowersQuery'
import { useFollowerRouteQuery } from '../requests/queries/useFollowerRouteQuery'
import FollowerMapCanvas, {
  type MapPin,
} from '../components/tracking/FollowerMapCanvas.vue'
import type { FollowerWorkSort } from '../domain/types'
import '../components/follower-work.css'
import '../components/declining/declining.css'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(() => auth.hasPermission('FollowerRoute', 'Read') === true)
const branchId = computed(() => {
  const n = Number(route.query.branchId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const followerId = computed(() => {
  const n = Number(route.query.followerId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const dateIraq = computed(() => {
  const v = String(route.query.date ?? '').trim()
  if (v) return v
  // يوم بغداد التقريبي من المتصفح
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
const sort = ref<FollowerWorkSort>('name')
const search = ref('')

const followersQuery = useFollowersQuery({
  branchId,
  search,
  page: ref(1),
  pageSize: ref(100),
  sort,
  enabled: canRead,
})

const routeQuery = useFollowerRouteQuery({
  followerId,
  branchId,
  date: dateIraq,
  page,
  pageSize,
  enabled: canRead,
})

watch([followerId, dateIraq, branchId], () => {
  selectedIndex.value = -1
})

const routeData = computed(() => routeQuery.data.value)
const points = computed(() => routeData.value?.points ?? [])

function patchQuery(next: Record<string, string | null | undefined>) {
  const q: Record<string, string> = { ...route.query } as Record<string, string>
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({
    name: route.name ?? 'ManagementFollowerRoutesRoute',
    query: q,
    params: route.params,
  })
}

const branchOptions = computed(() => [
  { label: 'اختر', value: null as number | null },
  ...(auth.branches ?? []).map((b) => ({ label: b.name, value: b.id })),
])
const followerOptions = computed(() => [
  { label: 'اختر', value: null as number | null },
  ...(followersQuery.data.value?.items ?? []).map((f) => ({
    label: f.followerName,
    value: f.followerId,
  })),
])

function onBranch(v: string | number | null | boolean) {
  patchQuery({
    branchId: v == null || v === '' || typeof v === 'boolean' ? null : String(v),
    followerId: null,
    page: '1',
  })
}
function onFollower(v: string | number | null | boolean) {
  patchQuery({
    followerId: v == null || v === '' || typeof v === 'boolean' ? null : String(v),
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
      <h1 class="fw-page__title">مسارات المتابعين</h1>
      <p class="fw-page__sub">
        نقاط حسب وقت الالتقاط ليوم بغداد — markers فقط بلا خط يوحي بمسار معروف عند الفجوات.
      </p>
    </header>

    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة المسارات."
    />

    <template v-else>
      <div class="q-filters">
        <AppSelect
          :model-value="branchId"
          label="المحافظة"
          placeholder="اختر"
          :options="branchOptions"
          @update:model-value="onBranch"
        />
        <AppSelect
          :model-value="followerId"
          label="المتابع"
          placeholder="اختر"
          :disabled="branchId == null"
          :options="followerOptions"
          @update:model-value="onFollower"
        />
        <AppDateField
          :model-value="dateIraq"
          label="التاريخ"
          @update:model-value="(v) => patchQuery({ date: v || null, page: '1' })"
        />
      </div>

      <DataState
        v-if="branchId == null || followerId == null"
        kind="empty"
        title="اختر مسارًا"
        message="اختر محافظة ومتابعًا لعرض المسار."
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
        <p v-if="gapsNote" class="fw-page__sub" role="status">{{ gapsNote }}</p>
        <p v-if="routeData.truncated" class="fw-page__sub" role="status">
          النتائج مبتورة — صفحة {{ formatNumber(routeData.page) }} / {{ formatNumber(routeData.totalPages) }} ({{ formatNumber(routeData.totalCount) }} نقطة).
        </p>

        <FollowerMapCanvas
          :pins="pins"
          :selected-key="selectedIndex >= 0 ? String(selectedIndex) : null"
          @select="(k) => (selectedIndex = Number(k))"
        />

        <div class="fdc-meta" style="margin-top: 0.75rem">
          <span>{{ routeData.followerName || `متابع ${routeData.followerId}` }}</span>
          <span>النقاط: <b>{{ formatNumber(points.length) }}</b> / {{ formatNumber(routeData.totalCount) }}</span>
        </div>

        <DataState v-if="!points.length" kind="empty" title="لا نقاط" message="لا نقاط لهذا اليوم." />
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
