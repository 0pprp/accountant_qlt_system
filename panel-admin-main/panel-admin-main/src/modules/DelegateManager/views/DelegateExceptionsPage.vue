<template>
  <div class="dxp" dir="rtl">
    <header class="dxp-toolbar">
      <div class="dxp-titles">
        <h2 class="dfb-title">طلبات الاستثناء</h2>
        <p class="dfb-subtitle">يطلبها مدير المبيعات، وتقرر أنت الموافقة أو الرفض.</p>
      </div>
      <div class="dxp-chips" role="group" aria-label="حالة الطلب">
        <button
          v-for="s in STATUSES"
          :key="s"
          type="button"
          class="dxp-chip"
          :aria-pressed="status === s"
          @click="setStatus(s)"
        >
          {{ EXCEPTION_STATUS_LABELS[s] }}
          <span class="dxp-chip-count">{{ countOf(s) }}</span>
        </button>
      </div>
      <input v-model="search" class="dfb-search dxp-search" type="search" placeholder="بحث باسم الزبون أو الهاتف أو الفرع" aria-label="بحث">
    </header>

    <div class="dxp-split">
      <section class="dxp-list" aria-label="قائمة الطلبات">
        <p v-if="inbox.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
        <div v-else-if="inbox.isError.value" class="dfb-error" role="alert">
          تعذر تحميل الطلبات.
          <button type="button" class="dfb-button is-small" @click="inbox.refetch()">إعادة المحاولة</button>
        </div>
        <p v-else-if="!rows.length" class="dfb-state">لا توجد طلبات استثناء {{ EXCEPTION_STATUS_LABELS[status] }}</p>
        <template v-else>
          <button
            v-for="r in rows"
            :key="r.id"
            type="button"
            class="dxp-row"
            :class="{ 'is-active': r.id === selectedId, 'is-unread': !r.isRead }"
            :aria-current="r.id === selectedId ? 'true' : undefined"
            @click="select(r)"
          >
            <span class="dxp-row-top">
              <strong class="dxp-row-name">{{ r.customerName }}</strong>
              <span v-if="!r.isRead" class="dxp-tag is-new">جديد</span>
              <span v-else class="dxp-tag">مقروء</span>
            </span>
            <span class="dxp-row-line">طالب الاستثناء: {{ r.requestedByName ?? 'غير معروف' }}</span>
            <span class="dxp-row-meta">{{ r.branchName ?? '' }} · {{ relative(r.requestedAtUtc) }}</span>
          </button>
          <nav v-if="pages > 1" class="dxp-pager" aria-label="الصفحات">
            <button type="button" class="dfb-button is-small" :disabled="page <= 1" @click="page--">السابق</button>
            <span>صفحة {{ page }} من {{ pages }}</span>
            <button type="button" class="dfb-button is-small" :disabled="page >= pages" @click="page++">التالي</button>
          </nav>
        </template>
      </section>

      <section class="dxp-detail" aria-label="تفاصيل الطلب">
        <p v-if="!selected" class="dfb-state">اختر طلباً من القائمة لعرض تفاصيله</p>
        <template v-else>
          <header class="dxp-detail-head">
            <div>
              <h3 class="dxp-name">{{ selected.customerName }}</h3>
              <p class="dfb-subtitle">طلب #{{ selected.id }} · {{ selected.branchName ?? '' }} · {{ relative(selected.requestedAtUtc) }}</p>
            </div>
            <span class="dxp-status" :class="`is-${selected.status.toLowerCase()}`">{{ EXCEPTION_STATUS_LABELS[selected.status] }}</span>
          </header>

          <div class="dxp-banner" :class="{ 'is-new': !selected.existingCustomerId }">
            <strong>{{ selected.existingCustomerId ? 'زبون موجود' : 'زبون جديد' }}</strong>
            <span v-if="selected.existingCustomerId && overview">
              مجموع مبيعاته {{ money(overview.sellTotal) }}، الواصل {{ money(overview.paidTotal) }}، الباقي {{ money(overview.remaining) }}.
            </span>
            <span v-else-if="!selected.existingCustomerId">لا يوجد حساب سابق مرتبط بهذا الطلب. راجع المطابقات أدناه.</span>
          </div>

          <div class="dxp-grid">
            <article class="dxp-card">
              <h4>تفاصيل الطلب الحالي</h4>
              <p v-if="detail.isPending.value" class="dfb-state">جاري التحميل…</p>
              <p v-else-if="detail.isError.value" class="dfb-error" role="alert">تعذر تحميل تفاصيل الطلب.</p>
              <dl v-else-if="request" class="dxp-kv">
                <div><dt>الهاتف</dt><dd dir="ltr">{{ request.customerPhone || '—' }}</dd></div>
                <div><dt>العنوان</dt><dd>{{ request.address || '—' }}</dd></div>
                <div><dt>المنتج</dt><dd>{{ request.requestedProduct || '—' }}</dd></div>
                <div><dt>ملاحظات الطلب</dt><dd>{{ request.submissionNote || '—' }}</dd></div>
                <div><dt>تاريخ الإنشاء</dt><dd>{{ date(request.createdAt) }}</dd></div>
                <div><dt>حالة الطلب</dt><dd>{{ request.statusLabel }}</dd></div>
              </dl>
            </article>
            <article class="dxp-card">
              <h4>مصدر الطلب</h4>
              <dl class="dxp-kv">
                <div><dt>المصدر</dt><dd>{{ request?.sourceTypeLabel ?? '—' }}</dd></div>
                <div><dt>أنشأه</dt><dd>{{ request?.createdByName ?? '—' }}</dd></div>
                <div><dt>الفرع</dt><dd>{{ selected.branchName ?? '—' }}</dd></div>
                <div><dt>مقدّم الاستثناء</dt><dd>{{ selected.requestedByName ?? '—' }}</dd></div>
              </dl>
            </article>
          </div>

          <article class="dxp-card">
            <h4>سبب الاستثناء</h4>
            <p class="dxp-text">{{ selected.requestReason || 'لم يُكتب سبب.' }}</p>
          </article>

          <article v-if="selected.existingCustomerId" class="dxp-card">
            <h4>تقرير بروفايل الزبون</h4>
            <p v-if="profile.isPending.value" class="dfb-state">جاري التحميل…</p>
            <p v-else-if="profile.isError.value" class="dfb-error">تعذر تحميل البروفايل.</p>
            <template v-else-if="profileData">
              <div class="dxp-stats">
                <div><strong>{{ money(overview?.sellTotal) }}</strong><span>سعر البيع</span></div>
                <div><strong>{{ money(overview?.paidTotal) }}</strong><span>المدفوع</span></div>
                <div><strong :class="{ 'is-bad': (overview?.remaining ?? 0) > 0 }">{{ money(overview?.remaining) }}</strong><span>المتبقي</span></div>
                <div><strong>{{ payments.length }}</strong><span>عدد السندات</span></div>
              </div>
              <div class="dxp-tabs" role="tablist" aria-label="تفاصيل البروفايل">
                <button v-for="t in PROFILE_TABS" :key="t.key" type="button" role="tab" class="dxp-chip" :aria-pressed="profileTab === t.key" :aria-selected="profileTab === t.key" @click="profileTab = t.key">
                  {{ t.label }} <span class="dxp-chip-count">{{ t.key === 'payments' ? payments.length : t.key === 'sales' ? sales.length : accounts.length }}</span>
                </button>
              </div>
              <table v-if="profileTab === 'payments'" class="dxp-table">
                <thead><tr><th>التاريخ</th><th>المبلغ</th><th>الوصل</th></tr></thead>
                <tbody>
                  <tr v-for="p in payments.slice(0, 12)" :key="p.id"><td>{{ p.date }}</td><td>{{ money(p.amount) }}</td><td>{{ p.receiptNumber ?? '—' }}</td></tr>
                  <tr v-if="!payments.length"><td colspan="3">لا توجد تسديدات</td></tr>
                </tbody>
              </table>
              <table v-else-if="profileTab === 'sales'" class="dxp-table">
                <thead><tr><th>التاريخ</th><th>البائع</th><th>المبلغ</th><th>المواد</th></tr></thead>
                <tbody>
                  <tr v-for="s in sales.slice(0, 8)" :key="s.id"><td>{{ date(s.createdAt) }}</td><td>{{ s.sellerName ?? '—' }}</td><td>{{ money(s.sellAmount) }}</td><td>{{ s.products.join('، ') || '—' }}</td></tr>
                  <tr v-if="!sales.length"><td colspan="4">لا توجد مبيعات</td></tr>
                </tbody>
              </table>
              <table v-else class="dxp-table">
                <thead><tr><th>المبيع</th><th>سعر البيع</th><th>المدفوع</th><th>المتبقي</th><th>السندات</th></tr></thead>
                <tbody>
                  <tr v-for="a in accounts" :key="a.orderId"><td>#{{ a.orderId }}</td><td>{{ money(a.sellAmount) }}</td><td>{{ money(a.paidAmount) }}</td><td>{{ money(a.remaining) }}</td><td>{{ a.payments.length }}</td></tr>
                  <tr v-if="!accounts.length"><td colspan="5">لا توجد حسابات</td></tr>
                </tbody>
              </table>
              <div v-if="ratings" class="dxp-ratings">
                <div v-for="r in ratings" :key="r.title" class="dxp-rating">
                  <strong>{{ r.title }}</strong>
                  <span>{{ r.ratedByName ?? '—' }} · {{ r.score != null ? `${r.score} / 10` : 'لم يُقيَّم' }}</span>
                  <span v-if="r.shopStockEstimatedValue != null">قيمة بضاعة المحل {{ money(r.shopStockEstimatedValue) }}</span>
                  <span v-if="r.estimatedDailyRevenue != null">الواردات اليومية {{ money(r.estimatedDailyRevenue) }}</span>
                  <span v-if="r.note">{{ r.note }}</span>
                </div>
              </div>
            </template>
          </article>

          <article class="dxp-card">
            <h4>مطابقات في الفرع</h4>
            <p v-if="matchesLoading" class="dfb-state">جاري البحث عن المطابقات…</p>
            <p v-else-if="!matches.length" class="dfb-state">لا توجد أسماء أو أرقام مطابقة في الفرع.</p>
            <ul v-else class="dxp-matches">
              <li v-for="m in matches" :key="`${m.criterion}-${m.hit.customerId}`">
                <strong>{{ m.hit.customerName }}<template v-if="m.hit.phone"> · <span dir="ltr">{{ m.hit.phone }}</span></template></strong>
                <span>{{ m.label }} · {{ m.hit.matchReason }}</span>
                <span>
                  مبيعات {{ money(m.hit.sellTotal) }} · مدفوع {{ money(m.hit.paidTotal) }} · باقي {{ money(m.hit.remaining) }}
                  <template v-if="m.hit.ratingLabel"> · {{ m.hit.ratingLabel }}</template>
                  <template v-if="m.hit.isLegal"> · قانونية</template>
                </span>
              </li>
            </ul>
          </article>

          <article v-if="selected.status === 'Pending' && canDecide" class="dxp-card dxp-decision">
            <h4>القرار</h4>
            <label class="dxp-field">
              <span>ملاحظة القرار (اختياري، حتى 500 حرف)</span>
              <textarea v-model="note" class="dfb-search" rows="3" maxlength="500" :disabled="decide.isPending.value" />
            </label>
            <p v-if="decisionError" class="dfb-error" role="alert">{{ decisionError }}</p>
            <div class="dxp-actions">
              <button type="button" class="dfb-button is-primary" :disabled="decide.isPending.value" @click="ask(true)">موافقة</button>
              <button type="button" class="dfb-button dxp-danger" :disabled="decide.isPending.value" @click="ask(false)">رفض</button>
            </div>
          </article>
          <article v-else-if="selected.status !== 'Pending'" class="dxp-card">
            <h4>القرار</h4>
            <dl class="dxp-kv">
              <div><dt>الحالة</dt><dd>{{ EXCEPTION_STATUS_LABELS[selected.status] }}</dd></div>
              <div><dt>صاحب القرار</dt><dd>{{ selected.decidedByName ?? '—' }}</dd></div>
              <div><dt>وقت القرار</dt><dd>{{ dateTime(selected.decidedAtUtc) }}</dd></div>
              <div><dt>ملاحظة القرار</dt><dd>{{ selected.decisionNote || '—' }}</dd></div>
            </dl>
          </article>
        </template>
      </section>
    </div>

    <NeuDialog
      :visible="confirming != null"
      :title="confirming ? 'الموافقة على الاستثناء' : 'رفض الاستثناء'"
      size="sm"
      :busy="decide.isPending.value"
      @close="confirming = null"
    >
      <p class="dxp-text">
        {{ confirming ? 'ستُسجَّل الموافقة ويستطيع مدير المبيعات إسناد الطلب بعدها.' : 'سيُسجَّل الرفض وينتقل الطلب إلى «مرفوض».' }}
        هل تريد المتابعة؟
      </p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="decide.isPending.value" @click="confirming = null">إلغاء</button>
        <button type="button" class="dfb-button" :class="confirming ? 'is-primary' : 'dxp-danger'" :disabled="decide.isPending.value" @click="submit">
          {{ confirming ? 'تأكيد الموافقة' : 'تأكيد الرفض' }}
        </button>
      </template>
    </NeuDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQueries } from '@tanstack/vue-query'
import { useToast } from 'primevue/usetoast'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { useAuthStore } from '@/modules/Auth/store'
import { useCustomerAuditProfileQuery } from '@/modules/Customer/requests/queries/useCustomerAuditProfileQuery'
import { useSalesRequestByIdQuery } from '@/modules/SalesRequests/requests/queries/useSalesRequestByIdQuery'
import { fetchMatchHits, MATCH_HITS_KEY } from '@/modules/SalesRequests/requests/queries/useMatchHitsQuery'
import type { MatchCriterion, MatchHit } from '@/modules/SalesRequests/types'
import { useDelegateBranchId } from '../composables/useDelegateSummary'
import {
  decisionErrorMessage,
  EXCEPTION_STATUS_LABELS,
  useExceptionDecisionMutation,
  useExceptionInboxQuery,
  useMarkExceptionReadMutation,
  type ExceptionInboxRow,
  type ExceptionInboxStatus,
} from '../requests/exceptionInbox'

const STATUSES: ExceptionInboxStatus[] = ['Pending', 'Approved', 'Rejected', 'Cancelled']
const PROFILE_TABS = [
  { key: 'payments', label: 'التسديدات' },
  { key: 'sales', label: 'المبيعات' },
  { key: 'accounts', label: 'الحسابات' },
] as const
const CRITERIA: { key: MatchCriterion; label: string }[] = [
  { key: 'phone', label: 'مطابق بالهاتف' },
  { key: 'tripleName', label: 'مطابق بالاسم الثلاثي' },
  { key: 'fatherGrandfather', label: 'مطابق باسم الأب والجد' },
  { key: 'sonFather', label: 'مطابق باسم الابن والأب' },
]

const route = useRoute()
const router = useRouter()
const toast = useToast()
const auth = useAuthStore()
const canDecide = computed(() => auth.hasPermission('SalesRequest', 'ExceptionDecide') === true)

const status = ref<ExceptionInboxStatus>(STATUSES.includes(route.query.status as ExceptionInboxStatus) ? (route.query.status as ExceptionInboxStatus) : 'Pending')
const search = ref('')
const debounced = ref('')
const page = ref(1)
const branchId = useDelegateBranchId()
let timer: ReturnType<typeof setTimeout> | undefined
watch(search, (v) => {
  clearTimeout(timer)
  timer = setTimeout(() => {
    debounced.value = v
    page.value = 1
  }, 300)
})
watch(branchId, () => {
  page.value = 1
})

const params = computed(() => ({ status: status.value, branchId: branchId.value, search: debounced.value, page: page.value }))
const inbox = useExceptionInboxQuery(params)
const rows = computed(() => inbox.data.value?.items ?? [])
const pages = computed(() => Math.max(1, Math.ceil((inbox.data.value?.totalCount ?? 0) / 20)))

function countOf(s: ExceptionInboxStatus) {
  const c = inbox.data.value?.counts
  if (!c) return '…'
  return { Pending: c.pending, Approved: c.approved, Rejected: c.rejected, Cancelled: c.cancelled }[s]
}

function setStatus(s: ExceptionInboxStatus) {
  if (s === status.value) return
  status.value = s
  page.value = 1
  selectedId.value = null
  void router.replace({ query: { ...route.query, status: s } })
}

const selectedId = ref<number | null>(null)
const selected = computed<ExceptionInboxRow | null>(() => rows.value.find((r) => r.id === selectedId.value) ?? null)
const markRead = useMarkExceptionReadMutation()

function select(r: ExceptionInboxRow) {
  selectedId.value = r.id
  note.value = ''
  decisionError.value = null
  profileTab.value = 'payments'
  if (!r.isRead) markRead.mutate(r.id)
}

watch(rows, (list) => {
  if (selectedId.value != null && list.some((r) => r.id === selectedId.value)) return
  selectedId.value = null
})

const detail = useSalesRequestByIdQuery(selectedId, computed(() => selectedId.value != null))
const request = computed(() => (detail.data.value && detail.data.value.id === selectedId.value ? detail.data.value : null))

const customerId = computed(() => selected.value?.existingCustomerId ?? 0)
const profile = useCustomerAuditProfileQuery(customerId, ref('overview,accounts,sales,ratings'), computed(() => customerId.value > 0))
const profileData = computed(() => profile.data.value ?? null)
const overview = computed(() => profileData.value?.overview ?? null)
const accounts = computed(() => profileData.value?.accounts?.rows ?? [])
const payments = computed(() =>
  accounts.value.flatMap((a) => a.payments).sort((x, y) => (x.date < y.date ? 1 : -1))
)
const sales = computed(() => profileData.value?.sales ?? [])
const ratings = computed(() => {
  const r = profileData.value?.ratingsAndLocation
  const list = [r?.delegateRating, r?.committeeFieldRating].filter((x): x is NonNullable<typeof x> => x != null)
  return list.length ? list : null
})
const profileTab = ref<'payments' | 'sales' | 'accounts'>('payments')

const matchQueries = useQueries({
  queries: computed(() =>
    CRITERIA.map((c) => ({
      queryKey: [MATCH_HITS_KEY, selectedId.value, c.key, 0, 5],
      queryFn: () => fetchMatchHits(selectedId.value!, c.key, 0, 5),
      enabled: selectedId.value != null,
    }))
  ),
})
const matchesLoading = computed(() => matchQueries.value.some((q) => q.isPending && q.fetchStatus !== 'idle'))
const matches = computed(() => {
  const seen = new Set<number>()
  const out: { criterion: MatchCriterion; label: string; hit: MatchHit }[] = []
  matchQueries.value.forEach((q, i) => {
    for (const hit of q.data?.items ?? []) {
      if (seen.has(hit.customerId) || hit.customerId === selected.value?.existingCustomerId) continue
      seen.add(hit.customerId)
      out.push({ criterion: CRITERIA[i].key, label: CRITERIA[i].label, hit })
    }
  })
  return out
})

const note = ref('')
const confirming = ref<boolean | null>(null)
const decisionError = ref<string | null>(null)
const decide = useExceptionDecisionMutation()

function ask(approve: boolean) {
  decisionError.value = null
  confirming.value = approve
}

async function submit() {
  const approve = confirming.value
  if (approve == null || !selected.value) return
  try {
    await decide.mutateAsync({ id: selected.value.id, approve, reason: note.value.trim() })
    toast.add({ severity: 'success', summary: approve ? 'تمت الموافقة على طلب الاستثناء' : 'تم رفض طلب الاستثناء', life: 3000 })
    confirming.value = null
    note.value = ''
  } catch (e) {
    decisionError.value = decisionErrorMessage(e)
    confirming.value = null
    void inbox.refetch()
  }
}

const moneyFmt = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 })
function money(v: number | null | undefined) {
  return v == null ? '—' : `${moneyFmt.format(v)} د.ع`
}
function date(v: string | null | undefined) {
  if (!v) return '—'
  const d = new Date(v)
  return Number.isNaN(d.getTime()) ? v : d.toLocaleDateString('en-GB')
}
function dateTime(v: string | null | undefined) {
  if (!v) return '—'
  const d = new Date(v)
  return Number.isNaN(d.getTime()) ? v : `${d.toLocaleDateString('en-GB')} ${d.toLocaleTimeString('ar-IQ', { hour: 'numeric', minute: '2-digit' })}`
}
function relative(v: string | null | undefined) {
  if (!v) return '—'
  const t = new Date(v).getTime()
  if (Number.isNaN(t)) return '—'
  const mins = Math.floor((Date.now() - t) / 60000)
  if (mins < 1) return 'الآن'
  if (mins < 60) return `قبل ${mins} دقيقة`
  const hours = Math.floor(mins / 60)
  if (hours < 24) return `قبل ${hours} ساعة`
  const days = Math.floor(hours / 24)
  if (days < 30) return `قبل ${days} يوم`
  return date(v)
}
</script>

<style scoped>
.dxp { display: flex; flex-direction: column; gap: 16px; color: var(--dfb-ink); }
.dxp-toolbar { align-items: center; display: flex; flex-wrap: wrap; gap: 12px 16px; }
.dxp-titles { flex: 1 1 220px; }
.dxp-chips { display: flex; flex-wrap: wrap; gap: 8px; }
.dxp-chip { align-items: center; background: var(--dfb-raised); border: 0; border-radius: 999px; box-shadow: var(--dfb-up); color: var(--dfb-muted); cursor: pointer; display: inline-flex; font: inherit; font-size: 14px; gap: 6px; padding: 7px 14px; }
.dxp-chip[aria-pressed='true'] { background: var(--dfb-well); box-shadow: var(--dfb-down); color: var(--dfb-primary); font-weight: 700; }
.dxp-chip:focus-visible, .dxp-row:focus-visible { outline: 3px solid #71b4a9; outline-offset: 2px; }
.dxp-chip-count { font-variant-numeric: tabular-nums; }
.dxp-search { flex: 0 1 300px; }
.dxp-split { align-items: start; display: grid; gap: 20px; grid-template-columns: minmax(280px, 380px) minmax(0, 1fr); }
.dxp-list { display: flex; flex-direction: column; gap: 10px; }
.dxp-row { background: var(--dfb-raised); border: 0; border-radius: 16px; box-shadow: var(--dfb-up); color: inherit; cursor: pointer; display: flex; flex-direction: column; font: inherit; gap: 3px; padding: 12px 14px; text-align: start; }
.dxp-row.is-active { background: var(--dfb-well); box-shadow: var(--dfb-down); }
.dxp-row.is-active .dxp-row-name { color: var(--dfb-primary); }
.dxp-row-top { align-items: center; display: flex; gap: 8px; justify-content: space-between; }
.dxp-row-name { font-size: 15px; }
.dxp-row.is-unread .dxp-row-name { font-weight: 800; }
.dxp-row-line { font-size: 13px; }
.dxp-row-meta { color: var(--dfb-muted); font-size: 12px; }
.dxp-tag { background: var(--dfb-well); border-radius: 999px; color: var(--dfb-muted); font-size: 12px; padding: 1px 10px; }
.dxp-tag.is-new { background: var(--dfb-primary-soft); color: var(--dfb-primary); font-weight: 700; }
.dxp-pager { align-items: center; display: flex; gap: 10px; justify-content: center; font-size: 13px; }
.dxp-detail { background: var(--dfb-well); border-radius: 20px; box-shadow: var(--dfb-down); display: flex; flex-direction: column; gap: 14px; min-inline-size: 0; padding: 18px; }
.dxp-detail-head { align-items: flex-start; display: flex; gap: 12px; justify-content: space-between; }
.dxp-name { font-size: 20px; margin: 0; }
.dxp-status { border-radius: 999px; font-size: 13px; font-weight: 700; padding: 4px 12px; white-space: nowrap; }
.dxp-status.is-pending { background: var(--dfb-warn-soft); color: var(--dfb-tier-weak); }
.dxp-status.is-approved { background: var(--dfb-good-soft); color: var(--dfb-tier-excellent); }
.dxp-status.is-rejected { background: var(--dfb-danger-soft); color: var(--dfb-tier-bad); }
.dxp-status.is-cancelled { background: #ececec; color: var(--dfb-tier-lagging); }
.dxp-banner { background: var(--dfb-primary-soft); border-radius: 14px; display: flex; flex-direction: column; gap: 2px; padding: 12px 16px; }
.dxp-banner strong { color: var(--dfb-primary); }
.dxp-banner.is-new { background: var(--dfb-warn-soft); }
.dxp-banner.is-new strong { color: var(--dfb-tier-weak); }
.dxp-grid { display: grid; gap: 14px; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); }
.dxp-card { background: var(--dfb-raised); border-radius: 16px; box-shadow: 3px 3px 8px var(--dfb-shade), -3px -3px 8px var(--dfb-light); display: flex; flex-direction: column; gap: 10px; padding: 14px 16px; }
.dxp-card h4 { font-size: 15px; margin: 0; }
.dxp-kv { display: flex; flex-direction: column; gap: 6px; margin: 0; }
.dxp-kv div { display: flex; gap: 12px; justify-content: space-between; }
.dxp-kv dt { color: var(--dfb-muted); font-size: 13px; }
.dxp-kv dd { font-size: 14px; font-weight: 500; margin: 0; text-align: end; }
.dxp-text { font-size: 14px; line-height: 1.8; margin: 0; white-space: pre-wrap; }
.dxp-stats { display: grid; gap: 10px; grid-template-columns: repeat(4, minmax(0, 1fr)); }
.dxp-stats div { background: var(--dfb-well); border-radius: 12px; display: flex; flex-direction: column; gap: 2px; padding: 10px 12px; }
.dxp-stats strong { font-size: 17px; font-variant-numeric: tabular-nums; }
.dxp-stats span { color: var(--dfb-muted); font-size: 12px; }
.is-bad { color: var(--dfb-tier-bad); }
.dxp-tabs { display: flex; flex-wrap: wrap; gap: 8px; }
.dxp-table { border-collapse: collapse; font-size: 13px; width: 100%; }
.dxp-table th { color: var(--dfb-muted); font-weight: 600; padding: 6px 8px; text-align: start; }
.dxp-table td { border-top: 1px solid #dce7e4; font-variant-numeric: tabular-nums; padding: 6px 8px; }
.dxp-ratings { display: grid; gap: 10px; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); }
.dxp-rating { background: var(--dfb-well); border-radius: 12px; display: flex; flex-direction: column; gap: 2px; font-size: 13px; padding: 10px 12px; }
.dxp-matches { display: flex; flex-direction: column; gap: 8px; list-style: none; margin: 0; padding: 0; }
.dxp-matches li { background: var(--dfb-well); border-radius: 12px; display: flex; flex-direction: column; gap: 2px; font-size: 13px; padding: 8px 12px; }
.dxp-matches li span { color: var(--dfb-muted); }
.dxp-decision { box-shadow: var(--dfb-up); }
.dxp-field { display: flex; flex-direction: column; gap: 6px; font-size: 13px; }
.dxp-field textarea { resize: vertical; width: 100%; }
.dxp-actions { display: flex; gap: 10px; justify-content: space-between; }
.dxp-danger { background: var(--dfb-danger-soft); color: var(--dfb-tier-bad); }
@media (max-width: 900px) {
  .dxp-split { grid-template-columns: minmax(0, 1fr); }
  .dxp-search { flex: 1 1 100%; }
  .dxp-stats { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
