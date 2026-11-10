<template>
  <div class="q-action-row" role="group">
    <button
      v-if="primary"
      type="button"
      class="q-action-row__primary"
      :disabled="primary.disabled || busy"
      @click="$emit('primary')"
    >
      {{ primary.label }}
    </button>
    <button
      v-for="(action, i) in secondary"
      :key="`${action.label}-${i}`"
      type="button"
      class="q-action-row__secondary"
      :disabled="action.disabled || busy"
      @click="$emit('secondary', action.id ?? action.label)"
    >
      {{ action.label }}
    </button>
    <slot />
  </div>
</template>

<script setup lang="ts">
export type UiAction = { id?: string; label: string; disabled?: boolean }

defineProps<{
  primary?: UiAction
  secondary?: UiAction[]
  busy?: boolean
}>()

defineEmits<{ primary: []; secondary: [id: string] }>()
</script>

<style scoped>
.q-action-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 16px;
}
.q-action-row__primary,
.q-action-row__secondary {
  min-height: 44px;
  min-width: 44px;
  padding: 10px 16px;
  border-radius: 10px;
  font: inherit;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
}
.q-action-row__primary {
  background: #006a64;
  color: #fff;
  border: 1px solid #006a64;
}
.q-action-row__secondary {
  background: #edf5f3;
  color: #006a64;
  border: 1px solid #cfdfda;
}
.q-action-row__primary:disabled,
.q-action-row__secondary:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}
.q-action-row__primary:focus-visible,
.q-action-row__secondary:focus-visible {
  outline: 3px solid #006a64;
  outline-offset: 2px;
}
</style>
