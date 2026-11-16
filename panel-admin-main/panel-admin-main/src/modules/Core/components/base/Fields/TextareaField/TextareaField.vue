<template>
  <AppTextarea
    v-model="textValue"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :error="error"
    :required="required"
    :readonly="readonly"
    :rows="rows"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props, Emits } from './TextareaField.types'
import { AppTextarea } from '@/modules/Core/components/ui'

defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<string | null>('modelValue')

const textValue = computed({
  get() {
    return localName.value ?? ''
  },
  set(value: string) {
    localName.value = value
  },
})
</script>
