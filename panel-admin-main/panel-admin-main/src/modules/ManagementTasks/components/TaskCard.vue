<template>
  <article
    class="mt-card"
    :class="{ overdue: task.isOverdue }"
  >
    <header class="mt-card-top">
      <div>
        <span class="mt-card-id">#{{ task.id }}</span>
        <h3 class="mt-card-title">{{ task.title }}</h3>
        <span
          class="mt-prio"
          :class="`p-${task.priority}`"
        >
          {{ $t('managementTasks.priorityLabel') }}:
          {{ $t(`managementTasks.priority.${MANAGEMENT_TASK_PRIORITY_KEYS[task.priority]}`) }}
        </span>
      </div>
      <span
        class="mt-badge"
        :class="`status-${task.status}`"
      >
        {{ $t(`managementTasks.status.${statusKey(task.status)}`) }}
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
      <div v-if="task.currentHolderUserId !== task.assigneeUserId">
        <dt>{{ $t('managementTasks.fields.holder') }}</dt>
        <dd>{{ task.currentHolderDisplayName }}</dd>
      </div>
      <div>
        <dt>{{ $t('managementTasks.fields.branch') }}</dt>
        <dd>{{ task.branchName || '—' }}</dd>
      </div>
      <div>
        <dt>{{ $t('managementTasks.fields.time') }}</dt>
        <dd>{{ formatDateTime(task.statusChangedAtUtc || task.createdAt) }}</dd>
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
    </dl>

    <p
      v-if="task.rejectNote"
      class="mt-reject"
    >
      <strong>{{ $t('managementTasks.fields.lastRejectReason') }}</strong>
      {{ task.rejectNote }}
    </p>

    <button
      type="button"
      class="mt-card-open"
      @click="emit('open', task)"
    >
      <span>{{ $t('managementTasks.actions.openDetail') }}</span>
      <span aria-hidden="true">←</span>
    </button>
  </article>
</template>

<script setup lang="ts">
import {
  MANAGEMENT_TASK_PRIORITY_KEYS,
  type ManagementTaskListItem,
} from '../domain/types'
import { useManagementTaskFormat } from '../composables/useManagementTaskFormat'

defineProps<{
  task: ManagementTaskListItem
}>()

const emit = defineEmits<{
  open: [ManagementTaskListItem]
}>()

const { formatDate, formatDateTime, statusKey } = useManagementTaskFormat()
</script>
