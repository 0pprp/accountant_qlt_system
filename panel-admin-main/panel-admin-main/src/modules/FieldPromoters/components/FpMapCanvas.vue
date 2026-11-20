<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import 'leaflet.markercluster'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'

export interface FpMarker {
  key: string
  lat: number
  lng: number
  color: string
  label?: string
  popupHtml: string
}

const props = withDefaults(defineProps<{
  markers: FpMarker[]
  route?: [number, number][]
  cluster?: boolean
  selectedKey?: string | null
  label?: string
}>(), { route: () => [], cluster: false, selectedKey: null, label: 'خريطة' })

const emit = defineEmits<{ select: [key: string] }>()

const mapEl = ref<HTMLElement | null>(null)
let map: L.Map | null = null
let layer: L.LayerGroup | null = null
let line: L.Polyline | null = null
const byKey = new Map<string, L.Marker>()
const IRAQ_CENTER: [number, number] = [32.0, 44.33]

function icon(m: FpMarker) {
  if (m.label) {
    return L.divIcon({
      className: 'fpp-num-pin',
      html: `<span style="background:${m.color}">${m.label}</span>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
    })
  }
  return L.divIcon({
    className: 'fw-map-pin',
    html: `<span style="display:block;width:16px;height:16px;border-radius:50%;background:${m.color};border:2px solid #fff;box-shadow:0 0 0 1px #00201e"></span>`,
    iconSize: [16, 16],
    iconAnchor: [8, 8],
  })
}

function rebuild() {
  if (!map || !layer) return
  layer.clearLayers()
  byKey.clear()
  line?.remove()
  line = null
  if (props.route.length > 1) {
    line = L.polyline(props.route, { color: '#006a64', weight: 3, opacity: 0.7 }).addTo(map)
  }
  const list: L.Marker[] = []
  for (const m of props.markers) {
    const marker = L.marker([m.lat, m.lng], { icon: icon(m) }).bindPopup(m.popupHtml)
    marker.on('click', () => emit('select', m.key))
    byKey.set(m.key, marker)
    list.push(marker)
  }
  if (layer instanceof L.MarkerClusterGroup) layer.addLayers(list)
  else list.forEach((x) => layer!.addLayer(x))
  fit()
}

function fit() {
  if (!map) return
  const pts: [number, number][] = [...props.markers.map((m) => [m.lat, m.lng] as [number, number]), ...props.route]
  if (pts.length === 0) return
  if (pts.length === 1) {
    map.setView(pts[0], 15)
    return
  }
  map.fitBounds(L.latLngBounds(pts), { padding: [40, 40], maxZoom: 16 })
}

defineExpose({ recenter: fit })

onMounted(() => {
  if (!mapEl.value) return
  map = L.map(mapEl.value, { center: IRAQ_CENTER, zoom: 7 })
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
  }).addTo(map)
  layer = props.cluster
    ? L.markerClusterGroup({ showCoverageOnHover: false, maxClusterRadius: 48, spiderfyOnMaxZoom: true, disableClusteringAtZoom: 18 })
    : L.layerGroup()
  map.addLayer(layer)
  rebuild()
  setTimeout(() => {
    map?.invalidateSize()
    fit()
  }, 300)
})

watch(() => [props.markers, props.route], rebuild, { deep: true })

watch(
  () => props.selectedKey,
  (key) => {
    if (!key || !map) return
    const m = byKey.get(key)
    if (!m) return
    if (layer instanceof L.MarkerClusterGroup) layer.zoomToShowLayer(m, () => m.openPopup())
    else {
      map.panTo(m.getLatLng())
      m.openPopup()
    }
  }
)

onBeforeUnmount(() => {
  layer?.clearLayers()
  map?.remove()
  map = null
  layer = null
  line = null
  byKey.clear()
})
</script>

<template>
  <div class="fpm-wrap">
    <div ref="mapEl" class="fpm" dir="ltr" role="application" :aria-label="label" />
    <button type="button" class="fpm-recenter" @click="fit">إعادة تمركز</button>
  </div>
</template>

<style scoped>
.fpm-wrap {
  background: #e8f0ef;
  border-radius: 14px;
  block-size: 100%;
  min-block-size: 300px;
  overflow: hidden;
  position: relative;
}
.fpm {
  block-size: 100%;
  inline-size: 100%;
  min-block-size: 300px;
  z-index: 0;
}
.fpm-recenter {
  background: #fff;
  border: 1px solid #006a64;
  border-radius: 8px;
  bottom: 12px;
  color: #006a64;
  cursor: pointer;
  font-family: inherit;
  font-size: 14px;
  inset-inline-start: 12px;
  min-height: 44px;
  padding: 0 1rem;
  position: absolute;
  z-index: 500;
}
.fpm-recenter:focus-visible {
  outline: 2px solid #00201e;
  outline-offset: 2px;
}
</style>
