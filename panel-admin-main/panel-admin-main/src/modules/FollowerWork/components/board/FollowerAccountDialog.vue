<template>
  <NeuDialog
    :visible="visible"
    size="md"
    :busy="busy"
    :title="mode === 'create' ? 'إنشاء متابع' : `تعديل المتابع — ${initialName ?? ''}`"
    :subtitle="branchName"
    @close="emit('close')"
  >
    <form id="fwb-account-form" class="fwb-form" novalidate @submit.prevent="submit">
      <label class="dfb-field">
        <span>الاسم الثلاثي *</span>
        <input v-model="fullName" class="dfb-search" maxlength="50" autocomplete="off" required>
      </label>
      <label v-if="mode === 'create'" class="dfb-field">
        <span>اسم المستخدم * (3 أحرف على الأقل)</span>
        <input v-model="username" class="dfb-search" maxlength="100" autocomplete="off" dir="ltr" required>
      </label>
      <label class="dfb-field">
        <span>{{ mode === 'create' ? 'الرمز السري * (8 أحرف على الأقل)' : 'رمز سري جديد (اتركه فارغاً لعدم التغيير)' }}</span>
        <input v-model="password" class="dfb-search" type="password" maxlength="50" autocomplete="new-password" dir="ltr">
      </label>
      <label class="dfb-field">
        <span>{{ mode === 'create' ? 'رقم الهاتف' : 'رقم هاتف جديد (اتركه فارغاً لعدم التغيير)' }}</span>
        <input v-model="phone" class="dfb-search" maxlength="30" dir="ltr" inputmode="tel">
      </label>

      <fieldset class="dfb-field fwb-lists">
        <legend>إسناد القوائم لهذا المتابع</legend>
        <p class="dfb-hint">إسناد قائمة ينقلها من متابعها الحالي. لا يمكن فكّ قائمة بلا متابع — انقلها لمتابع آخر من حسابه.</p>
        <p v-if="listsQuery.isPending.value" class="dfb-state">جاري تحميل القوائم…</p>
        <label v-for="l in lists" :key="l.listId" class="fwb-check">
          <input v-model="selected" type="checkbox" :value="l.listId" :disabled="l.followerId === followerId">
          <span>{{ l.listName }}</span>
          <small class="dfb-hint">{{ l.followerId === followerId ? 'مسندة له' : `حالياً: ${l.followerName ?? '—'}` }}</small>
        </label>
      </fieldset>

      <p v-if="error" class="dfb-form-error" role="alert">{{ error }}</p>
    </form>

    <template #footer>
      <button v-if="mode === 'edit'" type="button" class="dfb-button fwb-danger" :disabled="busy" @click="remove">حذف المتابع</button>
      <button type="button" class="dfb-button" :disabled="busy" @click="emit('close')">إلغاء</button>
      <button type="submit" form="fwb-account-form" class="dfb-button is-primary" :disabled="busy">
        {{ busy ? 'جاري الحفظ…' : mode === 'create' ? 'إنشاء' : 'حفظ' }}
      </button>
    </template>
  </NeuDialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import NeuDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { FOLLOWER_WORK_KEY } from '../../domain/queryKeys'
import { useManageFollowerMutations } from '../../requests/queries/useFollowerBoardQueries'

interface AssignableList { listId: number; listName: string; followerId: number; followerName: string | null }

const props = defineProps<{
  visible: boolean
  mode: 'create' | 'edit'
  branchId: number | null
  branchName?: string
  followerId: number | null
  initialName?: string | null
}>()

const emit = defineEmits<{ close: []; saved: [followerId: number | null] }>()

const fullName = ref('')
const username = ref('')
const password = ref('')
const phone = ref('')
const selected = ref<number[]>([])
const error = ref('')
const { create, update, remove: removeMutation } = useManageFollowerMutations()
const busy = computed(() => create.isPending.value || update.isPending.value || removeMutation.isPending.value)

const listsQuery = useQuery({
  queryKey: computed(() => [FOLLOWER_WORK_KEY, 'assignable-lists', props.branchId]),
  enabled: computed(() => props.visible && props.branchId != null),
  queryFn: async () => (await axiosInstance.get<AssignableList[]>(`${FollowerWorkEndpoints.manageFollowers}/assignable-lists`, { params: { branchId: props.branchId } })).data,
})
const lists = computed(() => listsQuery.data.value ?? [])

watch(() => props.visible, (v) => {
  if (!v) return
  fullName.value = props.mode === 'edit' ? props.initialName ?? '' : ''
  username.value = ''
  password.value = ''
  phone.value = ''
  selected.value = []
  error.value = ''
})

function apiError(e: unknown) {
  const data = (e as { response?: { data?: { message?: string; title?: string; errors?: Record<string, string[]> } } })?.response?.data
  const first = data?.errors ? Object.values(data.errors).flat()[0] : null
  return first || data?.message || data?.title || 'تعذر الحفظ'
}

async function submit() {
  error.value = ''
  if (fullName.value.trim() === '') { error.value = 'اسم المتابع مطلوب'; return }
  if (props.mode === 'create') {
    if (username.value.trim().length < 3) { error.value = 'اسم المستخدم 3 أحرف على الأقل'; return }
    if (password.value.length < 8) { error.value = 'الرمز السري 8 أحرف على الأقل'; return }
  } else if (password.value !== '' && password.value.length < 8) {
    error.value = 'الرمز السري 8 أحرف على الأقل'
    return
  }
  try {
    if (props.mode === 'create') {
      const id = await create.mutateAsync({
        fullName: fullName.value,
        username: username.value,
        password: password.value,
        phoneNumber: phone.value || null,
        branchIds: props.branchId != null ? [props.branchId] : [],
        listIds: selected.value,
      })
      emit('saved', id)
    } else if (props.followerId != null) {
      await update.mutateAsync({
        followerId: props.followerId,
        payload: {
          fullName: fullName.value,
          phoneNumber: phone.value || undefined,
          newPassword: password.value || undefined,
          assignListIds: selected.value,
        },
      })
      emit('saved', props.followerId)
    }
  } catch (e) {
    error.value = apiError(e)
  }
}

async function remove() {
  if (props.followerId == null) return
  if (window.confirm(`حذف حساب المتابع «${props.initialName ?? ''}» نهائياً؟ لا يمكن التراجع، ويُرفض إذا كان له قوائم أو سجل عمل.`) === false) return
  error.value = ''
  try {
    await removeMutation.mutateAsync(props.followerId)
    emit('saved', null)
  } catch (e) {
    error.value = apiError(e)
  }
}
</script>
