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

    <InputText
      v-model="localName"
      :placeholder="$t(placeholder)"
      name="name"
      :required
      :readonly
      :disabled
    />

    <Message
      v-if="error"
      severity="error"
      class="text-error text-xs pt-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { InputText, Message } from 'primevue'
import type { Props, Emits } from './TextField.types'

defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<string>('modelValue')
</script>
