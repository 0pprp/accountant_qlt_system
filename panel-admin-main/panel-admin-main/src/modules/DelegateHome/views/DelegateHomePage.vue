<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Summary {
  unreadComplaints: number | null
  unreadComplaintsAvailable: boolean
  pendingExceptions: number | null
  pendingExceptionsAvailable: boolean
  unreadWrongSales: number | null
  unreadWrongSalesAvailable: boolean
  approvedExceptions: number | null
  rejectedExceptions: number | null
  cancelledExceptions: number | null
  totalExceptions: number | null
  exceptionsSummaryAvailable: boolean
}

const auth = useAuthStore()
const router = useRouter()
const canRead = computed(() => auth.hasPermission('DelegateHome', 'Read') === true)
const canComplaints = computed(() => auth.hasPermission('Complaint', 'Read') === true)
const canExceptions = computed(
  () =>
    auth.hasPermission('SalesRequest', 'TabExceptions') === true ||
    auth.hasPermission('SalesRequest', 'ExceptionDecide') === true
)
const canWrong = computed(() => auth.hasPermission('WrongSale', 'Read') === true)

const summaryQuery = useQuery({
  queryKey: computed(() => ['delegate-home', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<Summary>('/admin/DelegateHome/summary', {
      signal,
    })
    return data
  },
})

function fmt(available: boolean, value: number | null | undefined) {
  if (!available) return 'غير متاح'
  if (value == null) return '—'
  return String(value)
}

function go(path: string, query?: Record<string, string>) {
  router.push({ path, query })
}
</script>

<template>
  <div
    class="dh-page"
    dir="rtl"
  >
    <header>
      <h1 class="dh-title">الرئيسية</h1>
      <p class="dh-sub">
        مؤشرات ضمن نطاق حسابك وصلاحياتك — ليست لوحة عامة.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="dh-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة رئيسية المفوض.
    </div>
    <template v-else>
      <button
        v-if="summaryQuery.isError.value"
        type="button"
        class="dh-btn"
        @click="summaryQuery.refetch()"
      >
        إعادة المحاولة
      </button>
      <p
        v-if="summaryQuery.isError.value"
        class="dh-err"
        role="alert"
      >
        تعذر تحميل الملخص — لم نعرض أصفارًا بديلة.
      </p>
      <div
        v-else-if="summaryQuery.isPending.value"
        class="dh-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else
        class="dh-grid"
      >
        <button
          type="button"
          class="dh-card"
          :disabled="!canComplaints"
          @click="canComplaints && go('/management/delegate/complaints')"
        >
          <span>رسائل وشكاوى غير مقروءة</span>
          <strong>{{
            fmt(
              summaryQuery.data.value?.unreadComplaintsAvailable === true,
              summaryQuery.data.value?.unreadComplaints
            )
          }}</strong>
        </button>
        <button
          type="button"
          class="dh-card"
          :disabled="!canExceptions"
          @click="canExceptions && go('/management/delegate/exceptions')"
        >
          <span>طلبات استثناء قيد الانتظار</span>
          <strong>{{
            fmt(
              summaryQuery.data.value?.pendingExceptionsAvailable === true,
              summaryQuery.data.value?.pendingExceptions
            )
          }}</strong>
        </button>
        <button
          type="button"
          class="dh-card"
          :disabled="!canWrong"
          @click="
            canWrong &&
              go('/management/delegate/wrong-sales', { mailbox: 'unread' })
          "
        >
          <span>مبيعات خاطئة غير مقروءة</span>
          <strong>{{
            fmt(
              summaryQuery.data.value?.unreadWrongSalesAvailable === true,
              summaryQuery.data.value?.unreadWrongSales
            )
          }}</strong>
        </button>
      </div>
      <section
        v-if="summaryQuery.data.value?.exceptionsSummaryAvailable"
        class="dh-section"
      >
        <h2>ملخص طلبات الاستثناء</h2>
        <p>موافق: {{ summaryQuery.data.value?.approvedExceptions ?? '—' }}</p>
        <p>مرفوض: {{ summaryQuery.data.value?.rejectedExceptions ?? '—' }}</p>
        <p>ملغى: {{ summaryQuery.data.value?.cancelledExceptions ?? '—' }}</p>
        <p>الإجمالي: {{ summaryQuery.data.value?.totalExceptions ?? '—' }}</p>
      </section>
      <p
        v-else-if="summaryQuery.data.value && !summaryQuery.isError.value"
        class="dh-meta"
      >
        ملخص الاستثناءات غير متاح لصلاحياتك أو نطاقك.
      </p>
    </template>
  </div>
</template>

<style scoped>
.dh-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.dh-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.dh-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.dh-grid {
  display: grid;
  gap: 0.75rem;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}
.dh-card {
  background: #f8f8f8;
  border: 0;
  border-radius: 10px;
  padding: 1rem;
  text-align: right;
  cursor: pointer;
  font: inherit;
  color: inherit;
  display: grid;
  gap: 0.5rem;
}
.dh-card:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.dh-card strong {
  font-size: 1.6rem;
  color: #006a64;
}
.dh-section {
  margin-top: 1.25rem;
  background: #f8f8f8;
  border-radius: 8px;
  padding: 1rem;
}
.dh-state,
.dh-err {
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.dh-err {
  color: #8b1a1a;
}
.dh-btn {
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  margin-bottom: 0.75rem;
  cursor: pointer;
}
.dh-meta {
  margin-top: 1rem;
  font-size: 0.9rem;
  opacity: 0.85;
}
</style>
