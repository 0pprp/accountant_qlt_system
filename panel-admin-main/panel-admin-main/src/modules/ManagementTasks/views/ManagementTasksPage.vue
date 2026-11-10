<template>
  <div
    class="mt-page"
    dir="rtl"
  >
    <header class="mt-page-head">
      <div class="mt-page-head-text">
        <h1 class="mt-page-title">{{ props.inboxOnly ? 'مهامي' : $t('managementTasks.title') }}</h1>
        <p
          v-if="canEnter"
          class="mt-realtime"
          :data-state="connectionState"
          aria-live="polite"
        >
          {{ statusLabel }}
        </p>
      </div>
      <button
        v-if="canCreate"
        type="button"
        class="mt-btn primary"
        :disabled="branchId == null"
        @click="formMode = 'create'"
      >
        <i
          class="pi pi-plus"
          aria-hidden="true"
        />
        {{ $t('managementTasks.actions.create') }}
      </button>
    </header>

    <p
      v-if="!canEnter"
      class="mt-error"
      role="alert"
    >
      {{ $t('managementTasks.noReadAccess') }}
    </p>

    <template v-else>
      <section class="mt-toolbar">
        <label class="mt-field">
          <span>{{ $t('managementTasks.filters.search') }}</span>
          <span class="mt-search-wrap">
            <input
              v-model="searchInput"
              type="search"
              class="mt-input"
              :placeholder="$t('managementTasks.filters.searchPlaceholder')"
              :aria-label="$t('managementTasks.filters.search')"
              autocomplete="off"
            >
            <i
              class="pi pi-search mt-search-icon"
              aria-hidden="true"
            />
          </span>
        </label>
        <button
          v-if="canUseInbox && !props.inboxOnly"
          type="button"
          class="mt-chip"
          :class="{ active: inboxForMe }"
          :aria-pressed="inboxForMe"
          @click="inboxForMe = !inboxForMe"
        >
          {{ $t('managementTasks.filters.inboxForMe') }}
        </button>
      </section>

      <TaskStatusStrip v-model="status" />

      <p
        v-if="listError"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.listError') }}
      </p>
      <p
        v-else-if="listLoading"
        class="mt-muted"
        aria-live="polite"
      >
        {{ $t('managementTasks.loading') }}
      </p>
      <p
        v-else-if="!items.length"
        class="mt-empty"
      >
        {{ $t('managementTasks.empty') }}
      </p>

      <div
        v-if="items.length"
        class="mt-cards"
      >
        <TaskCard
          v-for="item in items"
          :key="item.id"
          :task="item"
          @open="openDetail"
        />
      </div>

      <nav
        v-if="totalPages > 1"
        class="mt-pager"
        :aria-label="$t('managementTasks.pager.label')"
      >
        <button
          type="button"
          class="mt-btn ghost"
          :disabled="pageIndex <= 1"
          @click="pageIndex--"
        >
          {{ $t('managementTasks.pager.prev') }}
        </button>
        <span>{{ $t('managementTasks.pager.page', { page: pageIndex, total: totalPages }) }}</span>
        <button
          type="button"
          class="mt-btn ghost"
          :disabled="pageIndex >= totalPages"
          @click="pageIndex++"
        >
          {{ $t('managementTasks.pager.next') }}
        </button>
      </nav>
    </template>

    <TaskDetailDialog
      :visible="selectedTaskId != null"
      :detail="detail"
      :loading="detailQuery.isLoading.value"
      :error="detailQuery.isError.value"
      :busy="anyBusy"
      :uploading="uploadMutation.isPending.value"
      :downloading-id="downloadingId"
      @close="closeDetail"
      @action="onDetailAction"
      @upload="onUpload"
      @download="onDownload"
    />

    <TaskSimpleActionDialog
      :visible="simpleAction != null"
      :action="simpleAction ?? 'accept'"
      :task-title="task?.title ?? ''"
      :busy="actionMutation.isPending.value"
      @close="simpleAction = null"
      @submit="submitSimple"
    />

    <TaskRejectDialog
      :visible="rejectOpen"
      :busy="actionMutation.isPending.value"
      @close="rejectOpen = false"
      @submit="submitReject"
    />

    <TaskTransferDialog
      :visible="transferOpen"
      :branch-id="task?.branchId ?? branchId"
      :exclude-user-ids="transferExcluded"
      :busy="actionMutation.isPending.value"
      @close="transferOpen = false"
      @submit="submitTransfer"
    />

    <TaskFormDialog
      :visible="formMode != null"
      :mode="formMode ?? 'create'"
      :branch-id="formMode === 'resend' ? (task?.branchId ?? branchId) : branchId"
      :task="task"
      :busy="formBusy"
      @close="formMode = null"
      @submit="submitForm"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/modules/Auth/store'
import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'
import {
  type ManagementTaskAttachment,
  type ManagementTaskListFilters,
  type ManagementTaskListItem,
  type ManagementTaskSimpleAction,
  type ManagementTaskStatusValue,
} from '../domain/types'
import { useManagementTaskAccess } from '../composables/useManagementTaskAccess'
import { useManagementTasksQuery } from '../requests/queries/useManagementTasksQuery'
import { useManagementTaskDetailQuery } from '../requests/queries/useManagementTaskDetailQuery'
import { useManagementTaskRealtime } from '../realtime/managementTaskRealtime'
import { useManagementTaskActionMutation } from '../requests/mutations/useManagementTaskActionMutation'
import {
  useCreateManagementTaskMutation,
} from '../requests/mutations/useCreateManagementTaskMutation'
import { useResendManagementTaskMutation } from '../requests/mutations/useResendManagementTaskMutation'
import {
  uploadManagementTaskAttachment,
  useUploadManagementTaskAttachmentMutation,
} from '../requests/mutations/useUploadManagementTaskAttachmentMutation'
import { downloadManagementTaskAttachment } from '../requests/mutations/downloadManagementTaskAttachment'
import TaskCard from '../components/TaskCard.vue'
import TaskStatusStrip from '../components/TaskStatusStrip.vue'
import TaskDetailDialog, {
  type TaskDetailAction,
} from '../components/TaskDetailDialog.vue'
import TaskSimpleActionDialog from '../components/TaskSimpleActionDialog.vue'
import TaskRejectDialog from '../components/TaskRejectDialog.vue'
import TaskTransferDialog from '../components/TaskTransferDialog.vue'
import TaskFormDialog, {
  type TaskFormSubmit,
} from '../components/TaskFormDialog.vue'

const PAGE_SIZE = 12

const { t } = useI18n()
const auth = useAuthStore()
const toast = useToastStore()
const { canRead, canCreate, canUseInbox, currentUserId } = useManagementTaskAccess()

const props = withDefaults(defineProps<{ inboxOnly?: boolean }>(), { inboxOnly: false })
const canEnter = computed(() => canRead.value || (props.inboxOnly && canUseInbox.value))
const branchId = computed(() => auth.selectedBranch?.id ?? null)

// —— فلاتر ——
const status = ref<ManagementTaskStatusValue | null>(null)
const inboxForMe = ref(props.inboxOnly)
const searchInput = ref('')
const search = ref('')
const pageIndex = ref(1)
const pageSize = ref(PAGE_SIZE)

let searchTimer: ReturnType<typeof setTimeout> | undefined
watch(searchInput, (value) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    search.value = value.trim()
  }, 350)
})
onBeforeUnmount(() => clearTimeout(searchTimer))

watch([status, inboxForMe, search], () => {
  pageIndex.value = 1
})

const filters = computed<ManagementTaskListFilters>(() => ({
  branchId: branchId.value,
  status: status.value,
  search: search.value,
  inboxForMe: inboxForMe.value,
}))

const listQuery = useManagementTasksQuery(pageIndex, pageSize, filters)
const items = computed(() => listQuery.data.value?.items ?? [])
const totalPages = computed(() =>
  Math.max(1, Math.ceil((listQuery.data.value?.totalCount ?? 0) / pageSize.value))
)
const listLoading = computed(() => listQuery.isLoading.value)
const listError = computed(() => listQuery.isError.value)

// —— التفاصيل ——
const selectedTaskId = ref<number | null>(null)
const realtimeEnabled = computed(() => canEnter.value && branchId.value != null)
const { connectionState, statusLabel } = useManagementTaskRealtime({
  enabled: realtimeEnabled,
  openTaskId: selectedTaskId,
})
const detailQuery = useManagementTaskDetailQuery(selectedTaskId)
const detail = computed(() => detailQuery.data.value ?? null)
/** النسخة الحيّة — expectedVersion يُؤخذ منها دائماً */
const task = computed(() => detail.value?.task ?? null)

function openDetail(item: ManagementTaskListItem) {
  selectedTaskId.value = item.id
}

function closeDetail() {
  selectedTaskId.value = null
  simpleAction.value = null
  rejectOpen.value = false
  transferOpen.value = false
  if (formMode.value === 'resend') formMode.value = null
}

// —— الإجراءات ——
const actionMutation = useManagementTaskActionMutation()
const createMutation = useCreateManagementTaskMutation()
const resendMutation = useResendManagementTaskMutation()
const uploadMutation = useUploadManagementTaskAttachmentMutation()

const simpleAction = ref<ManagementTaskSimpleAction | null>(null)
const rejectOpen = ref(false)
const transferOpen = ref(false)
const formMode = ref<'create' | 'resend' | null>(null)
const creatingWithFiles = ref(false)
const downloadingId = ref<number | null>(null)

const formBusy = computed(
  () =>
    createMutation.isPending.value ||
    resendMutation.isPending.value ||
    creatingWithFiles.value
)
const anyBusy = computed(
  () => actionMutation.isPending.value || formBusy.value
)

const transferExcluded = computed(() => {
  const ids: number[] = []
  if (task.value) ids.push(task.value.currentHolderUserId)
  if (currentUserId.value != null) ids.push(currentUserId.value)
  return ids
})

function notifySuccess(key: string) {
  toast.setMassage({
    title: t(key),
    dialogState: DialogState.Success,
    isOpen: true,
  })
}

function onDetailAction(action: TaskDetailAction) {
  if (anyBusy.value) return
  switch (action) {
    case 'accept':
    case 'complete':
    case 'confirm':
      simpleAction.value = action
      break
    case 'reject':
      rejectOpen.value = true
      break
    case 'transfer':
      transferOpen.value = true
      break
    case 'resend':
      formMode.value = 'resend'
      break
  }
}

async function submitSimple() {
  const current = task.value
  const action = simpleAction.value
  if (!current || !action || actionMutation.isPending.value) return
  try {
    await actionMutation.mutateAsync({
      taskId: current.id,
      action,
      expectedVersion: current.expectedVersion,
    })
    simpleAction.value = null
    notifySuccess(`managementTasks.success.${action}`)
  } catch {
    // الخطأ يُعرض عبر اعتراض axios؛ التفاصيل تُحدَّث تلقائياً (expectedVersion)
    simpleAction.value = null
  }
}

async function submitReject(note: string) {
  const current = task.value
  if (!current || actionMutation.isPending.value) return
  try {
    await actionMutation.mutateAsync({
      taskId: current.id,
      action: 'reject',
      expectedVersion: current.expectedVersion,
      note,
    })
    rejectOpen.value = false
    notifySuccess('managementTasks.success.reject')
  } catch {
    /* toast من axios */
  }
}

async function submitTransfer(payload: { targetUserId: number; note?: string }) {
  const current = task.value
  if (!current || actionMutation.isPending.value) return
  try {
    await actionMutation.mutateAsync({
      taskId: current.id,
      action: 'transfer',
      expectedVersion: current.expectedVersion,
      targetUserId: payload.targetUserId,
      note: payload.note,
    })
    transferOpen.value = false
    notifySuccess('managementTasks.success.transfer')
  } catch {
    /* toast من axios */
  }
}

async function submitForm(payload: TaskFormSubmit) {
  if (formBusy.value) return
  if (formMode.value === 'resend') {
    const current = task.value
    if (!current) return
    try {
      await resendMutation.mutateAsync({
        taskId: current.id,
        payload: {
          expectedVersion: current.expectedVersion,
          title: payload.title,
          description: payload.description,
          priority: payload.priority,
          dueDate: payload.dueDate,
          assigneeUserId: payload.assigneeUserId,
          customerId: payload.customerId,
          orderId: payload.orderId,
        },
      })
      formMode.value = null
      notifySuccess('managementTasks.success.resend')
    } catch {
      /* toast من axios */
    }
    return
  }

  const targetBranch = branchId.value
  if (targetBranch == null) return
  creatingWithFiles.value = true
  try {
    const created = await createMutation.mutateAsync({
      title: payload.title,
      description: payload.description,
      priority: payload.priority,
      dueDate: payload.dueDate,
      branchId: targetBranch,
      assigneeUserId: payload.assigneeUserId,
      customerId: payload.customerId,
      orderId: payload.orderId,
    })
    for (const file of payload.files) {
      try {
        await uploadManagementTaskAttachment(created.id, file)
      } catch {
        /* toast من axios — المهمة أُنشئت، يمكن إعادة رفع المرفق من التفاصيل */
      }
    }
    formMode.value = null
    notifySuccess('managementTasks.success.create')
  } catch {
    /* toast من axios */
  } finally {
    creatingWithFiles.value = false
  }
}

async function onUpload(file: File) {
  const current = task.value
  if (!current || uploadMutation.isPending.value) return
  try {
    await uploadMutation.mutateAsync({ taskId: current.id, file })
    notifySuccess('managementTasks.success.upload')
  } catch {
    /* toast من axios */
  }
}

async function onDownload(file: ManagementTaskAttachment) {
  const current = task.value
  if (!current || downloadingId.value != null) return
  downloadingId.value = file.id
  try {
    await downloadManagementTaskAttachment(current.id, file.id, file.fileName)
  } catch {
    /* toast من axios */
  } finally {
    downloadingId.value = null
  }
}

// تبديل فرع الحساب: أغلق كل شيء وأعد الصفحة الأولى (ManagementWorkspace يعيد التركيب أيضاً)
watch(
  () => [branchId.value, auth.branchGeneration],
  () => {
    closeDetail()
    formMode.value = null
    pageIndex.value = 1
  }
)

// تجنّب صفحة خارج النطاق بعد تغيّر النتائج
watch(totalPages, (total) => {
  if (pageIndex.value > total) pageIndex.value = total
})
</script>

<style src="../components/management-tasks.css"></style>
