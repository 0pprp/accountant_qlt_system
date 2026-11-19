<template>
  <div class="fwb-stack">
    <div class="dfb-dialog-toolbar">
      <p class="dfb-hint">زبائن غير مصفّرين وغير قانونيين — نسبة التقاط موقع المحل وصورته لكل قائمة</p>
      <label v-if="followerId != null" class="fwb-toggle">
        <input v-model="wholeBranch" type="checkbox">
        كل قوائم الفرع
      </label>
    </div>

    <p v-if="query.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="query.isError.value" class="dfb-error" role="alert">
      تعذر تحميل مواقع وصور المحلات
      <button type="button" class="dfb-button is-small" @click="query.refetch()">إعادة المحاولة</button>
    </div>

    <template v-else-if="data">
      <div class="dfb-kpis">
        <div class="dfb-kpi"><span>الزبائن</span><strong><span class="dfb-num">{{ data.totalCustomers }}</span></strong></div>
        <div class="dfb-kpi"><span>بموقع</span><strong><span class="dfb-num">{{ pct(data.withLocation, data.totalCustomers) }}</span></strong></div>
        <div class="dfb-kpi"><span>بصورة</span><strong><span class="dfb-num">{{ pct(data.withPhoto, data.totalCustomers) }}</span></strong></div>
        <div class="dfb-kpi"><span>مكتمل</span><strong><span class="dfb-num">{{ pct(data.complete, data.totalCustomers) }}</span></strong></div>
      </div>

      <p v-if="!data.lists.length" class="dfb-state">لا يوجد زبائن مفتوحو الرصيد على هذه القوائم</p>
      <div v-else class="dfb-grid">
        <button
          v-for="l in data.lists"
          :key="l.listId"
          type="button"
          class="dfb-tile"
          :class="{ 'is-active': listId === l.listId }"
          :aria-pressed="listId === l.listId"
          @click="listId = listId === l.listId ? null : l.listId"
        >
          <span class="dfb-tile-label">{{ l.listName }}<template v-if="wholeBranch && l.followerName"> · {{ l.followerName }}</template></span>
          <span class="dfb-tile-row">الموقع <strong>{{ l.withLocation }} / {{ l.totalCustomers }}</strong></span>
          <div class="dfb-progress"><span :style="{ inlineSize: `${ratio(l.withLocation, l.totalCustomers)}%` }" /></div>
          <span class="dfb-tile-row">الصورة <strong>{{ l.withPhoto }} / {{ l.totalCustomers }}</strong></span>
          <div class="dfb-progress"><span :style="{ inlineSize: `${ratio(l.withPhoto, l.totalCustomers)}%` }" /></div>
        </button>
      </div>

      <template v-if="listId != null">
        <div class="dfb-dialog-toolbar">
          <div class="dfb-filter-chips" role="group" aria-label="تصفية">
            <button v-for="f in FILTERS" :key="f.key" type="button" class="dfb-chip" :aria-pressed="filter === f.key" @click="filter = f.key">
              {{ f.label }} <strong>{{ countBy(f.key) }}</strong>
            </button>
          </div>
          <input v-model="search" class="dfb-search" type="search" placeholder="بحث بالاسم أو المحل أو الهاتف" aria-label="بحث">
        </div>
        <div class="dfb-panel has-cards">
          <table class="dfb-ctable">
            <thead>
              <tr>
                <th scope="col">الزبون</th>
                <th scope="col">الباقي</th>
                <th scope="col">الموقع</th>
                <th scope="col">الصورة</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in rows" :key="c.customerId">
                <td class="is-wide">
                  <button type="button" class="dfb-name-link" @click="emit('open-customer', c.customerId)">{{ c.customerName }}</button>
                  <span class="dfb-cell-sub">{{ [c.shopName, c.phoneNumber].filter(Boolean).join(' · ') || '—' }}</span>
                </td>
                <td class="is-num"><span class="dfb-cell-label">الباقي</span>{{ money(c.amountRemaining) }}</td>
                <td>
                  <span class="dfb-cell-label">الموقع</span>
                  <a v-if="c.hasLocation" class="dfb-link" :href="`https://www.google.com/maps?q=${c.latitude},${c.longitude}`" target="_blank" rel="noopener noreferrer">خرائط Google</a>
                  <span v-else class="dfb-status is-open">بدون موقع</span>
                </td>
                <td>
                  <span class="dfb-cell-label">الصورة</span>
                  <button v-if="c.photoId" type="button" class="dfb-button is-small" @click="openPhoto(c)">عرض الصورة</button>
                  <span v-else class="dfb-status is-open">بدون صورة</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-if="!rows.length" class="dfb-state">لا يوجد زبائن مطابقون</p>
      </template>
    </template>

    <NeuDialog :visible="photoUrl != null" size="md" :title="photoTitle" @close="closePhoto">
      <img v-if="photoUrl" :src="photoUrl" :alt="`صورة محل ${photoTitle}`" class="fwb-photo">
    </NeuDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, toRef, watch } from 'vue'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import type { FollowerShopCaptureCustomer } from '../../domain/boardTypes'
import { useFollowerShopCaptureQuery } from '../../requests/queries/useFollowerBoardQueries'

const props = defineProps<{ branchId: number | null; followerId: number | null }>()
const emit = defineEmits<{ 'open-customer': [customerId: number] }>()

type Filter = 'all' | 'noLocation' | 'noPhoto' | 'complete'
const FILTERS: { key: Filter; label: string }[] = [
  { key: 'all', label: 'الكل' },
  { key: 'noLocation', label: 'بدون موقع' },
  { key: 'noPhoto', label: 'بدون صورة' },
  { key: 'complete', label: 'مكتمل' },
]

const wholeBranch = ref(false)
const scopedFollower = computed(() => (wholeBranch.value ? null : props.followerId))
const query = useFollowerShopCaptureQuery(toRef(props, 'branchId'), scopedFollower, computed(() => true))
const data = computed(() => query.data.value ?? null)
const listId = ref<number | null>(null)
const filter = ref<Filter>('all')
const search = ref('')
watch([() => props.followerId, () => props.branchId, wholeBranch], () => { listId.value = null })

const listRows = computed(() => (data.value?.customers ?? []).filter(c => c.listId === listId.value))
function matches(c: FollowerShopCaptureCustomer, f: Filter) {
  if (f === 'noLocation') return !c.hasLocation
  if (f === 'noPhoto') return !c.hasPhoto
  if (f === 'complete') return c.hasLocation && c.hasPhoto
  return true
}
const countBy = (f: Filter) => listRows.value.filter(c => matches(c, f)).length
const rows = computed(() => {
  const q = search.value.trim()
  return listRows.value.filter(c => matches(c, filter.value)).filter(c => !q || [c.customerName, c.shopName, c.phoneNumber].some(v => (v ?? '').includes(q)))
})

const ratio = (a: number, b: number) => (b > 0 ? Math.round((a / b) * 100) : 0)
const pct = (a: number, b: number) => `${a} (${ratio(a, b)}%)`
const money = (n: number) => new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)

const photoUrl = ref<string | null>(null)
const photoTitle = ref('')
async function openPhoto(c: FollowerShopCaptureCustomer) {
  if (c.photoId == null || props.branchId == null) return
  const { data: blob } = await axiosInstance.get<Blob>(FollowerWorkEndpoints.shopPhoto(c.photoId), {
    params: { branchId: props.branchId, source: c.photoSource ?? 'imported' },
    responseType: 'blob',
  })
  closePhoto()
  photoUrl.value = URL.createObjectURL(blob)
  photoTitle.value = c.shopName || c.customerName
}
function closePhoto() {
  if (photoUrl.value) URL.revokeObjectURL(photoUrl.value)
  photoUrl.value = null
}
onBeforeUnmount(closePhoto)
</script>
