<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Entry {
  id: number
  productName: string
  salePrice: number
  dailyInstallment: number
  downPayment: number
  targetProvinceId: number
  targetProvinceName: string
  createdByName: string | null
  updatedByName: string | null
  updatedAtUtc: string
  expectedVersion: number
}

interface ListResponse {
  items: Entry[]
  totalCount: number
  page: number
  pageSize: number
}

interface ProvinceRow {
  id: number
  name: string
}

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()
const qc = useQueryClient()

const canRead = computed(() => auth.hasPermission('PriceBoard', 'Read') === true)
const canCreate = computed(() => auth.hasPermission('PriceBoard', 'Create') === true)
const canUpdate = computed(() => auth.hasPermission('PriceBoard', 'Update') === true)
const canDelete = computed(() => auth.hasPermission('PriceBoard', 'Delete') === true)

const page = computed(() => {
  const n = Number(route.query.page)
  return Number.isInteger(n) && n > 0 ? n : 1
})
const provinceId = computed(() => {
  const n = Number(route.query.provinceId)
  return Number.isInteger(n) && n > 0 ? n : null
})
const searchInput = ref(String(route.query.q ?? ''))
const search = ref(searchInput.value)
const dialog = ref<'create' | 'edit' | null>(null)
const editing = ref<Entry | null>(null)
const form = ref({
  productName: '',
  salePrice: '0',
  dailyInstallment: '0',
  downPayment: '0',
  targetProvinceId: '',
})
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
    'price-board',
    page.value,
    provinceId.value,
    search.value,
    auth.branchGeneration,
    auth.sessionGeneration,
  ]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/PriceBoard', {
      signal,
      params: {
        page: page.value,
        pageSize: 50,
        provinceId: provinceId.value || undefined,
        search: search.value || undefined,
      },
    })
    return data
  },
})

const provincesQuery = useQuery({
  queryKey: computed(() => ['provinces-mini', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get('/admin/Provinces', {
      signal,
      params: { pageNumber: 1, pageSize: 200 },
    })
    const items = (data?.items ?? data?.Items ?? data ?? []) as ProvinceRow[]
    return Array.isArray(items) ? items : []
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

function openCreate() {
  editing.value = null
  form.value = {
    productName: '',
    salePrice: '0',
    dailyInstallment: '0',
    downPayment: '0',
    targetProvinceId: provinceId.value ? String(provinceId.value) : '',
  }
  formError.value = null
  dialog.value = 'create'
}

function openEdit(row: Entry) {
  editing.value = row
  form.value = {
    productName: row.productName,
    salePrice: String(row.salePrice),
    dailyInstallment: String(row.dailyInstallment),
    downPayment: String(row.downPayment),
    targetProvinceId: String(row.targetProvinceId),
  }
  formError.value = null
  dialog.value = 'edit'
}

const saveMut = useMutation({
  mutationFn: async () => {
    const body = {
      productName: form.value.productName.trim(),
      salePrice: Number(form.value.salePrice),
      dailyInstallment: Number(form.value.dailyInstallment),
      downPayment: Number(form.value.downPayment),
      targetProvinceId: Number(form.value.targetProvinceId),
    }
    if (dialog.value === 'create') {
      await axiosInstance.post('/admin/PriceBoard', body)
    } else {
      await axiosInstance.put(`/admin/PriceBoard/${editing.value!.id}`, {
        ...body,
        expectedVersion: editing.value!.expectedVersion,
      })
    }
  },
  onSuccess: async () => {
    dialog.value = null
    await qc.invalidateQueries({ queryKey: ['price-board'] })
  },
})

const deleteMut = useMutation({
  mutationFn: async (row: Entry) => {
    await axiosInstance.delete(`/admin/PriceBoard/${row.id}`, {
      params: { expectedVersion: row.expectedVersion },
    })
  },
  onSuccess: async () => qc.invalidateQueries({ queryKey: ['price-board'] }),
})

async function submit() {
  formError.value = null
  submitting.value = true
  try {
    await saveMut.mutateAsync()
  } catch {
    formError.value = 'تعذر الحفظ. تحقق من البيانات والصلاحية.'
  } finally {
    submitting.value = false
  }
}

function fmt(n: number) {
  return new Intl.NumberFormat('ar-IQ').format(n)
}
</script>

<template>
  <div
    class="pb-page"
    dir="rtl"
  >
    <header>
      <h1 class="pb-title">لوحة الأسعار</h1>
      <p class="pb-sub">
        إعلانات أسعار للمحافظات — معلوماتية فقط ولا تعدّل أسعار المنتجات أو المبيعات.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="pb-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة لوحة الأسعار.
    </div>
    <template v-else>
      <div class="pb-toolbar">
        <div class="pb-search">
          <input
            v-model="searchInput"
            type="search"
            placeholder="بحث منتج أو محافظة…"
            aria-label="بحث"
          >
          <span aria-hidden="true">🔍</span>
        </div>
        <select
          :value="provinceId ?? ''"
          aria-label="المحافظة"
          @change="
            patch({
              provinceId: ($event.target as HTMLSelectElement).value || null,
              page: '1',
            })
          "
        >
          <option value="">كل المحافظات</option>
          <option
            v-for="p in provincesQuery.data.value ?? []"
            :key="p.id"
            :value="p.id"
          >
            {{ p.name }}
          </option>
        </select>
        <button
          v-if="canCreate"
          type="button"
          class="pb-btn primary"
          @click="openCreate"
        >
          إضافة إعلان
        </button>
      </div>
      <p class="pb-meta">
        الإجمالي:
        {{ listQuery.data.value?.totalCount ?? (listQuery.isError.value ? '—' : '…') }}
      </p>
      <p
        v-if="listQuery.isError.value"
        class="pb-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="pb-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="!(listQuery.data.value?.items.length)"
        class="pb-state"
      >
        لا إعلانات أسعار.
      </div>
      <div
        v-else
        class="pb-table-wrap"
      >
        <table class="pb-table">
          <thead>
            <tr>
              <th>المنتج</th>
              <th>المحافظة</th>
              <th>سعر البيع</th>
              <th>القسط اليومي</th>
              <th>المقدمة</th>
              <th>إجراء</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in listQuery.data.value?.items ?? []"
              :key="row.id"
            >
              <td><strong>{{ row.productName }}</strong></td>
              <td>{{ row.targetProvinceName }}</td>
              <td>{{ fmt(row.salePrice) }}</td>
              <td>{{ fmt(row.dailyInstallment) }}</td>
              <td>{{ fmt(row.downPayment) }}</td>
              <td class="pb-actions">
                <button
                  v-if="canUpdate"
                  type="button"
                  class="pb-btn"
                  @click="openEdit(row)"
                >
                  تعديل
                </button>
                <button
                  v-if="canDelete"
                  type="button"
                  class="pb-btn"
                  :disabled="deleteMut.isPending.value"
                  @click="deleteMut.mutate(row)"
                >
                  حذف
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <div
      v-if="dialog"
      class="pb-backdrop"
      @click.self="dialog = null"
    >
      <div
        class="pb-dialog"
        role="dialog"
        aria-modal="true"
      >
        <h2>{{ dialog === 'create' ? 'إضافة إعلان أسعار' : 'تعديل إعلان أسعار' }}</h2>
        <label class="pb-field">اسم المنتج
          <input
            v-model="form.productName"
            type="text"
            maxlength="400"
          >
        </label>
        <label class="pb-field">المحافظة
          <select v-model="form.targetProvinceId">
            <option value="">اختر…</option>
            <option
              v-for="p in provincesQuery.data.value ?? []"
              :key="p.id"
              :value="String(p.id)"
            >
              {{ p.name }}
            </option>
          </select>
        </label>
        <label class="pb-field">سعر البيع
          <input
            v-model="form.salePrice"
            type="number"
            min="0"
            step="0.01"
          >
        </label>
        <label class="pb-field">القسط اليومي
          <input
            v-model="form.dailyInstallment"
            type="number"
            min="0"
            step="0.01"
          >
        </label>
        <label class="pb-field">المقدمة
          <input
            v-model="form.downPayment"
            type="number"
            min="0"
            step="0.01"
          >
        </label>
        <p
          v-if="formError"
          class="pb-err"
        >
          {{ formError }}
        </p>
        <div class="pb-actions">
          <button
            type="button"
            class="pb-btn"
            @click="dialog = null"
          >
            إلغاء
          </button>
          <button
            type="button"
            class="pb-btn primary"
            :disabled="submitting"
            @click="submit"
          >
            حفظ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.pb-page {
  font-family: 'El Messiri', Tahoma, sans-serif;
  color: #00201e;
  padding: 1rem 1.25rem 2rem;
  max-width: 72rem;
  margin: 0 auto;
}
.pb-title {
  margin: 0;
  color: #006a64;
  font-size: 1.75rem;
}
.pb-sub {
  margin: 0.35rem 0 0;
  color: #3d524f;
  font-size: 14px;
}
.pb-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin: 1.25rem 0 0.75rem;
}
.pb-search {
  display: flex;
  flex: 1 1 14rem;
  max-width: 28rem;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  overflow: hidden;
}
.pb-search input,
.pb-toolbar select,
.pb-field input,
.pb-field select {
  font-family: inherit;
  font-size: 16px;
  color: #00201e;
}
.pb-search input {
  flex: 1;
  border: 0;
  background: transparent;
  min-height: 44px;
  padding: 0 0.75rem;
}
.pb-search span {
  width: 44px;
  display: grid;
  place-items: center;
  color: #006a64;
}
.pb-toolbar select {
  min-height: 44px;
  background: #f8f8f8;
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  padding: 0 0.75rem;
}
.pb-btn {
  min-height: 40px;
  padding: 0 0.85rem;
  border-radius: 8px;
  border: 1px solid #006a64;
  background: #fff;
  color: #006a64;
  font-family: inherit;
  cursor: pointer;
}
.pb-btn.primary {
  background: #006a64;
  color: #fff;
}
.pb-btn:disabled {
  opacity: 0.55;
}
.pb-meta {
  font-size: 14px;
  color: #3d524f;
}
.pb-state {
  padding: 1.5rem;
  text-align: center;
  color: #3d524f;
}
.pb-err {
  color: #8b1a1a;
}
.pb-table-wrap {
  overflow-x: auto;
  border: 1px solid #d7e0df;
  border-radius: 10px;
}
.pb-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}
.pb-table th,
.pb-table td {
  padding: 0.75rem;
  border-bottom: 1px solid #e5eceb;
  text-align: right;
}
.pb-table th {
  background: #f3f7f6;
  color: #006a64;
}
.pb-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}
.pb-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  z-index: 40;
  padding: 1rem;
}
.pb-dialog {
  width: min(28rem, 100%);
  background: #fff;
  border-radius: 12px;
  padding: 1.25rem;
}
.pb-field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 0.75rem;
}
.pb-field input,
.pb-field select {
  border: 1px solid #c5c5c5;
  border-radius: 8px;
  background: #f8f8f8;
  padding: 0.75rem;
  min-height: 44px;
}
</style>
