<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :close-on-escape="!busy"
    :dismissable-mask="!busy"
    :header="$t('managementTasks.actions.reject')"
    :style="{ width: 'min(520px, 94vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !busy && emit('close')"
  >
    <form
      class="mt-dialog-body"
      novalidate
      @submit.prevent="submit"
    >
      <label class="mt-field">
        <span>{{ $t('managementTasks.fields.rejectNote') }} *</span>
        <textarea
          v-model="note"
          class="mt-textarea"
          rows="4"
          maxlength="1000"
          required
          :disabled="busy"
          :aria-invalid="touched && !trimmed"
        />
      </label>
      <p
        v-if="touched && !trimmed"
        class="mt-error"
        role="alert"
      >
        {{ $t('managementTasks.validation.rejectNoteRequired') }}
      </p>
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
          type="submit"
          class="mt-btn danger"
          :disabled="busy || !trimmed"
        >
          {{
            busy
              ? $t('managementTasks.actions.submitting')
              : $t('managementTasks.actions.reject')
          }}
        </button>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Dialog } from 'primevue'

const props = defineProps<{
  visible: boolean
  busy?: boolean
}>()

const emit = defineEmits<{
  close: []
  submit: [note: string]
}>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const note = ref('')
const touched = ref(false)
const trimmed = computed(() => note.value.trim())

watch(
  () => props.visible,
  (v) => {
    if (v) {
      note.value = ''
      touched.value = false
    }
  }
)

function submit() {
  touched.value = true
  if (props.busy || !trimmed.value) return
  emit('submit', trimmed.value)
}
</script>
