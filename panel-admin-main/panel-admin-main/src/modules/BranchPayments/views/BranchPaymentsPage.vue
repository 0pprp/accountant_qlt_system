<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface ListOpt {
  id: number
  name: string
}

interface PaymentRow {
  paymentId: number
  customerId: number
  customerName: string
  phoneNumber: string | null
  date: string
  amount: number
}

interface ListResponse {
  listId: number
  listName: string | null
  fromDate: string | null
  toDate: string | null
  totalAmount: number
  paymentCount: number
  items: PaymentRow[]
}

const auth = useAuthStore()
const canRead = computed(
  () =>
    auth.hasPermission('OrderList', 'Read') === true ||
    auth.hasPermission('ManagementRatings', 'Read') === true ||
    auth.hasPermission('InstallmentPayment', 'Read') === true
)
const branchId = computed(() => auth.selectedBranch?.id ?? auth.branches?.[0]?.id ?? 0)
const listId = ref(0)
const fromDate = ref('')
const toDate = ref('')

watch(
  branchId,
  () => {
    const now = new Date()
    const to = now.toISOString().slice(0, 10)
    const from = new Date(now.getTime() - 6 * 86400000).toISOString().slice(0, 10)
    toDate.value = to
    fromDate.value = from
  },
  { immediate: true }
)

const catalogQuery = useQuery({
  queryKey: computed(() => ['branch-pay-catalog', branchId.value, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady && branchId.value > 0),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<{
      lists?: Array<{ id: number; name: string }>
    }>('/admin/ManagementRatings/catalog', {
      signal,
      params: { branchId: branchId.value },
    })
    const lists = (data.lists ?? []).map((l) => ({ id: l.id, name: l.name }))
    if (listId.value <= 0 && lists[0]) listId.value = lists[0].id
    return lists as ListOpt[]
  },
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'branch-payments',
    listId.value,
    fromDate.value,
    toDate.value,
    auth.sessionGeneration,
  ]),
  enabled: computed(
    () =>
      canRead.value &&
      auth.permissionsReady &&
      listId.value > 0 &&
      !!fromDate.value &&
      !!toDate.value
  ),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchPayments', {
      signal,
      params: {
        listId: listId.value,
        fromDate: fromDate.value,
        toDate: toDate.value,
      },
    })
    return data
  },
})

function fmt(n: number) {
  return new Intl.NumberFormat('ar-IQ').format(n)
}
</script>

<template>
  <div
    class="bp-page"
    dir="rtl"
  >
    <header>
      <h1 class="bp-title">التسديدات</h1>
      <p class="bp-sub">
        تسديدات قائمة ضمن فترة — قراءة فقط وفق نطاق الفرع (ليست إنشاء تسديد).
      </p>
    </header>
    <div
      v-if="!canRead"
      class="bp-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة التسديدات.
    </div>
    <template v-else>
      <div class="bp-filters">
        <label>
          القائمة
          <select v-model.number="listId">
            <option
              v-for="l in catalogQuery.data.value ?? []"
              :key="l.id"
              :value="l.id"
            >
              {{ l.name }}
            </option>
          </select>
        </label>
        <label>
          من
          <input
            v-model="fromDate"
            type="date"
          >
        </label>
        <label>
          إلى
          <input
            v-model="toDate"
            type="date"
          >
        </label>
      </div>
      <div
        v-if="listQuery.isPending.value"
        class="bp-state"
      >
        جاري التحميل…
      </div>
      <p
        v-else-if="listQuery.isError.value"
        class="bp-err"
        role="alert"
      >
        تعذر التحميل —
        <button
          type="button"
          class="bp-btn"
          @click="listQuery.refetch()"
        >
          إعادة
        </button>
      </p>
      <template v-else-if="listQuery.data.value">
        <div class="bp-card">
          <p>عدد التسديدات: {{ listQuery.data.value.paymentCount }}</p>
          <p>
            الإجمالي:
            <strong>{{ fmt(listQuery.data.value.totalAmount) }}</strong>
          </p>
        </div>
        <div
          v-if="listQuery.data.value.items.length === 0"
          class="bp-state"
        >
          لا تسديدات في هذه الفترة.
        </div>
        <ul
          v-else
          class="bp-list"
        >
          <li
            v-for="row in listQuery.data.value.items"
            :key="row.paymentId"
          >
            <strong>{{ row.customerName }}</strong>
            <span>{{ row.phoneNumber || '—' }}</span>
            <span>{{ row.date }}</span>
            <span>{{ fmt(row.amount) }}</span>
          </li>
        </ul>
      </template>
    </template>
  </div>
</template>

<style scoped>
.bp-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.bp-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.bp-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.bp-filters {
  display: grid;
  gap: 0.75rem;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  margin-bottom: 1rem;
}
.bp-filters label {
  display: grid;
  gap: 0.25rem;
  font-size: 0.9rem;
}
.bp-filters input,
.bp-filters select {
  background: #f8f8f8;
  border: 1px solid #cfd8d7;
  border-radius: 6px;
  padding: 0.4rem 0.55rem;
  font: inherit;
}
.bp-state,
.bp-err,
.bp-card {
  margin-top: 0.75rem;
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.bp-err {
  color: #8b1a1a;
}
.bp-list {
  list-style: none;
  margin: 0.75rem 0 0;
  padding: 0;
  display: grid;
  gap: 0.45rem;
}
.bp-list li {
  display: grid;
  grid-template-columns: 1.4fr 1fr auto auto;
  gap: 0.5rem;
  background: #f8f8f8;
  border-radius: 8px;
  padding: 0.65rem 0.8rem;
  font-size: 0.92rem;
}
.bp-btn {
  background: transparent;
  border: none;
  color: #006a64;
  font: inherit;
  text-decoration: underline;
  cursor: pointer;
}
@media (max-width: 640px) {
  .bp-list li {
    grid-template-columns: 1fr 1fr;
  }
}
</style>
