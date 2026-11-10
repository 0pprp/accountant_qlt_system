<template>
  <Dialog
    :visible="visible"
    modal
    closable
    dismissable-mask
    close-on-escape
    :header="task?.title || $t('managementTasks.detail.title')"
    :style="{ width: 'min(860px, 96vw)' }"
    :pt="dialogMaskPt"
    class="mt-detail-dialog bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && emit('close')"
  >
    <div
      v-if="loading && !detail"
      class="mt-muted"
    >
      {{ $t('managementTasks.loading') }}
    </div>
    <div
      v-else-if="error && !detail"
      class="mt-error"
      role="alert"
    >
      {{ $t('managementTasks.detail.loadError') }}
    </div>

    <div
      v-else-if="detail && task"
      class="mt-dialog-body"
    >
      <header class="mt-card-top">
        <span
          class="mt-badge"
          :class="`status-${task.status}`"
        >
          {{ $t(`managementTasks.status.${statusKey(task.status)}`) }}
        </span>
        <span
          class="mt-prio"
          :class="`p-${task.priority}`"
        >
          {{ $t('managementTasks.priorityLabel') }}:
          {{ $t(`managementTasks.priority.${MANAGEMENT_TASK_PRIORITY_KEYS[task.priority]}`) }}
        </span>
      </header>

      <dl class="mt-grid">
        <div>
          <dt>{{ $t('managementTasks.fields.sender') }}</dt>
          <dd>{{ task.createdByDisplayName || '—' }}</dd>
        </div>
        <div>
          <dt>{{ $t('managementTasks.fields.receiver') }}</dt>
          <dd>{{ task.assigneeDisplayName }}</dd>
        </div>
        <div>
          <dt>{{ $t('managementTasks.fields.holder') }}</dt>
          <dd>{{ task.currentHolderDisplayName }}</dd>
        </div>
        <div>
          <dt>{{ $t('managementTasks.fields.branch') }}</dt>
          <dd>{{ task.branchName || '—' }}</dd>
        </div>
        <div>
          <dt>{{ $t('managementTasks.fields.createdAt') }}</dt>
          <dd>{{ formatDateTime(task.createdAt) }}</dd>
        </div>
        <div>
          <dt>{{ $t('managementTasks.fields.dueDate') }}</dt>
          <dd :class="{ 'mt-overdue': task.isOverdue }">
            {{ formatDate(task.dueDate) }}
            <template v-if="task.isOverdue">
              · {{ $t('managementTasks.overdue') }}
            </template>
          </dd>
        </div>
        <div v-if="task.customerId">
          <dt>{{ $t('managementTasks.fields.customerId') }}</dt>
          <dd>#{{ task.customerId }}</dd>
        </div>
        <div v-if="task.orderId">
          <dt>{{ $t('managementTasks.fields.orderId') }}</dt>
          <dd>#{{ task.orderId }}</dd>
        </div>
      </dl>

      <section
        v-if="task.description"
        class="mt-section"
      >
        <h3>{{ $t('managementTasks.fields.description') }}</h3>
        <p class="mt-desc">{{ task.description }}</p>
      </section>

      <p
        v-if="task.rejectNote"
        class="mt-reject"
      >
        <strong>{{ $t('managementTasks.fields.lastRejectReason') }}</strong>
        {{ task.rejectNote }}
      </p>

      <section class="mt-section">
        <h3>{{ $t('managementTasks.detail.attachments') }}</h3>
        <p
          v-if="!detail.attachments.length"
          class="mt-muted"
        >
          {{ $t('managementTasks.detail.noAttachments') }}
        </p>
        <ul
          v-else
          class="mt-list"
        >
          <li
            v-for="file in detail.attachments"
            :key="file.id"
            class="mt-list-item"
          >
            <span>
              {{ file.fileName }}
              <small>· {{ formatSize(file.sizeBytes) }} · {{ formatDateTime(file.uploadedAtUtc) }}</small>
            </span>
            <button
              v-if="canReadAttachment"
              type="button"
              class="mt-btn outline"
              :disabled="downloadingId === file.id"
              @click="emit('download', file)"
            >
              {{
                downloadingId === file.id
                  ? $t('managementTasks.actions.downloading')
                  : $t('managementTasks.actions.download')
              }}
            </button>
          </li>
        </ul>

        <div
          v-if="canCreate"
          class="mt-field"
          style="margin-top: 0.75rem"
        >
          <label class="mt-label">{{ $t('managementTasks.detail.addAttachment') }}</label>
          <input
            type="file"
            class="mt-file-input"
            :disabled="uploading"
            :aria-label="$t('managementTasks.detail.addAttachment')"
            @change="onFileChange"
          >
          <small v-if="uploading">{{ $t('managementTasks.actions.uploading') }}</small>
        </div>
      </section>

      <section class="mt-section">
        <h3>{{ $t('managementTasks.detail.history') }}</h3>
        <TaskEventHistory :events="sortedEvents" />
      </section>

      <footer class="mt-dialog-foot">
        <button
          v-if="actions.accept"
          type="button"
          class="mt-btn primary"
          :disabled="busy"
          @click="emit('action', 'accept')"
        >
          {{ $t('managementTasks.actions.accept') }}
        </button>
        <button
          v-if="actions.complete"
          type="button"
          class="mt-btn primary"
          :disabled="busy"
          @click="emit('action', 'complete')"
        >
          {{ $t('managementTasks.actions.complete') }}
        </button>
        <button
          v-if="actions.confirm"
          type="button"
          class="mt-btn primary"
          :disabled="busy"
          @click="emit('action', 'confirm')"
        >
          {{ $t('managementTasks.actions.confirm') }}
        </button>
        <button
          v-if="actions.transfer"
          type="button"
          class="mt-btn outline"
          :disabled="busy"
          @click="emit('action', 'transfer')"
        >
          {{ $t('managementTasks.actions.transfer') }}
        </button>
        <button
          v-if="actions.resend"
          type="button"
          class="mt-btn outline"
          :disabled="busy"
          @click="emit('action', 'resend')"
        >
          {{ $t('managementTasks.actions.resend') }}
        </button>
        <button
          v-if="actions.reject"
          type="button"
          class="mt-btn danger"
          :disabled="busy"
          @click="emit('action', 'reject')"
        >
          {{ $t('managementTasks.actions.reject') }}
        </button>
        <button
          type="button"
          class="mt-btn ghost"
          @click="emit('close')"
        >
          {{ $t('managementTasks.actions.close') }}
        </button>
      </footer>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Dialog } from 'primevue'
import {
  MANAGEMENT_TASK_PRIORITY_KEYS,
  type ManagementTaskAttachment,
  type ManagementTaskDetail,
} from '../domain/types'
import { useManagementTaskAccess } from '../composables/useManagementTaskAccess'
import { useManagementTaskFormat } from '../composables/useManagementTaskFormat'
import TaskEventHistory from './TaskEventHistory.vue'

export type TaskDetailAction =
  | 'accept'
  | 'complete'
  | 'confirm'
  | 'reject'
  | 'transfer'
  | 'resend'

const props = defineProps<{
  visible: boolean
  detail: ManagementTaskDetail | null
  loading?: boolean
  error?: boolean
  busy?: boolean
  uploading?: boolean
  downloadingId?: number | null
}>()

const emit = defineEmits<{
  close: []
  action: [TaskDetailAction]
  upload: [File]
  download: [ManagementTaskAttachment]
}>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const { actionsFor, canCreate, canReadAttachment } = useManagementTaskAccess()
const { formatDate, formatDateTime, formatSize, statusKey } = useManagementTaskFormat()

const task = computed(() => props.detail?.task ?? null)
const actions = computed(() =>
  task.value
    ? actionsFor(task.value)
    : {
        accept: false,
        reject: false,
        complete: false,
        transfer: false,
        confirm: false,
        resend: false,
      }
)
const sortedEvents = computed(() =>
  [...(props.detail?.events ?? [])].sort(
    (a, b) => new Date(b.atUtc).getTime() - new Date(a.atUtc).getTime()
  )
)

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file || props.uploading) return
  emit('upload', file)
  input.value = ''
}
</script>
