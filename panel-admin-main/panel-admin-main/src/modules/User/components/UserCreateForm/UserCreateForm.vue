<template>
  <div class="user-create-page q-page" dir="rtl">
    <form
      ref="formRef"
      class="user-create-form"
      novalidate
      @submit.prevent="onSubmit"
    >
      <header class="user-create-form__head">
        <h1 class="user-create-form__title">إنشاء حساب</h1>
        <p class="user-create-form__sub">
          أدخل بيانات الحساب الخمسة ثم اضغط إنشاء الحساب مرة واحدة.
        </p>
      </header>

      <div class="user-create-form__grid">
        <AppInput
          v-model="username"
          name="username"
          autocomplete="username"
          maxlength="100"
          required
          label="اسم المستخدم"
          placeholder="معرّف تسجيل الدخول"
          :error="errors.username"
          :disabled="isSubmitting"
        />

        <AppInput
          v-model="password"
          type="password"
          name="password"
          autocomplete="new-password"
          maxlength="50"
          required
          label="كلمة السر"
          placeholder="8–50 حرفًا"
          :error="errors.password"
          :disabled="isSubmitting"
        />

        <AppInput
          class="user-create-form__span"
          v-model="fullName"
          name="fullName"
          autocomplete="name"
          maxlength="50"
          required
          label="الاسم الثلاثي"
          placeholder="اسم عرض الحساب"
          :error="errors.fullName"
          :disabled="isSubmitting"
        />

        <div class="user-create-form__field">
          <AppSelect
            v-model="roleId"
            name="roleId"
            required
            label="المنصب الوظيفي"
            placeholder="اختر المنصب"
            :options="roleOptions"
            :error="errors.roleId || rolesLoadError"
            :disabled="isSubmitting || isRolesLoading"
            :empty-message="isRolesLoading ? 'جارٍ التحميل…' : 'لا توجد مناصب'"
          />
          <button
            v-if="rolesLoadError"
            type="button"
            class="user-create-form__retry"
            @click="refetchRoles"
          >
            إعادة محاولة تحميل المناصب
          </button>
        </div>

        <div class="user-create-form__field">
          <BranchSelectInput
            v-model="branchId"
            required
            label="user.userForm.branchIds.label"
            placeholder="user.userForm.branchIds.placeholder"
            :error="errors.branchId || branchesLoadError"
            :readonly="isSubmitting"
          />
          <button
            v-if="branchesLoadError"
            type="button"
            class="user-create-form__retry"
            @click="refetchBranches"
          >
            إعادة محاولة تحميل الفروع
          </button>
        </div>
      </div>

      <div class="user-create-form__actions">
        <AppButton
          type="submit"
          variant="primary"
          :loading="isSubmitting"
          :disabled="isSubmitting"
        >
          {{ isSubmitting ? 'جارٍ إنشاء الحساب…' : 'إنشاء الحساب' }}
        </AppButton>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { AppButton, AppInput, AppSelect } from '@/modules/Core/components/ui'
import BranchSelectInput from '@/modules/Core/components/shared/BranchSelectInput/BranchSelectInput.vue'
import { useRoleDataQuery } from '@/modules/Role/requests/queries'
import { useBranchDataQuery } from '@/modules/Branch/requests/queries'
import { useCreateUserMutation } from '../../requests/mutations'
import type { UserCreatePayload } from '../../types/api'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import { usePermission } from '@/modules/Core/composable/usePermission'

const router = useRouter()
const toastStore = useToastStore()
const { can } = usePermission()
const canCreateUser = can('User', 'Create')

const username = ref('')
const password = ref('')
const fullName = ref('')
const roleId = ref<number | null>(null)
const branchId = ref<number | null>(null)
const errors = ref<{
  username?: string
  password?: string
  fullName?: string
  roleId?: string
  branchId?: string
}>({})
const formRef = ref<HTMLFormElement | null>(null)

const {
  data: roleData,
  isFetching: isRolesLoading,
  isError: isRolesError,
  refetch: refetchRolesQuery,
} = useRoleDataQuery()

const pageIndex = ref(1)
const pageSize = ref(100)
const {
  isError: isBranchesError,
  refetch: refetchBranchesQuery,
} = useBranchDataQuery(pageIndex, pageSize)

const roleOptions = computed(() =>
  (roleData.value ?? []).map((role) => ({
    value: role.id,
    label: role.displayName,
  })),
)

const rolesLoadError = computed(() =>
  isRolesError.value ? 'تعذر تحميل المناصب' : undefined,
)
const branchesLoadError = computed(() =>
  isBranchesError.value ? 'تعذر تحميل الفروع' : undefined,
)

const { isPending: isCreating, mutateAsync: createUser } =
  useCreateUserMutation()
const isSubmitting = computed(() => isCreating.value)

onMounted(() => {
  localStorage.removeItem('activeStep')
})

function normalizeFullName(raw: string) {
  return raw.trim().split(/\s+/).filter(Boolean).join(' ')
}

function validate(): boolean {
  const next: typeof errors.value = {}
  const u = username.value.trim()
  const p = password.value
  const n = normalizeFullName(fullName.value)

  if (!u) next.username = 'اسم المستخدم مطلوب'
  if (!p) next.password = 'كلمة السر مطلوبة'
  else if (p.length < 8) next.password = 'كلمة السر يجب أن تحتوي على 8 أحرف على الأقل'
  else if (p.length > 50) next.password = 'يجب ألا تتجاوز كلمة السر 50 حرفًا'
  if (!n) next.fullName = 'الاسم الثلاثي مطلوب'
  else if (n.length > 50) next.fullName = 'يجب ألا يتجاوز الاسم الثلاثي 50 حرفًا'
  if (roleId.value == null || roleId.value <= 0) next.roleId = 'المنصب الوظيفي مطلوب'
  if (branchId.value == null || branchId.value <= 0) next.branchId = 'الفرع مطلوب'

  errors.value = next
  return Object.keys(next).length === 0
}

async function focusUsername() {
  await nextTick()
  formRef.value
    ?.querySelector<HTMLInputElement>('input[name="username"]')
    ?.focus()
}

function refetchRoles() {
  void refetchRolesQuery()
}

function refetchBranches() {
  void refetchBranchesQuery()
}

async function onSubmit() {
  if (!canCreateUser || isSubmitting.value) return
  if (!validate()) {
    if (errors.value.username) await focusUsername()
    return
  }

  const payload: UserCreatePayload = {
    username: username.value.trim(),
    password: password.value,
    fullName: normalizeFullName(fullName.value),
    roleId: roleId.value!,
    branchIds: [branchId.value!],
  }

  try {
    await createUser(payload)
    password.value = ''
    await router.push({ name: 'UserListRoute' })
  } catch (err: unknown) {
    const axiosErr = err as {
      response?: { status?: number; data?: { title?: string; detail?: string; message?: string } }
      message?: string
    }
    const status = axiosErr.response?.status
    const serverMsg =
      axiosErr.response?.data?.title ||
      axiosErr.response?.data?.detail ||
      axiosErr.response?.data?.message ||
      axiosErr.message ||
      'تعذر إنشاء الحساب'

    if (
      status === 409 ||
      /مكرر|already|exist|Username/i.test(String(serverMsg))
    ) {
      errors.value = {
        ...errors.value,
        username: 'اسم المستخدم مكرر',
      }
      await focusUsername()
      return
    }

    toastStore.setMassage({
      title: 'فشل إنشاء الحساب',
      description: String(serverMsg),
      dialogState: DialogState.Error,
      isOpen: true,
    })
  }
}
</script>

<style scoped>
.user-create-page {
  width: 100%;
  max-width: none;
  padding: 8px 0 32px;
  background: transparent;
  min-height: calc(100vh - 120px);
  box-sizing: border-box;
}

.user-create-form {
  width: 100%;
  max-width: 1100px;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 28px 32px;
  border: 0;
  border-radius: 20px;
  background: var(--q-neo-bg, #eef3f2);
  color: #00201e;
  box-shadow: var(
    --q-neo-raised,
    10px 10px 22px rgba(0, 32, 30, 0.08),
    -8px -8px 18px rgba(255, 255, 255, 0.95)
  );
  font-family: var(--q-font, 'ElMessiri', 'Rubik', sans-serif);
  box-sizing: border-box;
}

.user-create-form__head {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.user-create-form__title {
  margin: 0;
  font-size: clamp(1.5rem, 2vw, 1.85rem);
  font-weight: 700;
  color: #006a64;
  text-align: start;
}

.user-create-form__sub {
  margin: 0;
  font-size: 15px;
  line-height: 1.5;
  color: color-mix(in srgb, #00201e 62%, transparent);
}

.user-create-form__grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 18px 20px;
  align-items: start;
}

.user-create-form__span,
.user-create-form__actions {
  grid-column: 1 / -1;
}

.user-create-form__field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
  padding: 0;
}

.user-create-form__actions {
  display: flex;
  justify-content: flex-start;
  padding-top: 4px;
}

.user-create-form__actions :deep(.q-btn) {
  min-width: 220px;
  min-height: 48px;
  border: 0;
  border-radius: 14px;
  box-shadow:
    5px 5px 12px rgba(0, 32, 30, 0.12),
    -4px -4px 10px rgba(255, 255, 255, 0.9);
}

.user-create-form :deep(.q-input),
.user-create-form :deep(.q-select-menu__trigger) {
  min-height: 48px;
  height: 48px;
}

.user-create-form :deep(.q-field),
.user-create-form :deep(.q-control-shell),
.user-create-form :deep(.q-select-menu) {
  border: 0 !important;
  box-shadow: none !important;
  background: transparent !important;
  padding: 0 !important;
  margin: 0;
}

.user-create-form :deep(.q-btn--primary) {
  background: #006a64;
  color: #fff;
}

.user-create-form__retry {
  align-self: flex-start;
  margin: 0;
  padding: 0;
  border: 0;
  background: transparent;
  color: #006a64;
  font: inherit;
  font-size: 13px;
  cursor: pointer;
  text-decoration: underline;
}

@media (max-width: 900px) {
  .user-create-form {
    max-width: 100%;
    padding: 22px 20px;
  }

  .user-create-form__grid {
    grid-template-columns: 1fr;
  }

  .user-create-form__actions :deep(.q-btn) {
    width: 100%;
    min-width: 0;
  }
}

@media (max-width: 640px) {
  .user-create-page {
    padding: 0 0 24px;
  }

  .user-create-form {
    padding: 16px 16px;
    border-radius: 14px;
    box-shadow:
      4px 4px 10px rgba(0, 32, 30, 0.07),
      -4px -4px 10px rgba(255, 255, 255, 0.85);
  }
}
</style>
