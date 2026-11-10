<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

export interface MapPin {
  key: string
  lat: number
  lng: number
  color: string
  popupHtml: string
}

const props = defineProps<{
  pins: MapPin[]
  selectedKey?: string | null
  /** مركز افتراضي للخريطة فقط — ليس موقع شخص */
  fallbackCenter?: [number, number]
}>()

const emit = defineEmits<{
  select: [key: string]
}>()

const mapEl = ref<HTMLElement | null>(null)
let map: L.Map | null = null
const markers = new Map<string, L.Marker>()
let userAdjusted = false

const IRAQ_CENTER: [number, number] = [32.0, 44.33]

function markerIcon(color: string) {
  return L.divIcon({
    className: 'fw-map-pin',
    html: `<span style="display:block;width:16px;height:16px;border-radius:50%;background:${color};border:2px solid #fff;box-shadow:0 0 0 1px #00201e"></span>`,
    iconSize: [16, 16],
    iconAnchor: [8, 8],
  })
}

function syncMarkers() {
  if (!map) return
  const keys = new Set(props.pins.map((p) => p.key))
  for (const key of [...markers.keys()]) {
    if (keys.has(key)) continue
    markers.get(key)?.remove()
    markers.delete(key)
  }
  for (const pin of props.pins) {
    const existing = markers.get(pin.key)
    if (existing) {
      existing.setLatLng([pin.lat, pin.lng])
      existing.setIcon(markerIcon(pin.color))
      existing.setPopupContent(pin.popupHtml)
      continue
    }
    const m = L.marker([pin.lat, pin.lng], { icon: markerIcon(pin.color) })
      .bindPopup(pin.popupHtml)
      .addTo(map)
    m.on('click', () => emit('select', pin.key))
    markers.set(pin.key, m)
  }
}

function fitAll(force = false) {
  if (!map || props.pins.length === 0) return
  if (userAdjusted && !force) return
  if (props.pins.length === 1) {
    map.setView([props.pins[0].lat, props.pins[0].lng], 14)
    return
  }
  const b = L.latLngBounds(props.pins.map((p) => [p.lat, p.lng] as [number, number]))
  map.fitBounds(b, { padding: [40, 40], maxZoom: 15 })
}

function recenter() {
  userAdjusted = false
  fitAll(true)
}

defineExpose({ recenter })

onMounted(() => {
  if (!mapEl.value) return
  const center = props.fallbackCenter ?? IRAQ_CENTER
  map = L.map(mapEl.value, {
    center,
    zoom: 11,
    attributionControl: true,
  })
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
  }).addTo(map)
  map.on('dragstart', () => {
    userAdjusted = true
  })
  map.on('zoomstart', () => {
    userAdjusted = true
  })
  syncMarkers()
  fitAll(true)
})

watch(
  () => props.pins,
  () => {
    syncMarkers()
    fitAll(false)
  },
  { deep: true }
)

watch(
  () => props.selectedKey,
  (key) => {
    if (!key || !map) return
    const m = markers.get(key)
    if (!m) return
    m.openPopup()
  }
)

onBeforeUnmount(() => {
  for (const m of markers.values()) m.remove()
  markers.clear()
  map?.remove()
  map = null
})
</script>

<template>
  <div class="fw-map-wrap">
    <div ref="mapEl" class="fw-map" dir="ltr" role="application" aria-label="خريطة المتابعين" />
    <button type="button" class="fw-map-recenter" @click="recenter">إعادة تمركز</button>
  </div>
</template>

<style scoped>
.fw-map-wrap {
  position: relative;
  width: 100%;
  min-height: 280px;
  height: min(55vh, 520px);
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #d5e3e1;
  background: #f8f8f8;
}
.fw-map {
  width: 100%;
  height: 100%;
  z-index: 0;
}
.fw-map-recenter {
  position: absolute;
  z-index: 500;
  bottom: 12px;
  inset-inline-start: 12px;
  min-height: 44px;
  padding: 0 1rem;
  border: 1px solid #006a64;
  border-radius: 8px;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  font-size: 14px;
  cursor: pointer;
}
.fw-map-recenter:focus-visible {
  outline: 2px solid #00201e;
  outline-offset: 2px;
}
@media (max-width: 767px) {
  .fw-map-wrap {
    height: 42vh;
    min-height: 240px;
  }
}
</style>
