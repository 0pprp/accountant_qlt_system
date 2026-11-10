<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Row {
  customerId: number
  customerName: string
  phoneNumber: string | null
  mandobName: string | null
  totalSellAmount: number
  totalPaidAmount: number
  remainingAmount: number
  ratingScore: number | null
  ratingLabel: string | null
}

const auth = useAuthStore()
const router = useRouter()
const canRead = computed(() => auth.hasPermission('Customer', 'Read') === true)
const text = ref('')
const submitted = ref('')

watch(text, (v) => {
  const q = v.trim()
  if (q.length < 2 && !/^\d+$/.test(q)) {
    submitted.value = ''
    return
  }
  const t = setTimeout(() => {
    submitted.value = q
  }, 450)
  return () => clearTimeout(t)
})

const listQuery = useQuery({
  queryKey: computed(() => ['branch-customers', submitted.value, auth.sessionGeneration]),
  enabled: computed(
    () =>
      canRead.value &&
      auth.permissionsReady &&
      (submitted.value.length >= 2 || /^\d+$/.test(submitted.value))
  ),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<{ items: Row[] }>(
      '/admin/BranchCustomers/search',
      { signal, params: { text: submitted.value } }
    )
    return data
  },
})

function fmt(n: number) {
  return new Intl.NumberFormat('ar-IQ').format(n)
}

function openCustomer(id: number) {
  void router.push({
    path: `/customers/${id}`,
    query: { returnTo: '/management/branch/customers' },
  })
}
</script>

<template>
  <div
    class="bc-page"
    dir="rtl"
  >
    <header>
      <h1 class="bc-title">الزبائن</h1>
      <p class="bc-sub">
        بحث ضمن فروعك (اسم/هاتف/معرّف — حرفان فأكثر) — ليست قائمة عامة ولا إنشاء زبائن.
      </p>
    </header>
    <div
      v-if="!canRead"
      class="bc-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الزبائن.
    </div>
    <template v-else>
      <label class="bc-search">
        <span class="sr-only">بحث</span>
        <input
          v-model="text"
          type="search"
          placeholder="اسم أو هاتف أو معرّف…"
          aria-label="بحث الزبائن"
        >
      </label>
      <div
        v-if="submitted.length < 2 && !/^\d+$/.test(submitted)"
        class="bc-state"
      >
        اكتب حرفين على الأقل للبحث.
      </div>
      <div
        v-else-if="listQuery.isPending.value"
        class="bc-state"
      >
        جاري البحث…
      </div>
      <p
        v-else-if="listQuery.isError.value"
        class="bc-err"
        role="alert"
      >
        تعذر البحث.
        <button
          type="button"
          class="bc-btn"
          @click="listQuery.refetch()"
        >
          إعادة
        </button>
      </p>
      <div
        v-else-if="(listQuery.data.value?.items.length ?? 0) === 0"
        class="bc-state"
      >
        لا نتائج.
      </div>
      <ul
        v-else
        class="bc-list"
      >
        <li
          v-for="row in listQuery.data.value?.items ?? []"
          :key="row.customerId"
        >
          <button
            type="button"
            class="bc-row"
            @click="openCustomer(row.customerId)"
          >
            <strong>{{ row.customerName }}</strong>
            <span>{{ row.phoneNumber || '—' }}</span>
            <span>المندوب: {{ row.mandobName || '—' }}</span>
            <span>البيع: {{ fmt(row.totalSellAmount) }}</span>
            <span>المتبقي: {{ fmt(row.remainingAmount) }}</span>
            <span>التقييم: {{ row.ratingLabel || '—' }}</span>
          </button>
        </li>
      </ul>
    </template>
  </div>
</template>

<style scoped>
.bc-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.bc-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.bc-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.bc-search input {
  width: 100%;
  background: #f8f8f8;
  border: 1px solid #cfd8d7;
  border-radius: 8px;
  padding: 0.65rem 0.85rem;
  font: inherit;
}
.bc-state,
.bc-err {
  margin-top: 1rem;
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.bc-err {
  color: #8b1a1a;
}
.bc-list {
  list-style: none;
  margin: 1rem 0 0;
  padding: 0;
  display: grid;
  gap: 0.5rem;
}
.bc-row {
  width: 100%;
  text-align: right;
  display: grid;
  gap: 0.2rem;
  background: #f8f8f8;
  border: 1px solid #e2e8e7;
  border-radius: 8px;
  padding: 0.75rem 0.9rem;
  font: inherit;
  color: inherit;
  cursor: pointer;
}
.bc-row:hover {
  border-color: #006a64;
}
.bc-btn {
  background: transparent;
  border: none;
  color: #006a64;
  font: inherit;
  text-decoration: underline;
  cursor: pointer;
}
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
