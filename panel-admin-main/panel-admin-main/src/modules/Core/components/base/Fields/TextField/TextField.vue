<template>
  <AppInput
    v-model="textValue"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :error="error"
    :required="required"
    :readonly="readonly"
    :disabled="disabled"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props, Emits } from './TextField.types'
import { AppInput } from '@/modules/Core/components/ui'

defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<string | number>('modelValue')

const textValue = computed({
  get() {
    return localName.value == null ? '' : String(localName.value)
  },
  set(value: string) {
    localName.value = value
  },
})
</script>
