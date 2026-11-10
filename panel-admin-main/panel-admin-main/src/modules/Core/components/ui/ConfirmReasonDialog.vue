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
    <label class="q-confirm-reason__label">
      {{ reasonLabel }}
      <textarea
        v-model="reason"
        rows="4"
        :placeholder="reasonPlaceholder"
        :disabled="busy"
      />
    </label>
    <p v-if="error" class="q-confirm-reason__err" role="alert">{{ error }}</p>
    <div class="q-confirm-reason__actions">
      <button type="button" class="primary" :disabled="busy" @click="submit">
        {{ confirmLabel }}
      </button>
      <button type="button" class="ghost" :disabled="busy" @click="close">
        {{ cancelLabel }}
      </button>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'

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
  v => {
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
.q-confirm-reason__label {
  display: grid;
  gap: 8px;
  font-size: 14px;
  color: #526b67;
}
.q-confirm-reason__label textarea {
  min-height: 96px;
  background: #f8f8f8;
  border: 1px solid #cfd8d7;
  border-radius: 10px;
  padding: 12px;
  font: inherit;
  color: #00201e;
  resize: vertical;
}
.q-confirm-reason__err {
  margin: 8px 0 0;
  color: #963d35;
  font-size: 14px;
}
.q-confirm-reason__actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
}
.q-confirm-reason__actions button {
  min-height: 44px;
  padding: 10px 16px;
  border-radius: 10px;
  font: inherit;
  font-weight: 600;
  cursor: pointer;
}
.q-confirm-reason__actions .primary {
  flex: 2;
  background: #006a64;
  color: #fff;
  border: 1px solid #006a64;
}
.q-confirm-reason__actions .ghost {
  flex: 1;
  background: #fff;
  color: #006a64;
  border: 1px solid #cfdfda;
}
</style>
