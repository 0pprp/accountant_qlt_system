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

    <Password
      v-model:modelValue="localName"
      :placeholder="$t(placeholder)"
      :required
      toggleMask
      :feedback="false"
      :unstyled="false"
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
import { Password, Message } from 'primevue'
import type { Props, Emits } from './PasswordField.types'

defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<Props['modelValue']>('modelValue')
</script>
