<template>
  <section
    v-if="visible"
    class="sr-life"
  >
    <!-- استثناءات: قرار مستقل عن RequestException -->
    <div
      v-if="showExceptionDecide"
      class="sr-life-block"
    >
      <h4 class="sr-life-title">{{ $t('salesRequests.lifecycle.exceptionTitle') }}</h4>
      <p class="sr-life-meta">
        {{ $t('salesRequests.lifecycle.exceptionState') }}:
        <strong>{{ exceptionLabel }}</strong>
      </p>
      <p
        v-if="request.exceptionConsumedAtUtc"
        class="sr-life-meta"
      >
        {{ $t('salesRequests.lifecycle.consumedAt') }}:
        {{ formatDate(request.exceptionConsumedAtUtc) }}
      </p>
      <template v-if="canDecide && request.exceptionState === 'pending'">
        <label class="sr-field">
          {{ $t('salesRequests.actions.reason') }}
          <input
            v-model="decisionReason"
            type="text"
            class="sr-input"
            :disabled="busy"
          />
        </label>
        <div class="sr-row">
          <button
            type="button"
            class="sr-btn primary"
            :disabled="busy || !decisionReason.trim()"
            @click="onApprove"
          >
            {{
              busy && pending === 'approve'
                ? $t('salesRequests.actions.submitting')
                : $t('salesRequests.lifecycle.approveException')
            }}
          </button>
          <button
            type="button"
            class="sr-btn danger"
            :disabled="busy || !decisionReason.trim()"
            @click="onRejectException"
          >
            {{
              busy && pending === 'rejectEx'
                ? $t('salesRequests.actions.submitting')
                : $t('salesRequests.lifecycle.rejectException')
            }}
          </button>
        </div>
        <p
          v-if="errorMsg"
          class="sr-err"
          role="alert"
        >
          {{ errorMsg }}
        </p>
      </template>
      <p
        v-else-if="request.exceptionState === 'approved_consumed'"
        class="sr-life-meta"
      >
        {{ $t('salesRequests.lifecycle.consumedHint') }}
      </p>
    </div>

    <!-- حذف ناعم من سير العمل (ليس من تبويب المحذوفة) -->
    <div
      v-if="showSoftDelete"
      class="sr-life-block"
    >
      <h4 class="sr-life-title">{{ $t('salesRequests.lifecycle.deleteTitle') }}</h4>
      <p class="sr-life-meta">{{ $t('salesRequests.lifecycle.deleteImpact') }}</p>
      <label class="sr-field">
        {{ $t('salesRequests.actions.reason') }}
        <input
          v-model="deleteReason"
          type="text"
          class="sr-input"
          :disabled="busy"
        />
      </label>
      <button
        type="button"
        class="sr-btn danger"
        :disabled="busy || !deleteReason.trim()"
        @click="onSoftDelete"
      >
        {{
          busy && pending === 'delete'
            ? $t('salesRequests.actions.submitting')
            : $t('salesRequests.lifecycle.softDelete')
        }}
      </button>
      <p
        v-if="errorMsg && pending === 'delete'"
        class="sr-err"
        role="alert"
      >
        {{ errorMsg }}
      </p>
    </div>

    <!-- استعادة من المحذوفة -->
    <div
      v-if="showRestore"
      class="sr-life-block"
    >
      <h4 class="sr-life-title">{{ $t('salesRequests.lifecycle.restoreTitle') }}</h4>
      <p
        v-if="request.deleteReason"
        class="sr-life-meta"
      >
        {{ $t('salesRequests.lifecycle.deleteReason') }}: {{ request.deleteReason }}
      </p>
      <p
        v-if="request.deletedAtUtc"
        class="sr-life-meta"
      >
        {{ $t('salesRequests.lifecycle.deletedAt') }}:
        {{ formatDate(request.deletedAtUtc) }}
      </p>
      <p class="sr-life-meta">{{ $t('salesRequests.lifecycle.restoreImpact') }}</p>
      <label class="sr-field">
        {{ $t('salesRequests.actions.reason') }}
        <input
          v-model="restoreReason"
          type="text"
          class="sr-input"
          :disabled="busy"
        />
      </label>
      <button
        type="button"
        class="sr-btn primary"
        :disabled="busy"
        @click="onRestore"
      >
        {{
          busy && pending === 'restore'
            ? $t('salesRequests.actions.submitting')
            : $t('salesRequests.lifecycle.restore')
        }}
      </button>
      <p
        v-if="errorMsg && pending === 'restore'"
        class="sr-err"
        role="alert"
      >
        {{ errorMsg }}
      </p>
    </div>

    <div
      v-if="showSendLegal"
      class="sr-life-block"
    >
      <h4 class="sr-life-title">إرسال للمتابعة القانونية</h4>
      <p class="sr-life-meta">لغير المسند أو المرفوض فقط — ينشئ قضية ويصفّر تكليف الموظف.</p>
      <label class="sr-field">
        المتابع
        <select
          v-model="followerId"
          class="sr-input"
          :disabled="busy || followersLoading"
        >
          <option :value="0">اختر متابعًا</option>
          <option
            v-for="f in followers"
            :key="f.followerUserId"
            :value="f.followerUserId"
          >
            {{ f.followerName }}
          </option>
        </select>
      </label>
      <label class="sr-field">
        ملاحظة (اختياري)
        <input
          v-model="legalNote"
          type="text"
          class="sr-input"
          :disabled="busy"
        />
      </label>
      <button
        type="button"
        class="sr-btn primary"
        :disabled="busy || !followerId"
        @click="onSendLegal"
      >
        {{
          busy && pending === 'legal'
            ? $t('salesRequests.actions.submitting')
            : 'إرسال للقانونية'
        }}
      </button>
      <p
        v-if="errorMsg && pending === 'legal'"
        class="sr-err"
        role="alert"
      >
        {{ errorMsg }}
      </p>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { usePermission } from '@/modules/Core/composable/usePermission'
import type { SalesRequestListItem } from '../types'
import { useApproveExceptionMutation } from '../requests/mutations/useApproveExceptionMutation'
import { useRejectExceptionMutation } from '../requests/mutations/useRejectExceptionMutation'
import { useSoftDeleteMutation } from '../requests/mutations/useSoftDeleteMutation'
import { useRestoreMutation } from '../requests/mutations/useRestoreMutation'
import { useCreateLegalFollowupMutation } from '@/modules/SalesLegal/requests/mutations/useCreateLegalFollowupMutation'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { SalesLegalEndpoints } from '@/modules/SalesLegal/domain/endpoints'
import type { LegalFollowupFollowerOption } from '@/modules/SalesLegal/domain/types'

const props = defineProps<{
  request: SalesRequestListItem
  /** التبويب الحالي للمساحة — يقيّد أزرار الاستعادة/الحذف حسب السياق */
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

const decisionReason = ref('')
const deleteReason = ref('')
const restoreReason = ref('')
const legalNote = ref('')
const followerId = ref(0)
const followers = ref<LegalFollowupFollowerOption[]>([])
const followersLoading = ref(false)
const pending = ref<'approve' | 'rejectEx' | 'delete' | 'restore' | 'legal' | null>(null)
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

/** New=0 أو Rejected=8 وبلا استثناء حي — مطابق CanSendToLegalFollowup */
const showSendLegal = computed(() => {
  if (!canCreateLegal || props.request.isDeleted) return false
  if (props.request.exceptionState === 'pending' || props.request.exceptionState === 'approved')
    return false
  const st = Number(props.request.status)
  if (st === 8) return true
  return st === 0 && !(props.request.targetEmployeeId != null && props.request.targetEmployeeId > 0)
})

const visible = computed(
  () =>
    showExceptionDecide.value ||
    showSoftDelete.value ||
    showRestore.value ||
    showSendLegal.value
)

async function loadFollowers() {
  if (!showSendLegal.value) return
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

watch(showSendLegal, (v) => {
  if (v) void loadFollowers()
})
onMounted(() => {
  if (showSendLegal.value) void loadFollowers()
})

const exceptionLabel = computed(() => {
  switch (props.request.exceptionState) {
    case 'pending':
      return 'بانتظار القرار'
    case 'approved':
      return 'موافق عليه (غير مستهلك)'
    case 'approved_consumed':
      return 'موافق عليه ومُستهلك'
    case 'rejected':
      return 'مرفوض'
    default:
      return '—'
  }
})

function formatDate(value?: string | null) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('ar-IQ', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function mapError(e: unknown): string {
  const ax = e as {
    response?: { status?: number; data?: { title?: string; detail?: string; errors?: { code?: string; message?: string }[] } }
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
  if (status === 409) {
    return msg || 'تعارض تزامن (409) — أعد التحميل'
  }
  if (status === 403) return msg || 'غير مصرح'
  return msg || code || 'تعذر تنفيذ الإجراء'
}

async function confirm(message: string): Promise<boolean> {
  return window.confirm(message)
}

async function onApprove() {
  if (!decisionReason.value.trim() || busy.value) return
  if (!(await confirm('تأكيد الموافقة على الاستثناء؟ لا يمكن للموافق أن يكون طالب الاستثناء.')))
    return
  pending.value = 'approve'
  errorMsg.value = null
  try {
    await approveMut.mutateAsync({
      requestId: props.request.id,
      reason: decisionReason.value.trim(),
    })
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    pending.value = null
  }
}

async function onRejectException() {
  if (!decisionReason.value.trim() || busy.value) return
  if (!(await confirm('تأكيد رفض الاستثناء؟'))) return
  pending.value = 'rejectEx'
  errorMsg.value = null
  try {
    await rejectExMut.mutateAsync({
      requestId: props.request.id,
      reason: decisionReason.value.trim(),
    })
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    pending.value = null
  }
}

async function onSoftDelete() {
  if (!deleteReason.value.trim() || busy.value) return
  if (
    !(await confirm(
      'حذف ناعم: يختفي الطلب من سير العمل ويظهر في المحذوفة. لا يُحذف البيع/المالية. المتابعة؟'
    ))
  )
    return
  pending.value = 'delete'
  errorMsg.value = null
  try {
    await softDeleteMut.mutateAsync({
      requestId: props.request.id,
      reason: deleteReason.value.trim(),
    })
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    pending.value = null
  }
}

async function onRestore() {
  if (busy.value) return
  if (
    !(await confirm(
      'الاستعادة لا تعيد حالة غير قانونية ولا تفتح طلبًا مكتملًا. المتابعة؟'
    ))
  )
    return
  pending.value = 'restore'
  errorMsg.value = null
  try {
    await restoreMut.mutateAsync({
      requestId: props.request.id,
      reason: restoreReason.value.trim() || undefined,
    })
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    pending.value = null
  }
}

async function onSendLegal() {
  if (!followerId.value || busy.value) return
  if (!(await confirm('إرسال الطلب للمتابعة القانونية؟'))) return
  pending.value = 'legal'
  errorMsg.value = null
  try {
    await createLegalMut.mutateAsync({
      salesRequestId: props.request.id,
      followerUserId: followerId.value,
      note: legalNote.value.trim() || undefined,
      expectedConcurrencyStamp: props.request.concurrencyStamp,
    })
    emit('done')
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    pending.value = null
  }
}
</script>

<style scoped>
.sr-life {
  margin-top: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
}
.sr-life-block {
  border: 1px solid #d7e2e0;
  border-radius: 1rem;
  padding: 0.85rem;
  background: #fafcfc;
}
.sr-life-title {
  margin: 0 0 0.45rem;
  font-size: 0.95rem;
}
.sr-life-meta {
  margin: 0.2rem 0;
  font-size: 0.8rem;
  opacity: 0.85;
}
.sr-field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  font-size: 0.85rem;
  margin: 0.55rem 0;
}
.sr-input {
  border: 1px solid #d7e2e0;
  border-radius: 0.75rem;
  padding: 0.5rem 0.75rem;
  min-height: 44px;
  background: #fff;
  font-family: inherit;
}
.sr-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.sr-btn {
  min-height: 44px;
  border: none;
  border-radius: 0.85rem;
  padding: 0.45rem 1rem;
  color: #fff;
  font-family: inherit;
  cursor: pointer;
}
.sr-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.sr-btn.primary {
  background: #006a64;
}
.sr-btn.danger {
  background: #9b1c1c;
}
.sr-err {
  color: #9b1c1c;
  font-size: 0.85rem;
  margin: 0.45rem 0 0;
}
</style>
