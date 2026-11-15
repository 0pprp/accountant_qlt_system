<template>
  <Dialog
    v-model:visible="open"
    modal
    :closable="false"
    :header="title"
    class="q-confirm-reason"
    :pt="{
      mask: { class: 'bg-[#C6C6C680] backdrop-blur-[2px]' },
      root: { class: 'rounded-2xl overflow-hidden max-w-[420px] w-[92vw]' },
    }"
  >
    <p v-if="message" class="q-confirm-reason__msg">{{ message }}</p>
    <AppTextarea
      v-model="reason"
      :label="reasonLabel"
      :placeholder="reasonPlaceholder"
      :disabled="busy"
      :error="error || undefined"
      required
    />
    <div class="q-confirm-reason__actions">
      <AppButton variant="primary" :loading="busy" :disabled="busy" @click="submit">
        {{ confirmLabel }}
      </AppButton>
      <AppButton variant="ghost" :disabled="busy" @click="close">
        {{ cancelLabel }}
      </AppButton>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import AppButton from './AppButton.vue'
import AppTextarea from './AppTextarea.vue'

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    title: string
    message?: string
    reasonLabel?: string
    reasonPlaceholder?: string
    confirmLabel?: string
    cancelLabel?: string
    requireReason?: boolean
    busy?: boolean
  }>(),
  {
    reasonLabel: 'السبب / الملاحظة',
    reasonPlaceholder: 'اكتب السبب كاملاً',
    confirmLabel: 'تأكيد',
    cancelLabel: 'إلغاء',
    requireReason: true,
    busy: false,
  },
)

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: [reason: string]
  cancel: []
}>()

const reason = ref('')
const error = ref('')

const open = computed({
  get: () => props.modelValue,
  set: (v: boolean) => emit('update:modelValue', v),
})

watch(
  () => props.modelValue,
  (v) => {
    if (v) {
      reason.value = ''
      error.value = ''
    }
  },
)

function close() {
  emit('cancel')
  open.value = false
}

function submit() {
  const text = reason.value.trim()
  if (props.requireReason && !text) {
    error.value = 'السبب إلزامي'
    return
  }
  emit('confirm', text)
}
</script>

<style scoped>
.q-confirm-reason__msg {
  margin: 0 0 12px;
  font-size: 16px;
  line-height: 1.7;
  color: #00201e;
  white-space: pre-wrap;
}
.q-confirm-reason__actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
}
.q-confirm-reason__actions :deep(.q-btn--primary) {
  flex: 2;
}
.q-confirm-reason__actions :deep(.q-btn--ghost) {
  flex: 1;
}
</style>
