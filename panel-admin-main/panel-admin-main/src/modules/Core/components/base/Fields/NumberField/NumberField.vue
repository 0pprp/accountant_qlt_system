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

    <InputNumber
      v-model:modelValue="localName"
      :placeholder="displayPlaceholder"
      name="name"
      :required
      :readonly
      :disabled
      :suffix="suffix"
      :prefix="isPercent ? '%' : ''"
      :min="isPercent ? 0 : undefined"
      :max="isPercent ? 100 : undefined"
    />

    <Message
      v-if="error && required"
      severity="error"
      class="text-error text-xs py-2"
      variant="simple"
    >
      {{ error }}
    </Message>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { InputNumber, Message } from 'primevue'
import type { Props, Emits } from './NumberField.types'

const props = defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<Props['modelValue']>('modelValue')
const { t } = useI18n()

const displayPlaceholder = computed(() => {
  const isNumeric = /^\d+(\.\d+)?$/.test(props.placeholder)

  if (isNumeric) {
    return props.placeholder
  } else {
    return t(props.placeholder)
  }
})
</script>
