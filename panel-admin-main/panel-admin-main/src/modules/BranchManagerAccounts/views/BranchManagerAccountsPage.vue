<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

interface Account {
  id: number
  username: string
  fullName: string
  isActive: boolean
  branchIds: number[]
  branchNames: string[]
}

interface City {
  id: number
  name: string
}

interface ListResponse {
  items: Account[]
  totalCount: number
}

const auth = useAuthStore()
const qc = useQueryClient()
const canRead = computed(() => auth.hasPermission('BranchManagerAccount', 'Read') === true)
const canWrite = computed(() => auth.hasPermission('BranchManagerAccount', 'Write') === true)
const draftError = ref<string | null>(null)
const editing = ref<Account | null>(null)
const creating = ref(false)
const form = reactive({
  username: '',
  password: '',
  fullName: '',
  isActive: true,
  branchIds: [] as number[],
})

const listQuery = useQuery({
  queryKey: computed(() => ['bm-accounts', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchManagerAccounts', {
      signal,
    })
    return data
  },
})

const citiesQuery = useQuery({
  queryKey: computed(() => ['bm-account-cities', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<City[]>('/admin/BranchManagerAccounts/cities', {
      signal,
    })
    return data
  },
})

function openCreate() {
  creating.value = true
  editing.value = null
  draftError.value = null
  form.username = ''
  form.password = ''
  form.fullName = ''
  form.isActive = true
  form.branchIds = []
}

function openEdit(row: Account) {
  creating.value = false
  editing.value = row
  draftError.value = null
  form.username = row.username
  form.password = ''
  form.fullName = row.fullName
  form.isActive = row.isActive
  form.branchIds = [...row.branchIds]
}

function closeForm() {
  creating.value = false
  editing.value = null
  draftError.value = null
}

const saveMut = useMutation({
  mutationFn: async () => {
    if (editing.value) {
      const { data } = await axiosInstance.put<Account>(
        `/admin/BranchManagerAccounts/${editing.value.id}`,
        {
          username: form.username,
          password: form.password || undefined,
          fullName: form.fullName,
          isActive: form.isActive,
          branchIds: form.branchIds,
        }
      )
      return data
    }
    const { data } = await axiosInstance.post<Account>('/admin/BranchManagerAccounts', {
      username: form.username,
      password: form.password,
      fullName: form.fullName,
      branchIds: form.branchIds,
    })
    return data
  },
  onSuccess: async () => {
    closeForm()
    await qc.invalidateQueries({ queryKey: ['bm-accounts'] })
  },
  onError: () => {
    draftError.value =
      'فشل الحفظ — أبقينا المسودة. تحقق من النطاق والدور وكلمة المرور.'
  },
})

function toggleBranch(id: number) {
  if (form.branchIds.includes(id)) {
    form.branchIds = form.branchIds.filter((x) => x !== id)
  } else {
    form.branchIds = [...form.branchIds, id]
  }
}
</script>

<template>
  <div
    class="ba-page"
    dir="rtl"
  >
    <header>
      <h1 class="ba-title">حسابات إدارة الفرع</h1>
      <p class="ba-sub">
        إنشاء/تعطيل مديري الفرع ضمن نطاقك فقط — بلا ManagePermissions العامة.
      </p>
    </header>

    <div
      v-if="!canRead"
      class="ba-state"
      role="alert"
    >
      ليس لديك صلاحية قراءة حسابات إدارة الفرع.
    </div>
    <template v-else>
      <div class="ba-toolbar">
        <button
          v-if="canWrite"
          type="button"
          class="ba-btn"
          @click="openCreate"
        >
          إنشاء حساب
        </button>
        <button
          v-if="listQuery.isError.value"
          type="button"
          class="ba-btn ghost"
          @click="listQuery.refetch()"
        >
          إعادة المحاولة
        </button>
      </div>
      <p
        v-if="listQuery.isError.value"
        class="ba-err"
        role="alert"
      >
        تعذر التحميل.
      </p>
      <div
        v-else-if="listQuery.isPending.value"
        class="ba-state"
      >
        جاري التحميل…
      </div>
      <div
        v-else-if="(listQuery.data.value?.totalCount ?? 0) === 0"
        class="ba-state"
      >
        لا حسابات مدير فرع ضمن نطاقك.
      </div>
      <ul
        v-else
        class="ba-list"
      >
        <li
          v-for="row in listQuery.data.value?.items ?? []"
          :key="row.id"
          class="ba-row"
        >
          <div>
            <strong>{{ row.fullName }}</strong>
            <span> · {{ row.username }} · {{ row.isActive ? 'نشط' : 'معطّل' }}</span>
            <div class="ba-meta">{{ row.branchNames.join('، ') }}</div>
          </div>
          <button
            v-if="canWrite"
            type="button"
            class="ba-btn ghost"
            @click="openEdit(row)"
          >
            تعديل
          </button>
        </li>
      </ul>
    </template>

    <div
      v-if="creating || editing"
      class="ba-modal"
      role="dialog"
      aria-modal="true"
    >
      <div class="ba-dialog">
        <h2>{{ editing ? 'تعديل حساب' : 'إنشاء حساب' }}</h2>
        <label>
          الاسم الظاهر
          <input
            v-model="form.fullName"
            type="text"
          >
        </label>
        <label>
          اسم المستخدم
          <input
            v-model="form.username"
            type="text"
            autocomplete="off"
          >
        </label>
        <label>
          كلمة المرور{{ editing ? ' (اختياري)' : '' }}
          <input
            v-model="form.password"
            type="password"
            autocomplete="new-password"
          >
        </label>
        <label
          v-if="editing"
          class="ba-check"
        >
          <input
            v-model="form.isActive"
            type="checkbox"
          >
          نشط
        </label>
        <fieldset>
          <legend>الفروع ضمن نطاقك</legend>
          <label
            v-for="c in citiesQuery.data.value ?? []"
            :key="c.id"
            class="ba-check"
          >
            <input
              type="checkbox"
              :checked="form.branchIds.includes(c.id)"
              @change="toggleBranch(c.id)"
            >
            {{ c.name }}
          </label>
        </fieldset>
        <p
          v-if="draftError"
          class="ba-err"
          role="alert"
        >
          {{ draftError }}
        </p>
        <div class="ba-actions">
          <button
            type="button"
            class="ba-btn"
            :disabled="saveMut.isPending.value"
            @click="saveMut.mutate()"
          >
            حفظ
          </button>
          <button
            type="button"
            class="ba-btn ghost"
            @click="closeForm"
          >
            إلغاء
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ba-page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1rem 1.25rem 2rem;
  font-family: ElMessiri, 'Segoe UI', Tahoma, sans-serif;
  color: #00201e;
}
.ba-title {
  margin: 0;
  color: #006a64;
  font-size: 1.35rem;
}
.ba-sub {
  margin: 0.35rem 0 1rem;
  font-size: 0.9rem;
}
.ba-toolbar {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}
.ba-state,
.ba-err {
  padding: 1rem;
  background: #f8f8f8;
  border-radius: 8px;
}
.ba-err {
  color: #8b1a1a;
}
.ba-btn {
  background: #006a64;
  color: #fff;
  border: 0;
  padding: 0.45rem 0.9rem;
  border-radius: 6px;
  cursor: pointer;
}
.ba-btn.ghost {
  background: #f8f8f8;
  color: #00201e;
}
.ba-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.5rem;
}
.ba-row {
  display: flex;
  justify-content: space-between;
  gap: 0.75rem;
  background: #f8f8f8;
  padding: 0.75rem 1rem;
  border-radius: 8px;
}
.ba-meta {
  font-size: 0.85rem;
  opacity: 0.85;
  margin-top: 0.2rem;
}
.ba-modal {
  position: fixed;
  inset: 0;
  background: rgba(0, 32, 30, 0.45);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 40;
}
.ba-dialog {
  background: #fff;
  border-radius: 10px;
  padding: 1.25rem;
  width: min(520px, 100%);
  display: grid;
  gap: 0.65rem;
  max-height: 90vh;
  overflow: auto;
}
.ba-dialog label,
.ba-dialog fieldset {
  display: grid;
  gap: 0.3rem;
}
.ba-dialog input[type='text'],
.ba-dialog input[type='password'] {
  background: #f8f8f8;
  border: 1px solid #cfd8d7;
  border-radius: 6px;
  padding: 0.4rem 0.55rem;
  font: inherit;
}
.ba-check {
  display: flex !important;
  align-items: center;
  gap: 0.4rem;
}
.ba-actions {
  display: flex;
  gap: 0.5rem;
}
</style>
