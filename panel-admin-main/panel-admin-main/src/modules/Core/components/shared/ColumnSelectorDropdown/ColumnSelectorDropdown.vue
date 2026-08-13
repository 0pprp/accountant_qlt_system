<template>
  <div class="min-w-[240px] max-w-md">
    <span class="input__label" :aria-label="$t(labelKey)">
      {{ $t(labelKey) }}
    </span>

    <MultiSelect
      v-model="selectedColumnKeys"
      :options="options"
      optionLabel="displayName"
      optionValue="key"
      display="chip"
      :showToggleAll="true"
      :placeholder="$t(placeholderKey)"
      class="select w-full"
      :unstyled="false"
    />
  </div>
</template>

<script setup lang="ts">
import { MultiSelect } from 'primevue'
import type { Props } from './ColumnSelectorDropdown.types'

defineProps<Props>()

const selectedColumnKeys = defineModel<Array<string>>('modelValue', {
  default: () => [],
})
</script>
