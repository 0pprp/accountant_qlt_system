<template>
  <p
    v-if="!events.length"
    class="mt-muted"
  >
    {{ $t('managementTasks.detail.noEvents') }}
  </p>
  <ol
    v-else
    class="mt-timeline"
  >
    <li
      v-for="event in events"
      :key="event.id"
    >
      <strong>{{ $t(`managementTasks.event.${MANAGEMENT_TASK_EVENT_KEYS[event.eventType] ?? 'updated'}`) }}</strong>
      <small>
        {{ event.actorDisplayName || '—' }} · {{ formatDateTime(event.atUtc) }}
      </small>
      <small v-if="event.targetDisplayName">
        {{ $t('managementTasks.fields.target') }}: {{ event.targetDisplayName }}
      </small>
      <small v-if="event.previousStatus != null && event.newStatus != null">
        {{ $t(`managementTasks.status.${statusKey(event.previousStatus)}`) }}
        ←
        {{ $t(`managementTasks.status.${statusKey(event.newStatus)}`) }}
      </small>
      <p v-if="event.note">{{ event.note }}</p>
    </li>
  </ol>
</template>

<script setup lang="ts">
import {
  MANAGEMENT_TASK_EVENT_KEYS,
  type ManagementTaskEvent,
} from '../domain/types'
import { useManagementTaskFormat } from '../composables/useManagementTaskFormat'

defineProps<{
  events: ManagementTaskEvent[]
}>()

const { formatDateTime, statusKey } = useManagementTaskFormat()
</script>
