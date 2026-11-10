<template>
  <footer class="sr-actions">
    <div class="sr-actions-inner">
      <div
        v-if="canAssign"
        class="sr-row"
      >
        <button
          type="button"
          class="sr-btn primary"
          :disabled="busy"
          @click="dialogOpen = true"
        >
          {{
            isReassign
              ? $t('salesRequests.actions.reassign')
              : $t('salesRequests.actions.assign')
          }}
        </button>
      </div>

      <button v-if="canReject" type="button" class="sr-btn danger" :disabled="busy" @click="reasonMode = 'reject'">رفض</button>
      <button v-if="canException" type="button" class="sr-btn warn" :disabled="busy" @click="reasonMode = 'exception'">طلب استثناء</button>
    </div>
    <Dialog :visible="reasonMode != null" modal :closable="!busy" :close-on-escape="!busy" :header="reasonMode === 'reject' ? 'رفض الطلب' : 'طلب استثناء'" :style="{width:'min(480px, 92vw)'}" class="bg-white rounded-3xl p-6" :pt="{mask:{class:'bg-[#C6C6C680] backdrop-blur-[2px]'}}" dir="rtl" @update:visible="!$event && !busy && (reasonMode = null)">
      <form @submit.prevent="submitReason">
        <p class="mb-4">{{ request.customerName }} · طلب #{{ request.id }}</p>
        <label class="block" :for="`reason-${request.id}`">{{ reasonMode === 'reject' ? 'سبب الرفض' : 'سبب الاستثناء' }} <span aria-hidden="true">*</span></label>
        <textarea :id="`reason-${request.id}`" v-model="reasonText" required maxlength="1000" rows="4" autofocus class="sr-input w-full mt-2" style="font-size:16px;line-height:1.8" :disabled="busy" placeholder="اكتب السبب بوضوح" />
        <div class="flex gap-3 mt-5">
          <button type="submit" class="sr-btn" :class="reasonMode === 'reject' ? 'danger' : 'warn'" :disabled="busy || !reasonText.trim()">{{ busy ? 'جارٍ الإرسال…' : 'تأكيد' }}</button>
          <button type="button" class="sr-btn primary" :disabled="busy" @click="reasonMode = null">إلغاء</button>
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
  </footer>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import { computed, reactive, ref, watch } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import type { SalesRequestListItem } from '../types'
import { useAssignSalesRequestMutation } from '../requests/mutations/useAssignSalesRequestMutation'
import { useRejectSalesRequestMutation } from '../requests/mutations/useRejectSalesRequestMutation'
import { useRequestExceptionMutation } from '../requests/mutations/useRequestExceptionMutation'
import AssignEmployeeDialog from './AssignEmployeeDialog.vue'

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

function onRejectInput(e: Event) {
  localDraft.reject = (e.target as HTMLInputElement).value
  emitDraft()
}
function onExceptionInput(e: Event) {
  localDraft.exception = (e.target as HTMLInputElement).value
  emitDraft()
}

const isReassign = computed(
  () =>
    props.request.targetEmployeeId != null &&
    props.request.targetEmployeeId > 0 &&
    props.request.tabKey !== 'unassigned'
)

const reasonMode = ref<'reject' | 'exception' | null>(null)
const reasonText = computed({get: () => reasonMode.value ? localDraft[reasonMode.value] : '', set: (value: string) => {if (reasonMode.value) localDraft[reasonMode.value] = value; emitDraft()}})
async function submitReason() {
  if (!reasonText.value.trim() || busy.value) return
  try { if (reasonMode.value === 'reject') await onReject(); else await onException(); reasonMode.value = null } catch { /* mutation reports the error; preserve the draft */ }
}
const dialogOpen = ref(false)
const pending = ref<'assign' | 'reject' | 'exception' | null>(null)

const assignMut = useAssignSalesRequestMutation()
const rejectMut = useRejectSalesRequestMutation()
const exceptionMut = useRequestExceptionMutation()

const busy = computed(
  () =>
    assignMut.isPending.value ||
    rejectMut.isPending.value ||
    exceptionMut.isPending.value
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
</script>

<style scoped>
.sr-actions {
  position: static;
  bottom: 0;
  padding-bottom: max(0.75rem, env(safe-area-inset-bottom));
  margin-top: 1rem;
  background: linear-gradient(180deg, transparent, #fff 30%);
}
.sr-actions-inner {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 0.75rem;
  border-radius: 1rem;
  background: #fff;
  padding: 0.75rem 0;
}
.sr-row {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: 0.5rem;
}
.sr-input {
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
  padding: 0.5rem 0.75rem;
  min-height: 44px;
  background: #f8f8f8;
  color: #00201e;
}
.sr-btn {
  border-radius: 0.85rem;
  padding: 0.55rem 1rem;
  min-height: 44px;
  color: #fff;
  font-family: inherit;
  border: none;
  cursor: pointer;
}
.sr-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.sr-btn.primary {
  background: #006a64;
}
.sr-btn.danger {
  background: #9b1c1c;
}
.sr-btn.warn {
  background: #9a6700;
}
.grow {
  flex: 1;
}
</style>
