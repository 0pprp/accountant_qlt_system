<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface LegalNameRow {
  customerId: number
  customerName: string
  phone: string | null
  branchId: number
  branchName: string | null
  orderListId: number | null
  orderListName: string | null
  saleTotal: number
  weekPaid: number
  remaining: number
  paidPercent: number
  lastPaymentDate: string | null
  daysSinceLastPayment: number
}

interface LegalNamesResponse {
  items: LegalNameRow[]
  totalCount: number
  page: number
  pageSize: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('LegalNames', 'Read') === true)
const canDecide = computed(
  () => auth.hasPermission('CustomerDecision', 'Create') === true
)
const canCreateWaiver = computed(() => auth.hasPermission('Waiver', 'Create') === true)

const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const searchInput = ref(String(route.query.q ?? ''))
const search = ref(searchInput.value)
const selectedId = ref<number | null>(null)
const dialogOpen = ref(false)
const decisionType = ref<'Continue' | 'FakeSale' | 'LegalRequest' | 'WaiverRequest'>(
  'Continue'
)
const note = ref('')
const formError = ref<string | null>(null)
const submitting = ref(false)

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
    'legal-names',
    page.value,
    search.value,
    auth.branchGeneration,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<LegalNamesResponse>(
      '/admin/CustomerDecisions/legal-names',
      {
        signal,
        params: {
          page: page.value,
          pageSize: 50,
          search: search.value || undefined,
        },
      }
    )
    return data
  },
})

const selected = computed(
  () => listQuery.data.value?.items.find((r) => r.customerId === selectedId.value) ?? null
)

function patch(next: Record<string, string | null | undefined>) {
  const q = { ...(route.query as Record<string, string>) }
  for (const [k, v] of Object.entries(next)) {
    if (v == null || v === '') delete q[k]
    else q[k] = v
  }
  void router.replace({ name: route.name ?? undefined, query: q })
}

const postMut = useMutation({
  mutationFn: async () => {
    const { data } = await axiosInstance.post('/admin/CustomerDecisions', {
      customerId: selectedId.value,
      decisionType: decisionType.value,
      note: note.value,
    })
    return data
  },
  onSuccess: async () => {
    dialogOpen.value = false
    selectedId.value = null
    await qc.invalidateQueries({ queryKey: ['legal-names'] })
    await qc.invalidateQueries({ queryKey: ['customer-decisions'] })
    await qc.invalidateQueries({ queryKey: ['waivers'] })
  },
})

async function submitDecision() {
  formError.value = null
  if (!note.value.trim()) {
    formError.value = 'السبب مطلوب.'
    return
  }
  if (decisionType.value === 'WaiverRequest' && !canCreateWaiver.value) {
    formError.value = 'لا صلاحية لإنشاء طلب تنازل.'
    return
  }
  submitting.value = true
  try {
    await postMut.mutateAsync()
  } catch {
    formError.value = 'تعذر تسجيل القرار. تحقق من الطابور والصلاحية.'
  } finally {
    submitting.value = false
  }
}

function openDecision(row: LegalNameRow) {
  selectedId.value = row.customerId
  decisionType.value = 'Continue'
  note.value = ''
  formError.value = null
  dialogOpen.value = true
}

function fmt(n: number) {
  return new Intl.NumberFormat('ar-IQ').format(n)
}
</script>

<template>
  <div
    class="ln-page"
    dir="rtl"
  >
    <header>
      <h1 class="ln-title">الأسماء القانونية</h1>
      <p class="ln-sub">
        طابور ضعاف الدفع وفق قواعد المصدر — ليست متابعة قانونية للمبيعات.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="ln-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة الأسماء القانونية.
    </div>
    <template v-else>
      <div class="ln-toolbar">
        <div class="ln-search">
          <input
            v-model="searchInput"
            type="search"
            placeholder="بحث بالاسم أو الهاتف…"
            aria-label="بحث"
          >
          <span aria-hidden="true">🔍</span>
        </div>
      </div>
      <p class="ln-meta">
        الإجمالي:
        {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="ln-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="ln-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="ln-state"
      >
        لا أسماء في الطابور حاليًا.
      </div>
      <div
        v-else
        class="ln-table-wrap"
      >
        <table class="ln-table">
          <thead>
            <tr>
              <th>الزبون</th>
              <th>الهاتف</th>
              <th>المبيع</th>
              <th>المتبقي</th>
              <th>نسبة التسديد</th>
              <th>أيام منذ آخر دفعة</th>
              <th>إجراء</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in listQuery.data.value?.items ?? []"
              :key="row.customerId"
            >
              <td>
                <strong>{{ row.customerName }}</strong>
                <div class="ln-muted">{{ row.branchName }}</div>
              </td>
              <td>{{ row.phone || '—' }}</td>
              <td>{{ fmt(row.saleTotal) }}</td>
              <td>{{ fmt(row.remaining) }}</td>
              <td>{{ Math.round(row.paidPercent) }}%</td>
              <td>{{ row.daysSinceLastPayment }}</td>
              <td>
                <button
                  v-if="canDecide"
                  type="button"
                  class="ln-btn primary"
                  @click="openDecision(row)"
                >
                  قرار
                </button>
                <span
                  v-else
                  class="ln-muted"
                >قراءة فقط</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div
      v-if="dialogOpen && selected"
      class="ln-backdrop"
      @click.self="dialogOpen = false"
    >
      <div
        class="ln-dialog"
        role="dialog"
        aria-modal="true"
        :aria-label="`قرار بشأن ${selected.customerName}`"
      >
        <h2>قرار — {{ selected.customerName }}</h2>
        <p class="ln-muted">
          متبقي {{ fmt(selected.remaining) }} · تسديد
          {{ Math.round(selected.paidPercent) }}%
        </p>
        <fieldset class="ln-field">
          <legend>نوع القرار</legend>
          <label><input
            v-model="decisionType"
            type="radio"
            value="Continue"
          > استمرار (إرجاء أسبوع)</label>
          <label><input
            v-model="decisionType"
            type="radio"
            value="FakeSale"
          > بيع وهمي</label>
          <label><input
            v-model="decisionType"
            type="radio"
            value="LegalRequest"
          > طلب تحويل قانوني</label>
          <label v-if="canCreateWaiver"><input
            v-model="decisionType"
            type="radio"
            value="WaiverRequest"
          > طلب تنازل</label>
        </fieldset>
        <label class="ln-field">
          السبب (إلزامي)
          <textarea
            v-model="note"
            rows="4"
            required
          />
        </label>
        <p
          v-if="formError"
          class="ln-err"
        >
          {{ formError }}
        </p>
        <div class="ln-actions">
          <button
            type="button"
            class="ln-btn"
            @click="dialogOpen = false"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="ln-btn primary"
            :disabled="submitting"
            @click="submitDecision"
          >
            تأكيد القرار
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ln-page {
  font-family: 'El Messiri', Tahoma, sans-serif;
  color: #00201e;
  padding: 1rem 1.25rem 2rem;
  max-width: 72rem;
  margin: 0 auto;
}
.ln-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.ln-sub {
  margin: 0.35rem 0 0;
  color: #3d524f;
  font-size: 14px;
}
.ln-toolbar {
  margin: 1.25rem 0 0.75rem;
}
.ln-search {
  display: flex;
  max-width: 28rem;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
}
.ln-search input {
  flex: 1;
  border: 0;
  background: transparent;
  min-height: 44px;
  padding: 0 0.75rem;
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
.ln-search span {
  width: 44px;
  display: grid;
  place-items: center;
  color: #006a64;
}
.ln-meta {
  font-size: 14px;
  color: #3d524f;
}
.ln-state {
  padding: 1.5rem;
  text-align: center;
  color: #3d524f;
}
.ln-err {
  color: #8b1a1a;
}
.ln-muted {
  color: #3d524f;
  font-size: 13px;
}
.ln-table-wrap {
  overflow-x: auto;
  border: 1px solid #d7e0df;
  border-radius: 10px;
}
.ln-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}
.ln-table th,
.ln-table td {
  padding: 0.75rem;
  border-bottom: 1px solid #e5eceb;
  text-align: right;
  vertical-align: top;
}
.ln-table th {
  background: #f3f7f6;
  color: #006a64;
}
.ln-btn {
  min-height: 40px;
  padding: 0 0.85rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  cursor: pointer;
}
.ln-btn.primary {
  background: #006a64;
  color: #fff;
}
.ln-btn:disabled {
  opacity: 0.55;
}
.ln-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.ln-dialog {
  width: min(30rem, 100%);
  background: #fff;
  border-radius: 12px;
  padding: 1.25rem;
}
.ln-field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  margin: 0.85rem 0;
  border: 0;
  padding: 0;
}
.ln-field textarea {
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  background: #f8f8f8;
  padding: 0.75rem;
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
.ln-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: flex-end;
}
@media (max-width: 768px) {
  .ln-table th:nth-child(n + 3):nth-child(-n + 5),
  .ln-table td:nth-child(n + 3):nth-child(-n + 5) {
    display: none;
  }
}
</style>
