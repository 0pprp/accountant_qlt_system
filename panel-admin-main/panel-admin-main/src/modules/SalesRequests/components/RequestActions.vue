<template>
  <footer class="sr-actions">
    <button
      v-if="canEdit"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy || !canEditRequest"
      @click="editOpen = true"
    >
      تعديل
    </button>
    <button
      v-if="canAssign"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy || !canAssignByRecord"
      :title="assignBlockedReason || undefined"
      @click="dialogOpen = true"
    >
      {{
        isReassign
          ? $t('salesRequests.actions.reassign')
          : $t('salesRequests.actions.assign')
      }}
    </button>
    <p
      v-if="canAssign && assignBlockedReason"
      class="sr-actions__hint"
      role="status"
    >
      {{ assignBlockedReason }}
    </p>
    <button
      v-if="canReject"
      type="button"
      class="sr-neu-btn danger"
      :disabled="busy"
      @click="reasonMode = 'reject'"
    >
      {{ $t('salesRequests.actions.reject') }}
    </button>
    <button
      v-if="canException"
      type="button"
      class="sr-neu-btn warn"
      :disabled="busy"
      @click="reasonMode = 'exception'"
    >
      {{ $t('salesRequests.actions.exception') }}
    </button>

    <Dialog
      :visible="reasonMode != null"
      modal
      :closable="!busy"
      :close-on-escape="!busy"
      :header="reasonMode === 'reject' ? 'رفض الطلب' : 'طلب استثناء'"
      :style="{ width: 'min(480px, 92vw)' }"
      class="bg-white rounded-3xl p-6"
      :pt="{ mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' } }"
      dir="rtl"
      @update:visible="!$event && !busy && (reasonMode = null)"
    >
      <template #closebutton="{ closeCallback }">
        <button
          type="button"
          class="sr-dlg-close"
          :disabled="busy"
          aria-label="إغلاق"
          @click="closeCallback"
        >
          ×
        </button>
      </template>
      <form @submit.prevent="submitReason">
        <p class="mb-4">{{ request.customerName }} · طلب #{{ request.id }}</p>
        <AppTextarea
          ref="reasonField"
          v-model="reasonText"
          :label="reasonMode === 'reject' ? 'سبب الرفض' : 'سبب الاستثناء'"
          placeholder="اكتب السبب بوضوح"
          :maxlength="1000"
          :rows="4"
          :disabled="busy"
          required
        />
        <div class="sr-dialog-row">
          <button
            type="submit"
            class="sr-neu-btn"
            :class="reasonMode === 'reject' ? 'danger' : 'warn'"
            :disabled="busy || !reasonText.trim()"
          >
            {{ busy ? 'جارٍ الإرسال…' : 'تأكيد' }}
          </button>
          <button
            type="button"
            class="sr-neu-btn primary"
            :disabled="busy"
            @click="reasonMode = null"
          >
            إلغاء
          </button>
        </div>
      </form>
    </Dialog>

    <AssignEmployeeDialog
      :open="dialogOpen"
      :branch-id="request.branchId"
      :mode="isReassign ? 'reassign' : 'assign'"
      :busy="busy && pending === 'assign'"
      @close="dialogOpen = false"
      @confirm="onAssignConfirm"
    />

    <EditSalesRequestDialog
      :open="editOpen"
      :request="request"
      :busy="busy && pending === 'edit'"
      :error="editError"
      @close="editOpen = false"
      @confirm="onEditConfirm"
    />
  </footer>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, nextTick, reactive, ref, watch } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import { AppTextarea } from '@/modules/Core/components/ui'
import type { SalesRequestListItem } from '../types'
import { useAssignSalesRequestMutation } from '../requests/mutations/useAssignSalesRequestMutation'
import { useRejectSalesRequestMutation } from '../requests/mutations/useRejectSalesRequestMutation'
import { useRequestExceptionMutation } from '../requests/mutations/useRequestExceptionMutation'
import { useUpdateSalesRequestMutation } from '../requests/mutations/useUpdateSalesRequestMutation'
import AssignEmployeeDialog from './AssignEmployeeDialog.vue'
import EditSalesRequestDialog from './EditSalesRequestDialog.vue'

const props = defineProps<{
  request: SalesRequestListItem
  draft?: { reject?: string; exception?: string; assignReason?: string }
}>()

const emit = defineEmits<{
  done: []
  'draft-change': [
    { reject?: string; exception?: string; assignReason?: string },
  ]
}>()

const { can } = usePermission()
const canAssign = can('SalesRequest', 'Assign')
const canReject = can('SalesRequest', 'Reject')
const canException = can('SalesRequest', 'RequestException')
const canEdit =
  can('SalesRequest', 'UpdateRequest') || can('SalesRequest', 'Assign')
const canEditRequest = computed(
  () =>
    !props.request.isDeleted &&
    props.request.tabKey !== 'sold' &&
    Number(props.request.status) !== 9
)

/** قواعد السجل — ليست رسالة صلاحية */
const assignBlockedReason = computed(() => {
  if (props.request.isDeleted) return 'الطلب محذوف — لا يمكن إسناد جديد.'
  if (props.request.tabKey === 'sold' || Number(props.request.status) === 9) {
    return 'الطلب مكتمل/مباع — لا يقبل إسنادًا جديدًا.'
  }
  return null
})

const canAssignByRecord = computed(() => assignBlockedReason.value == null)

const localDraft = reactive({
  reject: props.draft?.reject || '',
  exception: props.draft?.exception || '',
  assignReason: props.draft?.assignReason || '',
})

watch(
  () => props.draft,
  (d) => {
    if (!d) return
    localDraft.reject = d.reject || localDraft.reject
    localDraft.exception = d.exception || localDraft.exception
    localDraft.assignReason = d.assignReason || localDraft.assignReason
  },
  { deep: true }
)

function emitDraft() {
  emit('draft-change', { ...localDraft })
}

const isReassign = computed(
  () =>
    props.request.targetEmployeeId != null &&
    props.request.targetEmployeeId > 0 &&
    props.request.tabKey !== 'unassigned'
)

const reasonMode = ref<'reject' | 'exception' | null>(null)
const reasonField = ref<{ focus: () => void } | null>(null)
watch(reasonMode, (mode) => {
  if (!mode) return
  void nextTick(() => reasonField.value?.focus())
})
const reasonText = computed({
  get: () => (reasonMode.value ? localDraft[reasonMode.value] : ''),
  set: (value: string) => {
    if (reasonMode.value) localDraft[reasonMode.value] = value
    emitDraft()
  },
})
async function submitReason() {
  if (!reasonText.value.trim() || busy.value) return
  try {
    if (reasonMode.value === 'reject') await onReject()
    else await onException()
    reasonMode.value = null
  } catch {
    /* mutation reports the error; preserve the draft */
  }
}
const dialogOpen = ref(false)
const editOpen = ref(false)
const editError = ref<string | null>(null)
const pending = ref<'assign' | 'reject' | 'exception' | 'edit' | null>(null)

const assignMut = useAssignSalesRequestMutation()
const rejectMut = useRejectSalesRequestMutation()
const exceptionMut = useRequestExceptionMutation()
const updateMut = useUpdateSalesRequestMutation()

const busy = computed(
  () =>
    assignMut.isPending.value ||
    rejectMut.isPending.value ||
    exceptionMut.isPending.value ||
    updateMut.isPending.value
)

async function onAssignConfirm(payload: {
  employeeId: number
  reason?: string
}) {
  if (busy.value) return
  pending.value = 'assign'
  try {
    await assignMut.mutateAsync({
      requestId: props.request.id,
      employeeId: payload.employeeId,
      reason: payload.reason,
    })
    dialogOpen.value = false
    emit('done')
  } finally {
    pending.value = null
  }
}

async function onReject() {
  if (busy.value) return
  pending.value = 'reject'
  try {
    await rejectMut.mutateAsync({
      requestId: props.request.id,
      reason: localDraft.reject || undefined,
    })
    emit('done')
  } finally {
    pending.value = null
  }
}

async function onException() {
  if (busy.value) return
  pending.value = 'exception'
  try {
    await exceptionMut.mutateAsync({
      requestId: props.request.id,
      reason: localDraft.exception || undefined,
    })
    emit('done')
  } finally {
    pending.value = null
  }
}

function mapEditError(e: unknown): string {
  const ax = e as {
    response?: {
      data?: {
        detail?: string
        title?: string
        errors?: { message?: string }[]
      }
    }
  }
  return (
    ax.response?.data?.errors?.[0]?.message ||
    ax.response?.data?.detail ||
    ax.response?.data?.title ||
    'تعذر حفظ التعديل'
  )
}

async function onEditConfirm(payload: {
  customerName: string
  customerPhone: string
  address?: string
  requestedProduct?: string
  branchId: number
  provinceId?: number | null
  employeeId?: number | null
  reason?: string
}) {
  if (busy.value) return
  pending.value = 'edit'
  editError.value = null
  try {
    await updateMut.mutateAsync({
      requestId: props.request.id,
      ...payload,
    })
    editOpen.value = false
    emit('done')
  } catch (e) {
    editError.value = mapEditError(e)
  } finally {
    pending.value = null
  }
}
</script>

<style scoped>
.sr-actions {
  display: contents;
}
.sr-actions__hint {
  margin: 0;
  grid-column: 1 / -1;
  font-size: 0.75rem;
  color: #8a4b00;
  line-height: 1.4;
}
.sr-dialog-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 1.1rem;
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
.sr-dlg-close {
  width: 2.35rem;
  height: 2.35rem;
  min-width: 2.35rem;
  min-height: 2.35rem;
  padding: 0;
  border: 1px solid #e8a39a;
  border-radius: 10px;
  background: #fff5f4;
  color: #c62828;
  font-size: 1.55rem;
  font-weight: 700;
  line-height: 1;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}
.sr-dlg-close:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
</style>
