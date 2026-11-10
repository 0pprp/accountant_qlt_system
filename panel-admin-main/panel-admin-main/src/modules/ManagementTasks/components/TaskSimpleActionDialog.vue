<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="$t(`managementTasks.actions.${action}`)"
    :style="{ width: 'min(480px, 94vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !busy && emit('close')"
  >
    <div class="mt-dialog-body">
      <p>{{ $t(`managementTasks.confirmMessage.${action}`, { title: taskTitle }) }}</p>
      <footer class="mt-dialog-foot">
        <button
          type="button"
          class="mt-btn ghost"
          :disabled="busy"
          @click="emit('close')"
        >
          {{ $t('managementTasks.actions.cancel') }}
        </button>
        <button
          type="button"
          class="mt-btn primary"
          :disabled="busy"
          @click="submit"
        >
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : $t(`managementTasks.actions.${action}`)
          }}
        </button>
      </footer>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { Dialog } from 'primevue'
import type { ManagementTaskSimpleAction } from '../domain/types'

const props = defineProps<{
  visible: boolean
  action: ManagementTaskSimpleAction
  taskTitle: string
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: []
}>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

function submit() {
  if (props.busy) return
  emit('submit')
}
</script>
