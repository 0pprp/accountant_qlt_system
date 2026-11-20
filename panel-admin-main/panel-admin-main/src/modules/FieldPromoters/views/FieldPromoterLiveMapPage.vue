<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">الخريطة الحية</h2>
        <p class="dfb-subtitle">آخر موقع لكل مروّج اليوم · تتحدث كل 20 ثانية</p>
      </div>
      <div class="fpp-legend" aria-label="دليل الألوان">
        <span v-for="s in LEGEND" :key="s"><i class="fpp-dot" :style="{ background: STATUS_META[s].color }" />{{ STATUS_META[s].label }}</span>
      </div>
    </header>

    <p v-if="live.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="live.isError.value" class="dfb-error" role="alert">
      تعذر تحميل المواقع.
      <button type="button" class="dfb-button is-small" @click="live.refetch()">إعادة المحاولة</button>
    </div>
    <div v-else class="fpp-split">
      <div class="fpp-card">
        <p v-if="!pins.length" class="dfb-state">لا توجد مواقع مسجلة اليوم.</p>
        <FollowerMapCanvas :pins="pins" :selectedKey="selectedKey" @select="selectedKey = $event" />
      </div>
      <aside class="fpp-card" aria-label="قائمة المروّجين">
        <h3 class="fpp-card-title">المروّجون ({{ num(rows.length) }})</h3>
        <p v-if="!rows.length" class="dfb-state">لا يوجد مروّجون في هذا النطاق.</p>
        <ul v-else class="fpp-side-list">
          <li v-for="p in rows" :key="p.id">
            <button
              type="button"
              class="fpp-side-item"
              :class="{ 'is-active': selectedKey === String(p.id) }"
              @click="select(p)"
            >
              <i class="fpp-dot" :style="{ background: STATUS_META[p.status]?.color ?? '#8a9694' }" aria-hidden="true" />
              <span class="fpp-side-main">
                <strong>{{ p.fullName }}</strong>
                <small>{{ STATUS_META[p.status]?.label ?? p.statusLabel }} · {{ p.lat != null ? relative(p.lastAt) : 'لا موقع اليوم' }}</small>
              </span>
            </button>
          </li>
        </ul>
        <button v-if="selected" type="button" class="dfb-button is-small" @click="openDay(selected.id)">عرض يوم {{ selected.fullName }}</button>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { usePromotersLiveQuery, type PromoterLiveRow, type PromoterStatus } from '../requests/fieldPromoters'
import { STATUS_META, escapeHtml, hm, num, relative, time } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'
import FollowerMapCanvas, { type MapPin } from '@/modules/FollowerWork/components/tracking/FollowerMapCanvas.vue'

const LEGEND: PromoterStatus[] = ['working', 'stationary', 'ended', 'notStarted', 'suspended']
const { branchId, openDay } = useFpNav()
const live = usePromotersLiveQuery(branchId, true)
const rows = computed(() => live.data.value?.promoters ?? [])
const selectedKey = ref<string | null>(null)
const selected = computed(() => rows.value.find((p) => String(p.id) === selectedKey.value) ?? null)

const pins = computed<MapPin[]>(() =>
  rows.value
    .filter((p) => p.lat != null && p.lng != null)
    .map((p) => {
      const meta = STATUS_META[p.status]
      return {
        key: String(p.id),
        lat: p.lat as number,
        lng: p.lng as number,
        color: meta?.color ?? '#8a9694',
        popupHtml: `<div dir="rtl" style="font-family:inherit;min-width:160px"><strong>${escapeHtml(p.fullName)}</strong><br>${escapeHtml(meta?.label ?? p.statusLabel)} · ${escapeHtml(time(p.lastAt))}<br>ساعات العمل ${hm(p.workMinutes)} · محلات ${num(p.pins)} · طلبات ${num(p.orders)}</div>`,
      }
    })
)

function select(p: PromoterLiveRow) {
  selectedKey.value = String(p.id)
}
</script>
