<template>
  <section class="dfb-section" aria-labelledby="dfb-map-title">
    <div class="dfb-section-head">
      <h2 id="dfb-map-title" class="dfb-section-title">خريطة محلات الزبائن</h2>
      <div class="dfb-actions">
        <div class="dfb-chips" role="group" aria-label="مصدر الموقع">
          <button v-for="option in modes" :key="option.key" type="button" class="dfb-button is-small" :aria-pressed="mode === option.key" @click="setMode(option.key)">
            {{ option.label }}
          </button>
        </div>
        <label class="dfb-map-active">
          <input v-model="activeOnly" type="checkbox">
          النشطين فقط (تسديد خلال 30 يوماً)
        </label>
      </div>
    </div>
    <p class="dfb-hint">
      <span class="dfb-pin is-solid" aria-hidden="true" /> موقع المتابع
      <span class="dfb-pin is-hollow" aria-hidden="true" /> موقع تطبيق المبيعات
      — اضغط اسم الزبون في الدبوس لفتح بروفايله
    </p>

    <div class="dfb-map-host">
      <div ref="mapHost" class="dfb-map-canvas" aria-label="مواقع الزبائن على الخريطة" />
      <p v-if="!lists.length" class="dfb-map-overlay">حدّد قائمة أو أكثر من المربعات أعلاه لعرض زبائنها على الخريطة</p>
      <p v-else-if="allLoaded && totalPins === 0" class="dfb-map-overlay">لا توجد مواقع لعرضها</p>
    </div>

    <div v-if="lists.length" class="dfb-grid">
      <article v-for="(list, index) in lists" :key="list.listId" class="dfb-tile" :style="{ borderInlineStartColor: colorOf(list.listId), borderInlineStartWidth: '6px' }">
        <span class="dfb-tile-label"><span class="dfb-swatch" :style="{ background: colorOf(list.listId) }" />{{ list.listName }}</span>
        <p v-if="queries[index]?.isPending" class="dfb-tile-note" role="status">جارٍ تحميل المواقع…</p>
        <div v-else-if="queries[index]?.isError" class="dfb-error" role="alert">
          تعذر تحميل مواقع القائمة
          <button type="button" class="dfb-button is-small" @click="queries[index]?.refetch()">إعادة المحاولة</button>
        </div>
        <template v-else-if="queries[index]?.data">
          <span class="dfb-tile-row">الزبائن <strong>{{ counts(index).total }}</strong></span>
          <span class="dfb-tile-row">على الخريطة <strong>{{ counts(index).shown }}</strong></span>
          <span class="dfb-tile-row">بدون موقع ({{ modeLabel }}) <strong>{{ counts(index).missing }}</strong></span>
          <button type="button" class="dfb-button is-small" @click="dailyIndex = index">
            معدل الاستحصال اليومي (30 يوماً): {{ money(queries[index].data!.avgDailyCollected30d) }}
          </button>
        </template>
      </article>
    </div>

    <DelegateHistoryDialog
      :visible="dailyIndex !== null"
      :title="`الاستحصال اليومي — ${dailyList?.listName ?? ''}`"
      subtitle="آخر 30 يوماً حتى اليوم"
      :summary="dailySummary"
      :items="dailyItems"
      @close="dailyIndex = null"
    />
  </section>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useQueries } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import DelegateHistoryDialog, { type HistoryItem } from '@/modules/Core/components/neumorphic/NeuHistoryDialog.vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { DelegateFollowupEndpoints } from '../domain/endpoints'

interface CustomerPoint {
  customerId: number
  customerName: string
  shopName: string | null
  phoneNumber: string | null
  address: string | null
  amountRemaining: number
  lastPaymentDate: string | null
  isActive: boolean
  ratingLabel: string
  followerLatitude: number | null
  followerLongitude: number | null
  salesLatitude: number | null
  salesLongitude: number | null
}
interface LocationDay { date: string; amount: number; count: number }
interface Locations { customers: CustomerPoint[]; avgDailyCollected30d: number; daily30d: LocationDay[] }
type Mode = 'sales' | 'follower' | 'both'
type Point = { lat: number; lng: number; kind: 'sales' | 'follower' }

const props = defineProps<{
  branchId: number
  lists: { listId: number; listName: string }[]
  colors: Record<number, string>
}>()

const MODE_STORAGE_KEY = 'dlsm-source-mode'
const BAGHDAD: L.LatLngTuple = [33.31, 44.36]
const router = useRouter()
const modes: { key: Mode; label: string }[] = [
  { key: 'sales', label: 'المبيعات' },
  { key: 'follower', label: 'المتابع' },
  { key: 'both', label: 'الاثنين' },
]

function readMode(): Mode {
  try {
    const v = localStorage.getItem(MODE_STORAGE_KEY)
    return v === 'sales' || v === 'follower' || v === 'both' ? v : 'both'
  } catch {
    return 'both'
  }
}

const mode = ref<Mode>(readMode())
const activeOnly = ref(false)
const modeLabel = computed(() => modes.find(m => m.key === mode.value)?.label ?? '')

function setMode(next: Mode) {
  mode.value = next
  try {
    localStorage.setItem(MODE_STORAGE_KEY, next)
  } catch {
    return
  }
}

const colorOf = (id: number) => props.colors[id] ?? '#006a64'

const queries = useQueries({
  queries: computed(() => props.lists.map(list => ({
    queryKey: ['delegate-followup-locations', props.branchId, list.listId],
    queryFn: ({ signal }: { signal: AbortSignal }) => axiosInstance
      .get<Locations>(DelegateFollowupEndpoints.locations(list.listId), { params: { branchId: props.branchId }, signal })
      .then(r => r.data),
    staleTime: 5 * 60_000,
  }))),
})

const money = (n: number) => new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)

function validPoint(lat: number | null, lng: number | null) {
  return lat != null && lng != null && Number.isFinite(lat) && Number.isFinite(lng)
    && Math.abs(lat) <= 90 && Math.abs(lng) <= 180 && (lat !== 0 || lng !== 0)
}

function pointsOf(c: CustomerPoint): Point[] {
  const result: Point[] = []
  if (mode.value !== 'follower' && validPoint(c.salesLatitude, c.salesLongitude))
    result.push({ lat: c.salesLatitude!, lng: c.salesLongitude!, kind: 'sales' })
  if (mode.value !== 'sales' && validPoint(c.followerLatitude, c.followerLongitude))
    result.push({ lat: c.followerLatitude!, lng: c.followerLongitude!, kind: 'follower' })
  return result
}

function rows(index: number) {
  return (queries.value[index]?.data?.customers ?? []).filter(c => !activeOnly.value || c.isActive)
}

function counts(index: number) {
  const data = rows(index)
  const shown = data.filter(c => pointsOf(c).length > 0).length
  return { total: data.length, shown, missing: data.length - shown }
}

const allLoaded = computed(() => props.lists.length > 0 && props.lists.every((_, i) => queries.value[i]?.data))
const totalPins = computed(() => props.lists.reduce((sum, _, i) => sum + rows(i).reduce((s, c) => s + pointsOf(c).length, 0), 0))

function distanceMeters(a: { lat: number; lng: number }, b: { lat: number; lng: number }) {
  const rad = Math.PI / 180
  const x = (b.lng - a.lng) * rad * Math.cos(((a.lat + b.lat) / 2) * rad)
  const y = (b.lat - a.lat) * rad
  return Math.round(Math.sqrt(x * x + y * y) * 6_371_000)
}

function line(label: string, value: string | null | undefined) {
  const p = document.createElement('div')
  const strong = document.createElement('strong')
  strong.textContent = `${label}: `
  p.append(strong, document.createTextNode(value && value !== '' ? value : '—'))
  return p
}

function popupFor(c: CustomerPoint, point: Point, list: { listId: number; listName: string }) {
  const root = document.createElement('div')
  root.dir = 'rtl'
  root.className = 'dfb-popup'
  const href = router.resolve({
    name: 'CustomerAuditProfileRoute',
    params: { id: c.customerId },
  }).href
  const name = document.createElement('a')
  name.href = href
  name.target = '_blank'
  name.rel = 'noopener noreferrer'
  name.textContent = c.customerName
  name.className = 'dfb-link'
  root.append(name)
  root.append(line('المحل', c.shopName))
  const listLine = line('القائمة', list.listName)
  const swatch = document.createElement('span')
  swatch.className = 'dfb-swatch'
  swatch.style.background = colorOf(list.listId)
  listLine.prepend(swatch)
  root.append(listLine)
  root.append(line('الهاتف', c.phoneNumber))
  root.append(line('العنوان', c.address))
  root.append(line('الباقي', money(c.amountRemaining)))
  root.append(line('آخر تسديد', c.lastPaymentDate))
  root.append(line('التقييم', c.ratingLabel))
  root.append(line('هذا الدبوس', point.kind === 'sales' ? 'موقع تطبيق المبيعات' : 'موقع المتابع'))
  const other = point.kind === 'sales'
    ? (validPoint(c.followerLatitude, c.followerLongitude) ? { lat: c.followerLatitude!, lng: c.followerLongitude! } : null)
    : (validPoint(c.salesLatitude, c.salesLongitude) ? { lat: c.salesLatitude!, lng: c.salesLongitude! } : null)
  if (other)
    root.append(line(point.kind === 'sales' ? 'المسافة عن موقع المتابع' : 'المسافة عن موقع المبيعات', `${distanceMeters(point, other)} م`))
  const gmaps = document.createElement('a')
  gmaps.href = `https://www.google.com/maps?q=${point.lat},${point.lng}`
  gmaps.target = '_blank'
  gmaps.rel = 'noopener noreferrer'
  gmaps.textContent = 'فتح في خرائط Google'
  gmaps.className = 'dfb-link'
  root.append(gmaps)
  return root
}

const mapHost = ref<HTMLElement>()
let map: L.Map | undefined
let layers: L.LayerGroup | undefined
let fittedKey = ''

async function renderMap() {
  await nextTick()
  if (!mapHost.value) return
  if (!map) {
    map = L.map(mapHost.value, { preferCanvas: true }).setView(BAGHDAD, 10)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap contributors', maxZoom: 19 }).addTo(map)
    layers = L.layerGroup().addTo(map)
  }
  layers!.clearLayers()
  const bounds: L.LatLngTuple[] = []
  const drawn: { list: { listId: number; listName: string }; c: CustomerPoint; point: Point }[] = []
  props.lists.forEach((list, index) => rows(index).forEach(c => pointsOf(c).forEach(point => drawn.push({ list, c, point }))))
  drawn.sort((a, b) => (a.point.kind === b.point.kind ? 0 : a.point.kind === 'sales' ? -1 : 1))
  for (const { list, c, point } of drawn) {
    bounds.push([point.lat, point.lng])
    const color = colorOf(list.listId)
    L.circleMarker([point.lat, point.lng], point.kind === 'sales'
      ? { radius: 7, color, weight: 3, fillColor: '#ffffff', fillOpacity: 1 }
      : { radius: 8, color: '#ffffff', weight: 2, fillColor: color, fillOpacity: 1 })
      .bindPopup(popupFor(c, point, list))
      .addTo(layers!)
  }
  const key = props.lists.filter((_, i) => queries.value[i]?.data).map(l => l.listId).join(',')
  if (bounds.length && key !== fittedKey) {
    map.fitBounds(bounds, { padding: [50, 50], maxZoom: 15 })
    fittedKey = key
  }
  if (!props.lists.length) fittedKey = ''
  map.invalidateSize()
}

const dailyIndex = ref<number | null>(null)
const dailyList = computed(() => (dailyIndex.value === null ? null : props.lists[dailyIndex.value] ?? null))
const dailyColor = computed(() => (dailyList.value ? colorOf(dailyList.value.listId) : '#006a64'))
const WEEKDAYS = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
const dailyRows = computed(() => {
  const days = dailyIndex.value === null ? [] : queries.value[dailyIndex.value]?.data?.daily30d ?? []
  const max = Math.max(1, ...days.map(d => d.amount))
  return [...days].reverse().map(d => {
    const [y, m, day] = d.date.split('-').map(Number)
    const weekday = WEEKDAYS[new Date(Date.UTC(y, m - 1, day)).getUTCDay()]
    return { ...d, weekday, dayMonth: `${String(day).padStart(2, '0')}/${String(m).padStart(2, '0')}`, percent: Math.round((d.amount / max) * 100) }
  })
})
const dailyDaysWithCollection = computed(() => dailyRows.value.filter(d => d.amount > 0).length)
const dailyItems = computed<HistoryItem[]>(() => dailyRows.value.map(d => ({
  key: d.date,
  date: d.date,
  value: d.amount ? money(d.amount) : '—',
  amount: d.amount,
  empty: !d.amount,
  note: d.count ? `${d.count} دفعة` : undefined,
})))
const dailySummary = computed(() => {
  const data = dailyIndex.value === null ? null : queries.value[dailyIndex.value]?.data
  return [
    { label: 'المعدل اليومي', value: money(data?.avgDailyCollected30d ?? 0) },
    { label: 'المجموع', value: money(dailyRows.value.reduce((s, d) => s + d.amount, 0)) },
    { label: 'أيام فيها استحصال', value: `${dailyDaysWithCollection.value} / 30` },
  ]
})

watch([() => props.lists, () => props.colors, () => props.branchId, queries, mode, activeOnly], renderMap, { deep: true })
onMounted(renderMap)
onBeforeUnmount(() => map?.remove())
</script>

<style scoped>
.dfb-map-host { position: relative; }
.dfb-map-canvas { block-size: clamp(320px, 52vh, 560px); border-radius: 20px; box-shadow: var(--dfb-well-shadow); z-index: 0; }
.dfb-map-overlay { background: rgba(246, 249, 248, 0.92); border-radius: 14px; box-shadow: var(--dfb-up); color: var(--dfb-muted); inset-block-start: 50%; inset-inline-start: 50%; margin: 0; max-inline-size: 80%; padding: 12px 18px; pointer-events: none; position: absolute; text-align: center; transform: translate(50%, -50%); z-index: 500; }
.dfb-map-active { align-items: center; color: var(--dfb-muted); cursor: pointer; display: inline-flex; font-size: 14px; gap: 8px; min-height: 44px; }
.dfb-map-active input { accent-color: var(--dfb-primary); block-size: 18px; inline-size: 18px; }
.dfb-pin { block-size: 12px; border-radius: 50%; display: inline-block; inline-size: 12px; margin-inline: 6px 4px; vertical-align: middle; }
.dfb-pin.is-solid { background: var(--dfb-primary); }
.dfb-pin.is-hollow { border: 3px solid var(--dfb-primary); }
.dfb-bar { block-size: 10px; border-radius: 6px; display: block; min-inline-size: 2px; }
:global(.dfb-swatch) { block-size: 12px; border-radius: 4px; display: inline-block; inline-size: 12px; margin-inline-end: 6px; vertical-align: middle; }
:global(.dfb-popup) { display: grid; font-family: var(--q-font, 'ElMessiri', sans-serif); font-size: 13px; gap: 4px; min-inline-size: 220px; }
</style>
