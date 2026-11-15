<template>
  <section
    v-if="visible"
    class="sr-life"
  >
    <template v-if="showExceptionDecide && canDecide && request.exceptionState === 'pending'">
      <button
        type="button"
        class="sr-neu-btn primary"
        :disabled="busy"
        @click="openDialog('approve')"
      >
        {{ $t('salesRequests.lifecycle.approveException') }}
      </button>
      <button
        type="button"
        class="sr-neu-btn danger"
        :disabled="busy"
        @click="openDialog('rejectEx')"
      >
        {{ $t('salesRequests.lifecycle.rejectException') }}
      </button>
    </template>

    <button
      v-if="showSoftDelete"
      type="button"
      class="sr-neu-btn danger"
      :disabled="busy"
      @click="openDialog('delete')"
    >
      {{ $t('salesRequests.lifecycle.softDelete') }}
    </button>

    <button
      v-if="showRestore"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy"
      @click="openDialog('restore')"
    >
      {{ $t('salesRequests.lifecycle.restore') }}
    </button>

    <button
      v-if="showSendLegal"
      type="button"
      class="sr-neu-btn warn"
      :disabled="busy"
      @click="openDialog('legal')"
    >
      قانونية
    </button>

    <Dialog
      :visible="dialogMode != null"
      modal
      :closable="!busy"
      :close-on-escape="!busy"
      :header="dialogTitle"
      :style="{ width: 'min(480px, 92vw)' }"
      class="bg-white rounded-3xl p-6"
      :pt="{ mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' } }"
      dir="rtl"
      @update:visible="!$event && !busy && closeDialog()"
    >
      <form
        class="sr-dialog-form"
        @submit.prevent="submitDialog"
      >
        <p class="sr-dialog-lead">
          {{ request.customerName }} · طلب #{{ request.id }}
        </p>
        <p
          v-if="dialogHint"
          class="sr-dialog-hint"
        >
          {{ dialogHint }}
        </p>

        <AppSelect
          v-if="dialogMode === 'legal'"
          v-model="followerId"
          label="المتابع"
          placeholder="اختر متابعًا"
          :options="followerOptions"
          :disabled="busy || followersLoading"
          required
        />

        <AppTextarea
          v-model="reasonText"
          :label="dialogReasonLabel"
          :placeholder="dialogMode === 'legal' ? 'ملاحظة اختيارية' : 'اكتب السبب بوضوح'"
          :maxlength="1000"
          :rows="4"
          :disabled="busy"
          :required="dialogMode !== 'legal' && dialogMode !== 'restore'"
        />

        <p
          v-if="errorMsg"
          class="sr-err"
          role="alert"
        >
          {{ errorMsg }}
        </p>

        <div class="sr-row">
          <button
            type="submit"
            class="sr-neu-btn"
            :class="dialogSubmitClass"
            :disabled="busy || !canSubmitDialog"
          >
            {{ busy ? $t('salesRequests.actions.submitting') : 'تأكيد' }}
          </button>
          <button
            type="button"
            class="sr-neu-btn primary"
            :disabled="busy"
            @click="closeDialog()"
          >
            إلغاء
          </button>
        </div>
      </form>
    </Dialog>
  </section>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, ref } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { AppSelect, AppTextarea } from '@/modules/Core/components/ui'
import type { SalesRequestListItem } from '../types'
import { useApproveExceptionMutation } from '../requests/mutations/useApproveExceptionMutation'
import { useRejectExceptionMutation } from '../requests/mutations/useRejectExceptionMutation'
import { useSoftDeleteMutation } from '../requests/mutations/useSoftDeleteMutation'
import { useRestoreMutation } from '../requests/mutations/useRestoreMutation'
import { useCreateLegalFollowupMutation } from '@/modules/SalesLegal/requests/mutations/useCreateLegalFollowupMutation'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesLegalEndpoints } from '@/modules/SalesLegal/domain/endpoints'
import type { LegalFollowupFollowerOption } from '@/modules/SalesLegal/domain/types'

type DialogMode = 'approve' | 'rejectEx' | 'delete' | 'restore' | 'legal'

const props = defineProps<{
  request: SalesRequestListItem
  activeTab: string
}>()

const emit = defineEmits<{
  done: []
}>()

const { can } = usePermission()
const canDecide = can('SalesRequest', 'ExceptionDecide')
const canSoftDelete = can('SalesRequest', 'SoftDelete')
const canRestore = can('SalesRequest', 'Restore')
const canCreateLegal = can('LegalFollowup', 'Create')

const dialogMode = ref<DialogMode | null>(null)
const reasonText = ref('')
const followerId = ref<number | null>(null)
const followers = ref<LegalFollowupFollowerOption[]>([])
const followersLoading = ref(false)
const errorMsg = ref<string | null>(null)

const approveMut = useApproveExceptionMutation()
const rejectExMut = useRejectExceptionMutation()
const softDeleteMut = useSoftDeleteMutation()
const restoreMut = useRestoreMutation()
const createLegalMut = useCreateLegalFollowupMutation()

const busy = computed(
  () =>
    approveMut.isPending.value ||
    rejectExMut.isPending.value ||
    softDeleteMut.isPending.value ||
    restoreMut.isPending.value ||
    createLegalMut.isPending.value
)

const followerOptions = computed(() =>
  followers.value.map((f) => ({
    label: f.followerName,
    value: f.followerUserId,
  }))
)

const showExceptionDecide = computed(
  () =>
    props.activeTab === 'exceptions' &&
    props.request.exceptionState != null &&
    !props.request.isDeleted
)

const showSoftDelete = computed(
  () =>
    canSoftDelete &&
    !props.request.isDeleted &&
    props.activeTab !== 'deleted' &&
    props.activeTab !== 'sold' &&
    !props.request.linkedOrderId
)

const showRestore = computed(
  () =>
    canRestore &&
    props.activeTab === 'deleted' &&
    props.request.isDeleted === true
)

const showSendLegal = computed(() => {
  if (!canCreateLegal || props.request.isDeleted) return false
  if (
    props.request.exceptionState === 'pending' ||
    props.request.exceptionState === 'approved'
  )
    return false
  const st = Number(props.request.status)
  if (st === 8) return true
  return (
    st === 0 &&
    !(props.request.targetEmployeeId != null && props.request.targetEmployeeId > 0)
  )
})

const visible = computed(
  () =>
    (showExceptionDecide.value &&
      canDecide &&
      props.request.exceptionState === 'pending') ||
    showSoftDelete.value ||
    showRestore.value ||
    showSendLegal.value
)

const dialogTitle = computed(() => {
  switch (dialogMode.value) {
    case 'approve':
      return 'الموافقة على الاستثناء'
    case 'rejectEx':
      return 'رفض الاستثناء'
    case 'delete':
      return 'نقل إلى المحذوفة'
    case 'restore':
      return 'استعادة الطلب'
    case 'legal':
      return 'إرسال للمتابعة القانونية'
    default:
      return ''
  }
})

const dialogHint = computed(() => {
  switch (dialogMode.value) {
    case 'delete':
      return 'لا يُحذف البيع أو السجلات المالية. يظهر الطلب في تبويب المحذوفة.'
    case 'restore':
      return 'الاستعادة لا تعيد حالة غير قانونية ولا تفتح طلبًا مكتملًا.'
    case 'legal':
      return 'لغير المسند أو المرفوض فقط — ينشئ قضية ويصفّر تكليف الموظف.'
    case 'approve':
      return 'لا يمكن لطالب الاستثناء الموافقة على طلبه.'
    default:
      return ''
  }
})

const dialogReasonLabel = computed(() => {
  switch (dialogMode.value) {
    case 'approve':
    case 'rejectEx':
      return 'سبب القرار'
    case 'delete':
      return 'سبب الحذف'
    case 'restore':
      return 'ملاحظة الاستعادة (اختياري)'
    case 'legal':
      return 'ملاحظة (اختياري)'
    default:
      return 'السبب'
  }
})

const dialogSubmitClass = computed(() => {
  if (dialogMode.value === 'delete' || dialogMode.value === 'rejectEx')
    return 'danger'
  if (dialogMode.value === 'legal') return 'warn'
  return 'primary'
})

const canSubmitDialog = computed(() => {
  if (dialogMode.value === 'legal') return followerId.value != null
  if (dialogMode.value === 'restore') return true
  return reasonText.value.trim().length > 0
})

async function loadFollowers() {
  followersLoading.value = true
  try {
    const { data } = await axiosInstance.get<LegalFollowupFollowerOption[]>(
      SalesLegalEndpoints.followers,
      { params: { branchId: props.request.branchId } }
    )
    followers.value = data ?? []
  } catch {
    followers.value = []
  } finally {
    followersLoading.value = false
  }
}

function openDialog(mode: DialogMode) {
  dialogMode.value = mode
  reasonText.value = ''
  followerId.value = null
  errorMsg.value = null
  if (mode === 'legal') void loadFollowers()
}

function closeDialog() {
  if (busy.value) return
  dialogMode.value = null
  reasonText.value = ''
  followerId.value = null
  errorMsg.value = null
}

function mapError(e: unknown): string {
  const ax = e as {
    response?: {
      status?: number
      data?: {
        title?: string
        detail?: string
        errors?: { code?: string; message?: string }[]
      }
    }
  }
  const status = ax.response?.status
  const body = ax.response?.data
  const code = String(body?.errors?.[0]?.code || body?.title || '')
  const msg = body?.errors?.[0]?.message || body?.detail || body?.title
  if (
    code.includes('Exception_Self_Approval') ||
    code.includes('ExceptionSelfApproval')
  ) {
    return 'لا يمكن لطالب الاستثناء الموافقة على طلبه أو رفضه.'
  }
  if (status === 409) return msg || 'تعارض تزامن (409) — أعد التحميل'
  if (status === 403) return msg || 'غير مصرح'
  return msg || code || 'تعذر تنفيذ الإجراء'
}

async function submitDialog() {
  if (!dialogMode.value || busy.value || !canSubmitDialog.value) return
  const mode = dialogMode.value
  const reason = reasonText.value.trim()
  errorMsg.value = null
  try {
    if (mode === 'approve') {
      await approveMut.mutateAsync({
        requestId: props.request.id,
        reason,
      })
    } else if (mode === 'rejectEx') {
      await rejectExMut.mutateAsync({
        requestId: props.request.id,
        reason,
      })
    } else if (mode === 'delete') {
      await softDeleteMut.mutateAsync({
        requestId: props.request.id,
        reason,
      })
    } else if (mode === 'restore') {
      await restoreMut.mutateAsync({
        requestId: props.request.id,
        reason: reason || undefined,
      })
    } else if (mode === 'legal') {
      await createLegalMut.mutateAsync({
        salesRequestId: props.request.id,
        followerUserId: followerId.value!,
        note: reason || undefined,
        expectedConcurrencyStamp: props.request.concurrencyStamp,
      })
    }
    dialogMode.value = null
    reasonText.value = ''
    followerId.value = null
    errorMsg.value = null
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  }
}
</script>

<style scoped>
.sr-life {
  display: contents;
}
.sr-dialog-form {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}
.sr-dialog-lead {
  margin: 0;
  font-weight: 700;
}
.sr-dialog-hint {
  margin: 0;
  font-size: 0.85rem;
  opacity: 0.85;
}
.sr-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.sr-neu-btn {
  min-height: 36px;
  border: none;
  border-radius: 12px;
  padding: 0.3rem 0.4rem;
  font: inherit;
  font-size: 0.78rem;
  font-weight: 700;
  line-height: 1.25;
  white-space: nowrap;
  cursor: pointer;
  background: #e8f0ef;
  box-shadow:
    4px 4px 10px rgba(0, 32, 30, 0.12),
    -4px -4px 10px rgba(255, 255, 255, 0.92);
  transition:
    box-shadow 0.15s ease,
    transform 0.15s ease;
}
.sr-neu-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.sr-neu-btn:active:not(:disabled) {
  transform: translateY(1px);
  box-shadow:
    inset 3px 3px 7px rgba(0, 32, 30, 0.14),
    inset -3px -3px 7px rgba(255, 255, 255, 0.85);
}
.sr-neu-btn.primary {
  color: #006a64;
}
.sr-neu-btn.danger {
  color: #9b1c1c;
}
.sr-neu-btn.warn {
  color: #8a4b00;
}
.sr-err {
  color: #9b1c1c;
  font-size: 0.85rem;
  margin: 0;
}
</style>
