<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
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
      <h1 class="fw-page__title">مسارات المتابعين</h1>
      <p class="fw-page__sub">
        نقاط حسب وقت الالتقاط ليوم بغداد — markers فقط بلا خط يوحي بمسار معروف عند الفجوات.
      </p>
    </header>

    <div v-if="!canRead" class="fw-state fw-state--forbidden" role="alert">
      ليس لديك صلاحية قراءة المسارات.
    </div>

    <template v-else>
      <div class="fw-toolbar">
        <label class="fw-sort">
          <span class="fw-sort__label">المحافظة</span>
          <select
            class="fw-sort__select"
            :value="branchId ?? ''"
            @change="
              patchQuery({
                branchId: ($event.target as HTMLSelectElement).value || null,
                followerId: null,
                page: '1',
              })
            "
          >
            <option value="">اختر</option>
            <option v-for="b in auth.branches ?? []" :key="b.id" :value="b.id">
              {{ b.name }}
            </option>
          </select>
        </label>
        <label class="fw-sort">
          <span class="fw-sort__label">المتابع</span>
          <select
            class="fw-sort__select"
            :value="followerId ?? ''"
            :disabled="branchId == null"
            @change="
              patchQuery({
                followerId: ($event.target as HTMLSelectElement).value || null,
                page: '1',
              })
            "
          >
            <option value="">اختر</option>
            <option
              v-for="f in followersQuery.data.value?.items ?? []"
              :key="f.followerId"
              :value="f.followerId"
            >
              {{ f.followerName }}
            </option>
          </select>
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

      <div v-if="branchId == null || followerId == null" class="fw-state">
        اختر محافظة ومتابعًا لعرض المسار.
      </div>
      <div v-else-if="routeQuery.isPending.value" class="fw-state fw-state--loading" aria-busy="true">
        جاري تحميل المسار…
      </div>
      <div v-else-if="routeQuery.isError.value" class="fw-state fw-state--error" role="alert">
        تعذر تحميل المسار.
        <button type="button" class="fw-retry" @click="routeQuery.refetch()">إعادة المحاولة</button>
      </div>

      <template v-else-if="routeData">
        <p v-if="gapsNote" class="fw-page__sub" role="status">{{ gapsNote }}</p>
        <p v-if="routeData.truncated" class="fw-page__sub" role="status">
          النتائج مبتورة — صفحة {{ routeData.page }} / {{ routeData.totalPages }} ({{ routeData.totalCount }} نقطة).
        </p>

        <FollowerMapCanvas
          :pins="pins"
          :selected-key="selectedIndex >= 0 ? String(selectedIndex) : null"
          @select="(k) => (selectedIndex = Number(k))"
        />

        <div class="fdc-meta" style="margin-top: 0.75rem">
          <span>{{ routeData.followerName || `متابع ${routeData.followerId}` }}</span>
          <span>النقاط: <b>{{ points.length }}</b> / {{ routeData.totalCount }}</span>
        </div>

        <div v-if="!points.length" class="fw-state" role="status">لا نقاط لهذا اليوم.</div>
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
