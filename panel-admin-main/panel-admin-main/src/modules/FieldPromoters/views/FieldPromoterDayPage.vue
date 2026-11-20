<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">يوم مروّج</h2>
        <p class="dfb-subtitle">المسار والمحلات والطلبات وفترات العمل خلال يوم واحد</p>
      </div>
      <div class="fpp-filters">
        <label class="fpp-field is-wide">
          <span>المروّج</span>
          <select class="dfb-search" :value="userId ?? ''" @change="setUser(($event.target as HTMLSelectElement).value)">
            <option value="" disabled>اختر مروّجاً</option>
            <option v-for="p in promoters" :key="p.id" :value="p.id">{{ p.fullName }}</option>
          </select>
        </label>
        <label class="fpp-field">
          <span>التاريخ</span>
          <input class="dfb-search" type="date" :value="date" :max="today" @change="setDate(($event.target as HTMLInputElement).value)">
        </label>
      </div>
    </header>

    <p v-if="userId == null" class="dfb-state">اختر مروّجاً لعرض يومه.</p>
    <p v-else-if="day.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="day.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(day.error.value, 'تعذر تحميل يوم المروّج.') }}
      <button type="button" class="dfb-button is-small" @click="day.refetch()">إعادة المحاولة</button>
    </div>
    <template v-else-if="data">
      <section class="fpp-tiles" aria-label="ملخص اليوم">
        <div class="fpp-tile">
          <span>ساعات العمل</span>
          <strong class="dfb-num">{{ hm(data.workMinutes) }}</strong>
          <small>الهدف {{ data.targetMinutes > 0 ? hm(data.targetMinutes) : 'بلا هدف' }}</small>
        </div>
        <div class="fpp-tile">
          <span>بداية الدوام</span>
          <strong>{{ time(data.firstShiftStart) }}</strong>
          <small>{{ num(data.sessions.length) }} جلسة</small>
        </div>
        <div class="fpp-tile">
          <span>المحلات</span>
          <strong>{{ num(data.pins) }}</strong>
          <small>{{ num(quickCount) }} زيارة سريعة</small>
        </div>
        <div class="fpp-tile">
          <span>الطلبات</span>
          <strong>{{ num(data.orders) }}</strong>
        </div>
        <div class="fpp-tile">
          <span>فترات العمل</span>
          <strong>{{ num(data.periods.length) }}</strong>
          <small>{{ num(gaps.length) }} فجوة ≥ 15 دقيقة</small>
        </div>
        <div class="fpp-tile">
          <span>حالة الهدف</span>
          <strong>{{ data.targetStatus || '—' }}</strong>
        </div>
      </section>

      <div class="fpp-split">
        <div class="fpp-card">
          <div class="fpp-card-head">
            <h3 class="fpp-card-title">المسار والمحلات</h3>
            <div class="fpp-legend">
              <span v-for="i in [5, 4, 3, 2, 1]" :key="i"><i class="fpp-dot" :style="{ background: INTEREST_COLORS[i] }" />{{ INTEREST_LABELS[i] }}</span>
            </div>
          </div>
          <p v-if="!markers.length && data.route.length < 2" class="dfb-state">لا توجد نقاط موقع أو محلات في هذا اليوم.</p>
          <div class="fpp-map">
            <FpMapCanvas :markers="markers" :route="routeLine" :selectedKey="selectedKey" label="مسار المروّج" @select="selectedKey = $event" />
          </div>
        </div>
        <aside class="fpp-card" aria-label="الخط الزمني">
          <h3 class="fpp-card-title">الخط الزمني</h3>
          <p v-if="!timeline.length" class="dfb-state">لا يوجد نشاط في هذا اليوم.</p>
          <ol v-else class="fpp-timeline">
            <li
              v-for="e in timeline"
              :key="e.key"
              class="fpp-tl-item"
              :class="`is-${e.kind}`"
              :style="{ '--tl-color': e.color }"
            >
              <span class="fpp-tl-time">{{ time(e.at) }}</span>
              <span class="fpp-tl-body">
                <strong>
                  <button v-if="e.mapKey" type="button" class="dfb-name-link" @click="selectedKey = e.mapKey">{{ e.title }}</button>
                  <template v-else>{{ e.title }}</template>
                  <span v-if="e.quick" class="fpp-quick">سريع</span>
                </strong>
                <small v-if="e.sub">{{ e.sub }}</small>
              </span>
            </li>
          </ol>
        </aside>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import FpMapCanvas, { type FpMarker } from '../components/FpMapCanvas.vue'
import { apiErrorMessage, usePromoterDayQuery, usePromotersLiveQuery } from '../requests/fieldPromoters'
import { INTEREST_COLORS, INTEREST_LABELS, escapeHtml, hm, iraqToday, num, routeDate, routeId, time } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'

const { route, branchId, replaceQuery } = useFpNav()
const today = iraqToday()
const userId = computed(() => routeId(route.query.userId))
const date = computed(() => {
  const d = routeDate(route.query.date)
  return d && d <= today ? d : today
})
const live = usePromotersLiveQuery(branchId)
const promoters = computed(() => live.data.value?.promoters ?? [])
const day = usePromoterDayQuery(userId, date)
const data = computed(() => (day.data.value && day.data.value.userId === userId.value ? day.data.value : null))
const selectedKey = ref<string | null>(null)

watch([userId, date], () => {
  selectedKey.value = null
})

function setUser(raw: string) {
  replaceQuery({ userId: raw || undefined })
}

function setDate(raw: string) {
  replaceQuery({ date: routeDate(raw) ?? undefined })
}

const quickCount = computed(() => data.value?.pinList.filter((p) => p.quick).length ?? 0)

const gaps = computed(() => {
  const periods = [...(data.value?.periods ?? [])].sort((a, b) => a.start.localeCompare(b.start))
  const out: { start: string; end: string; minutes: number }[] = []
  for (let i = 1; i < periods.length; i++) {
    const minutes = Math.round((Date.parse(periods[i].start) - Date.parse(periods[i - 1].end)) / 60000)
    if (minutes >= 15) out.push({ start: periods[i - 1].end, end: periods[i].start, minutes })
  }
  return out
})

const routeLine = computed<[number, number][]>(() => (data.value?.route ?? []).map((p) => [p.lat, p.lng]))

const markers = computed<FpMarker[]>(() => {
  const d = data.value
  if (!d) return []
  const pins = d.pinList.map((p, i) => ({
    key: `pin-${p.id}`,
    lat: p.lat,
    lng: p.lng,
    color: INTEREST_COLORS[p.interest] ?? '#8a9694',
    label: String(i + 1),
    popupHtml: `<div dir="rtl" style="min-width:160px"><strong>${i + 1}. ${escapeHtml(p.name)}</strong><br>${escapeHtml(p.interestLabel)} · ${escapeHtml(time(p.at))}${p.phone ? `<br><span dir="ltr">${escapeHtml(p.phone)}</span>` : ''}${p.note ? `<br>${escapeHtml(p.note)}` : ''}</div>`,
  }))
  const alerts = d.alerts.map((a) => ({
    key: `alert-${a.id}`,
    lat: a.lat,
    lng: a.lng,
    color: '#b23b3b',
    label: '!',
    popupHtml: `<div dir="rtl"><strong>وقوف ${num(a.minutes)} دقيقة</strong><br>من ${escapeHtml(time(a.since))} إلى ${escapeHtml(time(a.lastSeen))}</div>`,
  }))
  return [...pins, ...alerts]
})

interface TimelineEntry {
  key: string
  at: string
  kind: 'session' | 'pin' | 'order' | 'gap' | 'alert'
  title: string
  sub?: string
  color: string
  quick?: boolean
  mapKey?: string
}

const timeline = computed<TimelineEntry[]>(() => {
  const d = data.value
  if (!d) return []
  const out: TimelineEntry[] = []
  d.sessions.forEach((s, i) => {
    out.push({ key: `s-${i}`, at: s.startedAt, kind: 'session', title: 'بدء الدوام', color: '#006a64' })
    if (s.endedAt) out.push({ key: `se-${i}`, at: s.endedAt, kind: 'session', title: 'إنهاء الدوام', color: '#526967' })
  })
  d.pinList.forEach((p, i) => {
    const parts = [p.interestLabel]
    if (p.minutesSincePrevious != null) parts.push(`بعد ${num(p.minutesSincePrevious)} دقيقة من السابق`)
    out.push({
      key: `p-${p.id}`,
      at: p.at,
      kind: 'pin',
      title: `${i + 1}. ${p.name}`,
      sub: parts.join(' · '),
      color: INTEREST_COLORS[p.interest] ?? '#8a9694',
      quick: p.quick,
      mapKey: `pin-${p.id}`,
    })
  })
  d.orderList.forEach((o) => {
    if (!o.at) return
    out.push({ key: `o-${o.id}`, at: o.at, kind: 'order', title: `طلب: ${o.customerName}`, sub: `${o.product} · ${o.statusLabel}`, color: '#2f6fb0' })
  })
  gaps.value.forEach((g, i) => {
    out.push({ key: `g-${i}`, at: g.start, kind: 'gap', title: `فجوة ${num(g.minutes)} دقيقة بلا نشاط`, sub: `حتى ${time(g.end)}`, color: '#9a6210' })
  })
  d.alerts.forEach((a) => {
    out.push({
      key: `a-${a.id}`,
      at: a.since,
      kind: 'alert',
      title: `وقوف ${num(a.minutes)} دقيقة`,
      sub: a.resolvedAt ? `انتهى ${time(a.resolvedAt)}` : 'ما زال مفتوحاً',
      color: '#b23b3b',
      mapKey: `alert-${a.id}`,
    })
  })
  return out.sort((x, y) => Date.parse(x.at) - Date.parse(y.at))
})
</script>
