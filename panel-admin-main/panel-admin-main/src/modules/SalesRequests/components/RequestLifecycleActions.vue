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
      v-if="showReturn"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy"
      @click="openDialog('return')"
    >
      إرجاع للموظف
    </button>

    <button
      v-if="showPrepare"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy"
      @click="openDialog('prepare')"
    >
      تجهيز للبيع
    </button>

    <button
      v-if="showPend"
      type="button"
      class="sr-neu-btn warn"
      :disabled="busy"
      @click="openDialog('pend')"
    >
      تعليق
    </button>

    <button
      v-if="showTransfer"
      type="button"
      class="sr-neu-btn primary"
      :disabled="busy"
      @click="openDialog('transfer')"
    >
      نقل لفرع آخر
    </button>

    <button
      v-if="showCancel"
      type="button"
      class="sr-neu-btn warn"
      :disabled="busy"
      @click="openDialog('cancel')"
    >
      إلغاء وإعادة لغير المسند
    </button>

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

    <button
      v-if="showDeleteSold"
      type="button"
      class="sr-neu-btn danger sr-far"
      :disabled="busy"
      @click="openDialog('deleteSold')"
    >
      حذف المبيع نهائياً
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

        <template v-if="dialogMode === 'transfer'">
          <AppSelect
            v-model="targetBranchId"
            label="الفرع الجديد"
            placeholder="اختر الفرع"
            :options="branchOptions"
            :disabled="busy"
            required
          />
          <AppSelect
            v-model="targetEmployeeId"
            label="موظف في الفرع الجديد (اختياري)"
            placeholder="بدون إسناد — يدخل غير المسند"
            :options="targetEmployeeOptions"
            :disabled="busy || targetBranchId == null"
          />
        </template>

        <div
          v-if="dialogMode === 'deleteSold'"
          class="sr-impact"
          role="status"
        >
          <p v-if="impactLoading">
            جاري حساب أثر الحذف…
          </p>
          <template v-else-if="impact">
            <p class="sr-impact__warn">
              سيُمسح المبيع نهائياً ولا يمكن استرجاعه. يظهر بعدها في «حذوفات المبيعات» للأدمن فقط.
            </p>
            <dl>
              <div><dt>سعر البيع</dt><dd>{{ money(impact.sellAmount) }}</dd></div>
              <div><dt>المقدمة</dt><dd>{{ money(impact.prepaymentAmount) }}</dd></div>
              <div><dt>التسديدات التي ستُمسح</dt><dd>{{ impact.paymentsCount }} · {{ money(impact.paidTotal) }}</dd></div>
              <div v-if="impact.foreignPurchaseRefund > 0"><dt>إرجاع شراء خارجي للقاصة</dt><dd>{{ money(impact.foreignPurchaseRefund) }}</dd></div>
              <div v-if="impact.archiveRecords > 0"><dt>سجلات أرشيف تُمسح</dt><dd>{{ impact.archiveRecords }}</dd></div>
              <div class="is-wide"><dt>المواد</dt><dd>{{ itemsText }}</dd></div>
            </dl>
            <label
              v-if="impact.paymentsCount > 0"
              class="sr-confirm"
            >
              <input
                v-model="confirmPayments"
                type="checkbox"
              >
              أفهم أن {{ impact.paymentsCount }} تسديداً ستُمسح نهائياً
            </label>
          </template>
        </div>

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
          :required="dialogMode !== 'legal' && dialogMode !== 'restore' && dialogMode !== 'prepare'"
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
import { computed, ref, watch } from 'vue'
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
import { useAuthStore } from '@/modules/Auth/store'
import { SalesRequestEndpoints } from '../domain/endpoints'
import { fetchAssignableEmployees } from '../requests/queries/useAssignableEmployeesQuery'
import { useSalesManagerActionMutation } from '../requests/mutations/useSalesManagerActionMutation'
import type { AssignableEmployee } from '../types'

type DialogMode =
  | 'approve'
  | 'rejectEx'
  | 'delete'
  | 'restore'
  | 'legal'
  | 'return'
  | 'prepare'
  | 'pend'
  | 'cancel'
  | 'transfer'
  | 'deleteSold'

interface DeleteSoldImpact {
  sellAmount: number
  prepaymentAmount: number
  paidTotal: number
  paymentsCount: number
  foreignPurchaseRefund: number
  archiveRecords: number
  items: { name: string; quantity: number; returnsToStock: boolean }[]
}

const STATUS = {
  New: 0,
  Rejected: 8,
  PreparedForSale: 4,
  Inspected: 6,
  Pending: 7,
  Completed: 9,
  LegalFollowup: 10,
  ConvertedToSale: 11,
} as const

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
const canReturn = can('SalesRequest', 'ReturnRequest')
const canPrepare = can('SalesRequest', 'ManagerPrepare')
const canPend = can('SalesRequest', 'ManagerPend')
const canCancel = can('SalesRequest', 'CancelToUnassigned')
const canTransfer = can('SalesRequest', 'TransferBranch')
const canDeleteSold = can('SalesRequest', 'DeleteSoldSale')
const auth = useAuthStore()

const targetBranchId = ref<number | null>(null)
const targetEmployeeId = ref<number | null>(null)
const targetEmployees = ref<AssignableEmployee[]>([])
const impact = ref<DeleteSoldImpact | null>(null)
const impactLoading = ref(false)
const confirmPayments = ref(false)
const actionMut = useSalesManagerActionMutation()

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
    createLegalMut.isPending.value ||
    actionMut.isPending.value
)

const status = computed(() => Number(props.request.status))
const hasEmployee = computed(
  () => props.request.targetEmployeeId != null && props.request.targetEmployeeId > 0
)
const liveHold = computed(
  () =>
    props.request.exceptionState === 'pending' ||
    props.request.exceptionState === 'approved' ||
    props.request.exceptionState === 'rejected'
)
const alive = computed(() => !props.request.isDeleted)
const notCancellable: number[] = [STATUS.Completed, STATUS.ConvertedToSale, STATUS.Rejected, STATUS.New]
const notTransferable: number[] = [STATUS.Completed, STATUS.ConvertedToSale, STATUS.LegalFollowup]

const branchOptions = computed(() =>
  (auth.branches ?? [])
    .filter((b) => b.id !== props.request.branchId)
    .map((b) => ({ label: b.name, value: b.id }))
)
const targetEmployeeOptions = computed(() =>
  targetEmployees.value.map((e) => ({ label: e.fullName, value: e.id }))
)

const showReturn = computed(
  () => canReturn && alive.value && status.value === STATUS.Rejected && hasEmployee.value
)
const showPrepare = computed(
  () =>
    canPrepare && alive.value && hasEmployee.value && !liveHold.value &&
    (status.value === STATUS.Inspected || status.value === STATUS.Pending)
)
const showPend = computed(
  () =>
    canPend && alive.value && hasEmployee.value && !liveHold.value &&
    (status.value === STATUS.Inspected || status.value === STATUS.PreparedForSale)
)
const showCancel = computed(
  () => canCancel && alive.value && !props.request.linkedOrderId && !notCancellable.includes(status.value)
)
const showTransfer = computed(
  () =>
    canTransfer && alive.value && !props.request.linkedOrderId && !liveHold.value &&
    !notTransferable.includes(status.value) && branchOptions.value.length > 0
)
const showDeleteSold = computed(
  () => canDeleteSold && alive.value && status.value === STATUS.Completed && !!props.request.linkedOrderId
)

const itemsText = computed(() =>
  (impact.value?.items ?? [])
    .map((i) => `${i.name} × ${i.quantity}${i.returnsToStock ? ' (ترجع للمخزن)' : ''}`)
    .join('، ') || '—'
)

watch(targetBranchId, async (id) => {
  targetEmployeeId.value = null
  targetEmployees.value = []
  if (id == null) return
  try {
    targetEmployees.value = await fetchAssignableEmployees(id)
  } catch {
    targetEmployees.value = []
  }
})

function money(n: number | null | undefined) {
  return n == null || n === 0
    ? '—'
    : new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(n)
}

async function loadImpact() {
  impactLoading.value = true
  impact.value = null
  try {
    const { data } = await axiosInstance.get<DeleteSoldImpact>(
      SalesRequestEndpoints.deleteSoldImpact(props.request.id)
    )
    impact.value = data
  } catch (e) {
    errorMsg.value = mapError(e)
  } finally {
    impactLoading.value = false
  }
}

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
    showSendLegal.value ||
    showReturn.value ||
    showPrepare.value ||
    showPend.value ||
    showCancel.value ||
    showTransfer.value ||
    showDeleteSold.value
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
    case 'return':
      return 'إرجاع الطلب للموظف'
    case 'prepare':
      return 'تجهيز الطلب للبيع'
    case 'pend':
      return 'تعليق الطلب'
    case 'cancel':
      return 'إلغاء وإعادة لغير المسند'
    case 'transfer':
      return 'نقل الطلب لفرع آخر'
    case 'deleteSold':
      return 'حذف المبيع نهائياً'
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
    case 'rejectEx':
      return 'رفض الاستثناء ينقل الطلب إلى «مرفوض».'
    case 'return':
      return 'يرجع لنفس الموظف ويظهر عنده كطلب جديد.'
    case 'cancel':
      return 'يُفك الموظف والاستثناء، وتُلغى القضية القانونية المفتوحة إن وجدت.'
    case 'transfer':
      return 'يُنقل بنفس السجل والتاريخ. بدون موظف يدخل «غير المسند» في الفرع الجديد.'
    default:
      return ''
  }
})

const dialogReasonLabel = computed(() => {
  switch (dialogMode.value) {
    case 'approve':
    case 'rejectEx':
      return 'ملاحظة القرار (اختياري)'
    case 'delete':
      return 'سبب الحذف'
    case 'restore':
      return 'ملاحظة الاستعادة (اختياري)'
    case 'legal':
      return 'ملاحظة (اختياري)'
    case 'return':
      return 'ملاحظة الإرجاع'
    case 'prepare':
      return 'ملاحظة (اختياري)'
    case 'pend':
      return 'سبب التعليق'
    case 'cancel':
      return 'سبب الإلغاء'
    case 'transfer':
      return 'سبب النقل'
    case 'deleteSold':
      return 'سبب الحذف (يُحفظ في السجل)'
    default:
      return 'السبب'
  }
})

const dialogSubmitClass = computed(() => {
  if (dialogMode.value === 'delete' || dialogMode.value === 'rejectEx' || dialogMode.value === 'deleteSold')
    return 'danger'
  if (dialogMode.value === 'legal') return 'warn'
  return 'primary'
})

const canSubmitDialog = computed(() => {
  if (dialogMode.value === 'legal') return followerId.value != null
  if (dialogMode.value === 'restore' || dialogMode.value === 'prepare') return true
  if (dialogMode.value === 'transfer')
    return targetBranchId.value != null && reasonText.value.trim().length > 0
  if (dialogMode.value === 'deleteSold')
    return (
      impact.value != null &&
      reasonText.value.trim().length >= 5 &&
      (impact.value.paymentsCount === 0 || confirmPayments.value)
    )
  if (dialogMode.value === 'approve' || dialogMode.value === 'rejectEx')
    return reasonText.value.trim().length <= 500
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
  targetBranchId.value = null
  targetEmployeeId.value = null
  confirmPayments.value = false
  impact.value = null
  if (mode === 'legal') void loadFollowers()
  if (mode === 'deleteSold') void loadImpact()
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
    } else if (mode === 'return') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.returnToEmployee(props.request.id),
        body: { note: reason },
        requestIds: [props.request.id],
      })
    } else if (mode === 'prepare') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.managerPrepare(props.request.id),
        body: { note: reason || null },
        requestIds: [props.request.id],
      })
    } else if (mode === 'pend') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.managerPend(props.request.id),
        body: { note: reason },
        requestIds: [props.request.id],
      })
    } else if (mode === 'cancel') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.cancelToUnassigned(props.request.id),
        body: { reason },
        requestIds: [props.request.id],
      })
    } else if (mode === 'transfer') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.transferBranch(props.request.id),
        body: { targetBranchId: targetBranchId.value, employeeId: targetEmployeeId.value, reason },
        requestIds: [props.request.id],
      })
    } else if (mode === 'deleteSold') {
      await actionMut.mutateAsync({
        url: SalesRequestEndpoints.deleteSold(props.request.id),
        body: { reason, confirmPayments: confirmPayments.value },
        requestIds: [props.request.id],
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
.sr-far {
  margin-inline-start: auto;
}
.sr-impact {
  background: #fff5f4;
  border-radius: 14px;
  padding: 0.75rem 0.9rem;
  font-size: 0.85rem;
}
.sr-impact__warn {
  color: #9b1c1c;
  font-weight: 700;
  margin: 0 0 0.5rem;
}
.sr-impact dl {
  display: grid;
  gap: 0.35rem 1rem;
  grid-template-columns: 1fr 1fr;
  margin: 0;
}
.sr-impact dl > div.is-wide {
  grid-column: 1 / -1;
}
.sr-impact dt {
  color: #6b5b5a;
  font-size: 0.75rem;
}
.sr-impact dd {
  margin: 0;
  font-weight: 700;
}
.sr-confirm {
  align-items: center;
  display: flex;
  gap: 0.5rem;
  margin-top: 0.6rem;
  font-weight: 700;
}
.sr-err {
  color: #9b1c1c;
  font-size: 0.85rem;
  margin: 0;
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
