<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

const props = withDefaults(
  defineProps<{ audience?: 'delegate' | 'branch' }>(),
  { audience: 'delegate' }
)

interface Row {
  id: number
  orderId: number
  customerId: number
  branchId: number
  customerName: string
  customerPhone: string | null
  rating: number
  note: string | null
  reasonBlock: string
  status: string
  followerName: string | null
  createdAtUtc: string
  isDelegateUnread: boolean
  isBranchUnreviewed: boolean
  expectedVersion: number
}

interface ListResponse {
  items: Row[]
  totalCount: number
  unreadCount: number
  unreviewedCount: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('WrongSale', 'Read') === true)
const canMarkRead = computed(
  () => auth.hasPermission('WrongSale', 'MarkDelegateRead') === true
)
const canReview = computed(
  () => auth.hasPermission('WrongSale', 'MarkBranchReviewed') === true
)

const mailbox = computed(() => String(route.query.mailbox ?? (props.audience === 'branch' ? 'unreviewed' : 'unread')))

const listQuery = useQuery({
  queryKey: computed(() => ['wrong-sales', mailbox.value, props.audience, auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/WrongSales', {
      signal,
      params: { page: 1, pageSize: 50, mailbox: mailbox.value || undefined },
    })
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

const reviewTarget = ref<Row | null>(null)
const reviewNote = ref('')
const reviewError = ref<string | null>(null)

const noteValid = computed(() => {
  const t = reviewNote.value.trim()
  return t.length >= 3 && t.length <= 2000
})

const markReadMut = useMutation({
  mutationFn: async (row: Row) => {
    await axiosInstance.post(`/admin/WrongSales/${row.id}/mark-delegate-read`, {
      expectedVersion: row.expectedVersion,
    })
  },
  onSuccess: async () => {
    await qc.invalidateQueries({ queryKey: ['wrong-sales'] })
  },
})

const reviewMut = useMutation({
  mutationFn: async () => {
    const row = reviewTarget.value
    if (!row || !noteValid.value) throw new Error('invalid note')
    await axiosInstance.post(`/admin/WrongSales/${row.id}/mark-branch-reviewed`, {
      expectedVersion: row.expectedVersion,
      note: reviewNote.value.trim(),
    })
  },
  onSuccess: async () => {
    reviewTarget.value = null
    reviewNote.value = ''
    reviewError.value = null
    await qc.invalidateQueries({ queryKey: ['wrong-sales'] })
  },
})

function openReview(row: Row) {
  reviewTarget.value = row
  reviewNote.value = ''
  reviewError.value = null
}

async function submitReview() {
  reviewError.value = null
  if (!noteValid.value) {
    reviewError.value = 'الملاحظة بين 3 و2000 حرف.'
    return
  }
  try {
    await reviewMut.mutateAsync()
  } catch {
    reviewError.value = 'تعذر حفظ المراجعة. تحقق من الصلاحية أو التعارض.'
  }
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
    class="ws-page"
    dir="rtl"
  >
    <header>
      <h1 class="ws-title">المبيعات الخاطئة</h1>
      <p class="ws-sub">
        ناتج مصادقة متابع بتقييم ≤6 — ليس رفض طلب بيع ولا استثناء Hold عادي.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="ws-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة المبيعات الخاطئة.
    </div>
    <template v-else>
      <div class="ws-toolbar">
        <select
          :value="mailbox"
          aria-label="صندوق"
          @change="patch({ mailbox: ($event.target as HTMLSelectElement).value || null })"
        >
          <option value="all">الكل</option>
          <option value="unread">غير مقروء (مفوض)</option>
          <option value="read">مقروء (مفوض)</option>
          <option value="unreviewed">بانتظار مراجعة الفرع</option>
          <option value="reviewed">راجعتها إدارة الفرع</option>
        </select>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="ws-btn"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p class="ws-meta">
        الإجمالي: {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
        · غير مقروء: {{ listQuery.data.value?.unreadCount ?? '—' }}
        · بانتظار مراجعة: {{ listQuery.data.value?.unreviewedCount ?? '—' }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="ws-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="ws-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="ws-state"
      >
        لا مبيعات خاطئة في هذا الصندوق.
      </div>
      <ul
        v-else
        class="ws-list"
      >
        <li
          v-for="row in listQuery.data.value?.items ?? []"
          :key="row.id"
        >
          <div class="ws-head">
            <strong>{{ row.customerName }}</strong>
            <span :class="row.isDelegateUnread ? 'badge unread' : 'badge'">
              {{ row.isDelegateUnread ? 'غير مقروء' : 'مقروء' }}
            </span>
            <span :class="row.isBranchUnreviewed ? 'badge warn' : 'badge ok'">
              {{ row.isBranchUnreviewed ? 'بانتظار الفرع' : 'راجعه الفرع' }}
            </span>
          </div>
          <p class="ws-line">تقييم {{ row.rating }} · {{ row.followerName ?? 'متابع' }} · {{ fmtDate(row.createdAtUtc) }}</p>
          <pre class="ws-reason">{{ row.reasonBlock }}</pre>
          <div class="ws-actions">
            <button
              v-if="canMarkRead && row.isDelegateUnread"
              type="button"
              class="ws-btn"
              :disabled="markReadMut.isPending.value"
              @click="markReadMut.mutate(row)"
            >
              تعليم مقروء
            </button>
            <button
              v-if="canReview && row.isBranchUnreviewed"
              type="button"
              class="ws-btn primary"
              @click="openReview(row)"
            >
              مراجعة الفرع
            </button>
          </div>
        </li>
      </ul>

      <div
        v-if="reviewTarget"
        class="ws-backdrop"
        @click.self="reviewTarget = null"
      >
        <div
          class="ws-dialog"
          role="dialog"
          aria-modal="true"
        >
          <h2>مراجعة المبيع الخاطئ</h2>
          <p class="ws-line">{{ reviewTarget.customerName }} · تقييم {{ reviewTarget.rating }}</p>
          <label class="ws-field">ملاحظة المراجعة (3–2000)
            <textarea
              v-model="reviewNote"
              rows="4"
              maxlength="2000"
            />
          </label>
          <p
            v-if="reviewError"
            class="ws-err"
          >
            {{ reviewError }}
          </p>
          <div class="ws-actions">
            <button
              type="button"
              class="ws-btn"
              @click="reviewTarget = null"
            >
              إلغاء
            </button>
            <button
              type="button"
              class="ws-btn primary"
              :disabled="!noteValid || reviewMut.isPending.value"
              @click="submitReview"
            >
              تأكيد المراجعة
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.ws-page {
  --c-primary: #006a64;
  --c-ink: #00201e;
  --c-field: #f8f8f8;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: var(--c-ink);
  padding: 1rem 1.25rem 2rem;
  max-width: 900px;
  margin: 0 auto;
}
.ws-title {
  margin: 0;
  color: var(--c-primary);
  font-size: 1.5rem;
}
.ws-sub {
  margin: 0.35rem 0 1rem;
  color: #445;
}
.ws-toolbar {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.ws-toolbar select,
.ws-btn {
  font: inherit;
  border-radius: 8px;
  border: 1px solid #c5cecc;
  background: #fff;
  padding: 0.4rem 0.75rem;
}
.ws-btn.primary {
  background: var(--c-primary);
  border-color: var(--c-primary);
  color: #fff;
}
.ws-meta {
  font-size: 0.9rem;
}
.ws-err {
  color: #9b1c1c;
}
.ws-state {
  padding: 1.5rem;
  text-align: center;
  background: var(--c-field);
  border-radius: 12px;
}
.ws-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.75rem;
}
.ws-list li {
  background: var(--c-field);
  border-radius: 12px;
  padding: 0.85rem 1rem;
}
.ws-head {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  align-items: center;
}
.badge {
  font-size: 0.75rem;
  padding: 0.15rem 0.45rem;
  border-radius: 999px;
  background: #dde;
}
.badge.unread {
  background: #fde68a;
}
.badge.warn {
  background: #fed7aa;
}
.badge.ok {
  background: #bbf7d0;
}
.ws-line {
  margin: 0.35rem 0;
  font-size: 0.88rem;
  color: #556;
}
.ws-reason {
  white-space: pre-wrap;
  font: inherit;
  font-size: 0.85rem;
  margin: 0.4rem 0;
  background: #fff;
  padding: 0.55rem;
  border-radius: 8px;
}
.ws-actions {
  display: flex;
  gap: 0.4rem;
  flex-wrap: wrap;
}
.ws-backdrop {
  position: fixed;
  inset: 0;
  background: rgb(0 32 30 / 45%);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 40;
}
.ws-dialog {
  background: #fff;
  border-radius: 14px;
  padding: 1.1rem 1.2rem;
  width: min(480px, 100%);
}
.ws-field {
  display: grid;
  gap: 0.3rem;
  margin: 0.65rem 0;
  font-size: 0.9rem;
}
.ws-field textarea {
  font: inherit;
  background: var(--c-field);
  border: 1px solid #d0d5d4;
  border-radius: 8px;
  padding: 0.5rem 0.65rem;
  resize: vertical;
}
</style>
