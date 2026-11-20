<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">الرئيسية</h2>
        <p class="dfb-subtitle">لمحة اليوم عن دوام المروّجين ونشاطهم · تتحدث كل 20 ثانية</p>
      </div>
    </header>

    <p v-if="live.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="live.isError.value" class="dfb-error" role="alert">
      تعذر تحميل بيانات المروّجين.
      <button type="button" class="dfb-button is-small" @click="live.refetch()">إعادة المحاولة</button>
    </div>
    <template v-else-if="data">
      <section class="fpp-kpis" aria-label="مؤشرات اليوم">
        <article class="fpp-kpi is-good">
          <span>في الدوام الآن</span>
          <strong>{{ num(data.working) }} <small>/ {{ num(data.total) }}</small></strong>
          <em>{{ num(data.notStarted) }} لم يبدؤوا</em>
        </article>
        <article class="fpp-kpi">
          <span>محلات اليوم</span>
          <strong>{{ num(data.pinsToday) }}</strong>
          <em>مهتم فأكثر {{ num(data.interestedPinsToday) }}</em>
        </article>
        <article class="fpp-kpi">
          <span>طلبات اليوم</span>
          <strong>{{ num(data.ordersToday) }}</strong>
          <em>{{ num(data.soldToday) }} بِيعت</em>
        </article>
        <article class="fpp-kpi" :class="{ 'is-danger': data.unseenAlerts > 0 }">
          <span>تنبيهات الوقوف</span>
          <strong>{{ num(data.unseenAlerts) }}</strong>
          <em>غير مشاهَدة</em>
        </article>
        <article class="fpp-kpi">
          <span>حقّقوا الهدف أمس</span>
          <strong>{{ num(data.metYesterday) }} <small>/ {{ num(data.targetedYesterday) }}</small></strong>
          <em>{{ percent(data.metYesterday, data.targetedYesterday) }}%</em>
        </article>
      </section>

      <section class="fpp-card" aria-label="تنبيهات الوقوف">
        <div class="fpp-card-head">
          <h3 class="fpp-card-title">تنبيهات الوقوف المفتوحة</h3>
          <RouterLink v-if="alertsTab" class="dfb-link" :to="{ name: alertsTab, query: baseQuery() }">كل التنبيهات</RouterLink>
        </div>
        <p v-if="alerts.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
        <p v-else-if="alerts.isError.value" class="dfb-error" role="alert">تعذر تحميل التنبيهات.</p>
        <p v-else-if="!openAlerts.length" class="dfb-state">لا توجد تنبيهات وقوف مفتوحة.</p>
        <ul v-else class="fpp-list">
          <li v-for="a in openAlerts" :key="a.id" class="fpp-alert" :class="{ 'is-unseen': !a.seen }">
            <button type="button" class="fpp-alert-main" @click="openDay(a.userId)">
              <strong>{{ a.fullName }}</strong>
              <small>واقف منذ {{ time(a.since) }} · {{ num(a.minutes) }} دقيقة · آخر رصد {{ relative(a.lastSeen) }}</small>
            </button>
            <button v-if="!a.seen" type="button" class="dfb-button is-small" :disabled="seen.isPending.value" @click="seen.mutate(a.id)">تمت المشاهدة</button>
            <span v-else class="fpp-chip tone-muted">شوهد</span>
          </li>
        </ul>
      </section>

      <section class="fpp-card" aria-label="المروّجون اليوم">
        <div class="fpp-card-head">
          <h3 class="fpp-card-title">المروّجون اليوم</h3>
          <span class="fpp-muted">اضغط على المروّج لعرض يومه</span>
        </div>
        <p v-if="!data.promoters.length" class="dfb-state">لا يوجد مروّجون في هذا النطاق.</p>
        <div v-else class="fpp-table-wrap">
          <table class="fpp-table">
            <thead>
              <tr>
                <th>المروّج</th>
                <th>الحالة</th>
                <th>آخر موقع</th>
                <th>ساعات العمل</th>
                <th>المحلات</th>
                <th>الطلبات</th>
                <th>الهدف</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="p in data.promoters"
                :key="p.id"
                class="is-click"
                tabindex="0"
                @click="openDay(p.id)"
                @keydown.enter="openDay(p.id)"
              >
                <td><strong>{{ p.fullName }}</strong></td>
                <td><span class="fpp-chip" :class="`tone-${STATUS_META[p.status]?.tone ?? 'muted'}`">{{ STATUS_META[p.status]?.label ?? p.statusLabel }}</span></td>
                <td>{{ relative(p.lastAt) }}</td>
                <td><span class="dfb-num">{{ hm(p.workMinutes) }}</span></td>
                <td>{{ num(p.pins) }}</td>
                <td>{{ num(p.orders) }}</td>
                <td>
                  <span v-if="p.targetMinutes > 0" class="fpp-progress">
                    <span class="fpp-bar" :class="{ 'is-done': p.targetPercent >= 100 }"><span :style="{ inlineSize: `${Math.min(100, p.targetPercent)}%` }" /></span>
                    {{ num(p.targetPercent) }}%
                  </span>
                  <span v-else class="fpp-muted">بلا هدف</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { usePromotersLiveQuery, useAlertsQuery, useMarkAlertSeenMutation } from '../requests/fieldPromoters'
import { STATUS_META, hm, num, percent, relative, time } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'
import { getTabByIds } from '@/modules/Management/domain/sections'
import { useManagementAccess } from '@/modules/Management/composables/useManagementAccess'

const { branchId, openDay } = useFpNav()
const { canOpenTab } = useManagementAccess()
const live = usePromotersLiveQuery(branchId, true)
const data = computed(() => live.data.value ?? null)
const alerts = useAlertsQuery({ openOnly: true, page: 1 })
const seen = useMarkAlertSeenMutation()
const openAlerts = computed(() => (alerts.data.value?.items ?? []).slice(0, 6))
const alertsTab = computed(() => {
  const tab = getTabByIds('fieldPromoter', 'fpAlerts')
  return tab && canOpenTab(tab) ? tab.routeName : null
})

function baseQuery() {
  return branchId.value == null ? {} : { branchId: String(branchId.value) }
}
</script>
