<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/modules/Auth/store'
import { useMediaQuery } from '@vueuse/core'
import SalesMapCanvas from '../components/SalesMapCanvas.vue'
import SalesMapDetailCard from '../components/SalesMapDetailCard.vue'
import { useSalesMapProvincesQuery } from '../requests/queries/useSalesMapProvincesQuery'
import { useSalesMapListsQuery } from '../requests/queries/useSalesMapListsQuery'
import { useSalesMapPointsQuery } from '../requests/queries/useSalesMapPointsQuery'
import { useSalesMapOrderDetailQuery } from '../requests/queries/useSalesMapOrderDetailQuery'
import {
  SalesMapStatusFilter,
  type SalesMapStatusFilterValue,
} from '../domain/types'

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const isMobile = useMediaQuery('(max-width: 768px)')

const canRead = computed(
  () => auth.hasPermission('ManagementSalesMap', 'Read') === true
)
const enabled = computed(() => canRead.value)

const provinceId = ref<number | null>(null)
const selectedListIds = ref<number[] | null>(null)
const status = ref<SalesMapStatusFilterValue>(SalesMapStatusFilter.All)
const selectedOrderId = ref<number | null>(null)
const listSearch = ref('')
const legendOpen = ref(false)
const hydrated = ref(false)

const provincesQuery = useSalesMapProvincesQuery(enabled)
const listsQuery = useSalesMapListsQuery(provinceId, enabled)
const pointsQuery = useSalesMapPointsQuery(
  provinceId,
  selectedListIds,
  status,
  enabled
)
const detailQuery = useSalesMapOrderDetailQuery(selectedOrderId)

const provinces = computed(() => provincesQuery.data.value ?? [])
const lists = computed(() => listsQuery.data.value ?? [])
const filteredLists = computed(() => {
  const q = listSearch.value.trim()
  if (!q) return lists.value
  return lists.value.filter((l) => l.listName.includes(q) || l.letter.includes(q))
})
const points = computed(() => pointsQuery.data.value ?? [])

const statusTabs = computed(() => [
  { value: SalesMapStatusFilter.All, label: t('managementSalesMap.map.statusAll') },
  {
    value: SalesMapStatusFilter.Continuous,
    label: t('managementSalesMap.map.statusContinuous'),
  },
  {
    value: SalesMapStatusFilter.Stopped,
    label: t('managementSalesMap.map.statusStopped'),
  },
  { value: SalesMapStatusFilter.Legal, label: t('managementSalesMap.map.statusLegal') },
])

const provinceName = computed(() => {
  const id = provinceId.value
  return provinces.value.find((p) => p.provinceId === id)?.provinceName ?? ''
})

const allListsSelected = computed(() => {
  if (selectedListIds.value === null) return true
  if (lists.value.length === 0) return false
  return (
    selectedListIds.value.length === lists.value.length &&
    lists.value.every((l) => selectedListIds.value!.includes(l.listId))
  )
})

watch(
  () => [route.query.provinceId, route.query.status, route.query.listIds, route.query.orderId],
  () => {
    if (!canRead.value) return
    const pid = Number(route.query.provinceId)
    provinceId.value = Number.isFinite(pid) && pid > 0 ? pid : null
    const st = Number(route.query.status)
    status.value =
      st === 1 || st === 2 || st === 3 ? (st as SalesMapStatusFilterValue) : SalesMapStatusFilter.All
    if (route.query.listIds === undefined || route.query.listIds === '') {
      selectedListIds.value = null
    } else if (route.query.listIds === 'none') {
      selectedListIds.value = []
    } else {
      const raw = Array.isArray(route.query.listIds)
        ? route.query.listIds.join(',')
        : String(route.query.listIds)
      selectedListIds.value = raw
        .split(',')
        .map((x) => Number(x))
        .filter((n) => Number.isFinite(n) && n > 0)
    }
    const oid = Number(route.query.orderId)
    selectedOrderId.value = Number.isFinite(oid) && oid > 0 ? oid : null
    hydrated.value = true
  },
  { immediate: true }
)

watch(
  lists,
  (rows) => {
    if (!hydrated.value || provinceId.value == null) return
    if (selectedListIds.value === null && rows.length > 0) {
      // كل القوائم افتراضيًا — لا نكتب listIds حتى يغيّر المستخدم
      return
    }
    if (selectedListIds.value && selectedListIds.value.length > 0) {
      const allowed = new Set(rows.map((r) => r.listId))
      selectedListIds.value = selectedListIds.value.filter((id) => allowed.has(id))
    }
  },
  { deep: true }
)

function syncQuery(patch: Record<string, string | undefined>) {
  const next = { ...route.query, ...patch }
  for (const k of Object.keys(next)) {
    if (next[k] === undefined || next[k] === '') delete next[k]
  }
  void router.replace({ query: next })
}

function openProvince(id: number) {
  selectedOrderId.value = null
  selectedListIds.value = null
  status.value = SalesMapStatusFilter.All
  syncQuery({
    provinceId: String(id),
    listIds: undefined,
    status: undefined,
    orderId: undefined,
  })
}

function backToProvinces() {
  selectedOrderId.value = null
  selectedListIds.value = null
  syncQuery({
    provinceId: undefined,
    listIds: undefined,
    status: undefined,
    orderId: undefined,
  })
}

function setStatus(v: SalesMapStatusFilterValue) {
  status.value = v
  syncQuery({
    status: v === SalesMapStatusFilter.All ? undefined : String(v),
  })
}

function toggleAllLists() {
  if (allListsSelected.value) {
    selectedListIds.value = []
    syncQuery({ listIds: 'none' })
    return
  }
  selectedListIds.value = null
  syncQuery({ listIds: undefined })
}

function toggleList(id: number) {
  const all = lists.value.map((l) => l.listId)
  let current =
    selectedListIds.value === null ? [...all] : [...selectedListIds.value]
  if (current.includes(id)) current = current.filter((x) => x !== id)
  else current.push(id)

  if (current.length === 0) {
    selectedListIds.value = []
    syncQuery({ listIds: 'none' })
    return
  }
  if (current.length === all.length) {
    selectedListIds.value = null
    syncQuery({ listIds: undefined })
    return
  }
  selectedListIds.value = current
  syncQuery({ listIds: current.join(',') })
}

function selectPoint(orderId: number) {
  selectedOrderId.value = orderId
  syncQuery({ orderId: String(orderId) })
}

function closeDetail() {
  selectedOrderId.value = null
  syncQuery({ orderId: undefined })
}

watch(
  () => auth.branchGeneration,
  () => {
    selectedOrderId.value = null
    provincesQuery.refetch()
  }
)
</script>

<template>
  <div class="msm-page" dir="rtl">
    <header class="msm-page__head">
      <h1>{{ t('managementSalesMap.title') }}</h1>
    </header>

    <div v-if="!canRead" class="msm-locked">
      {{ t('managementSalesMap.locked') }}
    </div>

    <template v-else-if="provinceId == null">
      <p class="msm-page__sub">{{ t('managementSalesMap.provinces.heading') }}</p>
      <div v-if="provincesQuery.isLoading.value" class="msm-state">…</div>
      <div v-else-if="provincesQuery.isError.value" class="msm-state msm-state--err">
        {{ t('managementSalesMap.provinces.loadError') }}
      </div>
      <div v-else-if="provinces.length === 0" class="msm-state">
        {{ t('managementSalesMap.provinces.empty') }}
      </div>
      <div v-else class="msm-cards">
        <button
          v-for="p in provinces"
          :key="p.provinceId"
          type="button"
          class="msm-card"
          @click="openProvince(p.provinceId)"
          @keydown.enter="openProvince(p.provinceId)"
        >
          <span class="msm-card__name">{{ p.provinceName }}</span>
          <span class="msm-card__meta">
            {{ t('managementSalesMap.provinces.lists') }}: {{ p.listCount }}
          </span>
          <span class="msm-card__meta">
            {{ t('managementSalesMap.provinces.withLocation') }}:
            {{ p.salesWithLocation }}
          </span>
          <span class="msm-card__meta">
            {{ t('managementSalesMap.provinces.withoutLocation') }}:
            {{ p.salesWithoutLocation }}
          </span>
        </button>
      </div>
    </template>

    <template v-else>
      <div class="msm-toolbar">
        <button type="button" class="msm-back" @click="backToProvinces">
          ← {{ t('managementSalesMap.map.back') }}
        </button>
        <h2>{{ provinceName }}</h2>
      </div>

      <div class="msm-filters">
        <div class="msm-lists">
          <div class="msm-lists__head">
            <strong>{{ t('managementSalesMap.map.lists') }}</strong>
            <button type="button" class="msm-chip" @click="toggleAllLists">
              {{ t('managementSalesMap.map.allLists') }}
              <span v-if="allListsSelected">✓</span>
            </button>
          </div>
          <input
            v-model="listSearch"
            class="msm-search"
            type="search"
            :placeholder="t('managementSalesMap.map.searchLists')"
          >
          <div class="msm-lists__opts">
            <button
              v-for="l in filteredLists"
              :key="l.listId"
              type="button"
              class="msm-chip"
              :class="{
                'msm-chip--on':
                  selectedListIds === null || selectedListIds.includes(l.listId),
              }"
              @click="toggleList(l.listId)"
            >
              <span class="msm-letter">{{ l.letter }}</span>
              {{ l.listName }}
            </button>
          </div>
          <p
            v-if="selectedListIds && selectedListIds.length === 0"
            class="msm-hint"
          >
            {{ t('managementSalesMap.map.noListsSelected') }}
          </p>
        </div>

        <div class="msm-status" role="tablist">
          <button
            v-for="tab in statusTabs"
            :key="tab.value"
            type="button"
            class="msm-status__btn"
            :class="{ 'msm-status__btn--on': status === tab.value }"
            @click="setStatus(tab.value)"
          >
            {{ tab.label }}
          </button>
        </div>
      </div>

      <div class="msm-stage">
        <div class="msm-map-wrap">
          <button
            type="button"
            class="msm-legend-toggle"
            @click="legendOpen = !legendOpen"
          >
            {{ t('managementSalesMap.map.legend') }}
          </button>
          <div v-if="legendOpen" class="msm-legend">
            <p>{{ t('managementSalesMap.map.legendLetter') }}</p>
            <p>{{ t('managementSalesMap.map.legendStatus') }}</p>
            <p>{{ t('managementSalesMap.map.legendColor') }}</p>
          </div>

          <div v-if="pointsQuery.isLoading.value" class="msm-map-overlay">
            {{ t('managementSalesMap.map.loading') }}
          </div>
          <div v-else-if="pointsQuery.isError.value" class="msm-map-overlay msm-map-overlay--err">
            {{ t('managementSalesMap.map.loadError') }}
            <button type="button" @click="pointsQuery.refetch()">
              {{ t('managementSalesMap.map.retry') }}
            </button>
          </div>
          <div
            v-else-if="points.length === 0 && selectedListIds && selectedListIds.length === 0"
            class="msm-map-overlay"
          >
            {{ t('managementSalesMap.map.noListsSelected') }}
          </div>
          <div v-else-if="points.length === 0" class="msm-map-overlay">
            {{ t('managementSalesMap.map.emptyPoints') }}
          </div>

          <SalesMapCanvas
            :points="points"
            :selected-order-id="selectedOrderId"
            @select="selectPoint"
          />
        </div>

        <SalesMapDetailCard
          v-if="selectedOrderId != null && !isMobile"
          class="msm-side"
          :detail="detailQuery.data.value ?? null"
          :loading="detailQuery.isLoading.value"
          :error="detailQuery.isError.value"
          @close="closeDetail"
          @retry="detailQuery.refetch()"
        />
      </div>

      <div
        v-if="selectedOrderId != null && isMobile"
        class="msm-sheet"
      >
        <SalesMapDetailCard
          mobile
          :detail="detailQuery.data.value ?? null"
          :loading="detailQuery.isLoading.value"
          :error="detailQuery.isError.value"
          @close="closeDetail"
          @retry="detailQuery.refetch()"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.msm-page {
  /* امتداد نحو اليسار (inline-end في RTL) مع بادينغ خفيف */
  margin-block: -4px;
  margin-inline-end: -1rem;
  padding: 8px 8px 12px 4px;
  color: #00201e;
  min-width: 0;
}
@media (min-width: 768px) {
  .msm-page {
    margin-inline-end: -1.5rem;
    padding: 6px 10px 10px 4px;
  }
}
.msm-page__head h1 {
  margin: 0 0 6px;
  font-size: 1.35rem;
  color: #006a64;
}
.msm-page__sub {
  color: #6f797a;
}
.msm-locked {
  padding: 48px 16px;
  text-align: center;
  background: #f4f7f7;
  border-radius: 16px;
  color: #6f797a;
}
.msm-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 12px;
}
.msm-card {
  text-align: right;
  border: 1px solid #d5e2e0;
  background: #fff;
  border-radius: 16px;
  padding: 16px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.msm-card:focus-visible {
  outline: 2px solid #006a64;
  outline-offset: 2px;
}
.msm-card__name {
  font-weight: 800;
  font-size: 1.1rem;
}
.msm-card__meta {
  font-size: 0.85rem;
  color: #6f797a;
}
.msm-toolbar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
}
.msm-toolbar h2 {
  margin: 0;
  font-size: 1.2rem;
}
.msm-back {
  border: 0;
  background: #e7f1f0;
  color: #006a64;
  border-radius: 999px;
  padding: 8px 14px;
  font-weight: 700;
  cursor: pointer;
}
.msm-filters {
  display: grid;
  gap: 8px;
  margin-bottom: 8px;
}
.msm-lists__head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 8px;
}
.msm-search {
  width: 100%;
  border: 1px solid #d5e2e0;
  border-radius: 12px;
  padding: 8px 12px;
  margin-bottom: 8px;
}
.msm-lists__opts {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.msm-chip {
  border: 1px solid #d5e2e0;
  background: #fff;
  border-radius: 999px;
  padding: 6px 12px;
  cursor: pointer;
  font-weight: 600;
}
.msm-chip--on {
  background: #006a64;
  color: #fff;
  border-color: #006a64;
}
.msm-letter {
  display: inline-block;
  min-width: 1.2em;
  margin-left: 4px;
  font-weight: 800;
}
.msm-hint {
  color: #b45309;
  font-size: 0.875rem;
}
.msm-status {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.msm-status__btn {
  border: 1px solid #d5e2e0;
  background: #fff;
  border-radius: 12px;
  padding: 8px 14px;
  cursor: pointer;
  font-weight: 700;
}
.msm-status__btn--on {
  background: #004d48;
  color: #fff;
  border-color: #004d48;
}
.msm-stage {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
  min-height: 520px;
  width: 100%;
}
@media (min-width: 1024px) {
  .msm-stage {
    grid-template-columns: minmax(0, 1fr) 280px;
  }
}
.msm-map-wrap {
  position: relative;
  min-height: 520px;
  height: min(78vh, 860px);
  width: 100%;
}
.msm-map-overlay {
  position: absolute;
  z-index: 500;
  inset: auto 12px 12px 12px;
  background: rgba(255, 255, 255, 0.92);
  border-radius: 12px;
  padding: 10px 14px;
  text-align: center;
  color: #004d48;
  font-weight: 700;
}
.msm-map-overlay--err button {
  margin-inline-start: 8px;
  border: 0;
  background: #006a64;
  color: #fff;
  border-radius: 8px;
  padding: 4px 10px;
  cursor: pointer;
}
.msm-legend-toggle {
  position: absolute;
  z-index: 500;
  top: 12px;
  inset-inline-start: 12px;
  border: 0;
  background: #fff;
  border-radius: 10px;
  padding: 6px 10px;
  font-weight: 700;
  color: #006a64;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
  cursor: pointer;
}
.msm-legend {
  position: absolute;
  z-index: 500;
  top: 48px;
  inset-inline-start: 12px;
  background: #fff;
  border-radius: 12px;
  padding: 10px 12px;
  max-width: 280px;
  font-size: 0.85rem;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.12);
}
.msm-legend p {
  margin: 0 0 6px;
}
.msm-sheet {
  position: fixed;
  z-index: 1200;
  left: 0;
  right: 0;
  bottom: 0;
  max-height: 55vh;
  background: transparent;
}
.msm-state {
  padding: 40px;
  text-align: center;
  color: #6f797a;
}
.msm-state--err {
  color: #b91c1c;
}
</style>
