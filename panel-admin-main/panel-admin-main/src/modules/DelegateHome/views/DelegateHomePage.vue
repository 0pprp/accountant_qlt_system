<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { getTabByIds } from '@/modules/Management/domain/sections'
import { useManagementAccess } from '@/modules/Management/composables/useManagementAccess'
import { useDelegateBranchId, useDelegateSummary } from '@/modules/DelegateManager/composables/useDelegateSummary'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const branchId = useDelegateBranchId()
const canRead = computed(() => auth.hasPermission('DelegateHome', 'Read') === true)
const summaryQuery = useDelegateSummary(branchId)
const { canOpenTab } = useManagementAccess()
const data = computed(() => summaryQuery.data.value)

interface Tile {
  key: string
  label: string
  value: number | null | undefined
  available: boolean
  chip: string
  tone: 'info' | 'warn' | 'danger'
  tabId: string
  query?: Record<string, string>
}

const tiles = computed<Tile[]>(() => {
  const d = data.value
  return [
    { key: 'complaints', label: 'رسائل وشكاوى غير مقروءة', value: d?.unreadComplaints, available: d?.unreadComplaintsAvailable === true, chip: 'جديدة', tone: 'info', tabId: 'complaints' },
    { key: 'exceptions', label: 'طلبات استثناء قيد الانتظار', value: d?.pendingExceptions, available: d?.pendingExceptionsAvailable === true, chip: 'تحتاج قرارك', tone: 'warn', tabId: 'exceptions' },
    { key: 'wrong', label: 'مبيعات خاطئة غير مقروءة', value: d?.unreadWrongSales, available: d?.unreadWrongSalesAvailable === true, chip: 'اطلاع', tone: 'danger', tabId: 'wrongSalesDelegate', query: { mailbox: 'unread' } },
    { key: 'waivers', label: 'طلبات تنازل قيد الانتظار', value: d?.pendingWaivers, available: d?.waiversAvailable === true, chip: 'تحتاج قرارك', tone: 'warn', tabId: 'delegateWaivers' },
  ]
})

const exceptionSummary = computed(() => {
  const d = data.value
  if (!d?.exceptionsSummaryAvailable) return null
  return [
    { label: 'موافق عليه', value: d.approvedExceptions, tone: 'good' },
    { label: 'مرفوض', value: d.rejectedExceptions, tone: 'bad' },
    { label: 'ملغى', value: d.cancelledExceptions, tone: 'muted' },
    { label: 'الإجمالي', value: d.totalExceptions, tone: '' },
  ]
})

const waiverSummary = computed(() => {
  const d = data.value
  if (!d?.waiversAvailable) return null
  return [
    { label: 'بانتظار المحاسب', value: d.awaitingAccountantWaivers, tone: 'warn' },
    { label: 'مطبّق', value: d.approvedWaivers, tone: 'good' },
    { label: 'مرفوض', value: d.rejectedWaivers, tone: 'bad' },
    { label: 'الإجمالي', value: d.totalWaivers, tone: '' },
  ]
})

function open(tile: Tile) {
  if (!tile.available) return
  const tab = getTabByIds('delegate', tile.tabId)
  if (!tab) return
  if (!canOpenTab(tab)) {
    void router.push({ name: 'AccessDeniedRoute', query: { from: tab.routeName } })
    return
  }
  const query: Record<string, string> = { ...(tile.query ?? {}) }
  if (branchId.value != null) query.branchId = String(branchId.value)
  void router.push({ name: tab.routeName, query })
}

function num(v: number | null | undefined) {
  return v == null ? '—' : v.toLocaleString('en-US')
}
</script>

<template>
  <div class="dh-page" dir="rtl">
    <header class="dh-head">
      <h2 class="dfb-title">الرئيسية</h2>
      <p class="dfb-subtitle">ما يحتاج انتباهك. اضغط على أي بطاقة لفتح قسمها.</p>
    </header>

    <p v-if="!canRead" class="dfb-error" role="alert">ليس لديك صلاحية قراءة رئيسية المفوض.</p>
    <template v-else>
      <div v-if="summaryQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل الملخص.
        <button type="button" class="dfb-button is-small" @click="summaryQuery.refetch()">إعادة المحاولة</button>
      </div>
      <p v-else-if="summaryQuery.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
      <template v-else>
        <div class="dh-tiles">
          <button
            v-for="t in tiles"
            :key="t.key"
            type="button"
            class="dh-tile"
            :disabled="!t.available"
            @click="open(t)"
          >
            <span class="dh-chip" :class="`is-${t.tone}`">{{ t.available ? t.chip : 'غير متاح' }}</span>
            <strong class="dh-value">{{ t.available ? num(t.value) : '—' }}</strong>
            <span class="dh-label">{{ t.label }}</span>
          </button>
        </div>

        <div class="dh-summaries">
          <section v-if="exceptionSummary" class="dh-summary" aria-label="ملخص طلبات الاستثناء">
            <h3>ملخص طلبات الاستثناء</h3>
            <div class="dh-cells">
              <div v-for="c in exceptionSummary" :key="c.label" class="dh-cell">
                <strong :class="`is-${c.tone}`">{{ num(c.value) }}</strong>
                <span>{{ c.label }}</span>
              </div>
            </div>
          </section>
          <section v-if="waiverSummary" class="dh-summary" aria-label="ملخص طلبات التنازل">
            <h3>ملخص طلبات التنازل</h3>
            <div class="dh-cells">
              <div v-for="c in waiverSummary" :key="c.label" class="dh-cell">
                <strong :class="`is-${c.tone}`">{{ num(c.value) }}</strong>
                <span>{{ c.label }}</span>
              </div>
            </div>
          </section>
        </div>
        <p class="dfb-subtitle">الأرقام ضمن المحافظة المختارة في الأعلى ونطاق حسابك.</p>
      </template>
    </template>
  </div>
</template>

<style scoped>
.dh-page { display: flex; flex-direction: column; gap: 18px; color: var(--dfb-ink, #00201e); }
.dh-head { display: flex; flex-direction: column; gap: 2px; }
.dh-tiles { display: grid; gap: 18px; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); }
.dh-tile {
  align-items: flex-start; background: var(--dfb-raised, #f6f9f8); border: 0; border-radius: 20px;
  box-shadow: var(--dfb-up); color: inherit; cursor: pointer; display: flex; flex-direction: column;
  font: inherit; gap: 8px; padding: 18px 20px; text-align: start; transition: box-shadow .18s ease;
}
.dh-tile:hover:not(:disabled) { box-shadow: var(--dfb-up-hover); }
.dh-tile:active:not(:disabled) { box-shadow: var(--dfb-down); }
.dh-tile:focus-visible { outline: 3px solid #71b4a9; outline-offset: 3px; }
.dh-tile:disabled { cursor: default; opacity: .7; }
.dh-chip { border-radius: 999px; font-size: 12px; font-weight: 600; padding: 2px 10px; }
.dh-chip.is-info { background: var(--dfb-primary-soft); color: var(--dfb-primary); }
.dh-chip.is-warn { background: var(--dfb-warn-soft); color: var(--dfb-tier-weak); }
.dh-chip.is-danger { background: var(--dfb-danger-soft); color: var(--dfb-tier-bad); }
.dh-value { font-size: 38px; font-variant-numeric: tabular-nums; line-height: 1.2; }
.dh-label { color: var(--dfb-muted); font-size: 15px; }
.dh-summaries { display: grid; gap: 18px; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); }
.dh-summary { background: var(--dfb-well); border-radius: 20px; box-shadow: var(--dfb-down); display: flex; flex-direction: column; gap: 12px; padding: 18px 20px; }
.dh-summary h3 { font-size: 17px; margin: 0; }
.dh-cells { display: grid; gap: 10px; grid-template-columns: repeat(4, minmax(0, 1fr)); }
.dh-cell { background: var(--dfb-raised); border-radius: 14px; display: flex; flex-direction: column; gap: 2px; padding: 10px 12px; }
.dh-cell strong { font-size: 22px; font-variant-numeric: tabular-nums; }
.dh-cell span { color: var(--dfb-muted); font-size: 13px; }
.is-good { color: var(--dfb-tier-excellent); }
.is-bad { color: var(--dfb-tier-bad); }
.is-warn { color: var(--dfb-tier-weak); }
.is-muted { color: var(--dfb-muted); }
@media (max-width: 600px) {
  .dh-tiles { grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
  .dh-tile { padding: 14px; }
  .dh-value { font-size: 30px; }
  .dh-summaries { grid-template-columns: minmax(0, 1fr); }
  .dh-cells { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
