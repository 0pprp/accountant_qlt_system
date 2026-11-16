<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { AppButton, AppInput, AppSearchField, AppSelect, AppTextarea } from '@/modules/Core/components/ui'

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
  if (dialog.value === 'approve') {
    const n = Number(amount.value)
    if (!Number.isFinite(n) || n <= 0) {
      formError.value = 'أدخل مبلغ موافقة أكبر من صفر.'
      return
    }
  }
  if (dialog.value === 'create') {
    const cid = Number(createCustomerId.value)
    if (!Number.isInteger(cid) || cid <= 0) {
      formError.value = 'معرّف الزبون غير صالح.'
      return
    }
    if (!note.value.trim()) {
      formError.value = 'الملاحظة مطلوبة.'
      return
    }
  }
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

function applyWithConfirm() {
  if (
    !window.confirm(
      'تطبيق الخصم يخصم المبلغ من سعر البيع ولا يمكن التراجع بسهولة. هل تريد المتابعة؟',
    )
  ) {
    return
  }
  applyMut.mutate()
}

function prevPage() {
  if (page.value <= 1) return
  patch({ page: String(page.value - 1) })
}

function nextPage() {
  const total = listQuery.data.value?.totalCount ?? 0
  const pageSize = listQuery.data.value?.pageSize ?? 20
  if (page.value * pageSize >= total) return
  patch({ page: String(page.value + 1) })
}

const statusOptions = [
  { label: 'كل الحالات', value: '' },
  { label: 'قيد مراجعة المفوض', value: 'Pending' },
  { label: 'بانتظار المحاسب', value: 'AwaitingAccountant' },
  { label: 'مطبّق', value: 'Approved' },
  { label: 'مرفوض', value: 'Rejected' },
]

function onStatus(v: string | number | null | boolean) {
  patch({ status: v == null || v === '' ? null : String(v), page: '1' })
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
      class="q-state q-state--forbidden"
      role="alert"
    >
      ليس لديك صلاحية قراءة التنازلات.
    </div>
    <template v-else>
      <div class="q-filters">
        <AppSearchField
          v-model="searchInput"
          label="بحث"
          placeholder="بحث زبون أو ملاحظة…"
          grow
        />
        <AppSelect
          :model-value="status"
          label="الحالة"
          :options="statusOptions"
          @update:model-value="onStatus"
        />
        <AppButton
          v-if="canCreate && scope === 'branch'"
          variant="primary"
          @click="dialog = 'create'; note = ''; createCustomerId = ''; formError = null"
        >
          طلب تنازل جديد
        </AppButton>
      </div>
      <p class="wv-meta">
        قيد المفوض: {{ listQuery.data.value?.pendingCount ?? (listQuery.isError.value ? '—' : '…') }}
        · بانتظار المحاسب:
        {{ listQuery.data.value?.awaitingAccountantCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="q-state q-state--error"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="q-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="q-state"
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
          <div
            v-if="(listQuery.data.value?.totalCount ?? 0) > (listQuery.data.value?.pageSize ?? 20)"
            class="wv-pager"
          >
            <AppButton
              variant="secondary"
              size="sm"
              :disabled="page <= 1"
              @click="prevPage"
            >
              السابق
            </AppButton>
            <span class="wv-pager__meta">
              صفحة {{ page }}
              · {{ listQuery.data.value?.totalCount ?? 0 }} طلب
            </span>
            <AppButton
              variant="secondary"
              size="sm"
              :disabled="page * (listQuery.data.value?.pageSize ?? 20) >= (listQuery.data.value?.totalCount ?? 0)"
              @click="nextPage"
            >
              التالي
            </AppButton>
          </div>
        </div>
        <aside class="wv-detail">
          <div
            v-if="selectedId == null"
            class="q-state"
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
              <AppButton
                v-if="canDecide && detailQuery.data.value.status === 'Pending'"
                @click="dialog = 'approve'; amount = ''; note = ''; formError = null"
              >
                قبول بمبلغ
              </AppButton>
              <AppButton
                v-if="canDecide && detailQuery.data.value.status === 'Pending'"
                variant="secondary"
                @click="dialog = 'reject'; note = ''; formError = null"
              >
                رفض
              </AppButton>
              <AppButton
                v-if="canApply && detailQuery.data.value.status === 'AwaitingAccountant'"
                :disabled="applyMut.isPending.value"
                :loading="applyMut.isPending.value"
                @click="applyWithConfirm"
              >
                تطبيق الخصم
              </AppButton>
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
        <AppInput
          v-if="dialog === 'create'"
          v-model="createCustomerId"
          label="معرّف الزبون"
          type="number"
        />
        <AppInput
          v-if="dialog === 'approve'"
          v-model="amount"
          label="المبلغ"
          type="number"
        />
        <AppTextarea v-model="note" label="ملاحظة" :rows="4" />
        <p
          v-if="formError"
          class="wv-err"
        >
          {{ formError }}
        </p>
        <div class="wv-actions">
          <AppButton variant="secondary" @click="dialog = null">
            إلغاء
          </AppButton>
          <AppButton :disabled="submitting" :loading="submitting" @click="submitDialog">
            تأكيد
          </AppButton>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.wv-page {
  font-family: inherit;
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
.wv-meta {
  font-size: 14px;
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
.wv-pager {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  padding: 0.65rem 0.85rem;
  border-top: 1px solid #e5eceb;
  background: #f8faf9;
}
.wv-pager__meta {
  font-size: 13px;
  color: #3d524f;
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
</style>
