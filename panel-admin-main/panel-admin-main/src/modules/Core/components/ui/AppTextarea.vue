<template>
  <AppField :label="label" :hint="hint" :error="error" :required="required" :grow="grow">
    <textarea
      class="q-textarea"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :disabled="disabled"
      :readonly="readonly"
      :rows="rows"
      :maxlength="maxlength"
      :name="name"
      ref="el"
      :aria-invalid="error ? true : undefined"
      @input="onInput"
    />
  </AppField>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import AppField from './AppField.vue'

const el = ref<HTMLTextAreaElement | null>(null)
defineExpose({ focus: () => el.value?.focus() })

withDefaults(
  defineProps<{
    modelValue?: string | null
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    placeholder?: string
    disabled?: boolean
    readonly?: boolean
    rows?: number
    maxlength?: number | string
    name?: string
  }>(),
  {
    modelValue: '',
    required: false,
    grow: true,
    disabled: false,
    readonly: false,
    rows: 4,
  },
)

const emit = defineEmits<{ 'update:modelValue': [string] }>()

function onInput(e: Event) {
  emit('update:modelValue', (e.target as HTMLTextAreaElement).value)
}
</script>
