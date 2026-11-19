<template>
  <DfbDialog :visible="visible" :title="title ?? 'تقييم القائمة'" :subtitle="listName" size="sm" :busy="busy" @close="emit('close')">
    <form id="dfb-rate-form" class="dfb-rate-form" novalidate @submit.prevent="submit">
      <div class="dfb-field" role="group" aria-label="الدرجة">
        <span>الدرجة من {{ BOARD_SCORE_MIN }} إلى {{ BOARD_SCORE_MAX }} *</span>
        <div class="dfb-score-grid">
          <button
            v-for="n in scores"
            :key="n"
            type="button"
            class="dfb-score"
            :aria-pressed="score === n"
            :disabled="busy"
            @click="score = n"
          >
            <span class="dfb-num">{{ n }}</span>
          </button>
        </div>
        <p v-if="touched && score == null" class="dfb-form-error" role="alert">اختر الدرجة أولاً</p>
      </div>

      <label class="dfb-field">
        <span>{{ noteRequired ? 'الملاحظة * (إلزامية عند التقييم 6 أو أقل)' : 'الملاحظة (اختيارية)' }}</span>
        <textarea v-model="note" class="dfb-textarea" rows="4" :maxlength="BOARD_NOTE_MAX" :disabled="busy" />
        <span class="dfb-hint"><span class="dfb-num">{{ note.length }}/{{ BOARD_NOTE_MAX }}</span></span>
        <p v-if="touched && noteMissing" class="dfb-form-error" role="alert">الملاحظة إلزامية عندما يكون التقييم 6 أو أقل</p>
      </label>
    </form>
    <template #footer>
      <button type="button" class="dfb-button" :disabled="busy" @click="emit('close')">إلغاء</button>
      <button type="submit" form="dfb-rate-form" class="dfb-button is-primary" :disabled="busy">
        {{ busy ? 'جاري الحفظ…' : 'حفظ التقييم' }}
      </button>
    </template>
  </DfbDialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import DfbDialog from '@/modules/Core/components/neumorphic/NeuDialog.vue'
import {
  BOARD_NOTE_MAX,
  BOARD_NOTE_REQUIRED_BELOW,
  BOARD_SCORE_MAX,
  BOARD_SCORE_MIN,
} from '../domain/types'

const props = defineProps<{
  visible: boolean
  busy?: boolean
  listName?: string
  initialScore?: number | null
  initialNote?: string | null
  title?: string
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: { score: number; note: string | null }]
}>()

const scores = Array.from({ length: BOARD_SCORE_MAX - BOARD_SCORE_MIN + 1 }, (_, i) => BOARD_SCORE_MIN + i)
const score = ref<number | null>(null)
const note = ref('')
const touched = ref(false)
const noteRequired = computed(() => score.value != null && score.value < BOARD_NOTE_REQUIRED_BELOW)
const noteMissing = computed(() => noteRequired.value && note.value.trim() === '')

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
  if (props.busy || score.value == null || noteMissing.value) return
  const trimmed = note.value.trim()
  emit('submit', { score: score.value, note: trimmed === '' ? null : trimmed })
}
</script>

<style scoped>
.dfb-rate-form { display: flex; flex-direction: column; gap: 20px; }
</style>
