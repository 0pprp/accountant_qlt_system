<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">التنبيهات</h2>
        <p class="dfb-subtitle">تنبيهات وقوف المروّجين في مكان واحد أثناء الدوام · غير المشاهَدة {{ num(unseen) }}</p>
      </div>
      <div class="fpp-filters">
        <label class="fpp-check">
          <input v-model="openOnly" type="checkbox">
          المفتوحة فقط
        </label>
        <button type="button" class="dfb-button" :disabled="unseen === 0 || seen.isPending.value" @click="markAll">قراءة الكل</button>
      </div>
    </header>

    <p v-if="alerts.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="alerts.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(alerts.error.value, 'تعذر تحميل التنبيهات.') }}
      <button type="button" class="dfb-button is-small" @click="alerts.refetch()">إعادة المحاولة</button>
    </div>
    <template v-else>
      <p v-if="!rows.length" class="dfb-state">{{ openOnly ? 'لا توجد تنبيهات مفتوحة.' : 'لا توجد تنبيهات.' }}</p>
      <ul v-else class="fpp-list">
        <li v-for="a in rows" :key="a.id" class="fpp-alert" :class="{ 'is-unseen': !a.seen }">
          <button type="button" class="fpp-alert-main" @click="openDay(a.userId, iraqDay(a.since))">
            <strong>{{ a.fullName }}</strong>
            <small>
              واقف {{ num(a.minutes) }} دقيقة · من {{ dateTime(a.since) }} حتى {{ time(a.lastSeen) }}
            </small>
          </button>
          <span class="fpp-chip" :class="a.resolvedAt ? 'tone-muted' : 'tone-danger'">{{ a.resolvedAt ? `انتهى ${time(a.resolvedAt)}` : 'مفتوح' }}</span>
          <button v-if="!a.seen" type="button" class="dfb-button is-small" :disabled="seen.isPending.value" @click="seen.mutate(a.id)">تمت المشاهدة</button>
          <span v-else class="fpp-chip tone-plain">شوهد</span>
        </li>
      </ul>
      <nav v-if="page > 1 || hasMore" class="fpp-pager" aria-label="الصفحات">
        <button type="button" class="dfb-button is-small" :disabled="page <= 1" @click="page--">السابق</button>
        <span>صفحة {{ num(page) }}</span>
        <button type="button" class="dfb-button is-small" :disabled="!hasMore" @click="page++">التالي</button>
      </nav>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useFpToast } from '../composables/useFpToast'
import { apiErrorMessage, useAlertsQuery, useMarkAlertSeenMutation } from '../requests/fieldPromoters'
import { dateTime, iraqDayOf as iraqDay, num, time } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'

const toast = useFpToast()
const { openDay } = useFpNav()
const openOnly = ref(false)
const page = ref(1)
watch(openOnly, () => {
  page.value = 1
})

const alerts = useAlertsQuery(computed(() => ({ openOnly: openOnly.value, page: page.value })))
const rows = computed(() => alerts.data.value?.items ?? [])
const unseen = computed(() => alerts.data.value?.unseen ?? 0)
const hasMore = computed(() => alerts.data.value?.hasMore ?? false)
const seen = useMarkAlertSeenMutation()

function markAll() {
  seen.mutate(null, {
    onSuccess: () => toast.add({ severity: 'success', summary: 'تم تعليم كل التنبيهات كمشاهَدة', life: 3000 }),
    onError: (e) => toast.add({ severity: 'error', summary: 'تعذر التنفيذ', detail: apiErrorMessage(e), life: 4000 }),
  })
}
</script>
