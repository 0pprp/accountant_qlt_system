<template>
  <div
    class="select-field"
    :class="{ 'select-field--clearable': clearable && localValue != null }"
  >
    <AppSelect
      v-model="selectValue"
      :label="$t(label)"
      :placeholder="$t(placeholder)"
      :options="options"
      :error="error"
      :required="required"
      :disabled="Boolean(readonly || loading)"
    />

    <button
      v-if="clearable && localValue != null"
      type="button"
      class="select-field__clear"
      :disabled="Boolean(readonly || loading)"
      aria-label="مسح"
      @click="localValue = null"
    >
      <i
        class="pi pi-times"
        aria-hidden="true"
      />
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Props, Emits } from './SelectField.types'
import { AppSelect } from '@/modules/Core/components/ui'

defineProps<Props>()
defineEmits<Emits>()

const localValue = defineModel<number | null | undefined>('modelValue', {
  set(value) {
    return value ?? null
  },
})

const selectValue = computed<string | number | null | boolean>({
  get() {
    return localValue.value ?? null
  },
  set(value) {
    localValue.value = typeof value === 'number' ? value : null
  },
})
</script>

<style scoped>
.select-field {
  position: relative;
  min-width: 0;
}

.select-field__clear {
  position: absolute;
  z-index: 2;
  inset-inline-end: 36px;
  top: calc(14px * 1.4 + 6px);
  display: grid;
  place-items: center;
  width: 28px;
  height: 48px;
  padding: 0;
  border: 0;
  background: transparent;
  color: var(--q-muted, #5c6f6d);
  cursor: pointer;
}

.select-field__clear:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.select-field--clearable :deep(.q-select-menu__value) {
  padding-inline-end: 40px;
}
</style>
