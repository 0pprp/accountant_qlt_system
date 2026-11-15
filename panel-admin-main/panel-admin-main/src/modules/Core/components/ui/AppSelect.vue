<template>
  <!-- Designed searchable select — never a naked native <select> -->
  <AppSelectMenu
    :model-value="modelValue"
    :options="options"
    :label="label"
    :hint="hint"
    :error="error"
    :required="required"
    :grow="grow"
    :placeholder="placeholder"
    :disabled="disabled"
    :filterable="filterable"
    :filter-placeholder="filterPlaceholder"
    :empty-message="emptyMessage"
    @update:model-value="$emit('update:modelValue', $event)"
  />
</template>

<script setup lang="ts">
import AppSelectMenu from './AppSelectMenu.vue'
import type { AppSelectMenuOption } from './AppSelectMenu.vue'

export type AppSelectOption = AppSelectMenuOption

withDefaults(
  defineProps<{
    modelValue?: string | number | null | boolean
    options: AppSelectOption[]
    label?: string
    hint?: string
    error?: string
    required?: boolean
    grow?: boolean
    placeholder?: string
    disabled?: boolean
    filterable?: boolean
    filterPlaceholder?: string
    emptyMessage?: string
    /** @deprecated kept for call-site compat; unused */
    name?: string
    emptyValue?: string
  }>(),
  {
    required: false,
    grow: false,
    disabled: false,
    filterable: true,
    filterPlaceholder: 'ابحث…',
    emptyMessage: 'لا نتائج',
  },
)

defineEmits<{ 'update:modelValue': [string | number | null | boolean] }>()
</script>
