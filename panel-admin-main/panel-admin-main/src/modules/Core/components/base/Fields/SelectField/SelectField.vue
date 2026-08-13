<template>
  <div class="flex flex-col">
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <Select
      v-model="localValue"
      :options="options"
      optionLabel="label"
      :unstyled="false"
      optionValue="value"
      :showClear="clearable"
      :class="['select', { 'select--clearable': clearable }]"
      :required
      :disabled="readonly"
      :loading
      :placeholder="$t(placeholder)"
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { Select, Message } from 'primevue'
import type { Props, Emits } from './SelectField.types'

defineProps<Props>()
defineEmits<Emits>()

const localValue = defineModel<number | null | undefined>('modelValue', {
  set(value) {
    return value ?? null
  },
})
</script>

<style scoped>
.select--clearable :deep(.p-select-clear-icon) {
  margin-inline-end: 8px;
}
</style>
