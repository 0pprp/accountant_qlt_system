<template>
  <AppDateField
    v-if="!timeOnly"
    v-model="formattedDate"
    :label="$t(label)"
    :hint="$t(placeholder)"
    :error="error"
    :required="required"
    :disabled="readonly"
  />

  <AppInput
    v-else
    v-model="timeValue"
    type="time"
    :label="$t(label)"
    :placeholder="$t(placeholder)"
    :error="error"
    :required="required"
    :readonly="readonly"
    :disabled="readonly"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props, Emits } from './CalendarField.types'
import { AppDateField, AppInput } from '@/modules/Core/components/ui'

defineProps<Props>()
defineEmits<Emits>()

const formattedDate = defineModel<string | null>('modelValue')

const timeValue = computed({
  get() {
    return formattedDate.value ?? ''
  },
  set(value: string) {
    formattedDate.value = value ? value.slice(0, 5) : ''
  },
})
</script>
