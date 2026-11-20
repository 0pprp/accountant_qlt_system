<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">الحضور والأهداف</h2>
        <p class="dfb-subtitle">تحقيق هدف ساعات العمل اليومي لكل مروّج</p>
      </div>
      <div class="fpp-filters">
        <label class="fpp-field">
          <span>من</span>
          <input v-model="from" class="dfb-search" type="date" :max="to">
        </label>
        <label class="fpp-field">
          <span>إلى</span>
          <input v-model="to" class="dfb-search" type="date" :min="from" :max="today">
        </label>
        <button type="button" class="dfb-button" :disabled="!!rangeError || exporting" @click="exportExcel">
          {{ exporting ? 'جاري التصدير…' : 'تصدير Excel' }}
        </button>
      </div>
    </header>

    <p v-if="rangeError" class="dfb-error" role="alert">{{ rangeError }}</p>
    <p v-else-if="attendance.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="attendance.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(attendance.error.value, 'تعذر تحميل الحضور.') }}
      <button type="button" class="dfb-button is-small" @click="attendance.refetch()">إعادة المحاولة</button>
    </div>
    <section v-else-if="data" class="fpp-card">
      <div class="fpp-legend">
        <span v-for="(m, k) in ATTENDANCE_META" :key="k"><span class="fpp-chip" :class="`tone-${m.tone}`">{{ m.label }}</span></span>
      </div>
      <p v-if="!data.rows.length" class="dfb-state">لا يوجد مروّجون في هذا النطاق.</p>
      <div v-else class="fpp-table-wrap">
        <table class="fpp-grid">
          <thead>
            <tr>
              <th class="is-name">المروّج</th>
              <th v-for="d in data.days" :key="d">{{ dayHeader(d).weekday }}<small>{{ dayHeader(d).day }}</small></th>
              <th>حقّق</th>
              <th>لم يحقّق</th>
              <th>غائب</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in data.rows" :key="r.userId">
              <td class="is-name">
                <button type="button" class="dfb-name-link" @click="openDay(r.userId)">{{ r.fullName }}</button>
                <small class="dfb-cell-sub">الهدف {{ r.targetMinutes > 0 ? hm(r.targetMinutes) : 'بلا هدف' }}</small>
              </td>
              <td v-for="c in r.days" :key="c.day">
                <button
                  type="button"
                  class="fpp-chip fpp-chip-btn"
                  :class="`tone-${ATTENDANCE_META[c.status]?.tone ?? 'plain'}`"
                  :title="tooltip(c)"
                  :aria-label="`${r.fullName} ${c.day}: ${tooltip(c)}`"
                  @click="openDay(r.userId, c.day)"
                >
                  {{ ATTENDANCE_META[c.status]?.label ?? c.label }}
                </button>
              </td>
              <td>{{ num(r.met) }}</td>
              <td>{{ num(r.missed) }}</td>
              <td>{{ num(r.off) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useFpToast } from '../composables/useFpToast'
import { apiErrorMessage, downloadAttendanceExport, useAttendanceQuery, type AttendanceCell } from '../requests/fieldPromoters'
import { ATTENDANCE_META, dayHeader, daysBetween, hm, iraqToday, num } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'

const toast = useFpToast()
const { branchId, openDay } = useFpNav()
const today = iraqToday()
const from = ref(iraqToday(-6))
const to = ref(today)
const exporting = ref(false)

const rangeError = computed(() => {
  if (!from.value || !to.value) return 'اختر بداية ونهاية الفترة.'
  if (from.value > to.value) return 'تاريخ البداية بعد تاريخ النهاية.'
  if (daysBetween(from.value, to.value) > 93) return 'أقصى مدة 93 يوماً.'
  return null
})

const params = computed(() => ({ from: from.value, to: to.value, branchId: branchId.value }))
const attendance = useAttendanceQuery(params, () => rangeError.value == null)
const data = computed(() => (rangeError.value ? null : attendance.data.value ?? null))

function tooltip(c: AttendanceCell) {
  const target = c.targetMinutes > 0 ? ` من ${hm(c.targetMinutes)}` : ''
  return `${c.label} · ${hm(c.workMinutes)}${target} · محلات ${num(c.pins)} · طلبات ${num(c.orders)}`
}

async function exportExcel() {
  if (rangeError.value) return
  exporting.value = true
  try {
    await downloadAttendanceExport(params.value)
  } catch (e) {
    toast.add({ severity: 'error', summary: 'تعذر تصدير الملف', detail: apiErrorMessage(e, 'حاول مرة أخرى'), life: 4000 })
  } finally {
    exporting.value = false
  }
}
</script>
