<template>
  <div>
    <span
      class="input__label"
      :class="{ required }"
      :aria-required="required"
      :aria-label="$t(label)"
    >
      {{ $t(label) }}
    </span>

    <Textarea
      v-model:modelValue="localName"
      :placeholder="$t(placeholder)"
      name="name"
      :rows="rows"
      :readonly
      :required
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs py-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { Textarea, Message } from 'primevue'
import type { Props, Emits } from './TextareaField.types'

defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<Props['modelValue']>('modelValue')
</script>
