<template>
  <AppField :label="label" :hint="hint" :error="error" :required="required" :grow="grow">
    <div class="q-date">
      <input
        class="q-input q-date__input"
        type="date"
        :value="modelValue ?? ''"
        :disabled="disabled"
        :min="min"
        :max="max"
        :name="name"
        :aria-invalid="error ? true : undefined"
        lang="en"
        @input="onInput"
      />
      <i class="pi pi-calendar q-date__icon" aria-hidden="true" />
    </div>
  </AppField>
</template>

<script setup lang="ts">
import AppField from './AppField.vue'

withDefaults(
  defineProps<{
    modelValue?: string | null
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    disabled?: boolean
    min?: string
    max?: string
    name?: string
  }>(),
  {
    modelValue: '',
    required: false,
    grow: false,
    disabled: false,
  },
)

const emit = defineEmits<{ 'update:modelValue': [string] }>()

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLInputElement).value)
}
</script>

<style scoped>
.q-date {
  position: relative;
}
.q-date__input {
  padding-inline-end: 42px;
  font-variant-numeric: lining-nums tabular-nums;
}
.q-date__input::-webkit-calendar-picker-indicator {
  opacity: 0;
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  cursor: pointer;
}
.q-date__icon {
  position: absolute;
  inset-inline-end: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #006a64;
  pointer-events: none;
  font-size: 14px;
}
</style>
