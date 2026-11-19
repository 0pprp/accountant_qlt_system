<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { isAxiosError } from 'axios'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { formatDate, formatDateTime, formatMoney } from '@/modules/Core/utils/formatDisplay'
import { DELEGATE_SUMMARY_KEY, useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'
import { formatWaiverAmountInput, parseWaiverAmount } from '@/modules/Waivers/utils/waiverAmount'

type WaiverStatus = 'Pending' | 'AwaitingAccountant' | 'Approved' | 'Rejected'

interface Row {
  id: number
  customerId: number
  customerName: string
  customerPhone: string | null
  branchId: number
  branchName: string | null
  status: WaiverStatus
  requesterName: string | null
  approvedAmount: number | null
  createdAtUtc: string
  expectedVersion: number
}

interface Counts {
  pending: number
  awaitingAccountant: number
  approved: number
  rejected: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
  page: number
  pageSize: number
  counts: Counts | null
}

interface RelatedOrder {
  orderId: number
  saleDate: string | null
  sellAmount: number
  paidTotal: number
  remaining: number
  approvalStatus: string
  isWaiverOrder: boolean
}

interface RelatedCustomer {
  customerId: number
  fullName: string
  phone: string | null
  matchReasons: string[]
  sellTotal: number
  paidTotal: number
  remainingRounded: number
}

interface Detail {
  id: number
  customerId: number
  customerName: string
  customerPhone: string | null
  branchId: number
  branchName: string | null
  orderId: number | null
  orderSellAmount: number | null
  orderPaidTotal: number | null
  orderRemaining: number | null
  status: WaiverStatus
  requestNote: string
  requesterName: string | null
  decisionNote: string | null
  decidedByName: string | null
  approvedAmount: number | null
  appliedAmount: number | null
  applied: boolean
  appliedByName: string | null
  createdAtUtc: string
  decidedAtUtc: string | null
  appliedAtUtc: string | null
  expectedVersion: number
  account: {
    customerSellTotal: number
    customerPaidTotal: number
    customerRemaining: number
    customerRemainingRounded: number
  } | null
  relatedOrders: RelatedOrder[] | null
  relatedCustomers: RelatedCustomer[] | null
}

const PAGE_SIZE = 20
const NOTE_MAX = 500
const CONFLICT_MESSAGE = 'تم اتخاذ قرار مسبقاً على هذا الطلب'

const STATUS_CHIPS: { value: WaiverStatus; label: string; key: keyof Counts }[] = [
  { value: 'Pending', label: 'قيد الانتظار', key: 'pending' },
  { value: 'AwaitingAccountant', label: 'بانتظار المحاسب', key: 'awaitingAccountant' },
  { value: 'Approved', label: 'مطبّق', key: 'approved' },
  { value: 'Rejected', label: 'مرفوض', key: 'rejected' },
]

const STATUS_LABEL: Record<WaiverStatus, string> = {
  Pending: 'قيد الانتظار',
  AwaitingAccountant: 'بانتظار المحاسب',
  Approved: 'مطبّق',
  Rejected: 'مرفوض',
}

const STATUS_TONE: Record<WaiverStatus, string> = {
  Pending: 'is-open',
  AwaitingAccountant: 'is-await',
  Approved: 'is-sold',
  Rejected: 'is-rejected',
}

const ORDER_STATUS_LABEL: Record<string, string> = {
  Approved: 'معتمد',
  Pending: 'قيد الموافقة',
  Rejected: 'مرفوض',
}

const auth = useAuthStore()
const route = useRoute()
const qc = useQueryClient()
const branchId = useDelegateBranchId()
const canRead = computed(() => auth.hasPermission('Waiver', 'Read') === true)
const canDecide = computed(() => auth.hasPermission('Waiver', 'Decide') === true)

const status = ref<WaiverStatus>('Pending')
const page = ref(1)
const searchInput = ref('')
const search = ref('')
const selectedId = ref<number | null>(null)
const amountInput = ref('')
const decisionNote = ref('')
const formError = ref<string | null>(null)
const actionError = ref<string | null>(null)
const notice = ref<string | null>(null)
const confirm = ref<'approve' | 'reject' | null>(null)
let searchTimer: ReturnType<typeof setTimeout> | undefined

watch(searchInput, (value) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    search.value = value.trim()
    page.value = 1
  }, 300)
})

onBeforeUnmount(() => clearTimeout(searchTimer))

watch(branchId, () => {
  page.value = 1
  selectedId.value = null
  notice.value = null
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'waivers',
    'delegate',
    auth.sessionGeneration,
    branchId.value,
    status.value,
    search.value,
    page.value,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  placeholderData: keepPreviousData,
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/Waivers', {
      signal,
      params: {
        page: page.value,
        pageSize: PAGE_SIZE,
        status: status.value,
        search: search.value || undefined,
        branchId: branchId.value ?? undefined,
      },
    })
    return data
  },
})

const items = computed(() => listQuery.data.value?.items ?? [])
const totalCount = computed(() => listQuery.data.value?.totalCount ?? 0)
const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / PAGE_SIZE)))
const counts = computed(() => listQuery.data.value?.counts ?? null)

const detailQuery = useQuery({
  queryKey: computed(() => ['waivers', 'delegate-detail', auth.sessionGeneration, selectedId.value]),
  enabled: computed(() => canRead.value && auth.permissionsReady && selectedId.value != null),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<Detail>(`/admin/Waivers/${selectedId.value}`, { signal })
    return data
  },
})

const detail = computed(() => (detailQuery.data.value?.id === selectedId.value ? detailQuery.data.value : null))
const isPending = computed(() => detail.value?.status === 'Pending')
const parsedAmount = computed(() => parseWaiverAmount(amountInput.value))

const accountRows = computed(() => {
  const d = detail.value
  if (!d) return []
  const rows: { label: string; value: number | null; strong?: boolean }[] = []
  if (d.orderId != null) {
    rows.push({ label: 'سعر البيع', value: d.orderSellAmount })
    rows.push({ label: 'الواصل', value: d.orderPaidTotal })
    rows.push({ label: 'باقي الحساب', value: d.orderRemaining })
  } else if (d.account) {
    rows.push({ label: 'سعر البيع', value: d.account.customerSellTotal })
    rows.push({ label: 'الواصل', value: d.account.customerPaidTotal })
  }
  if (d.account) rows.push({ label: 'باقي الزبون', value: d.account.customerRemainingRounded, strong: true })
  return rows
})

function selectStatus(next: WaiverStatus) {
  if (status.value === next) return
  status.value = next
  page.value = 1
  selectedId.value = null
}

function open(row: Row) {
  selectedId.value = row.id
  amountInput.value = ''
  decisionNote.value = ''
  formError.value = null
  actionError.value = null
  notice.value = null
}

function closeDetail() {
  selectedId.value = null
  actionError.value = null
}

function onAmountInput(event: Event) {
  const el = event.target as HTMLInputElement
  const formatted = formatWaiverAmountInput(el.value)
  amountInput.value = formatted
  el.value = formatted
}

function rowMeta(row: Row): string {
  return [row.requesterName || 'مدير الفرع غير محدد', row.branchName || 'فرع غير محدد', formatDate(row.createdAtUtc)].join(' · ')
}

function errorCode(error: unknown): string | undefined {
  if (!isAxiosError(error)) return undefined
  const errors = (error.response?.data as { errors?: { code?: string }[] } | undefined)?.errors
  return Array.isArray(errors) ? errors[0]?.code : undefined
}

function errorMessage(error: unknown, fallback: string): string {
  if (!isAxiosError(error)) return fallback
  const errors = (error.response?.data as { errors?: { message?: string }[] } | undefined)?.errors
  const msg = Array.isArray(errors) ? errors[0]?.message : undefined
  return msg || fallback
}

function isConflict(error: unknown): boolean {
  if (!isAxiosError(error)) return false
  if (error.response?.status === 409) return true
  const code = errorCode(error)
  return code === 'Waiver.Concurrency' || code === 'Waiver.InvalidTransition'
}

async function refreshAll() {
  await Promise.all([
    qc.invalidateQueries({ queryKey: ['waivers'] }),
    qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] }),
  ])
}

const decideMut = useMutation({
  mutationFn: async (kind: 'approve' | 'reject') => {
    const d = detail.value!
    const note = decisionNote.value.trim() || undefined
    if (kind === 'approve') {
      await axiosInstance.post(`/admin/Waivers/${d.id}/approve`, {
        approvedAmount: parsedAmount.value,
        expectedVersion: d.expectedVersion,
        note,
      })
    } else {
      await axiosInstance.post(`/admin/Waivers/${d.id}/reject`, {
        expectedVersion: d.expectedVersion,
        note,
      })
    }
    return kind
  },
  onSuccess: async (kind) => {
    confirm.value = null
    actionError.value = null
    amountInput.value = ''
    decisionNote.value = ''
    notice.value = kind === 'approve' ? 'أُرسل الطلب إلى المحاسب الرئيسي' : 'تم رفض طلب التنازل'
    await refreshAll()
  },
  onError: async (error) => {
    confirm.value = null
    if (isConflict(error)) {
      actionError.value = CONFLICT_MESSAGE
      await refreshAll()
      return
    }
    actionError.value = errorMessage(error, 'تعذر حفظ القرار. حاول مرة أخرى.')
  },
})

function askApprove() {
  formError.value = null
  if (parsedAmount.value == null) {
    formError.value = 'أدخل مبلغ التنازل بشكل صحيح'
    return
  }
  if (decisionNote.value.trim().length > NOTE_MAX) {
    formError.value = `الحد الأقصى لملاحظة القرار ${NOTE_MAX} حرف`
    return
  }
  confirm.value = 'approve'
}

function askReject() {
  formError.value = null
  if (decisionNote.value.trim().length > NOTE_MAX) {
    formError.value = `الحد الأقصى لملاحظة القرار ${NOTE_MAX} حرف`
    return
  }
  confirm.value = 'reject'
}

const confirmText = computed(() =>
  confirm.value === 'approve'
    ? `سيُرسل الطلب بمبلغ ${formatMoney(parsedAmount.value)} إلى المحاسب الرئيسي، ولا يُخصم من حساب الزبون إلا بعد أن يطبّقه المحاسب.`
    : 'سيتم رفض الطلب وإرجاعه لمدير الفرع.',
)

function prevPage() {
  if (page.value > 1) page.value -= 1
}

function nextPage() {
  if (page.value < pageCount.value) page.value += 1
}
</script>

<template>
  <section class="dwv" dir="rtl">
    <header class="dwv-head">
      <h2 class="dfb-title">طلبات التنازل</h2>
      <p class="dfb-subtitle">طلبات مدراء الفروع ضمن نطاقك. الموافقة ترسل الطلب للمحاسب الرئيسي، والخصم لا يتم إلا بعد تطبيقه.</p>
    </header>

    <p v-if="!canRead" class="dfb-state" role="alert">ليس لديك صلاحية قراءة طلبات التنازل.</p>

    <template v-else>
      <div class="dwv-toolbar">
        <div class="dfb-filter-chips" role="group" aria-label="حالة الطلب">
          <button
            v-for="chip in STATUS_CHIPS"
            :key="chip.value"
            type="button"
            class="dfb-chip"
            :aria-pressed="status === chip.value"
            @click="selectStatus(chip.value)"
          >
            {{ chip.label }}
            <strong class="dfb-num">{{ counts ? counts[chip.key] : '…' }}</strong>
          </button>
        </div>
        <input
          v-model="searchInput"
          type="search"
          class="dfb-search"
          placeholder="بحث باسم الزبون"
          aria-label="بحث باسم الزبون"
          maxlength="100"
        >
      </div>

      <p v-if="notice" class="dwv-notice" role="status">{{ notice }}</p>
      <p v-if="actionError && !detail" class="dfb-error" role="alert">{{ actionError }}</p>

      <div class="dwv-layout" :class="{ 'has-detail': selectedId != null }">
        <div class="dwv-list-col">
          <div v-if="listQuery.isError.value" class="dfb-error" role="alert">
            تعذر تحميل طلبات التنازل.
            <button type="button" class="dfb-button is-small" @click="listQuery.refetch()">إعادة المحاولة</button>
          </div>
          <p v-else-if="listQuery.isPending.value" class="dfb-state">جاري التحميل…</p>
          <p v-else-if="items.length === 0" class="dfb-state">لا توجد طلبات تنازل بهذه الحالة.</p>
          <template v-else>
            <ul class="dwv-list">
              <li v-for="row in items" :key="row.id">
                <button
                  type="button"
                  class="dwv-row"
                  :class="{ 'is-active': selectedId === row.id }"
                  :aria-current="selectedId === row.id ? 'true' : undefined"
                  @click="open(row)"
                >
                  <span class="dwv-row-top">
                    <span class="dwv-row-name">{{ row.customerName }}</span>
                    <span class="dfb-status" :class="STATUS_TONE[row.status]">{{ STATUS_LABEL[row.status] }}</span>
                  </span>
                  <span class="dwv-row-sub">{{ rowMeta(row) }}</span>
                </button>
              </li>
            </ul>
            <div v-if="pageCount > 1" class="dwv-pager">
              <button type="button" class="dfb-button is-small" :disabled="page <= 1" @click="prevPage">السابق</button>
              <span class="dwv-pager-meta">
                صفحة <span class="dfb-num">{{ page }}</span> من <span class="dfb-num">{{ pageCount }}</span>
                · <span class="dfb-num">{{ totalCount }}</span> طلب
              </span>
              <button type="button" class="dfb-button is-small" :disabled="page >= pageCount" @click="nextPage">التالي</button>
            </div>
          </template>
        </div>

        <aside v-if="selectedId != null" class="dwv-detail" aria-label="تفاصيل طلب التنازل">
          <div v-if="detailQuery.isError.value && !detail" class="dfb-error" role="alert">
            تعذر تحميل الطلب.
            <button type="button" class="dfb-button is-small" @click="detailQuery.refetch()">إعادة المحاولة</button>
          </div>
          <p v-else-if="!detail" class="dfb-state">جاري التحميل…</p>
          <template v-else>
            <div class="dwv-detail-head">
              <h3 class="dwv-detail-title">{{ detail.customerName }}</h3>
              <span class="dfb-status" :class="STATUS_TONE[detail.status]">{{ STATUS_LABEL[detail.status] }}</span>
            </div>

            <div class="dwv-note">
              <h4 class="dwv-note-title">ملاحظة مدير الفرع</h4>
              <p class="dwv-note-body">{{ detail.requestNote || '-' }}</p>
            </div>

            <div class="dwv-card">
              <h4 class="dwv-card-title">بيانات الطلب</h4>
              <dl class="dwv-facts">
                <div><dt>الزبون</dt><dd>{{ detail.customerName }}</dd></div>
                <div><dt>الهاتف</dt><dd class="dfb-num">{{ detail.customerPhone || '-' }}</dd></div>
                <div><dt>الفرع</dt><dd>{{ detail.branchName || '-' }}</dd></div>
                <div><dt>مدير الفرع</dt><dd>{{ detail.requesterName || '-' }}</dd></div>
                <div><dt>الحالة</dt><dd>{{ STATUS_LABEL[detail.status] }}</dd></div>
                <div><dt>تاريخ الطلب</dt><dd>{{ formatDateTime(detail.createdAtUtc) }}</dd></div>
                <div v-if="detail.approvedAmount != null"><dt>المبلغ المعتمد</dt><dd>{{ formatMoney(detail.approvedAmount) }}</dd></div>
              </dl>
            </div>

            <div class="dwv-card">
              <h4 class="dwv-card-title">الحساب</h4>
              <dl class="dwv-facts">
                <div v-for="r in accountRows" :key="r.label" :class="{ 'is-strong': r.strong }">
                  <dt>{{ r.label }}</dt>
                  <dd>{{ formatMoney(r.value) }}</dd>
                </div>
              </dl>
            </div>

            <div class="dwv-card">
              <h4 class="dwv-card-title">الحسابات والأسماء المرتبطة</h4>
              <p v-if="!(detail.relatedOrders?.length) && !(detail.relatedCustomers?.length)" class="dfb-state">لا توجد حسابات أو أسماء مرتبطة.</p>
              <template v-if="detail.relatedOrders?.length">
                <p class="dwv-sub-title">حسابات الزبون</p>
                <ul class="dwv-related">
                  <li v-for="o in detail.relatedOrders" :key="o.orderId" :class="{ 'is-current': o.isWaiverOrder }">
                    <span>{{ o.saleDate ? formatDate(o.saleDate) : 'بدون تاريخ' }}</span>
                    <span>{{ formatMoney(o.sellAmount) }}</span>
                    <span>الباقي {{ formatMoney(o.remaining) }}</span>
                    <span class="dwv-related-tag">{{ ORDER_STATUS_LABEL[o.approvalStatus] ?? o.approvalStatus }}{{ o.isWaiverOrder ? ' · حساب الطلب' : '' }}</span>
                  </li>
                </ul>
              </template>
              <template v-if="detail.relatedCustomers?.length">
                <p class="dwv-sub-title">أسماء مطابقة في الفرع</p>
                <ul class="dwv-related">
                  <li v-for="m in detail.relatedCustomers" :key="m.customerId">
                    <strong>{{ m.fullName }}</strong>
                    <span class="dfb-num">{{ m.phone || '-' }}</span>
                    <span>الباقي {{ formatMoney(m.remainingRounded) }}</span>
                    <span class="dwv-related-tag">{{ m.matchReasons.join(' • ') }}</span>
                  </li>
                </ul>
              </template>
            </div>

            <div v-if="detail.status !== 'Pending'" class="dwv-card">
              <h4 class="dwv-card-title">القرار</h4>
              <dl class="dwv-facts">
                <div><dt>صاحب القرار</dt><dd>{{ detail.decidedByName || '-' }}</dd></div>
                <div v-if="detail.decidedAtUtc"><dt>تاريخ القرار</dt><dd>{{ formatDateTime(detail.decidedAtUtc) }}</dd></div>
                <div class="is-wide"><dt>ملاحظة القرار</dt><dd>{{ detail.decisionNote || '-' }}</dd></div>
                <div v-if="detail.approvedAmount != null"><dt>المبلغ المعتمد</dt><dd>{{ formatMoney(detail.approvedAmount) }}</dd></div>
                <template v-if="detail.applied">
                  <div><dt>المبلغ المطبّق</dt><dd>{{ formatMoney(detail.appliedAmount) }}</dd></div>
                  <div><dt>طبّقه</dt><dd>{{ detail.appliedByName || '-' }}</dd></div>
                  <div v-if="detail.appliedAtUtc"><dt>تاريخ التطبيق</dt><dd>{{ formatDateTime(detail.appliedAtUtc) }}</dd></div>
                </template>
                <div v-else-if="detail.status === 'AwaitingAccountant'" class="is-wide">
                  <dt>التطبيق</dt><dd>لم يطبّقه المحاسب بعد</dd>
                </div>
              </dl>
            </div>

            <div v-if="isPending && canDecide" class="dwv-card">
              <h4 class="dwv-card-title">القرار</h4>
              <label class="dfb-field">
                <span>مبلغ التنازل (إجباري عند الموافقة)</span>
                <input
                  :value="amountInput"
                  type="text"
                  inputmode="numeric"
                  dir="ltr"
                  class="dfb-input dwv-amount"
                  placeholder="مثال: 1,500,000"
                  autocomplete="off"
                  @input="onAmountInput"
                >
              </label>
              <label class="dfb-field">
                <span>ملاحظة القرار (اختياري)</span>
                <textarea v-model="decisionNote" class="dfb-textarea" :maxlength="NOTE_MAX" rows="3" />
                <small class="dwv-counter"><span class="dfb-num">{{ decisionNote.length }}</span> / <span class="dfb-num">{{ NOTE_MAX }}</span></small>
              </label>
              <p v-if="formError" class="dfb-form-error" role="alert">{{ formError }}</p>
              <div class="dwv-actions">
                <button type="button" class="dfb-button is-primary" :disabled="decideMut.isPending.value" @click="askApprove">
                  موافقة وإرسال للمحاسب
                </button>
                <button type="button" class="dfb-button dwv-danger" :disabled="decideMut.isPending.value" @click="askReject">
                  رفض وإرجاع لمدير الفرع
                </button>
              </div>
            </div>

            <p v-if="actionError" class="dfb-error" role="alert">{{ actionError }}</p>

            <div class="dwv-actions">
              <button type="button" class="dfb-button is-small" @click="closeDetail">إغلاق</button>
            </div>
          </template>
        </aside>
      </div>
    </template>

    <NeuDialog
      :visible="confirm != null"
      :title="confirm === 'approve' ? 'الموافقة على التنازل' : 'رفض طلب التنازل'"
      size="sm"
      :busy="decideMut.isPending.value"
      @close="confirm = null"
    >
      <p class="dwv-confirm">{{ confirmText }}</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="decideMut.isPending.value" @click="confirm = null">إلغاء</button>
        <button
          type="button"
          class="dfb-button is-primary"
          :disabled="decideMut.isPending.value"
          @click="confirm && decideMut.mutate(confirm)"
        >
          {{ decideMut.isPending.value ? 'جاري الحفظ…' : confirm === 'approve' ? 'موافقة' : 'رفض' }}
        </button>
      </template>
    </NeuDialog>
  </section>
</template>

<style scoped>
.dwv {
  display: flex;
  flex-direction: column;
  gap: 18px;
  min-width: 0;
}

.dwv-head { display: flex; flex-direction: column; gap: 4px; }

.dwv-toolbar {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.dwv-notice {
  background: var(--dfb-primary-soft);
  border-radius: 12px;
  color: var(--dfb-primary);
  font-weight: 600;
  margin: 0;
  padding: 10px 14px;
}

.dwv-layout {
  align-items: start;
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 1fr);
}

.dwv-layout.has-detail {
  grid-template-columns: minmax(300px, 380px) minmax(0, 1fr);
}

.dwv-list-col {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: 0;
}

.dwv-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dwv-row {
  background: var(--dfb-raised);
  border: 0;
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  color: inherit;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  font: inherit;
  gap: 4px;
  padding: 12px 16px;
  text-align: start;
  width: 100%;
}

.dwv-row:hover { box-shadow: var(--dfb-up-hover); }
.dwv-row.is-active { box-shadow: var(--dfb-down); }

.dwv-row-top {
  align-items: center;
  display: flex;
  gap: 8px;
  justify-content: space-between;
}

.dwv-row-name {
  font-weight: 700;
  line-height: 1.4;
  overflow-wrap: anywhere;
}

.dwv-row-sub {
  color: var(--dfb-muted);
  font-size: 13px;
  line-height: 1.5;
}

.dfb-status.is-await { background: #e2ecf7; color: var(--dfb-tier-good); }

.dwv-pager {
  align-items: center;
  display: flex;
  gap: 10px;
  justify-content: space-between;
}

.dwv-pager-meta {
  color: var(--dfb-muted);
  font-size: 13px;
}

.dwv-detail {
  background: var(--dfb-well);
  border-radius: 20px;
  box-shadow: var(--dfb-down);
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
  padding: 18px;
}

.dwv-detail-head {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.dwv-detail-title {
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
  margin: 0;
  overflow-wrap: anywhere;
}

.dwv-note {
  background: var(--dfb-warn-soft);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  padding: 14px 16px;
}

.dwv-note-title {
  color: var(--dfb-tier-weak);
  font-size: 13px;
  font-weight: 700;
  margin: 0 0 6px;
}

.dwv-note-body {
  line-height: 1.8;
  margin: 0;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}

.dwv-card {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 14px 16px;
}

.dwv-card-title {
  font-size: 15px;
  font-weight: 700;
  margin: 0;
}

.dwv-sub-title {
  color: var(--dfb-muted);
  font-size: 13px;
  font-weight: 700;
  margin: 0;
}

.dwv-facts {
  display: grid;
  gap: 10px 18px;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 160px), 1fr));
  margin: 0;
}

.dwv-facts dt {
  color: var(--dfb-muted);
  font-size: 12px;
  font-weight: 600;
}

.dwv-facts dd {
  font-weight: 600;
  margin: 2px 0 0;
  overflow-wrap: anywhere;
}

.dwv-facts .is-wide { grid-column: 1 / -1; }
.dwv-facts .is-strong dd { color: var(--dfb-primary); font-size: 17px; }

.dwv-related {
  display: flex;
  flex-direction: column;
  gap: 8px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dwv-related li {
  background: var(--dfb-well);
  border-radius: 12px;
  display: flex;
  flex-wrap: wrap;
  font-size: 13px;
  gap: 4px 14px;
  padding: 8px 12px;
}

.dwv-related li.is-current { background: var(--dfb-primary-soft); }

.dwv-related-tag { color: var(--dfb-muted); }

.dwv-amount {
  font-size: 18px;
  font-weight: 700;
  text-align: left;
}

.dwv-counter {
  align-self: flex-end;
  color: var(--dfb-muted);
  font-size: 12px;
}

.dwv-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.dwv-danger { color: var(--dfb-tier-bad); }

.dwv-confirm {
  line-height: 1.8;
  margin: 0;
}

@media (max-width: 900px) {
  .dwv-layout.has-detail {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
