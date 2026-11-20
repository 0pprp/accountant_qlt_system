<template>
  <div class="fpp" dir="rtl">
    <header class="fpp-head">
      <div>
        <h2 class="dfb-title">المروّجون</h2>
        <p class="dfb-subtitle">حسابات المروّجين الميدانيين وفروعهم وأهدافهم اليومية</p>
      </div>
      <div class="fpp-filters">
        <label class="fpp-field is-wide">
          <span>بحث</span>
          <input v-model="search" class="dfb-search" type="search" placeholder="الاسم أو اسم المستخدم أو الهاتف" aria-label="بحث في المروّجين">
        </label>
        <label class="fpp-field">
          <span>الحالة</span>
          <select v-model="status" class="dfb-search">
            <option value="">الكل</option>
            <option value="active">فعّال</option>
            <option value="suspended">موقوف</option>
          </select>
        </label>
        <button v-if="canManage" type="button" class="dfb-button is-primary" @click="openCreate">إضافة مروّج</button>
      </div>
    </header>

    <p v-if="accounts.isPending.value" class="dfb-state" role="status">جاري التحميل…</p>
    <div v-else-if="accounts.isError.value" class="dfb-error" role="alert">
      {{ apiErrorMessage(accounts.error.value, 'تعذر تحميل المروّجين.') }}
      <button type="button" class="dfb-button is-small" @click="accounts.refetch()">إعادة المحاولة</button>
    </div>
    <section v-else class="fpp-card">
      <p v-if="!rows.length" class="dfb-state">لا يوجد مروّجون مطابقون.</p>
      <div v-else class="fpp-table-wrap">
        <table class="fpp-table">
          <thead>
            <tr>
              <th>المروّج</th>
              <th>الهاتف</th>
              <th>الفروع</th>
              <th>الهدف اليومي</th>
              <th>الحالة</th>
              <th v-if="canManage"><span class="sr-only">إجراءات</span></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in rows" :key="p.id">
              <td>
                <button type="button" class="dfb-name-link" @click="openDay(p.id)">{{ p.fullName }}</button>
                <small class="dfb-cell-sub" dir="ltr">{{ p.username }}</small>
              </td>
              <td><span dir="ltr">{{ p.phone || '—' }}</span></td>
              <td>{{ p.branches.map((b) => b.name).join('، ') || '—' }}</td>
              <td>
                <span class="dfb-num">{{ p.targetMinutes == null ? 'الافتراضي' : hm(p.targetMinutes) }}</span>
              </td>
              <td>
                <span class="fpp-chip" :class="p.isSuspended ? 'tone-dark' : 'tone-good'">{{ p.isSuspended ? 'موقوف' : 'فعّال' }}</span>
                <small v-if="p.isSuspended && p.suspendReason" class="dfb-cell-sub">{{ p.suspendReason }}</small>
              </td>
              <td v-if="canManage">
                <div class="fpp-row-actions">
                  <button type="button" class="dfb-button is-small" @click="openEdit(p)">تعديل</button>
                  <button type="button" class="dfb-button is-small" @click="openPassword(p)">كلمة مرور جديدة</button>
                  <button type="button" class="dfb-button is-small" @click="openTarget(p)">الهدف اليومي</button>
                  <button type="button" class="dfb-button is-small" :class="{ 'fpp-danger': !p.isSuspended }" @click="openSuspend(p)">
                    {{ p.isSuspended ? 'تفعيل' : 'إيقاف' }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <NeuDialog
      :visible="form != null"
      :title="form?.id == null ? 'إضافة مروّج' : 'تعديل المروّج'"
      size="md"
      :busy="save.isPending.value"
      @close="form = null"
    >
      <form v-if="form" id="fp-account-form" class="fpp-form" novalidate @submit.prevent="submitForm">
        <label class="fpp-field">
          <span>الاسم</span>
          <input v-model="form.fullName" class="dfb-search" maxlength="150" required>
        </label>
        <label class="fpp-field">
          <span>اسم المستخدم</span>
          <input v-model="form.username" class="dfb-search" dir="ltr" maxlength="100" autocomplete="off" required>
        </label>
        <label class="fpp-field">
          <span>الهاتف (اختياري)</span>
          <input v-model="form.phone" class="dfb-search" dir="ltr" inputmode="tel" maxlength="20" placeholder="07XXXXXXXXX">
        </label>
        <label v-if="form.id == null" class="fpp-field">
          <span>كلمة المرور (8 أحرف على الأقل)</span>
          <span class="fpp-inline">
            <input v-model="form.password" class="dfb-search" dir="ltr" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" minlength="8">
            <button type="button" class="dfb-button is-small" @click="showPassword = !showPassword">{{ showPassword ? 'إخفاء' : 'إظهار' }}</button>
            <button type="button" class="dfb-button is-small" @click="generateInto(form)">توليد</button>
          </span>
        </label>
        <fieldset class="fpp-field fpp-fieldset is-full">
          <span>الفروع</span>
          <div class="fpp-checks">
            <label v-for="b in branches" :key="b.id" class="fpp-check">
              <input v-model="form.branchIds" type="checkbox" :value="b.id">
              {{ b.name }}
            </label>
          </div>
        </fieldset>
        <p v-if="formError" class="dfb-error is-full" role="alert">{{ formError }}</p>
      </form>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="save.isPending.value" @click="form = null">إلغاء</button>
        <button type="submit" form="fp-account-form" class="dfb-button is-primary" :disabled="save.isPending.value">
          {{ save.isPending.value ? 'جاري الحفظ…' : 'حفظ' }}
        </button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="passwordFor != null"
      title="كلمة مرور جديدة"
      :subtitle="passwordFor?.fullName"
      size="sm"
      :busy="setPassword.isPending.value"
      @close="passwordFor = null"
    >
      <div class="fpp-form">
        <label class="fpp-field is-full">
          <span>كلمة المرور الجديدة (8 أحرف على الأقل)</span>
          <span class="fpp-inline">
            <input v-model="newPassword" class="dfb-search" dir="ltr" :type="showPassword ? 'text' : 'password'" autocomplete="new-password" minlength="8">
            <button type="button" class="dfb-button is-small" @click="showPassword = !showPassword">{{ showPassword ? 'إخفاء' : 'إظهار' }}</button>
            <button type="button" class="dfb-button is-small" @click="newPassword = generatePassword(); showPassword = true">توليد</button>
          </span>
        </label>
        <p class="fpp-muted is-full">سيُسجَّل خروج المروّج من كل الأجهزة. أبلغه بكلمة المرور الجديدة مباشرةً.</p>
        <p v-if="passwordError" class="dfb-error is-full" role="alert">{{ passwordError }}</p>
      </div>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="setPassword.isPending.value" @click="passwordFor = null">إلغاء</button>
        <button type="button" class="dfb-button is-primary" :disabled="setPassword.isPending.value" @click="submitPassword">حفظ كلمة المرور</button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="suspendFor != null"
      :title="suspendFor?.isSuspended ? 'تفعيل المروّج' : 'إيقاف المروّج'"
      :subtitle="suspendFor?.fullName"
      size="sm"
      :busy="suspend.isPending.value"
      @close="suspendFor = null"
    >
      <div class="fpp-form">
        <p class="fpp-muted is-full">
          {{ suspendFor?.isSuspended
            ? 'سيتمكن المروّج من تسجيل الدخول واستعمال التطبيق مجدداً.'
            : 'سيُسجَّل خروج المروّج فوراً ويُنهى دوامه المفتوح، ولن يتمكن من الدخول حتى التفعيل.' }}
        </p>
        <label v-if="!suspendFor?.isSuspended" class="fpp-field is-full">
          <span>السبب (اختياري)</span>
          <textarea v-model="suspendReason" class="dfb-search" rows="3" maxlength="500" />
        </label>
        <p v-if="suspendError" class="dfb-error is-full" role="alert">{{ suspendError }}</p>
      </div>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="suspend.isPending.value" @click="suspendFor = null">إلغاء</button>
        <button type="button" class="dfb-button" :class="suspendFor?.isSuspended ? 'is-primary' : 'fpp-danger'" :disabled="suspend.isPending.value" @click="submitSuspend">
          {{ suspendFor?.isSuspended ? 'تأكيد التفعيل' : 'تأكيد الإيقاف' }}
        </button>
      </template>
    </NeuDialog>

    <NeuDialog
      :visible="targetFor != null"
      title="الهدف اليومي"
      :subtitle="targetFor?.fullName"
      size="sm"
      :busy="target.isPending.value"
      @close="targetFor = null"
    >
      <div class="fpp-form">
        <label class="fpp-field">
          <span>الساعات</span>
          <input v-model="targetHours" class="dfb-search" type="number" min="0" max="24" inputmode="numeric">
        </label>
        <label class="fpp-field">
          <span>الدقائق</span>
          <input v-model="targetMinutes" class="dfb-search" type="number" min="0" max="59" inputmode="numeric">
        </label>
        <p class="fpp-muted is-full">اترك الحقلين فارغين لاستعمال الهدف الافتراضي من الإعدادات.</p>
        <p v-if="targetError" class="dfb-error is-full" role="alert">{{ targetError }}</p>
      </div>
      <template #footer>
        <button type="button" class="dfb-button" :disabled="target.isPending.value" @click="targetFor = null">إلغاء</button>
        <button type="button" class="dfb-button is-primary" :disabled="target.isPending.value" @click="submitTarget">حفظ الهدف</button>
      </template>
    </NeuDialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useFpToast } from '../composables/useFpToast'
import {
  apiErrorMessage,
  usePromoterAccountsQuery,
  usePromoterPermissions,
  useSavePromoterMutation,
  useSetPromoterPasswordMutation,
  useSetPromoterTargetMutation,
  useSuspendPromoterMutation,
  type PromoterAccount,
} from '../requests/fieldPromoters'
import { hm } from '../domain/format'
import { useFpNav } from '../composables/useFpNav'
import { useAuthStore } from '@/modules/Auth/store'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'

interface AccountForm {
  id: number | null
  fullName: string
  username: string
  phone: string
  password: string
  branchIds: number[]
}

const toast = useFpToast()
const auth = useAuthStore()
const { branchId, openDay } = useFpNav()
const { canManage } = usePromoterPermissions()
const branches = computed(() => auth.branches ?? [])

const search = ref('')
const debounced = ref('')
const status = ref<'' | 'active' | 'suspended'>('')
let timer: ReturnType<typeof setTimeout> | undefined
watch(search, (v) => {
  clearTimeout(timer)
  timer = setTimeout(() => {
    debounced.value = v
  }, 300)
})

const accounts = usePromoterAccountsQuery(computed(() => ({ q: debounced.value, status: status.value, branchId: branchId.value })))
const rows = computed(() => accounts.data.value ?? [])
const showPassword = ref(false)

function generatePassword(length = 10) {
  const alphabet = 'ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789'
  const bytes = new Uint32Array(length)
  crypto.getRandomValues(bytes)
  return Array.from(bytes, (b) => alphabet[b % alphabet.length]).join('')
}

function generateInto(f: AccountForm) {
  f.password = generatePassword()
  showPassword.value = true
}

const form = ref<AccountForm | null>(null)
const formError = ref<string | null>(null)
const save = useSavePromoterMutation()

function openCreate() {
  showPassword.value = false
  formError.value = null
  form.value = {
    id: null,
    fullName: '',
    username: '',
    phone: '',
    password: '',
    branchIds: branchId.value != null ? [branchId.value] : branches.value.length === 1 ? [branches.value[0].id] : [],
  }
}

function openEdit(p: PromoterAccount) {
  showPassword.value = false
  formError.value = null
  form.value = {
    id: p.id,
    fullName: p.fullName,
    username: p.username,
    phone: p.phone ?? '',
    password: '',
    branchIds: p.branches.map((b) => b.id),
  }
}

function submitForm() {
  const f = form.value
  if (!f) return
  const fullName = f.fullName.trim()
  const username = f.username.trim()
  if (fullName.length < 2) return void (formError.value = 'اكتب اسم المروّج.')
  if (username.length < 3) return void (formError.value = 'اسم المستخدم 3 أحرف على الأقل.')
  if (!f.branchIds.length) return void (formError.value = 'اختر فرعاً واحداً على الأقل.')
  if (f.id == null && f.password.length < 8) return void (formError.value = 'كلمة المرور 8 أحرف على الأقل.')
  formError.value = null
  const body = {
    fullName,
    username,
    phone: f.phone.trim() || null,
    branchIds: f.branchIds,
    ...(f.id == null ? { password: f.password } : {}),
  }
  save.mutate(
    { id: f.id, body },
    {
      onSuccess: () => {
        toast.add({ severity: 'success', summary: f.id == null ? 'تمت إضافة المروّج' : 'تم حفظ التعديلات', life: 3000 })
        form.value = null
      },
      onError: (e) => {
        formError.value = apiErrorMessage(e, 'تعذر حفظ المروّج')
      },
    }
  )
}

const passwordFor = ref<PromoterAccount | null>(null)
const newPassword = ref('')
const passwordError = ref<string | null>(null)
const setPassword = useSetPromoterPasswordMutation()

function openPassword(p: PromoterAccount) {
  showPassword.value = false
  newPassword.value = ''
  passwordError.value = null
  passwordFor.value = p
}

function submitPassword() {
  const p = passwordFor.value
  if (!p) return
  if (newPassword.value.length < 8) return void (passwordError.value = 'كلمة المرور 8 أحرف على الأقل.')
  passwordError.value = null
  setPassword.mutate(
    { id: p.id, password: newPassword.value },
    {
      onSuccess: () => {
        toast.add({ severity: 'success', summary: 'تم تغيير كلمة المرور', detail: `سُجِّل خروج ${p.fullName} من كل الأجهزة`, life: 3500 })
        newPassword.value = ''
        passwordFor.value = null
      },
      onError: (e) => {
        passwordError.value = apiErrorMessage(e, 'تعذر تغيير كلمة المرور')
      },
    }
  )
}

const suspendFor = ref<PromoterAccount | null>(null)
const suspendReason = ref('')
const suspendError = ref<string | null>(null)
const suspend = useSuspendPromoterMutation()

function openSuspend(p: PromoterAccount) {
  suspendReason.value = ''
  suspendError.value = null
  suspendFor.value = p
}

function submitSuspend() {
  const p = suspendFor.value
  if (!p) return
  const suspended = !p.isSuspended
  suspend.mutate(
    { id: p.id, suspended, reason: suspended ? suspendReason.value.trim() || null : null },
    {
      onSuccess: () => {
        toast.add({ severity: 'success', summary: suspended ? 'تم إيقاف المروّج' : 'تم تفعيل المروّج', life: 3000 })
        suspendFor.value = null
      },
      onError: (e) => {
        suspendError.value = apiErrorMessage(e)
      },
    }
  )
}

const targetFor = ref<PromoterAccount | null>(null)
const targetHours = ref<string | number>('')
const targetMinutes = ref<string | number>('')
const targetError = ref<string | null>(null)
const target = useSetPromoterTargetMutation()

function openTarget(p: PromoterAccount) {
  targetError.value = null
  targetHours.value = p.targetMinutes == null ? '' : Math.floor(p.targetMinutes / 60)
  targetMinutes.value = p.targetMinutes == null ? '' : p.targetMinutes % 60
  targetFor.value = p
}

function submitTarget() {
  const p = targetFor.value
  if (!p) return
  const hRaw = String(targetHours.value ?? '').trim()
  const mRaw = String(targetMinutes.value ?? '').trim()
  let minutes: number | null = null
  if (hRaw !== '' || mRaw !== '') {
    const h = hRaw === '' ? 0 : Number(hRaw)
    const m = mRaw === '' ? 0 : Number(mRaw)
    if (!Number.isInteger(h) || !Number.isInteger(m) || h < 0 || m < 0 || m > 59) return void (targetError.value = 'أدخل ساعات ودقائق صحيحة.')
    minutes = h * 60 + m
    if (minutes > 1440) return void (targetError.value = 'الهدف لا يتجاوز 24 ساعة.')
  }
  targetError.value = null
  target.mutate(
    { id: p.id, minutes },
    {
      onSuccess: () => {
        toast.add({ severity: 'success', summary: 'تم حفظ الهدف اليومي', life: 3000 })
        targetFor.value = null
      },
      onError: (e) => {
        targetError.value = apiErrorMessage(e)
      },
    }
  )
}
</script>
