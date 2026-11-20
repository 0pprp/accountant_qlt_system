<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">المحلات</h2>
        <p class="dfb-subtitle">المحلات التي سجّلها المروّجون مع درجة الاهتمام</p>
      </div>
    </header>

    <section class="fpp-filters" aria-label="تصفية المحلات">
      <label class="fpp-field">
        <span>المروّج</span>
        <select v-model="userId" class="dfb-search">
          <option :value="null">كل المروّجين</option>
          <option v-for="p in promoters" :key="p.id" :value="p.id">{{ p.fullName }}</option>
        </select>
      </label>
      <label class="fpp-field">
        <span>أدنى اهتمام</span>
        <select v-model="minInterest" class="dfb-search">
          <option :value="null">الكل</option>
          <option v-for="i in [2, 3, 4, 5]" :key="i" :value="i">{{ INTEREST_LABELS[i] }} فأكثر</option>
        </select>
      </label>
      <label class="fpp-field">
        <span>من</span>
        <input v-model="from" class="dfb-search" type="date" :max="to || today">
      </label>
      <label class="fpp-field">
        <span>إلى</span>
        <input v-model="to" class="dfb-search" type="date" :min="from" :max="today">
      </label>
      <label class="fpp-field is-wide">
        <span>بحث</span>
        <input v-model="search" class="dfb-search" type="search" placeholder="اسم المحل أو الهاتف" aria-label="بحث في المحلات">
      </label>
    </section>

    <p v-if="pins.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="pins.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(pins.error.value, 'تعذر تحميل المحلات.') }}
      <button type="button" class="dfb-button is-small" @click="pins.refetch()">إعادة المحاولة</button>
    </div>
    <template v-else>
      <section class="fpp-card">
        <div class="fpp-card-head">
          <h3 class="fpp-card-title">الخريطة · {{ num(total) }} محل</h3>
          <div class="fpp-legend">
            <span v-for="i in [5, 4, 3, 2, 1]" :key="i"><i class="fpp-dot" :style="{ background: INTEREST_COLORS[i] }" />{{ INTEREST_LABELS[i] }}</span>
          </div>
        </div>
        <div class="fpp-map">
          <FpMapCanvas :markers="markers" cluster :selectedKey="selectedKey" label="خريطة المحلات" @select="selectedKey = $event" />
        </div>
      </section>

      <section class="fpp-card">
        <p v-if="!rows.length" class="dfb-state">لا توجد محلات مطابقة.</p>
        <div v-else class="fpp-table-wrap">
          <table class="fpp-table">
            <thead>
              <tr>
                <th>المحل</th>
                <th>الاهتمام</th>
                <th>الهاتف</th>
                <th>المروّج</th>
                <th>الوقت</th>
                <th v-if="canManage"><span class="sr-only">إجراءات</span></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in rows" :key="p.id" class="is-click" tabindex="0" @click="selectedKey = `pin-${p.id}`" @keydown.enter="selectedKey = `pin-${p.id}`">
                <td>
                  <strong>{{ p.name }}</strong>
                  <small v-if="p.note" class="dfb-cell-sub">{{ p.note }}</small>
                </td>
                <td>
                  <span class="fpp-chip" :style="{ background: `${INTEREST_COLORS[p.interest]}22`, color: INTEREST_COLORS[p.interest] }">
                    <i class="fpp-dot" :style="{ background: INTEREST_COLORS[p.interest] }" />{{ p.interestLabel }}
                  </span>
                </td>
                <td><span dir="ltr">{{ p.phone || '—' }}</span></td>
                <td>
                  <button type="button" class="dfb-name-link" @click.stop="openDay(p.userId, iraqDay(p.createdAt))">{{ p.promoterName }}</button>
                </td>
                <td class="nowrap">{{ dateTime(p.createdAt) }}</td>
                <td v-if="canManage">
                  <button type="button" class="dfb-button is-small fpp-danger" @click.stop="confirmDelete = p">حذف</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <nav v-if="page > 1 || hasMore" class="fpp-pager" aria-label="الصفحات">
          <button type="button" class="dfb-button is-small" :disabled="page <= 1" @click="page--">السابق</button>
          <span>صفحة {{ num(page) }}</span>
          <button type="button" class="dfb-button is-small" :disabled="!hasMore" @click="page++">التالي</button>
        </nav>
      </section>
    </template>

    <NeuDialog :visible="confirmDelete != null" title="حذف المحل" size="sm" :busy="remove.isPending.value" @close="confirmDelete = null">
      <p class="dxp-text">سيُحذف المحل «{{ confirmDelete?.name }}» من سجلات المروّج ومن الخريطة. هل تريد المتابعة؟</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="remove.isPending.value" @click="confirmDelete = null">إلغاء</button>
        <button type="button" class="dfb-button fpp-danger" :disabled="remove.isPending.value" @click="doDelete">تأكيد الحذف</button>
      </template>
    </NeuDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useFpToast } from '../composables/useFpToast'
import FpMapCanvas, { type FpMarker } from '../components/FpMapCanvas.vue'
import {
  apiErrorMessage,
  useDeletePinMutation,
  usePinsQuery,
  usePromoterPermissions,
  usePromotersLiveQuery,
  type AdminPin,
} from '../requests/fieldPromoters'
import { INTEREST_COLORS, INTEREST_LABELS, dateTime, escapeHtml, iraqDayOf as iraqDay, iraqToday, num } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'

const toast = useFpToast()
const { branchId, openDay } = useFpNav()
const { canManage } = usePromoterPermissions()
const today = iraqToday()
const userId = ref<number | null>(null)
const minInterest = ref<number | null>(null)
const from = ref(iraqToday(-6))
const to = ref(today)
const search = ref('')
const debounced = ref('')
const page = ref(1)
const selectedKey = ref<string | null>(null)
const confirmDelete = ref<AdminPin | null>(null)

let timer: ReturnType<typeof setTimeout> | undefined
watch(search, (v) => {
  clearTimeout(timer)
  timer = setTimeout(() => {
    debounced.value = v
  }, 300)
})
watch([branchId, userId, minInterest, from, to, debounced], () => {
  page.value = 1
})

const live = usePromotersLiveQuery(branchId)
const promoters = computed(() => live.data.value?.promoters ?? [])
const pins = usePinsQuery(
  computed(() => ({
    branchId: branchId.value,
    userId: userId.value,
    minInterest: minInterest.value,
    from: from.value,
    to: to.value,
    q: debounced.value,
    page: page.value,
  }))
)
const rows = computed(() => pins.data.value?.items ?? [])
const total = computed(() => pins.data.value?.total ?? 0)
const hasMore = computed(() => pins.data.value?.hasMore ?? false)

const markers = computed<FpMarker[]>(() =>
  rows.value.map((p) => ({
    key: `pin-${p.id}`,
    lat: p.lat,
    lng: p.lng,
    color: INTEREST_COLORS[p.interest] ?? '#8a9694',
    popupHtml: `<div dir="rtl" style="min-width:160px"><strong>${escapeHtml(p.name)}</strong><br>${escapeHtml(p.interestLabel)}<br>${escapeHtml(p.promoterName)} · ${escapeHtml(dateTime(p.createdAt))}${p.phone ? `<br><span dir="ltr">${escapeHtml(p.phone)}</span>` : ''}</div>`,
  }))
)

const remove = useDeletePinMutation()
function doDelete() {
  const pin = confirmDelete.value
  if (!pin) return
  remove.mutate(pin.id, {
    onSuccess: () => {
      toast.add({ severity: 'success', summary: 'تم حذف المحل', life: 3000 })
      confirmDelete.value = null
    },
    onError: (e) => toast.add({ severity: 'error', summary: 'تعذر الحذف', detail: apiErrorMessage(e), life: 4000 }),
  })
}
</script>

<style scoped>
.dxp-text { font-size: 14px; line-height: 1.8; margin: 0; }
</style>
