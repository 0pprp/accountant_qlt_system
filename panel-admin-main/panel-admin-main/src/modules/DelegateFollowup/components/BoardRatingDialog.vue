<template>
  <Dialog
    :visible="visible"
    modal
    :closable="!busy"
    :closeOnEscape="!busy"
    :dismissableMask="!busy"
    :header="$t('delegateFollowup.dialog.rateTitle')"
    :style="{ width: 'min(560px, 96vw)' }"
    :pt="dialogMaskPt"
    class="bg-white rounded-3xl p-5"
    dir="rtl"
    @update:visible="!$event && !busy && emit('close')"
  >
    <form class="df-dialog-body" novalidate @submit.prevent="submit">
      <p v-if="listName" class="df-muted">{{ listName }}</p>

      <div class="df-field" role="group" :aria-label="$t('delegateFollowup.dialog.scoreLabel')">
        <span>{{ $t('delegateFollowup.dialog.scoreLabel') }} *</span>
        <div class="df-score-grid">
          <button
            v-for="n in scores"
            :key="n"
            type="button"
            class="df-score-btn"
            :aria-pressed="score === n"
            :disabled="busy"
            @click="score = n"
          >
            <span class="df-num">{{ n }}</span>
          </button>
        </div>
        <p v-if="touched && score == null" class="df-form-error" role="alert">
          {{ $t('delegateFollowup.dialog.scoreRequired') }}
        </p>
      </div>

      <label class="df-field">
        <span>{{ $t('delegateFollowup.dialog.noteLabel') }}</span>
        <textarea
          v-model="note"
          class="df-textarea"
          rows="4"
          :maxlength="BOARD_NOTE_MAX"
          :disabled="busy"
        />
        <span class="df-muted">
          {{ $t('delegateFollowup.dialog.noteCounter') }} ·
          <span class="df-num">{{ note.length }}/{{ BOARD_NOTE_MAX }}</span>
        </span>
      </label>

      <footer class="df-dialog-foot">
        <button
          type="button"
          class="df-btn ghost"
          :disabled="busy"
          @click="emit('close')"
        >
          {{ $t('delegateFollowup.dialog.cancel') }}
        </button>
        <button type="submit" class="df-btn primary" :disabled="busy">
          {{
            busy
              ? $t('delegateFollowup.dialog.saving')
              : $t('delegateFollowup.dialog.save')
          }}
        </button>
      </footer>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { Dialog } from 'primevue'
import {
  BOARD_NOTE_MAX,
  BOARD_SCORE_MAX,
  BOARD_SCORE_MIN,
} from '../domain/types'

const props = defineProps<{
  visible: boolean
  busy?: boolean
  listName?: string
  initialScore?: number | null
  initialNote?: string | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: { score: number; note: string | null }]
}>()

const dialogMaskPt = {
  mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
}

const scores = Array.from(
  { length: BOARD_SCORE_MAX - BOARD_SCORE_MIN + 1 },
  (_, i) => BOARD_SCORE_MIN + i
)

const score = ref<number | null>(null)
const note = ref('')
const touched = ref(false)

watch(
  () => props.visible,
  (v) => {
    if (v) {
      score.value = props.initialScore ?? null
      note.value = props.initialNote ?? ''
      touched.value = false
    }
  },
  { immediate: true }
)

function submit() {
  touched.value = true
  if (props.busy || score.value == null) return
  const trimmed = note.value.trim()
  emit('submit', { score: score.value, note: trimmed === '' ? null : trimmed })
}
</script>
