<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppDateField, AppSelect, DataState } from '@/modules/Core/components/ui'
import { formatDate, formatMoney, formatNumber } from '@/modules/Core/utils/formatDisplay'

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

const listOptions = computed(() =>
  (catalogQuery.data.value ?? []).map((l) => ({ label: l.name, value: l.id })),
)

function fmt(n: number) {
  return formatMoney(n)
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
    <DataState
      v-if="!canRead"
      kind="forbidden"
      title="غير مصرّح"
      message="ليس لديك صلاحية قراءة التسديدات."
    />
    <template v-else>
      <div class="q-filters">
        <AppSelect
          v-model="listId"
          label="القائمة"
          :options="listOptions"
          :filterable="(catalogQuery.data.value?.length ?? 0) > 6"
        />
        <AppDateField v-model="fromDate" label="من" />
        <AppDateField v-model="toDate" label="إلى" />
      </div>
      <DataState
        v-if="listQuery.isPending.value"
        kind="loading"
        title="جاري التحميل"
        message="جاري جلب التسديدات."
      />
      <DataState
        v-else-if="listQuery.isError.value"
        kind="error"
        title="تعذر التحميل"
        message="تعذر تحميل التسديدات."
      >
        <template #actions>
          <AppButton variant="secondary" @click="listQuery.refetch()">إعادة</AppButton>
        </template>
      </DataState>
      <template v-else-if="listQuery.data.value">
        <div class="bp-card">
          <p>عدد التسديدات: {{ formatNumber(listQuery.data.value.paymentCount) }}</p>
          <p>
            الإجمالي:
            <strong>{{ fmt(listQuery.data.value.totalAmount) }}</strong>
          </p>
        </div>
        <DataState
          v-if="listQuery.data.value.items.length === 0"
          kind="empty"
          title="لا تسديدات"
          message="لا تسديدات في هذه الفترة."
        />
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
            <span>{{ formatDate(row.date) }}</span>
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
  font-family: inherit;
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
