<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { formatMoney, formatNumber } from '@/modules/Core/utils/formatDisplay'
import { DELEGATE_SUMMARY_KEY, useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'
import {
  WRONG_SALE_PAGE_SIZE,
  normalizeDelegateMailbox,
  type DelegateMailbox,
  type WrongSaleDetail,
  type WrongSaleListResponse,
  type WrongSaleRow,
  wrongSaleDateTime,
  wrongSaleDate,
} from '../domain/delegateWrongSales'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('WrongSale', 'Read') === true)
const canMarkRead = computed(() => auth.hasPermission('WrongSale', 'MarkDelegateRead') === true)

const branchId = useDelegateBranchId()
const mailbox = computed<DelegateMailbox>(() => normalizeDelegateMailbox(route.query.mailbox))
const page = ref(1)
const selectedId = ref<number | null>(null)
const markedIds = new Set<number>()

watch([mailbox, branchId], () => {
  page.value = 1
})

const listQuery = useQuery({
  queryKey: computed(() => ['wrong-sales', 'delegate', mailbox.value, branchId.value, page.value, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  placeholderData: keepPreviousData,
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<WrongSaleListResponse>('/admin/WrongSales', {
      signal,
      params: {
        page: page.value,
        pageSize: WRONG_SALE_PAGE_SIZE,
        mailbox: mailbox.value,
        branchId: branchId.value ?? undefined,
      },
    })
    return data
  },
})

const items = computed(() => listQuery.data.value?.items ?? [])
const totalCount = computed(() => listQuery.data.value?.totalCount ?? 0)
const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / WRONG_SALE_PAGE_SIZE)))

const chips = computed(() => {
  const d = listQuery.data.value
  return [
    { value: 'unread' as const, label: 'غير المقروءة', count: d?.unreadCount },
    { value: 'seen' as const, label: 'تمت رؤيتها', count: d?.seenCount },
    { value: 'reviewed' as const, label: 'تمت مراجعتها', count: d?.reviewedCount },
  ]
})

const detailQuery = useQuery({
  queryKey: computed(() => ['wrong-sale-detail', selectedId.value, auth.sessionGeneration]),
  enabled: computed(() => selectedId.value != null && canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<WrongSaleDetail>(`/admin/WrongSales/${selectedId.value}`, { signal })
    return data
  },
})

const detail = computed(() => (detailQuery.data.value?.notice.id === selectedId.value ? detailQuery.data.value : null))

const markReadMut = useMutation({
  mutationFn: async (row: { id: number; expectedVersion: number }) => {
    await axiosInstance.post(`/admin/WrongSales/${row.id}/mark-delegate-read`, {
      expectedVersion: row.expectedVersion,
    })
  },
  onSuccess: async () => {
    await Promise.all([
      qc.invalidateQueries({ queryKey: ['wrong-sales'] }),
      qc.invalidateQueries({ queryKey: ['wrong-sale-detail'] }),
      qc.invalidateQueries({ queryKey: [DELEGATE_SUMMARY_KEY] }),
    ])
  },
})

function setMailbox(value: DelegateMailbox) {
  selectedId.value = null
  void router.replace({ name: route.name ?? undefined, query: { ...route.query, mailbox: value } })
}

function open(row: WrongSaleRow) {
  selectedId.value = row.id
  if (canMarkRead.value && row.isDelegateUnread && markedIds.has(row.id) === false) {
    markedIds.add(row.id)
    markReadMut.mutate(
      { id: row.id, expectedVersion: row.expectedVersion },
      { onError: () => markedIds.delete(row.id) }
    )
  }
}

function goPage(next: number) {
  if (next < 1 || next > pageCount.value) return
  page.value = next
  selectedId.value = null
}

function dash(value: string | null | undefined) {
  const t = (value ?? '').trim()
  return t === '' ? '—' : t
}
</script>

<template>
  <section
    class="dws"
    dir="rtl"
  >
    <header class="dws-head">
      <div>
        <h2 class="dfb-title">المبيعات الخاطئة</h2>
        <p class="dfb-subtitle">إشعارات اطلاع من المتابع أو مندوب الجباية بتقييم 6 فأقل، الأحدث أولاً.</p>
      </div>
    </header>

    <p
      v-if="!canRead"
      class="dfb-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة المبيعات الخاطئة.
    </p>
    <template v-else>
      <div
        class="dfb-filter-chips"
        role="group"
        aria-label="تصفية المبيعات الخاطئة"
      >
        <button
          v-for="chip in chips"
          :key="chip.value"
          type="button"
          class="dfb-chip"
          :aria-pressed="mailbox === chip.value"
          @click="setMailbox(chip.value)"
        >
          {{ chip.label }}
          <strong
            v-if="chip.count != null"
            class="dfb-num"
          >{{ formatNumber(chip.count) }}</strong>
        </button>
      </div>

      <div
        class="dws-layout"
        :class="{ 'has-detail': selectedId != null }"
      >
        <div class="dws-list-col">
          <div
            v-if="listQuery.isError.value"
            class="dfb-error"
            role="alert"
          >
            تعذر تحميل المبيعات الخاطئة.
            <button
              type="button"
              class="dfb-button is-small"
              @click="listQuery.refetch()"
            >
              إعادة المحاولة
            </button>
          </div>
          <p
            v-else-if="listQuery.isPending.value"
            class="dfb-state"
          >
            جاري التحميل…
          </p>
          <p
            v-else-if="items.length === 0"
            class="dfb-state"
          >
            لا توجد مبيعات خاطئة في هذا التصنيف.
          </p>
          <template v-else>
            <ul class="dws-list">
              <li
                v-for="row in items"
                :key="row.id"
              >
                <button
                  type="button"
                  class="dws-row"
                  :class="{ 'is-unread': row.isDelegateUnread, 'is-active': selectedId === row.id }"
                  :aria-current="selectedId === row.id ? 'true' : undefined"
                  @click="open(row)"
                >
                  <span class="dws-row-top">
                    <span class="dws-row-name">{{ row.customerName || 'زبون' }}</span>
                    <span
                      class="dfb-status"
                      :class="row.isBranchUnreviewed ? (row.isDelegateUnread ? 'is-open' : '') : 'is-sold'"
                    >
                      {{ row.isBranchUnreviewed ? (row.isDelegateUnread ? 'غير مقروء' : 'تمت رؤيته') : 'تمت مراجعته' }}
                    </span>
                  </span>
                  <span class="dws-row-sub">
                    تقييم <span class="dfb-num">{{ formatNumber(row.rating) }}/10</span>
                    · {{ row.followerName || 'متابع' }}
                  </span>
                  <span class="dws-row-time dfb-num">{{ wrongSaleDateTime(row.createdAtUtc) }}</span>
                </button>
              </li>
            </ul>
            <nav
              v-if="pageCount > 1"
              class="dws-pager"
              aria-label="الصفحات"
            >
              <button
                type="button"
                class="dfb-button is-small"
                :disabled="page <= 1 || listQuery.isFetching.value"
                @click="goPage(page - 1)"
              >
                السابق
              </button>
              <span class="dws-pager-info">
                صفحة <span class="dfb-num">{{ page }}</span> من <span class="dfb-num">{{ pageCount }}</span>
              </span>
              <button
                type="button"
                class="dfb-button is-small"
                :disabled="page >= pageCount || listQuery.isFetching.value"
                @click="goPage(page + 1)"
              >
                التالي
              </button>
            </nav>
          </template>
        </div>

        <aside
          v-if="selectedId != null"
          class="dws-detail"
          aria-label="تفاصيل المبيع الخاطئ"
        >
          <div class="dws-detail-head">
            <h3 class="dws-detail-title">{{ detail?.customer.name || 'مبيع خاطئ' }}</h3>
            <button
              type="button"
              class="dfb-button is-small"
              @click="selectedId = null"
            >
              إغلاق
            </button>
          </div>

          <div
            v-if="detailQuery.isError.value"
            class="dfb-error"
            role="alert"
          >
            تعذر تحميل الإشعار.
            <button
              type="button"
              class="dfb-button is-small"
              @click="detailQuery.refetch()"
            >
              إعادة المحاولة
            </button>
          </div>
          <p
            v-else-if="!detail"
            class="dfb-state"
          >
            جاري التحميل…
          </p>
          <template v-else>
            <div
              v-if="detail.review"
              class="dws-card is-review"
            >
              <h4 class="dws-card-title">بيانات المراجعة</h4>
              <dl class="dws-facts">
                <div><dt>المراجع</dt><dd>{{ dash(detail.review.reviewerName) }}</dd></div>
                <div><dt>وقت المراجعة</dt><dd class="dfb-num">{{ wrongSaleDateTime(detail.review.reviewedAtUtc) }}</dd></div>
                <div class="is-wide"><dt>الملاحظة</dt><dd class="dws-pre">{{ dash(detail.review.note) }}</dd></div>
              </dl>
            </div>

            <div
              class="dws-card is-notice"
              role="note"
            >
              <div class="dws-card-row">
                <h4 class="dws-card-title">مبيع خاطئ — إشعار اطلاع</h4>
                <span
                  class="dfb-status"
                  :class="detail.notice.isDelegateUnread ? 'is-open' : ''"
                >
                  {{ detail.notice.isDelegateUnread ? 'غير مقروء' : 'مقروء' }}
                </span>
              </div>
              <p class="dws-notice-text">
                هذا إشعار فقط، لا موافقة ولا رفض. يبقى محفوظاً ويُعلَّم مقروءاً عند الفتح.
              </p>
            </div>

            <div class="dws-card">
              <h4 class="dws-card-title">بيانات الزبون</h4>
              <dl class="dws-facts">
                <div><dt>الاسم</dt><dd>{{ dash(detail.customer.name) }}</dd></div>
                <div><dt>الهاتف</dt><dd class="dfb-num">{{ dash(detail.customer.phone) }}</dd></div>
                <div><dt>الفرع</dt><dd>{{ dash(detail.customer.branchName) }}</dd></div>
                <div><dt>القائمة</dt><dd>{{ dash(detail.customer.listName) }}</dd></div>
                <div class="is-wide"><dt>العنوان</dt><dd>{{ dash(detail.customer.address) }}</dd></div>
              </dl>
            </div>

            <div class="dws-card">
              <h4 class="dws-card-title">البيع والقسط</h4>
              <dl class="dws-facts">
                <div><dt>سعر البيع</dt><dd class="dfb-num">{{ formatMoney(detail.sale.sellAmount) }}</dd></div>
                <div><dt>القسط اليومي</dt><dd class="dfb-num">{{ formatMoney(detail.sale.dailyInstallment) }}</dd></div>
                <div><dt>المدفوع</dt><dd class="dfb-num">{{ formatMoney(detail.sale.paidOnOrder) }}</dd></div>
                <div><dt>المتبقي</dt><dd class="dfb-num">{{ formatMoney(detail.sale.remainingOnOrder) }}</dd></div>
                <div><dt>المتبقي على الزبون (كل المبيعات)</dt><dd class="dfb-num">{{ formatMoney(detail.sale.customerRemaining) }}</dd></div>
                <div><dt>تاريخ البيع</dt><dd class="dfb-num">{{ wrongSaleDate(detail.sale.saleDate) }}</dd></div>
              </dl>
            </div>

            <div class="dws-card">
              <h4 class="dws-card-title">{{ detail.evaluation.title }}</h4>
              <dl class="dws-facts">
                <div><dt>{{ detail.evaluation.evaluatorLabel }}</dt><dd>{{ dash(detail.evaluation.evaluatorName) }}</dd></div>
                <div><dt>القائمة</dt><dd>{{ dash(detail.evaluation.listName) }}</dd></div>
                <template v-if="detail.evaluation.isEvaluated">
                  <div><dt>التقييم</dt><dd class="dfb-num">{{ formatNumber(detail.evaluation.rating) }} / 10</dd></div>
                  <div><dt>قيمة بضاعة المحل</dt><dd class="dfb-num">{{ formatMoney(detail.evaluation.shopStockEstimatedValue) }}</dd></div>
                  <div><dt>واردات المحل اليومية</dt><dd class="dfb-num">{{ formatMoney(detail.evaluation.estimatedDailyRevenue) }}</dd></div>
                  <div><dt>سعر البيع (عند التقييم)</dt><dd class="dfb-num">{{ formatMoney(detail.evaluation.saleAmountAtEvaluation) }}</dd></div>
                  <div class="is-wide"><dt>الملاحظة</dt><dd class="dws-pre">{{ dash(detail.evaluation.note) }}</dd></div>
                </template>
                <div
                  v-else
                  class="is-wide"
                >
                  <dt>الحالة</dt><dd>لم يُقيَّم بعد</dd>
                </div>
              </dl>
            </div>

            <div
              v-if="detail.notice.reasonBlock?.trim()"
              class="dws-card"
            >
              <h4 class="dws-card-title">نص الإشعار</h4>
              <p class="dws-pre">{{ detail.notice.reasonBlock }}</p>
            </div>

            <div
              v-if="detail.paymentsCount > 0"
              class="dws-card"
            >
              <h4 class="dws-card-title">
                التسديدات (<span class="dfb-num">{{ formatNumber(detail.paymentsCount) }}</span>)
              </h4>
              <ul class="dws-rows">
                <li
                  v-for="p in detail.payments"
                  :key="p.id"
                >
                  <span class="dfb-num">{{ wrongSaleDate(p.date) }}</span>
                  <span>
                    <span class="dfb-num">{{ formatMoney(p.amount) }}</span>
                    <template v-if="p.description"> · {{ p.description }}</template>
                  </span>
                </li>
              </ul>
              <p
                v-if="detail.paymentsCount > detail.payments.length"
                class="dws-hint"
              >
                يظهر آخر {{ formatNumber(detail.payments.length) }} تسديد فقط.
              </p>
            </div>

            <div class="dws-card">
              <h4 class="dws-card-title">أسماء سابقة / مطابقة (هاتف أو اسم) في الفرع</h4>
              <p
                v-if="detail.matches.length === 0"
                class="dws-hint"
              >
                لا توجد أسماء سابقة مطابقة غير هذا الزبون في الفرع.
              </p>
              <ul
                v-else
                class="dws-matches"
              >
                <li
                  v-for="m in detail.matches"
                  :key="m.customerId"
                  class="dws-match"
                >
                  <strong>{{ m.name }}</strong>
                  <span class="dws-hint">{{ m.matchReasons.join(' · ') }}</span>
                  <dl class="dws-facts is-compact">
                    <div><dt>الهاتف</dt><dd class="dfb-num">{{ dash(m.phone) }}</dd></div>
                    <div><dt>تاريخ البيع</dt><dd class="dfb-num">{{ wrongSaleDate(m.lastSaleDate) }}</dd></div>
                    <div><dt>سعر البيع</dt><dd class="dfb-num">{{ formatMoney(m.totalSales) }}</dd></div>
                    <div><dt>المدفوع</dt><dd class="dfb-num">{{ formatMoney(m.totalPaid) }}</dd></div>
                    <div><dt>المتبقي</dt><dd class="dfb-num">{{ formatMoney(m.remaining) }}</dd></div>
                  </dl>
                </li>
              </ul>
            </div>
          </template>
        </aside>
      </div>
    </template>
  </section>
</template>

<style scoped>
.dws {
  display: flex;
  flex-direction: column;
  gap: 18px;
  min-width: 0;
}

.dws-head {
  align-items: flex-start;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.dws-layout {
  align-items: start;
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 1fr);
}

.dws-layout.has-detail {
  grid-template-columns: minmax(300px, 380px) minmax(0, 1fr);
}

.dws-list-col {
  display: flex;
  flex-direction: column;
  gap: 12px;
  min-width: 0;
}

.dws-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dws-row {
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

.dws-row:hover { box-shadow: var(--dfb-up-hover); }
.dws-row.is-unread { background: var(--dfb-warn-soft); }
.dws-row.is-active { box-shadow: var(--dfb-down); }

.dws-row-top {
  align-items: center;
  display: flex;
  gap: 8px;
  justify-content: space-between;
}

.dws-row-name {
  font-weight: 600;
  line-height: 1.4;
  overflow-wrap: anywhere;
}

.dws-row.is-unread .dws-row-name { font-weight: 700; }

.dws-row-sub,
.dws-row-time,
.dws-hint {
  color: var(--dfb-muted);
  font-size: 13px;
  line-height: 1.5;
  margin: 0;
}

.dws-row-time { font-size: 12px; }

.dws-pager {
  align-items: center;
  display: flex;
  gap: 12px;
  justify-content: center;
}

.dws-pager-info {
  color: var(--dfb-muted);
  font-size: 13px;
}

.dws-detail {
  background: var(--dfb-well);
  border-radius: 20px;
  box-shadow: var(--dfb-down);
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-width: 0;
  padding: 18px;
}

.dws-detail-head,
.dws-card-row {
  align-items: flex-start;
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.dws-detail-title {
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
  margin: 0;
  overflow-wrap: anywhere;
}

.dws-card {
  background: var(--dfb-raised);
  border-radius: 16px;
  box-shadow: var(--dfb-up);
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 14px 16px;
}

.dws-card.is-notice { background: var(--dfb-warn-soft); }
.dws-card.is-review { background: var(--dfb-primary-soft); }

.dws-card-title {
  font-size: 15px;
  font-weight: 700;
  margin: 0;
}

.dws-notice-text {
  color: var(--dfb-tier-weak);
  font-size: 13px;
  line-height: 1.7;
  margin: 0;
}

.dws-facts {
  display: grid;
  gap: 10px 18px;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 170px), 1fr));
  margin: 0;
}

.dws-facts .is-wide { grid-column: 1 / -1; }

.dws-facts dt {
  color: var(--dfb-muted);
  font-size: 12px;
  font-weight: 600;
}

.dws-facts dd {
  font-weight: 600;
  margin: 2px 0 0;
  overflow-wrap: anywhere;
}

.dws-facts.is-compact { gap: 6px 14px; }

.dws-pre {
  line-height: 1.8;
  margin: 0;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}

.dws-rows {
  display: flex;
  flex-direction: column;
  gap: 6px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dws-rows li {
  display: flex;
  gap: 12px;
  justify-content: space-between;
}

.dws-matches {
  display: flex;
  flex-direction: column;
  gap: 12px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.dws-match {
  background: var(--dfb-well);
  border-radius: 14px;
  box-shadow: var(--dfb-well-shadow);
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 12px 14px;
}

@media (max-width: 900px) {
  .dws-layout.has-detail {
    grid-template-columns: minmax(0, 1fr);
  }
}
</style>
