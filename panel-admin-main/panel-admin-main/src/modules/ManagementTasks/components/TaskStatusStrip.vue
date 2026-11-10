<template>
  <div
    class="mt-strip"
    role="group"
    :aria-label="$t('managementTasks.filters.statusStrip')"
  >
    <button
      type="button"
      class="mt-chip"
      :class="{ active: modelValue == null }"
      :aria-pressed="modelValue == null"
      @click="emit('update:modelValue', null)"
    >
      {{ $t('managementTasks.filters.all') }}
    </button>
    <button
      v-for="status in MANAGEMENT_TASK_STATUS_ORDER"
      :key="status"
      type="button"
      class="mt-chip"
      :class="{ active: modelValue === status }"
      :aria-pressed="modelValue === status"
      @click="emit('update:modelValue', status)"
    >
      {{ $t(`managementTasks.status.${MANAGEMENT_TASK_STATUS_KEYS[status]}`) }}
    </button>
  </div>
</template>

<script setup lang="ts">
import {
  MANAGEMENT_TASK_STATUS_KEYS,
  MANAGEMENT_TASK_STATUS_ORDER,
  type ManagementTaskStatusValue,
} from '../domain/types'

defineProps<{
  modelValue: ManagementTaskStatusValue | null
}>()

const emit = defineEmits<{
  'update:modelValue': [ManagementTaskStatusValue | null]
}>()
</script>
