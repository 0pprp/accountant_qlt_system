<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface WaiverSummary {
  id: number
  customerId: number
  customerName: string
  customerPhone: string | null
  status: string
  statusLabel: string
  requestNote: string
  requesterName: string | null
  approvedAmount: number | null
  appliedAmount: number | null
  createdAtUtc: string
  expectedVersion: number
}

interface WaiverListResponse {
  items: WaiverSummary[]
  totalCount: number
  page: number
  pageSize: number
  pendingCount: number
  awaitingAccountantCount: number
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('Waiver', 'Read') === true)
const canCreate = computed(() => auth.hasPermission('Waiver', 'Create') === true)
const canDecide = computed(() => auth.hasPermission('Waiver', 'Decide') === true)
const canApply = computed(() => auth.hasPermission('Waiver', 'Apply') === true)
const scope = computed(() => String(route.query.scope ?? route.meta.managementSectionId ?? 'oversight'))
const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const status = computed(() => String(route.query.status ?? ''))
const searchInput = ref(String(route.query.q ?? ''))
const search = ref(searchInput.value)
const selectedId = ref<number | null>(null)
const dialog = ref<'approve' | 'reject' | 'create' | null>(null)
const amount = ref('')
const note = ref('')
const createCustomerId = ref('')
const formError = ref<string | null>(null)
const submitting = ref(false)

let t: ReturnType<typeof setTimeout> | null = null
watch(searchInput, (v) => {
  if (t) clearTimeout(t)
  t = setTimeout(() => {
    search.value = v.trim()
    patch({ q: search.value || null, page: '1' })
  }, 300)
})

const listQuery = useQuery({
  queryKey: computed(() => [
    'waivers',
    scope.value,
    page.value,
    status.value,
    search.value,
    auth.branchGeneration,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<WaiverListResponse>('/admin/Waivers', {
      signal,
      params: {
        page: page.value,
        pageSize: 20,
        status: status.value || undefined,
        search: search.value || undefined,
        mineOnly: scope.value === 'branch' || undefined,
      },
    })
    return data
  },
})

const detailQuery = useQuery({
  queryKey: computed(() => ['waivers', 'detail', selectedId.value]),
  enabled: computed(() => canRead.value && selectedId.value != null),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get(`/admin/Waivers/${selectedId.value}`, { signal })
    return data as WaiverSummary & {
      expectedVersion: number
      decisionNote: string | null
      orderRemaining: number | null
    }
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

const approveMut = useMutation({
  mutationFn: async () => {
    const id = selectedId.value!
    const ver = detailQuery.data.value?.expectedVersion ?? 0
    const { data } = await axiosInstance.post(`/admin/Waivers/${id}/approve`, {
      approvedAmount: Number(amount.value),
      expectedVersion: ver,
      note: note.value || undefined,
    })
    return data
  },
  onSuccess: async () => {
    dialog.value = null
    await qc.invalidateQueries({ queryKey: ['waivers'] })
  },
})
const rejectMut = useMutation({
  mutationFn: async () => {
    const id = selectedId.value!
    const ver = detailQuery.data.value?.expectedVersion ?? 0
    const { data } = await axiosInstance.post(`/admin/Waivers/${id}/reject`, {
      expectedVersion: ver,
      note: note.value || undefined,
    })
    return data
  },
  onSuccess: async () => {
    dialog.value = null
    await qc.invalidateQueries({ queryKey: ['waivers'] })
  },
})
const applyMut = useMutation({
  mutationFn: async () => {
    const id = selectedId.value!
    const ver = detailQuery.data.value?.expectedVersion ?? 0
    const { data } = await axiosInstance.post(`/admin/Waivers/${id}/apply`, {
      expectedVersion: ver,
    })
    return data
  },
  onSuccess: async () => qc.invalidateQueries({ queryKey: ['waivers'] }),
})
const createMut = useMutation({
  mutationFn: async () => {
    const { data } = await axiosInstance.post('/admin/Waivers', {
      customerId: Number(createCustomerId.value),
      note: note.value,
    })
    return data
  },
  onSuccess: async () => {
    dialog.value = null
    await qc.invalidateQueries({ queryKey: ['waivers'] })
  },
})

async function submitDialog() {
  formError.value = null
  submitting.value = true
  try {
    if (dialog.value === 'approve') await approveMut.mutateAsync()
    else if (dialog.value === 'reject') await rejectMut.mutateAsync()
    else if (dialog.value === 'create') await createMut.mutateAsync()
  } catch {
    formError.value = 'تعذر تنفيذ الإجراء. تحقق من الحالة والصلاحية.'
  } finally {
    submitting.value = false
  }
}

const title = computed(() => {
  if (scope.value === 'branch') return 'طلبات التنازل'
  if (scope.value === 'delegate') return 'طلبات التنازل — المفوض'
  return 'التنازلات'
})
</script>

<template>
  <div
    class="wv-page"
    dir="rtl"
  >
    <header>
      <h1 class="wv-title">{{ title }}</h1>
      <p class="wv-sub">دورة فرع → مفوض → محاسب. الخصم على سعر البيع عند التطبيق فقط.</p>
    </header>

    <div
      v-if="!canRead"
      class="wv-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة التنازلات.
    </div>
    <template v-else>
      <div class="wv-toolbar">
        <div class="wv-search">
          <input
            v-model="searchInput"
            type="search"
            placeholder="بحث زبون أو ملاحظة…"
            aria-label="بحث"
          >
          <span aria-hidden="true">🔍</span>
        </div>
        <select
          :value="status"
          aria-label="الحالة"
          @change="patch({ status: ($event.target as HTMLSelectElement).value || null, page: '1' })"
        >
          <option value="">كل الحالات</option>
          <option value="Pending">قيد مراجعة المفوض</option>
          <option value="AwaitingAccountant">بانتظار المحاسب</option>
          <option value="Approved">مطبّق</option>
          <option value="Rejected">مرفوض</option>
        </select>
        <button
          v-if="canCreate && scope === 'branch'"
          type="button"
          class="wv-btn primary"
          @click="dialog = 'create'; note = ''; createCustomerId = ''; formError = null"
        >
          طلب تنازل جديد
        </button>
      </div>
      <p class="wv-meta">
        قيد المفوض: {{ listQuery.data.value?.pendingCount ?? (listQuery.isError.value ? '—' : '…') }}
        · بانتظار المحاسب:
        {{ listQuery.data.value?.awaitingAccountantCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="wv-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="wv-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="wv-state"
      >
        لا طلبات.
      </div>
      <div
        v-else
        class="wv-layout"
      >
        <div class="wv-list">
          <button
            v-for="row in listQuery.data.value?.items ?? []"
            :key="row.id"
            type="button"
            class="wv-row"
            :class="{ active: selectedId === row.id }"
            @click="selectedId = row.id"
          >
            <strong>{{ row.customerName }}</strong>
            <span>{{ row.statusLabel }}</span>
            <div>{{ row.requestNote }}</div>
          </button>
        </div>
        <aside class="wv-detail">
          <div
            v-if="selectedId == null"
            class="wv-state"
          >
            اختر طلبًا.
          </div>
          <template v-else-if="detailQuery.data.value">
            <h2>{{ detailQuery.data.value.customerName }}</h2>
            <p>{{ detailQuery.data.value.statusLabel }}</p>
            <p>{{ detailQuery.data.value.requestNote }}</p>
            <p v-if="detailQuery.data.value.approvedAmount != null">
              مبلغ الموافقة: {{ detailQuery.data.value.approvedAmount }}
            </p>
            <div class="wv-actions">
              <button
                v-if="canDecide && detailQuery.data.value.status === 'Pending'"
                type="button"
                class="wv-btn primary"
                @click="dialog = 'approve'; amount = ''; note = ''; formError = null"
              >
                قبول بمبلغ
              </button>
              <button
                v-if="canDecide && detailQuery.data.value.status === 'Pending'"
                type="button"
                class="wv-btn"
                @click="dialog = 'reject'; note = ''; formError = null"
              >
                رفض
              </button>
              <button
                v-if="canApply && detailQuery.data.value.status === 'AwaitingAccountant'"
                type="button"
                class="wv-btn primary"
                :disabled="applyMut.isPending.value"
                @click="applyMut.mutate()"
              >
                تطبيق الخصم
              </button>
            </div>
          </template>
        </aside>
      </div>
    </template>

    <div
      v-if="dialog"
      class="wv-backdrop"
      @click.self="dialog = null"
    >
      <div
        class="wv-dialog"
        role="dialog"
        aria-modal="true"
      >
        <h2>
          {{
            dialog === 'approve'
              ? 'قبول التنازل'
              : dialog === 'reject'
                ? 'رفض الطلب'
                : 'طلب تنازل'
          }}
        </h2>
        <label
          v-if="dialog === 'create'"
          class="wv-field"
        >
          معرّف الزبون
          <input
            v-model="createCustomerId"
            type="number"
            min="1"
          >
        </label>
        <label
          v-if="dialog === 'approve'"
          class="wv-field"
        >
          المبلغ
          <input
            v-model="amount"
            type="number"
            min="0.01"
            step="0.01"
          >
        </label>
        <label class="wv-field">
          ملاحظة
          <textarea
            v-model="note"
            rows="4"
          />
        </label>
        <p
          v-if="formError"
          class="wv-err"
        >
          {{ formError }}
        </p>
        <div class="wv-actions">
          <button
            type="button"
            class="wv-btn"
            @click="dialog = null"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="wv-btn primary"
            :disabled="submitting"
            @click="submitDialog"
          >
            تأكيد
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.wv-page {
  font-family: 'El Messiri', Tahoma, sans-serif;
  color: #00201e;
  padding: 1rem 1.25rem 2rem;
  max-width: 64rem;
  margin: 0 auto;
}
.wv-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.wv-sub {
  margin: 0.35rem 0 0;
  color: #3d524f;
  font-size: 14px;
}
.wv-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1.25rem 0 0.75rem;
}
.wv-search {
  display: flex;
  flex: 1 1 14rem;
  max-width: 28rem;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
}
.wv-search input,
.wv-toolbar select,
.wv-field input,
.wv-field textarea {
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
.wv-search input {
  flex: 1;
  border: 0;
  background: transparent;
  min-height: 44px;
  padding: 0 0.75rem;
}
.wv-search span {
  width: 44px;
  display: grid;
  place-items: center;
  color: #006a64;
}
.wv-toolbar select {
  min-height: 44px;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  padding: 0 0.75rem;
}
.wv-btn {
  min-height: 44px;
  padding: 0 1rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  cursor: pointer;
}
.wv-btn.primary {
  background: #006a64;
  color: #fff;
}
.wv-btn:disabled {
  opacity: 0.55;
}
.wv-meta {
  font-size: 14px;
  color: #3d524f;
}
.wv-state {
  padding: 1.5rem;
  text-align: center;
  color: #3d524f;
}
.wv-err {
  color: #8b1a1a;
}
.wv-layout {
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 1rem;
}
@media (max-width: 768px) {
  .wv-layout {
    grid-template-columns: 1fr;
  }
}
.wv-list {
  border: 1px solid #d7e0df;
  border-radius: 10px;
  overflow: hidden;
}
.wv-row {
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
.wv-row.active,
.wv-row:hover {
  background: #f3f7f6;
}
.wv-detail {
  border: 1px solid #d7e0df;
  border-radius: 10px;
  padding: 1rem;
  background: #fff;
}
.wv-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.75rem;
}
.wv-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.wv-dialog {
  width: min(28rem, 100%);
  background: #fff;
  border-radius: 12px;
  padding: 1.25rem;
}
.wv-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 0.75rem;
}
.wv-field input,
.wv-field textarea {
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  background: #f8f8f8;
  padding: 0.75rem;
}
</style>
