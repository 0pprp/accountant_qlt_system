<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Line {
  productId: number
  productName: string
  quantity: number
  unitSalePrice: number
  lineSalePrice: number
}

interface Row {
  orderId: number
  branchId: number
  customerId: number
  customerName: string
  customerPhone: string | null
  sellerName: string | null
  sellAmount: number
  dailyInstallmentAmount: number
  prepaymentAmount: number
  salesCompletedAtUtc: string | null
  isRead: boolean
  items: Line[]
}

interface ListResponse {
  items: Row[]
  totalCount: number
  unreadCount: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('BranchNewSale', 'Read') === true)
const canMark = computed(() => auth.hasPermission('BranchNewSale', 'MarkRead') === true)
const unreadOnly = computed(() => route.query.unread === '1')
const selectedId = computed(() => {
  const raw = route.query.orderId
  const n = Number(raw)
  return Number.isFinite(n) && n > 0 ? n : null
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'branch-new-sales',
    unreadOnly.value,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchNewSales', {
      signal,
      params: {
        page: 1,
        pageSize: 100,
        unreadOnly: unreadOnly.value || undefined,
      },
    })
    return data
  },
})

const detailQuery = useQuery({
  queryKey: computed(() => ['branch-new-sale', selectedId.value, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady && selectedId.value != null),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<Row>(
      `/admin/BranchNewSales/${selectedId.value}`,
      { signal }
    )
    return data
  },
})

function patch(next: Record<string, string | null | undefined>) {
  const q = { ...(route.query as Record<string, string>) }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: route.name ?? undefined, query: q })
}

const markOne = useMutation({
  mutationFn: async (orderId: number) => {
    await axiosInstance.post(`/admin/BranchNewSales/${orderId}/mark-read`)
  },
  onSuccess: async () => {
    await qc.invalidateQueries({ queryKey: ['branch-new-sales'] })
    await qc.invalidateQueries({ queryKey: ['branch-new-sale'] })
  },
})

// مصدر BM: فتح التفاصيل يعلّم مقروءًا (قراءة مشتركة للفرع)
watch(
  () => detailQuery.data.value,
  (row) => {
    if (!row || row.isRead || !canMark.value || markOne.isPending.value) return
    markOne.mutate(row.orderId)
  }
)

const markAll = useMutation({
  mutationFn: async () => {
    await axiosInstance.post('/admin/BranchNewSales/mark-all-read')
  },
  onSuccess: async () => {
    await qc.invalidateQueries({ queryKey: ['branch-new-sales'] })
  },
})

function fmt(n: number) {
  return new Intl.NumberFormat('ar-IQ').format(n)
}

function fmtDate(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Intl.DateTimeFormat('ar-IQ', {
      dateStyle: 'medium',
      timeStyle: 'short',
    }).format(new Date(iso))
  } catch {
    return iso
  }
}
</script>

<template>
  <div
    class="bns-page"
    dir="rtl"
  >
    <header>
      <h1 class="bns-title">المبيعات</h1>
      <p class="bns-sub">
        صندوق المبيعات المكتملة للفرع — القراءة مشتركة ولا تغيّر حالة البيع.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="bns-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة مبيعات الفرع الجديدة.
    </div>
    <template v-else>
      <div class="bns-toolbar">
        <label class="bns-check">
          <input
            type="checkbox"
            :checked="unreadOnly"
            @change="patch({ unread: ($event.target as HTMLInputElement).checked ? '1' : null })"
          >
          غير المقروء فقط
        </label>
        <button
          v-if="canMark"
          type="button"
          class="bns-btn"
          :disabled="markAll.isPending.value"
          @click="markAll.mutate()"
        >
          تعليم الكل كمقروء
        </button>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="bns-btn"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p class="bns-meta">
        الإجمالي: {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
        · غير مقروء: {{ listQuery.data.value?.unreadCount ?? '—' }}
      </p>

      <div class="bns-layout">
        <section class="bns-list-pane">
          <p
            v-if="listQuery.isError.value"
            class="bns-err"
            role="alert"
          >
            تعذر التحميل.
          </p>
          <div
            v-else-if="listQuery.isPending.value"
            class="bns-state"
          >
            جاري التحميل…
          </div>
          <div
            v-else-if="!(listQuery.data.value?.items.length)"
            class="bns-state"
          >
            لا مبيعات في هذا الصندوق.
          </div>
          <ul
            v-else
            class="bns-list"
          >
            <li
              v-for="row in listQuery.data.value?.items ?? []"
              :key="row.orderId"
            >
              <button
                type="button"
                class="bns-row"
                :class="{ active: selectedId === row.orderId, unread: !row.isRead }"
                @click="patch({ orderId: String(row.orderId) })"
              >
                <strong>{{ row.customerName }}</strong>
                <span>{{ fmt(row.sellAmount) }}</span>
                <span class="bns-muted">{{ fmtDate(row.salesCompletedAtUtc) }}</span>
                <span :class="row.isRead ? 'badge' : 'badge unread'">
                  {{ row.isRead ? 'مقروء' : 'غير مقروء' }}
                </span>
              </button>
            </li>
          </ul>
        </section>

        <section
          v-if="selectedId"
          class="bns-detail"
        >
          <p
            v-if="detailQuery.isError.value"
            class="bns-err"
            role="alert"
          >
            تعذر تحميل التفاصيل.
          </p>
          <div
            v-else-if="detailQuery.isPending.value"
            class="bns-state"
          >
            جاري التحميل…
          </div>
          <template v-else-if="detailQuery.data.value">
            <h2>{{ detailQuery.data.value.customerName }}</h2>
            <p class="bns-muted">{{ detailQuery.data.value.customerPhone ?? '—' }}</p>
            <p>البائع: {{ detailQuery.data.value.sellerName ?? '—' }}</p>
            <p>سعر البيع: {{ fmt(detailQuery.data.value.sellAmount) }}</p>
            <p>القسط اليومي: {{ fmt(detailQuery.data.value.dailyInstallmentAmount) }}</p>
            <p>المقدمة: {{ fmt(detailQuery.data.value.prepaymentAmount) }}</p>
            <p>الإكمال: {{ fmtDate(detailQuery.data.value.salesCompletedAtUtc) }}</p>
            <h3>البنود</h3>
            <ul>
              <li
                v-for="(line, i) in detailQuery.data.value.items"
                :key="i"
              >
                {{ line.productName }} × {{ line.quantity }} — {{ fmt(line.lineSalePrice) }}
              </li>
            </ul>
            <button
              v-if="canMark && !detailQuery.data.value.isRead"
              type="button"
              class="bns-btn primary"
              :disabled="markOne.isPending.value"
              @click="markOne.mutate(detailQuery.data.value.orderId)"
            >
              تعليم كمقروء
            </button>
            <button
              type="button"
              class="bns-btn"
              @click="patch({ orderId: null })"
            >
              إغلاق التفاصيل
            </button>
          </template>
        </section>
      </div>
    </template>
  </div>
</template>

<style scoped>
.bns-page {
  --c-primary: #006a64;
  --c-ink: #00201e;
  --c-field: #f8f8f8;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: var(--c-ink);
  padding: 1rem 1.25rem 2rem;
  max-width: 1100px;
  margin: 0 auto;
}
.bns-title {
  margin: 0;
  color: var(--c-primary);
  font-size: 1.5rem;
}
.bns-sub {
  margin: 0.35rem 0 1rem;
  color: #445;
}
.bns-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  align-items: center;
}
.bns-check {
  display: flex;
  gap: 0.35rem;
  align-items: center;
}
.bns-btn {
  font: inherit;
  border-radius: 8px;
  border: 1px solid #c5cecc;
  background: #fff;
  padding: 0.4rem 0.75rem;
  cursor: pointer;
}
.bns-btn.primary {
  background: var(--c-primary);
  border-color: var(--c-primary);
  color: #fff;
}
.bns-meta {
  font-size: 0.9rem;
}
.bns-err {
  color: #9b1c1c;
}
.bns-state {
  padding: 1.25rem;
  text-align: center;
  background: var(--c-field);
  border-radius: 12px;
}
.bns-layout {
  display: grid;
  gap: 1rem;
  grid-template-columns: 1fr;
}
@media (min-width: 768px) {
  .bns-layout {
    grid-template-columns: 1fr 1fr;
  }
}
.bns-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.4rem;
}
.bns-row {
  width: 100%;
  text-align: right;
  font: inherit;
  background: var(--c-field);
  border: 1px solid transparent;
  border-radius: 10px;
  padding: 0.65rem 0.75rem;
  display: grid;
  gap: 0.2rem;
  cursor: pointer;
  color: inherit;
}
.bns-row.active {
  border-color: var(--c-primary);
}
.bns-row.unread {
  background: #fff8e8;
}
.bns-muted {
  color: #667;
  font-size: 0.85rem;
}
.badge {
  font-size: 0.75rem;
  width: fit-content;
  padding: 0.1rem 0.45rem;
  border-radius: 999px;
  background: #dde;
}
.badge.unread {
  background: #fde68a;
}
.bns-detail {
  background: var(--c-field);
  border-radius: 12px;
  padding: 1rem;
}
.bns-detail h2 {
  margin: 0 0 0.35rem;
  color: var(--c-primary);
}
.bns-detail .bns-btn {
  margin-top: 0.5rem;
  margin-left: 0.35rem;
}
</style>
