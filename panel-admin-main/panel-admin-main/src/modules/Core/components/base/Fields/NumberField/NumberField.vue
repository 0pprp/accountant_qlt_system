<template>
  <AppInput
    :modelValue="numberText"
    type="number"
    :label="$t(label)"
    :placeholder="displayPlaceholder"
    :hint="fieldHint"
    :error="required && error ? error : undefined"
    :required="required"
    :readonly="readonly"
    :disabled="disabled"
    @update:model-value="onNumberInput"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import type { Props, Emits } from './NumberField.types'
import { AppInput } from '@/modules/Core/components/ui'

const props = defineProps<Props>()
defineEmits<Emits>()

const localName = defineModel<number | null>('modelValue')
const { t } = useI18n()
const numberText = ref('')

const displayPlaceholder = computed(() => {
  const isNumeric = /^\d+(\.\d+)?$/.test(props.placeholder)

  if (isNumeric) return props.placeholder
  return t(props.placeholder)
})

const fieldHint = computed(() => {
  const parts: string[] = []
  if (props.isPercent) parts.push('%')
  if (props.suffix) parts.push(props.suffix)
  return parts.length ? parts.join(' ') : undefined
})

watch(
  localName,
  (value) => {
    const shown = Number(numberText.value)
    if (numberText.value !== '' && !Number.isNaN(shown) && shown === value) return
    if (
      numberText.value.endsWith('.') &&
      Number(numberText.value.slice(0, -1)) === value
    ) {
      return
    }
    numberText.value = value == null ? '' : String(value)
  },
  { immediate: true }
)

function clamp(value: number) {
  if (!props.isPercent) return value
  return Math.min(100, Math.max(0, value))
}

function onNumberInput(value: string) {
  numberText.value = value
  const trimmed = value.trim()
  if (trimmed === '') {
    localName.value = null
    return
  }

  const parsed = Number(trimmed)
  if (Number.isNaN(parsed)) return

  const next = clamp(parsed)
  localName.value = next
  if (next !== parsed) numberText.value = String(next)
}
</script>
