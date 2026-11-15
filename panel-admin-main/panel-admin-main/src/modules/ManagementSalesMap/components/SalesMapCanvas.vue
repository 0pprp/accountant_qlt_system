<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import 'leaflet.markercluster'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import {
  pointColor,
  statusGlyph,
  type SalesMapPoint,
} from '../domain/types'

const props = defineProps<{
  points: SalesMapPoint[]
  selectedOrderId?: number | null
}>()

const emit = defineEmits<{
  select: [orderId: number]
  viewChanged: [payload: { lat: number; lng: number; zoom: number }]
}>()

const mapEl = ref<HTMLElement | null>(null)
let map: L.Map | null = null
let cluster: L.MarkerClusterGroup | null = null
const IRAQ_CENTER: [number, number] = [32.0, 44.33]

function makeIcon(p: SalesMapPoint) {
  const color = pointColor(p.ratingTier)
  const glyph = statusGlyph(p.statusCode)
  const letter = p.listLetter || '؟'
  return L.divIcon({
    className: 'msm-pin',
    html: `<span class="msm-pin-badge" style="background:${color}">${letter}${glyph}</span>`,
    iconSize: [36, 28],
    iconAnchor: [18, 14],
  })
}

function rebuild() {
  if (!map || !cluster) return
  cluster.clearLayers()
  const markers: L.Marker[] = []
  for (const p of props.points) {
    const m = L.marker([p.latitude, p.longitude], { icon: makeIcon(p) })
    m.on('click', () => emit('select', p.orderId))
    markers.push(m)
  }
  cluster.addLayers(markers)
  if (props.points.length === 1) {
    map.setView([props.points[0].latitude, props.points[0].longitude], 14)
  } else if (props.points.length > 1) {
    const b = L.latLngBounds(
      props.points.map((p) => [p.latitude, p.longitude] as [number, number])
    )
    map.fitBounds(b, { padding: [48, 48], maxZoom: 16 })
  }
}

function emitView() {
  if (!map) return
  const c = map.getCenter()
  emit('viewChanged', { lat: c.lat, lng: c.lng, zoom: map.getZoom() })
}

onMounted(() => {
  if (!mapEl.value) return
  map = L.map(mapEl.value, { center: IRAQ_CENTER, zoom: 7 })
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap',
  }).addTo(map)
  cluster = L.markerClusterGroup({
    showCoverageOnHover: false,
    maxClusterRadius: 48,
    spiderfyOnMaxZoom: true,
    zoomToBoundsOnClick: true,
    disableClusteringAtZoom: 18,
  })
  map.addLayer(cluster)
  map.on('moveend', emitView)
  map.on('zoomend', emitView)
  rebuild()
})

watch(
  () => props.points,
  () => rebuild(),
  { deep: true }
)

onBeforeUnmount(() => {
  cluster?.clearLayers()
  map?.remove()
  map = null
  cluster = null
})
</script>

<template>
  <div ref="mapEl" class="msm-canvas" dir="ltr" />
</template>

<style scoped>
.msm-canvas {
  width: 100%;
  height: 100%;
  min-height: 480px;
  border-radius: 10px;
  overflow: hidden;
  background: #e8f0ef;
}
</style>

<style>
.msm-pin {
  background: transparent !important;
  border: none !important;
}
.msm-pin-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 34px;
  height: 26px;
  padding: 0 6px;
  border-radius: 8px;
  color: #fff;
  font-weight: 800;
  font-size: 13px;
  border: 2px solid #fff;
  box-shadow: 0 1px 4px rgba(0, 32, 30, 0.35);
  white-space: nowrap;
  direction: rtl;
}
</style>
