<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface DecisionRow {
  id: number
  customerId: number
  customerName: string
  phone: string | null
  branchId: number
  decisionType: string
  decisionTypeLabel: string
  note: string
  actorName: string | null
  weekPaid: number | null
  amountTotalSales: number | null
  paidPercent: number | null
  snoozeUntil: string | null
  createdAtUtc: string
  customerIsLegal: boolean
  customerIsFakeSale: boolean
}

interface DecisionListResponse {
  items: DecisionRow[]
  totalCount: number
  page: number
  pageSize: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const canRead = computed(
  () => auth.hasPermission('CustomerDecision', 'Read') === true
)

const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const decisionType = computed(() => String(route.query.type ?? ''))
const searchInput = ref(String(route.query.q ?? ''))
const search = ref(searchInput.value)
const selectedId = ref<number | null>(null)

let timer: ReturnType<typeof setTimeout> | null = null
watch(searchInput, (v) => {
  if (timer) clearTimeout(timer)
  timer = setTimeout(() => {
    search.value = v.trim()
    patch({ q: search.value || null, page: '1' })
  }, 300)
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'customer-decisions',
    page.value,
    decisionType.value,
    search.value,
    auth.branchGeneration,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<DecisionListResponse>(
      '/admin/CustomerDecisions',
      {
        signal,
        params: {
          page: page.value,
          pageSize: 50,
          decisionType: decisionType.value || undefined,
          search: search.value || undefined,
        },
      }
    )
    return data
  },
})

const selected = computed(
  () => listQuery.data.value?.items.find((r) => r.id === selectedId.value) ?? null
)

function patch(next: Record<string, string | null | undefined>) {
  const q = { ...(route.query as Record<string, string>) }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: route.name ?? undefined, query: q })
}

function fmtDate(iso: string) {
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
    class="cd-page"
    dir="rtl"
  >
    <header>
      <h1 class="cd-title">سجل القرارات</h1>
      <p class="cd-sub">
        سجل وظيفي لقرارات الفرع على الزبائن — ليس سجل النشاط العام.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="cd-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة سجل القرارات.
    </div>
    <template v-else>
      <div class="cd-toolbar">
        <div class="cd-search">
          <input
            v-model="searchInput"
            type="search"
            placeholder="بحث زبون أو سبب…"
            aria-label="بحث"
          >
          <span aria-hidden="true">🔍</span>
        </div>
        <select
          :value="decisionType"
          aria-label="نوع القرار"
          @change="
            patch({
              type: ($event.target as HTMLSelectElement).value || null,
              page: '1',
            })
          "
        >
          <option value="">كل الأنواع</option>
          <option value="Continue">استمرار</option>
          <option value="FakeSale">بيع وهمي</option>
          <option value="LegalRequest">طلب قانوني</option>
          <option value="WaiverRequest">طلب تنازل</option>
          <option value="Legal">تحويل قانوني</option>
          <option value="Waiver">تنازل مطبّق</option>
        </select>
      </div>
      <p class="cd-meta">
        الإجمالي:
        {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="cd-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="cd-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="cd-state"
      >
        لا قرارات مسجّلة.
      </div>
      <div
        v-else
        class="cd-layout"
      >
        <div class="cd-list">
          <button
            v-for="row in listQuery.data.value?.items ?? []"
            :key="row.id"
            type="button"
            class="cd-row"
            :class="{ active: selectedId === row.id }"
            @click="selectedId = row.id"
          >
            <div class="cd-row-top">
              <strong>{{ row.customerName }}</strong>
              <span>{{ row.decisionTypeLabel }}</span>
            </div>
            <div class="cd-muted">{{ fmtDate(row.createdAtUtc) }} · {{ row.actorName || '—' }}</div>
          </button>
        </div>
        <aside class="cd-detail">
          <div
            v-if="!selected"
            class="cd-state"
          >
            اختر قرارًا لعرض التفاصيل.
          </div>
          <template v-else>
            <h2>{{ selected.customerName }}</h2>
            <p><strong>النوع:</strong> {{ selected.decisionTypeLabel }}</p>
            <p><strong>الفاعل:</strong> {{ selected.actorName || '—' }}</p>
            <p><strong>التوقيت:</strong> {{ fmtDate(selected.createdAtUtc) }}</p>
            <p><strong>الهاتف:</strong> {{ selected.phone || '—' }}</p>
            <p v-if="selected.snoozeUntil">
              <strong>إرجاء حتى:</strong> {{ fmtDate(selected.snoozeUntil) }}
            </p>
            <p>
              <strong>صفة قانونية حالية:</strong>
              {{ selected.customerIsLegal ? 'نعم' : 'لا' }}
              ·
              <strong>بيع وهمي:</strong>
              {{ selected.customerIsFakeSale ? 'نعم' : 'لا' }}
            </p>
            <section class="cd-note">
              <h3>السبب</h3>
              <p>{{ selected.note }}</p>
            </section>
            <p class="cd-muted">السجل للقراءة فقط — لا تعديل من هذه الشاشة.</p>
          </template>
        </aside>
      </div>
    </template>
  </div>
</template>

<style scoped>
.cd-page {
  font-family: 'El Messiri', Tahoma, sans-serif;
  color: #00201e;
  padding: 1rem 1.25rem 2rem;
  max-width: 64rem;
  margin: 0 auto;
}
.cd-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.cd-sub {
  margin: 0.35rem 0 0;
  color: #3d524f;
  font-size: 14px;
}
.cd-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1.25rem 0 0.75rem;
}
.cd-search {
  display: flex;
  flex: 1 1 14rem;
  max-width: 28rem;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
}
.cd-search input,
.cd-toolbar select {
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
.cd-search input {
  flex: 1;
  border: 0;
  background: transparent;
  min-height: 44px;
  padding: 0 0.75rem;
}
.cd-search span {
  width: 44px;
  display: grid;
  place-items: center;
  color: #006a64;
}
.cd-toolbar select {
  min-height: 44px;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  padding: 0 0.75rem;
}
.cd-meta {
  font-size: 14px;
  color: #3d524f;
}
.cd-state {
  padding: 1.5rem;
  text-align: center;
  color: #3d524f;
}
.cd-err {
  color: #8b1a1a;
}
.cd-muted {
  color: #3d524f;
  font-size: 13px;
}
.cd-layout {
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 1rem;
}
@media (max-width: 768px) {
  .cd-layout {
    grid-template-columns: 1fr;
  }
}
.cd-list {
  border: 1px solid #d7e0df;
  border-radius: 10px;
  overflow: hidden;
  max-height: 70vh;
  overflow-y: auto;
}
.cd-row {
  display: block;
  width: 100%;
  text-align: right;
  border: 0;
  border-bottom: 1px solid #e5eceb;
  background: #fff;
  padding: 0.85rem 1rem;
  font-family: inherit;
  cursor: pointer;
}
.cd-row.active,
.cd-row:hover {
  background: #f3f7f6;
}
.cd-row-top {
  display: flex;
  justify-content: space-between;
  gap: 0.5rem;
}
.cd-detail {
  border: 1px solid #d7e0df;
  border-radius: 10px;
  padding: 1rem;
  background: #fff;
}
.cd-note {
  margin-top: 1rem;
  padding: 0.75rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.cd-note h3 {
  margin: 0 0 0.35rem;
  font-size: 14px;
  color: #006a64;
}
.cd-note p {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
}
</style>
