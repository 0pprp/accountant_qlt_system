<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">الإعدادات</h2>
        <p class="dfb-subtitle">إعدادات عامة لكل المروّجين الميدانيين</p>
      </div>
    </header>

    <p v-if="settings.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="settings.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(settings.error.value, 'تعذر تحميل الإعدادات.') }}
      <button type="button" class="dfb-button is-small" @click="settings.refetch()">إعادة المحاولة</button>
    </div>
    <form v-else-if="form" class="fpp-card" novalidate @submit.prevent="submit">
      <fieldset class="fpp-fieldset" :disabled="!canManage || save.isPending.value">
        <div class="fpp-form">
          <h3 class="fpp-card-title is-full">الهدف اليومي الافتراضي</h3>
          <label class="fpp-field">
            <span>الساعات</span>
            <input v-model.number="form.targetHours" class="dfb-search" type="number" min="0" max="24" inputmode="numeric">
          </label>
          <label class="fpp-field">
            <span>الدقائق</span>
            <input v-model.number="form.targetMinutes" class="dfb-search" type="number" min="0" max="59" inputmode="numeric">
          </label>

          <h3 class="fpp-card-title is-full">تنبيه الوقوف</h3>
          <label class="fpp-field">
            <span>نصف القطر (متر، 30–2000)</span>
            <input v-model.number="form.stationaryRadiusMeters" class="dfb-search" type="number" min="30" max="2000" inputmode="numeric">
          </label>
          <label class="fpp-field">
            <span>مدة الوقوف (دقيقة، 5–600)</span>
            <input v-model.number="form.stationaryMinutes" class="dfb-search" type="number" min="5" max="600" inputmode="numeric">
          </label>

          <h3 class="fpp-card-title is-full">الدعم</h3>
          <label class="fpp-field">
            <span>هاتف الدعم</span>
            <input v-model="form.supportPhone" class="dfb-search" dir="ltr" inputmode="tel" maxlength="30">
          </label>
          <label class="fpp-field">
            <span>واتساب الدعم</span>
            <input v-model="form.supportWhatsapp" class="dfb-search" dir="ltr" inputmode="tel" maxlength="30">
          </label>

          <p class="fpp-muted is-full">تُحذف نقاط الموقع بعد {{ num(settings.data.value?.locationRetentionDays) }} يوماً.</p>
          <p v-if="!canManage" class="fpp-muted is-full">لا تملك صلاحية تعديل الإعدادات؛ العرض للقراءة فقط.</p>
          <p v-if="error" class="dfb-error is-full" role="alert">{{ error }}</p>
        </div>
      </fieldset>
      <div v-if="canManage" class="dfb-actions">
        <button type="submit" class="dfb-button is-primary" :disabled="save.isPending.value">{{ save.isPending.value ? 'جاري الحفظ…' : 'حفظ الإعدادات' }}</button>
        <button type="button" class="dfb-button" :disabled="save.isPending.value" @click="reset">تراجع</button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useFpToast } from '../composables/useFpToast'
import { apiErrorMessage, usePromoterPermissions, useSaveSettingsMutation, useSettingsQuery, type PromoterSettings } from '../requests/fieldPromoters'
import { num } from '../domain/format'

interface SettingsForm {
  targetHours: number | ''
  targetMinutes: number | ''
  stationaryRadiusMeters: number | ''
  stationaryMinutes: number | ''
  supportPhone: string
  supportWhatsapp: string
}

const toast = useFpToast()
const { canManage } = usePromoterPermissions()
const settings = useSettingsQuery()
const save = useSaveSettingsMutation()
const form = ref<SettingsForm | null>(null)
const error = ref<string | null>(null)

function fromSettings(s: PromoterSettings): SettingsForm {
  return {
    targetHours: Math.floor(s.defaultDailyWorkMinutes / 60),
    targetMinutes: s.defaultDailyWorkMinutes % 60,
    stationaryRadiusMeters: s.stationaryRadiusMeters,
    stationaryMinutes: s.stationaryMinutes,
    supportPhone: s.supportPhone ?? '',
    supportWhatsapp: s.supportWhatsapp ?? '',
  }
}

watch(
  () => settings.data.value,
  (s) => {
    if (s) form.value = fromSettings(s)
  },
  { immediate: true }
)

function reset() {
  error.value = null
  if (settings.data.value) form.value = fromSettings(settings.data.value)
}

function int(v: number | '') {
  return v === '' ? 0 : Number(v)
}

function submit() {
  const f = form.value
  const current = settings.data.value
  if (!f || !current) return
  const h = int(f.targetHours)
  const m = int(f.targetMinutes)
  const radius = int(f.stationaryRadiusMeters)
  const minutes = int(f.stationaryMinutes)
  if (![h, m, radius, minutes].every(Number.isInteger) || h < 0 || m < 0 || m > 59 || h * 60 + m > 1440)
    return void (error.value = 'الهدف اليومي بين 0 و 24 ساعة.')
  if (radius < 30 || radius > 2000) return void (error.value = 'نصف القطر بين 30 و 2000 متر.')
  if (minutes < 5 || minutes > 600) return void (error.value = 'مدة الوقوف بين 5 و 600 دقيقة.')
  error.value = null
  save.mutate(
    {
      defaultDailyWorkMinutes: h * 60 + m,
      stationaryRadiusMeters: radius,
      stationaryMinutes: minutes,
      supportPhone: f.supportPhone.trim() || null,
      supportWhatsapp: f.supportWhatsapp.trim() || null,
      locationRetentionDays: current.locationRetentionDays,
    },
    {
      onSuccess: () => toast.add({ severity: 'success', summary: 'تم حفظ الإعدادات', life: 3000 }),
      onError: (e) => {
        error.value = apiErrorMessage(e, 'تعذر حفظ الإعدادات')
      },
    }
  )
}
</script>
