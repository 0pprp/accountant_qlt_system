<script setup lang="ts">
import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Row {
  customerId: number
  customerName: string
  phoneNumber: string | null
  branchId: number
  previousBand: string
  currentBand: string
  currentScore: number
  previousCaptureDate: string
  currentCaptureDate: string
  daysBetween: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
  historyInsufficient: boolean
}

const auth = useAuthStore()
const canRead = computed(() => auth.hasPermission('BranchDeclining', 'Read') === true)

const listQuery = useQuery({
  queryKey: computed(() => ['branch-declining', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchDeclining', {
      signal,
      params: { page: 1, pageSize: 100 },
    })
    return data
  },
})
</script>

<template>
  <div
    class="bd-page"
    dir="rtl"
  >
    <header>
      <h1 class="bd-title">الزبائن المتدنية</h1>
      <p class="bd-sub">
        تراجع ممتاز → سيء/متلكئ خلال 15 يومًا ضمن فروعك — ليست متدنية المتابع.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="bd-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة زبائن الفرع المتدنية.
    </div>
    <template v-else>
      <button
        v-if="listQuery.isError.value"
        type="button"
        class="bd-btn"
        @click="listQuery.refetch()"
      >
        إعادة المحاولة
      </button>
      <p
        v-if="listQuery.isError.value"
        class="bd-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="bd-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="listQuery.data.value?.historyInsufficient"
        class="bd-state"
      >
        لا يتوفر تاريخ تقييمات كافٍ لهذه الفترة — ليس بالضرورة عدم وجود زبائن متدنية.
      </div>
      <div
        v-else-if="(listQuery.data.value?.totalCount ?? 0) === 0"
        class="bd-state"
      >
        لا زبائن متدنية ضمن القاعدة والنطاق الحاليين.
      </div>
      <ul
        v-else
        class="bd-list"
      >
        <li
          v-for="row in listQuery.data.value?.items ?? []"
          :key="row.customerId"
          class="bd-row"
        >
          <div class="bd-name">{{ row.customerName }}</div>
          <div class="bd-meta">
            {{ row.phoneNumber || '—' }} · فرع {{ row.branchId }} ·
            {{ row.previousBand }} → {{ row.currentBand }}
            ({{ row.daysBetween }} يوم)
          </div>
        </li>
      </ul>
    </template>
  </div>
</template>

<style scoped>
.bd-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.bd-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.bd-sub {
  margin: 0.35rem 0 1rem;
  color: #335;
  font-size: 0.9rem;
}
.bd-state,
.bd-err {
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.bd-err {
  color: #8b1a1a;
}
.bd-btn {
  margin-bottom: 0.75rem;
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  cursor: pointer;
}
.bd-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.5rem;
}
.bd-row {
  background: #f8f8f8;
  padding: 0.75rem 1rem;
  border-radius: 8px;
}
.bd-name {
  font-weight: 700;
}
.bd-meta {
  font-size: 0.85rem;
  opacity: 0.85;
  margin-top: 0.25rem;
}
</style>
