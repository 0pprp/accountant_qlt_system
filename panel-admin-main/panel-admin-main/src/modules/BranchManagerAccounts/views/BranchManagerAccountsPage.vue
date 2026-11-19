<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { isAxiosError } from 'axios'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'

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

const formOpen = ref(false)
const editing = ref<Account | null>(null)
const formError = ref<string | null>(null)
const notice = ref<string | null>(null)
const actionError = ref<string | null>(null)
const toggleTarget = ref<Account | null>(null)
const form = reactive({
  username: '',
  password: '',
  fullName: '',
  branchIds: [] as number[],
})

const listQuery = useQuery({
  queryKey: computed(() => ['bm-accounts', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<ListResponse>('/admin/BranchManagerAccounts', { signal })
    return data
  },
})

const citiesQuery = useQuery({
  queryKey: computed(() => ['bm-account-cities', auth.sessionGeneration]),
  enabled: computed(() => canRead.value && auth.permissionsReady),
  queryFn: async ({ signal }) => {
    const { data } = await axiosInstance.get<City[]>('/admin/BranchManagerAccounts/cities', { signal })
    return data
  },
})

const items = computed(() => listQuery.data.value?.items ?? [])

function serverMessage(error: unknown, fallback: string): string {
  if (!isAxiosError(error)) return fallback
  const errors = (error.response?.data as { errors?: { message?: string }[] } | undefined)?.errors
  return (Array.isArray(errors) ? errors[0]?.message : undefined) || fallback
}

function resetForm() {
  form.username = ''
  form.password = ''
  form.fullName = ''
  form.branchIds = []
  formError.value = null
}

function openCreate() {
  editing.value = null
  resetForm()
  notice.value = null
  formOpen.value = true
}

function openEdit(row: Account) {
  editing.value = row
  resetForm()
  form.username = row.username
  form.fullName = row.fullName
  form.branchIds = [...row.branchIds]
  notice.value = null
  formOpen.value = true
}

function closeForm() {
  if (saveMut.isPending.value) return
  formOpen.value = false
  editing.value = null
  resetForm()
}

function toggleBranch(id: number) {
  form.branchIds = form.branchIds.includes(id)
    ? form.branchIds.filter((x) => x !== id)
    : [...form.branchIds, id]
}

function validate(): string | null {
  if (form.fullName.trim().length < 2) return 'الاسم الظاهر مطلوب (حرفان على الأقل)'
  if (form.username.trim().length < 3) return 'اسم الدخول 3 أحرف على الأقل'
  if (!editing.value && form.password.length < 4) return 'كلمة المرور 4 أحرف على الأقل'
  if (editing.value && form.password.length > 0 && form.password.length < 4) return 'كلمة المرور الجديدة 4 أحرف على الأقل'
  if (form.branchIds.length === 0) return 'اختر فرعاً واحداً على الأقل'
  return null
}

const saveMut = useMutation({
  mutationFn: async () => {
    const payload = {
      username: form.username.trim(),
      fullName: form.fullName.trim(),
      branchIds: form.branchIds,
    }
    if (editing.value) {
      await axiosInstance.put(`/admin/BranchManagerAccounts/${editing.value.id}`, {
        ...payload,
        isActive: editing.value.isActive,
        password: form.password || undefined,
      })
      return 'edit' as const
    }
    await axiosInstance.post('/admin/BranchManagerAccounts', { ...payload, password: form.password })
    return 'create' as const
  },
  onSuccess: async (kind) => {
    formOpen.value = false
    editing.value = null
    resetForm()
    notice.value = kind === 'create' ? 'تم إنشاء الحساب' : 'تم حفظ التعديلات'
    await qc.invalidateQueries({ queryKey: ['bm-accounts'] })
  },
  onError: (error) => {
    form.password = ''
    formError.value = serverMessage(error, 'تعذر حفظ الحساب. حاول مرة أخرى.')
  },
})

function submit() {
  formError.value = validate()
  if (formError.value) return
  saveMut.mutate()
}

const toggleMut = useMutation({
  mutationFn: async (row: Account) => {
    await axiosInstance.put(`/admin/BranchManagerAccounts/${row.id}`, {
      username: row.username,
      fullName: row.fullName,
      branchIds: row.branchIds,
      isActive: !row.isActive,
    })
    return !row.isActive
  },
  onSuccess: async (active) => {
    toggleTarget.value = null
    actionError.value = null
    notice.value = active ? 'تم تفعيل الحساب' : 'تم إيقاف الحساب'
    await qc.invalidateQueries({ queryKey: ['bm-accounts'] })
  },
  onError: (error) => {
    toggleTarget.value = null
    actionError.value = serverMessage(error, 'تعذر تغيير حالة الحساب.')
  },
})

const toggleText = computed(() => {
  const row = toggleTarget.value
  if (!row) return ''
  return row.isActive
    ? `سيتم إيقاف حساب «${row.fullName}»، ولن يستطيع دخول تطبيق إدارة الفرع حتى تعيد تفعيله.`
    : `سيتم تفعيل حساب «${row.fullName}» ليتمكن من دخول تطبيق إدارة الفرع.`
})
</script>

<template>
  <section class="bma" dir="rtl">
    <header class="bma-head">
      <div>
        <h2 class="dfb-title">حسابات إدارة الفرع</h2>
        <p class="dfb-subtitle">أسماء دخول مدراء الفروع ضمن نطاقك.</p>
      </div>
      <button v-if="canRead && canWrite" type="button" class="dfb-button is-primary" @click="openCreate">
        إنشاء حساب
      </button>
    </header>

    <p class="bma-info">
      حسابات تطبيق إدارة الفرع: من هنا فقط تُنشأ أسماء الدخول وكلمات المرور والفروع؛ بدون حساب هنا لن يفتح التطبيق.
    </p>

    <p v-if="!canRead" class="dfb-state" role="alert">ليس لديك صلاحية قراءة حسابات إدارة الفرع.</p>

    <template v-else>
      <p v-if="notice" class="bma-notice" role="status">{{ notice }}</p>
      <p v-if="actionError" class="dfb-error" role="alert">{{ actionError }}</p>

      <div v-if="listQuery.isError.value" class="dfb-error" role="alert">
        تعذر تحميل الحسابات.
        <button type="button" class="dfb-button is-small" @click="listQuery.refetch()">إعادة المحاولة</button>
      </div>
      <p v-else-if="listQuery.isPending.value" class="dfb-state">جاري التحميل…</p>
      <p v-else-if="items.length === 0" class="dfb-state">لا يوجد مستخدمون بعد — أنشئ أول حساب لمدير فرع</p>
      <ul v-else class="bma-grid">
        <li v-for="row in items" :key="row.id" class="bma-card" :class="{ 'is-off': !row.isActive }">
          <div class="bma-card-head">
            <strong class="bma-name">{{ row.fullName }}</strong>
            <span v-if="!row.isActive" class="dfb-status is-rejected">الحساب موقوف</span>
          </div>
          <p class="bma-meta">اسم الدخول: <span class="dfb-num">{{ row.username }}</span></p>
          <ul class="bma-branches" aria-label="الفروع">
            <li v-for="name in row.branchNames" :key="name">{{ name }}</li>
            <li v-if="row.branchNames.length === 0" class="is-empty">بدون فروع</li>
          </ul>
          <div v-if="canWrite" class="bma-actions">
            <button type="button" class="dfb-button is-small" @click="openEdit(row)">تعديل</button>
            <button
              type="button"
              class="dfb-button is-small"
              :class="{ 'bma-danger': row.isActive }"
              :disabled="toggleMut.isPending.value"
              @click="toggleTarget = row; actionError = null"
            >
              {{ row.isActive ? 'إيقاف الحساب' : 'تفعيل الحساب' }}
            </button>
          </div>
        </li>
      </ul>
    </template>

    <NeuDialog
      :visible="formOpen"
      :title="editing ? 'تعديل حساب مدير فرع' : 'إنشاء حساب مدير فرع'"
      size="sm"
      :busy="saveMut.isPending.value"
      @close="closeForm"
    >
      <form class="bma-form" autocomplete="off" @submit.prevent="submit">
        <label class="dfb-field">
          <span>الاسم الظاهر</span>
          <input v-model="form.fullName" type="text" class="dfb-input" maxlength="200">
        </label>
        <label class="dfb-field">
          <span>اسم الدخول</span>
          <input v-model="form.username" type="text" class="dfb-input" dir="ltr" maxlength="100" autocomplete="off">
        </label>
        <label class="dfb-field">
          <span>{{ editing ? 'كلمة مرور جديدة (اختياري)' : 'كلمة المرور' }}</span>
          <input v-model="form.password" type="password" class="dfb-input" dir="ltr" autocomplete="new-password">
        </label>
        <fieldset class="bma-fieldset">
          <legend>الفروع</legend>
          <p v-if="citiesQuery.isPending.value" class="dfb-state">جاري تحميل الفروع…</p>
          <p v-else-if="citiesQuery.isError.value" class="dfb-error">تعذر تحميل الفروع.</p>
          <div v-else class="dfb-filter-chips">
            <button
              v-for="c in citiesQuery.data.value ?? []"
              :key="c.id"
              type="button"
              class="dfb-chip"
              :aria-pressed="form.branchIds.includes(c.id)"
              @click="toggleBranch(c.id)"
            >
              {{ c.name }}
            </button>
          </div>
        </fieldset>
        <p v-if="formError" class="dfb-form-error" role="alert">{{ formError }}</p>
        <button type="submit" hidden />
      </form>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="saveMut.isPending.value" @click="closeForm">إلغاء</button>
        <button type="button" class="dfb-button is-primary" :disabled="saveMut.isPending.value" @click="submit">
          {{ saveMut.isPending.value ? 'جاري الحفظ…' : 'حفظ' }}
        </button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="toggleTarget != null"
      :title="toggleTarget?.isActive ? 'إيقاف الحساب' : 'تفعيل الحساب'"
      size="sm"
      :busy="toggleMut.isPending.value"
      @close="toggleTarget = null"
    >
      <p class="bma-confirm">{{ toggleText }}</p>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="toggleMut.isPending.value" @click="toggleTarget = null">إلغاء</button>
        <button
          type="button"
          class="dfb-button is-primary"
          :disabled="toggleMut.isPending.value"
          @click="toggleTarget && toggleMut.mutate(toggleTarget)"
        >
          {{ toggleMut.isPending.value ? 'جاري الحفظ…' : toggleTarget?.isActive ? 'إيقاف' : 'تفعيل' }}
        </button>
      </template>
    </NeuDialog>
  </section>
</template>

<style scoped>
.bma {
  display: flex;
  flex-direction: column;
  gap: 18px;
  min-width: 0;
}

.bma-head {
  align-items: flex-start;
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  justify-content: space-between;
}

.bma-info {
  background: var(--dfb-primary-soft);
  border-radius: 14px;
  box-shadow: var(--dfb-well-shadow);
  line-height: 1.8;
  margin: 0;
  padding: 12px 16px;
}

.bma-notice {
  background: var(--dfb-primary-soft);
  border-radius: 12px;
  color: var(--dfb-primary);
  font-weight: 600;
  margin: 0;
  padding: 10px 14px;
}

.bma-grid {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 280px), 1fr));
  list-style: none;
  margin: 0;
  padding: 0;
}

.bma-card {
  background: var(--dfb-raised);
  border-radius: 18px;
  box-shadow: var(--dfb-up);
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 16px 18px;
}

.bma-card.is-off { background: var(--dfb-well); }

.bma-card-head {
  align-items: center;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: space-between;
}

.bma-name {
  font-size: 16px;
  overflow-wrap: anywhere;
}

.bma-meta {
  color: var(--dfb-muted);
  font-size: 13px;
  margin: 0;
}

.bma-branches {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  list-style: none;
  margin: 0;
  padding: 0;
}

.bma-branches li {
  background: var(--dfb-well);
  border-radius: 10px;
  font-size: 13px;
  padding: 3px 10px;
}

.bma-branches li.is-empty { color: var(--dfb-muted); }

.bma-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: auto;
}

.bma-danger { color: var(--dfb-tier-bad); }

.bma-form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.bma-form .dfb-input { width: 100%; }

.bma-fieldset {
  border: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin: 0;
  padding: 0;
}

.bma-fieldset legend {
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 8px;
}

.bma-confirm {
  line-height: 1.8;
  margin: 0;
}
</style>
